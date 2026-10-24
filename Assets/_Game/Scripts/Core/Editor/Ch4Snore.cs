using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Tú Bà's snoring for Chapter 4 from real recordings (docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md §16). Sources:
    /// Wikimedia Commons, CC BY-SA, in Assets/_Game/Audio/Ch4Foley/Source/commons_snore (see Ch4Foley/CREDITS.md).
    ///   Measure: -executeMethod ThuyKieu.Core.Editor.Ch4Snore.AnalyseBatch   (report: Tools/LauTuBa/QA/SnoreSources.txt)
    /// The breath clock in <see cref="ThuyKieu.Gameplay.Chapters.LauTuBa.TuBaSleeper"/> is the rule; each clip is cut to fit
    /// inside its phase (an in-breath snore never runs on into the out-breath).
    /// </summary>
    public static class Ch4Snore
    {
        public const string SourceFolder = Ch4Foley.SourceRoot + "/commons_snore";

        public static void AnalyseBatch()
        {
            try
            {
                var report = new StringBuilder();
                report.AppendLine("Snore sources: measured, not listened to.");
                report.AppendLine("Timeline: one character per 100 ms, level above the file's quiet floor in 3 dB steps (. = floor, 9 = 27 dB or more).");
                foreach (string path in AssetDatabase.FindAssets("t:AudioClip", new[] { SourceFolder }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p))
                {
                    var x = Ch4Foley.Samples(path, out int rate, out int channels);
                    var frames = Frames(x, rate, 0.05f);
                    float floor = Percentile(frames, 0.15f), peak = frames.Max();
                    report.AppendLine();
                    report.AppendLine($"{Path.GetFileName(path)} · {channels} ch · {rate} Hz · {x.Length / (float)rate:F1} s · floor {floor:F1} dB · loudest 50 ms {peak:F1} dB");
                    var line = new StringBuilder();
                    for (int i = 0; i + 1 < frames.Length; i += 2)
                    {
                        float above = Mathf.Max(frames[i], frames[i + 1]) - floor;
                        line.Append(above < 3f ? '.' : (char)('0' + Mathf.Clamp((int)(above / 3f), 0, 9)));
                        if (line.Length % 100 == 0) { report.AppendLine($"  {(i / 2 - 99) / 10f,6:F1} s  {line}"); line.Clear(); }
                    }
                    if (line.Length > 0) report.AppendLine($"  {(frames.Length / 2 - line.Length) / 10f,6:F1} s  {line}");
                    foreach (var e in Events(frames, floor, peak, 0.05f))
                        report.AppendLine($"  event {e.start:F2}–{e.start + e.length:F2} s · {e.length:F2} s · peak +{e.peak - floor:F0} dB · then quiet {e.gap:F2} s · low share {LowShare(x, rate, e.start, e.length):F2}");
                }
                Directory.CreateDirectory("Tools/LauTuBa/QA");
                File.WriteAllText("Tools/LauTuBa/QA/SnoreSources.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        // ------------------------------------------------------------------ build

        /// <summary>Raised a semitone: a little lighter than the men recorded, without a pitched-up voice.</summary>
        private const float Semitones = 1f;
        private const int Rate = 22050;
        /// <summary>Exhale phase of the breath clock (TuBaSleeper._exhale): an out-breath take never runs past it.</summary>
        private const float ExhaleSeconds = 2.6f;
        private const float InhaleDb = -16f, ExhaleDb = -31f;

        /// <summary>Recordings with a steady sleep rhythm: one snore on each in-breath, then a quiet, audible out-breath.</summary>
        private static readonly (string file, int takes)[] Sources =
        {
            ("Tongue_base_snoring.ogg", 6), ("Palatal_snoring_1.ogg", 4), ("Palatal_snoring_2.ogg", 2)
        };

        /// <summary>
        /// Renders the in-breath (snore_deep) and out-breath (snore_out) takes and puts them in the Chapter 4 Foley bank,
        /// replacing any older snore groups (the step and door groups are left as they are). A/B file and report as for
        /// the Foley.   Batch: -executeMethod ThuyKieu.Core.Editor.Ch4Snore.BuildBatch
        /// </summary>
        public static void BuildBatch()
        {
            try
            {
                var bank = AssetDatabase.LoadAssetAtPath<ThuyKieu.Presentation.FoleyBank>(Ch4Foley.BankPath);
                if (bank == null) throw new System.InvalidOperationException(Ch4Foley.BankPath + " missing: run Ch4Foley.BuildBatch first");
                var groups = bank.Groups.Where(g => g.Id != "snore_deep" && g.Id != "snore_out").ToList();
                var report = new StringBuilder();
                AddTo(groups, report);
                bank.Groups = groups.ToArray();
                EditorUtility.SetDirty(bank);
                AssetDatabase.SaveAssets();
                File.WriteAllText("Tools/LauTuBa/QA/SnoreBuild.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>Builds the snore takes and adds their two groups to <paramref name="groups"/> (also called by Ch4Foley.Build).</summary>
        public static void AddTo(List<ThuyKieu.Presentation.FoleyBank.Group> groups, StringBuilder report)
        {
            report.AppendLine("Tú Bà's breath (measured, not listened to)");
            report.AppendLine("columns: output · from · source seconds · length s · gain dB · peak dBFS · loudest 300 ms RMS dBFS");
            var inhales = new List<string>();
            var exhales = new List<string>();
            var abCycles = new List<(float[] snore, float[] breath)>();
            foreach (var (file, wanted) in Sources)
            {
                var x = Ch4Foley.Samples(SourceFolder + "/" + file, out int rate, out _);
                var frames = Frames(x, rate, 0.05f);
                float floor = Percentile(frames, 0.15f), peak = frames.Max();
                // Steady cycles only: a snore of a natural length with a full quiet out-breath after it.
                var cycles = Events(frames, floor, peak, 0.05f).Where(e => e.start > 0.5f && e.length >= 0.95f && e.length <= 1.6f && e.gap >= 2.2f).ToList();
                for (int k = 0; k < wanted && cycles.Count > 0; k++)
                {
                    var e = cycles[Mathf.Min(cycles.Count - 1, (int)((k + 0.5f) * cycles.Count / wanted))];
                    float pitch = Mathf.Pow(2f, Semitones / 12f);
                    float snoreFrom = e.start - 0.05f, snoreTo = e.start + e.length + 0.1f;
                    // The out-breath: from the end of the snore to just before the next one, at most one exhale phase long.
                    float breathFrom = snoreTo, breathTo = Mathf.Min(snoreTo + e.gap - 0.3f, breathFrom + (ExhaleSeconds - 0.08f) * pitch);
                    var snore = Render(x, rate, snoreFrom, snoreTo, pitch, 0.012f, 0.09f);
                    var breath = Render(x, rate, breathFrom, breathTo, pitch, 0.05f, 0.35f);
                    // One gain for the pair keeps their natural balance; the out-breath alone may be lifted (at most +9 dB) so
                    // it can be heard at the bedside.
                    float gain = Gain(snore, InhaleDb);
                    float lift = Mathf.Clamp(Mathf.Pow(10f, ExhaleDb / 20f) / Mathf.Max(1e-6f, Loudest(breath, 0.3f) * gain), 1f, Mathf.Pow(10f, 9f / 20f));
                    float breathGain = Limit(breath, gain * lift);
                    gain = Limit(snore, gain);
                    Scale(snore, gain);
                    Scale(breath, breathGain);
                    string from = $"{file} {snoreFrom:F2}–{snoreTo:F2} s";
                    inhales.Add(Write("snore_deep", inhales.Count + 1, snore, from, gain, report));
                    exhales.Add(Write("snore_out", exhales.Count + 1, breath, $"{file} {breathFrom:F2}–{breathTo:F2} s", breathGain, report));
                    abCycles.Add((snore, breath));
                    cycles.Remove(e);
                }
            }
            AssetDatabase.Refresh();
            foreach (var path in inhales.Concat(exhales)) Configure(path);
            groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = "snore_deep", Takes = inhales.Select(AssetDatabase.LoadAssetAtPath<AudioClip>).ToArray(), Gain = 1f });
            groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = "snore_out", Takes = exhales.Select(AssetDatabase.LoadAssetAtPath<AudioClip>).ToArray(), Gain = 1f });
            WriteAB(abCycles, report);
            report.AppendLine($"  in-breaths {inhales.Count}, lasting {abCycles.Min(c => c.snore.Length) / (float)Rate:F2}–{abCycles.Max(c => c.snore.Length) / (float)Rate:F2} s (the in-breath phase lasts as long as its take); out-breaths at most {abCycles.Max(c => c.breath.Length) / (float)Rate:F2} s (phase {ExhaleSeconds:F1} s)");
        }

        /// <summary>A stretch of the source, raised by <paramref name="pitch"/>, at 22.05 kHz, low-passed under the source's band, faded.</summary>
        private static float[] Render(float[] x, int rate, float from, float to, float pitch, float fadeIn, float fadeOut)
        {
            int a = Mathf.Clamp((int)(from * rate), 0, x.Length - 1), b = Mathf.Clamp((int)(to * rate), a + 1, x.Length);
            var part = new float[b - a];
            System.Array.Copy(x, a, part, 0, part.Length);
            // Reading the source faster raises it; resampling to the output rate in the same step.
            var y = new float[(int)(part.Length * (double)Rate / (rate * pitch))];
            for (int i = 0; i < y.Length; i++)
            {
                double p = i * (double)rate * pitch / Rate;
                int i0 = Mathf.Min(part.Length - 1, (int)p), i1 = Mathf.Min(part.Length - 1, i0 + 1);
                y[i] = Mathf.Lerp(part[i0], part[i1], (float)(p - i0));
            }
            // Twice through a low-pass just under the source's top (8 kHz recordings stop at 4 kHz): no images of the
            // upsampling heard as hiss.
            float cutoff = Mathf.Min(rate * pitch * 0.45f, 9000f);
            for (int pass = 0; pass < 2; pass++) LowPass(y, cutoff);
            int fi = Mathf.Min(y.Length / 4, (int)(fadeIn * Rate)), fo = Mathf.Min(y.Length / 3, (int)(fadeOut * Rate));
            for (int i = 0; i < fi; i++) y[i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fi);
            for (int i = 0; i < fo; i++) y[y.Length - 1 - i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fo);
            return y;
        }

        /// <summary>RBJ biquad low-pass, Q 0.707, in place.</summary>
        private static void LowPass(float[] y, float cutoff)
        {
            double w = 2.0 * System.Math.PI * cutoff / Rate, alpha = System.Math.Sin(w) / (2.0 * 0.7071), cos = System.Math.Cos(w);
            double a0 = 1 + alpha, b0 = (1 - cos) / 2 / a0, b1 = (1 - cos) / a0, b2 = b0, a1 = -2 * cos / a0, a2 = (1 - alpha) / a0;
            double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int i = 0; i < y.Length; i++)
            {
                double x0 = y[i], o = b0 * x0 + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
                x2 = x1; x1 = x0; y2 = y1; y1 = o;
                y[i] = (float)o;
            }
        }

        private static float Loudest(float[] y, float seconds)
        {
            int n = Mathf.Max(1, (int)(seconds * Rate)), step = Mathf.Max(1, n / 4);
            float best = 0f;
            for (int i = 0; i + n <= y.Length; i += step) best = Mathf.Max(best, Ch4Foley.Rms(y, i, i + n - 1));
            return best > 0f ? best : Ch4Foley.Rms(y, 0, y.Length - 1);
        }

        private static float Gain(float[] y, float db) => Mathf.Pow(10f, db / 20f) / Mathf.Max(1e-6f, Loudest(y, 0.3f));

        /// <summary>The gain, lowered if needed to keep 1 dB of headroom.</summary>
        private static float Limit(float[] y, float gain)
        {
            float peak = y.Max(v => Mathf.Abs(v)) * gain;
            return peak > 0.89f ? gain * 0.89f / peak : gain;
        }

        private static void Scale(float[] y, float gain) { for (int i = 0; i < y.Length; i++) y[i] *= gain; }

        private static string Write(string id, int index, float[] y, string from, float gain, StringBuilder report)
        {
            string folder = Ch4Foley.Root + "/" + id;
            Directory.CreateDirectory(folder);
            string path = $"{folder}/{id}_{index:00}.wav";
            AudioBankBaker.WriteWav(path, y, Rate);
            report.AppendLine($"{Path.GetFileName(path)} · {from} · {y.Length / (float)Rate:F2} · {Ch4Foley.Db(gain):+0.0;-0.0} · {Ch4Foley.Db(y.Max(v => Mathf.Abs(v))):F1} · {Ch4Foley.Db(Loudest(y, 0.3f)):F1}");
            return path;
        }

        private static void Configure(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is AudioImporter importer)) return;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.8f;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
        }

        /// <summary>
        /// For listening: four cycles of the old synthetic breath, a second of silence, then the new takes, each laid on the
        /// game's clock (in-breath for as long as its take, out-breath phase 2.6 s), as Tú Bà breathes in the chapter.
        /// </summary>
        private static void WriteAB(List<(float[] snore, float[] breath)> cycles, StringBuilder report)
        {
            var mix = new List<float>();
            void Lay(float[] clip, float seconds, float gain)
            {
                int length = Mathf.Max(clip?.Length ?? 0, (int)(seconds * Rate));
                for (int i = 0; i < length; i++) mix.Add(clip != null && i < clip.Length ? Mathf.Clamp(clip[i] * gain, -1f, 1f) : 0f);
            }
            float[] Old(string id)
            {
                var clip = ThuyKieu.Presentation.AudioBank.Get(ThuyKieu.Presentation.AudioBank.Sfx, id);
                if (clip == null || clip.loadType != AudioClipLoadType.DecompressOnLoad || !clip.LoadAudioData()) return null;
                var data = new float[clip.samples * clip.channels];
                clip.GetData(data, 0);
                var mono = new float[clip.samples];
                for (int i = 0; i < clip.samples; i++) { float s = 0f; for (int c = 0; c < clip.channels; c++) s += data[i * clip.channels + c]; mono[i] = s / clip.channels; }
                var y = new float[(int)((long)mono.Length * Rate / clip.frequency)];
                for (int i = 0; i < y.Length; i++) y[i] = mono[Mathf.Min(mono.Length - 1, (int)((long)i * clip.frequency / Rate))];
                return y;
            }
            float[] oldIn = Old("snore_deep"), oldOut = Old("snore_out");
            // The old sounds at the in-game volumes of the time (0.75 in, 0.35 out), the new at the new ones (0.8 / 0.5).
            for (int k = 0; k < 4; k++) { Lay(oldIn, 1.7f, 0.75f); Lay(oldOut, 2.6f, 0.35f); }
            Lay(null, 1f, 1f);
            foreach (var (snore, breath) in cycles) { Lay(snore, 0f, 0.8f); Lay(breath, ExhaleSeconds, 0.5f); }
            Directory.CreateDirectory("Tools/LauTuBa/Foley");
            AudioBankBaker.WriteWav("Tools/LauTuBa/Foley/AB_snore.wav", mix.ToArray(), Rate);
            report.AppendLine($"  A/B: Tools/LauTuBa/Foley/AB_snore.wav ({mix.Count / (float)Rate:F0} s; old 4 cycles, 1 s silence, new {cycles.Count} cycles)" + (oldIn == null ? " · old in-breath not readable" : ""));
        }

        /// <summary>Level in dBFS of consecutive frames of <paramref name="seconds"/>.</summary>
        public static float[] Frames(float[] x, int rate, float seconds)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(seconds * rate));
            var frames = new float[x.Length / n];
            for (int f = 0; f < frames.Length; f++) frames[f] = Ch4Foley.Db(Ch4Foley.Rms(x, f * n, f * n + n - 1));
            return frames;
        }

        private static float Percentile(float[] values, float p)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            return sorted.Length == 0 ? -120f : sorted[Mathf.Clamp((int)(p * sorted.Length), 0, sorted.Length - 1)];
        }

        /// <summary>Loud stretches (above 45 % of the floor-to-peak range), joined across dips shorter than 150 ms.</summary>
        public static List<(float start, float length, float peak, float gap)> Events(float[] frames, float floor, float peak, float frame)
        {
            float threshold = floor + (peak - floor) * 0.45f;
            var spans = new List<(int a, int b)>();
            int start = -1;
            for (int i = 0; i <= frames.Length; i++)
            {
                bool loud = i < frames.Length && frames[i] >= threshold;
                if (loud && start < 0) start = i;
                if (!loud && start >= 0) { spans.Add((start, i)); start = -1; }
            }
            var joined = new List<(int a, int b)>();
            foreach (var s in spans)
            {
                if (joined.Count > 0 && (s.a - joined[^1].b) * frame < 0.15f) joined[^1] = (joined[^1].a, s.b);
                else joined.Add(s);
            }
            var events = new List<(float, float, float, float)>();
            for (int k = 0; k < joined.Count; k++)
            {
                var (a, b) = joined[k];
                if ((b - a) * frame < 0.2f) continue;
                float top = -200f;
                for (int i = a; i < b; i++) top = Mathf.Max(top, frames[i]);
                float gap = (k + 1 < joined.Count ? joined[k + 1].a - b : frames.Length - b) * frame;
                events.Add((a * frame, (b - a) * frame, top, gap));
            }
            return events;
        }

        /// <summary>Share of energy under ~300 Hz in a stretch (a snore is low and rough, a breath is airy).</summary>
        public static float LowShare(float[] x, int rate, float start, float length)
        {
            int a = Mathf.Clamp((int)(start * rate), 0, x.Length - 1), b = Mathf.Clamp((int)((start + length) * rate), 0, x.Length - 1);
            float k = 1f - Mathf.Exp(-2f * Mathf.PI * 300f / rate), lp = 0f, low = 0f, all = 0f;
            for (int i = a; i <= b; i++) { lp += k * (x[i] - lp); low += lp * lp; all += x[i] * x[i]; }
            return all > 0f ? low / all : 0f;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Chapter 4 Foley from recordings (docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md §14). The source recordings sit in
    /// Assets/_Game/Audio/Ch4Foley/Source with their licences; this tool measures them and renders the edited one-shots.
    ///   Measure: -executeMethod ThuyKieu.Core.Editor.Ch4Foley.AnalyseBatch   (report: Tools/LauTuBa/QA/FoleySources.txt)
    /// Measurements only describe the signal (level, onset, tail, hits, brightness); they do not say a take sounds right.
    /// </summary>
    public static class Ch4Foley
    {
        public const string Root = "Assets/_Game/Audio/Ch4Foley";
        public const string SourceRoot = Root + "/Source";

        public struct Measure
        {
            public string Path;
            public int Channels, Rate;
            public float Length, PeakDb, RmsDb, AttackDb, Onset, Body, Zcr, LowShare;
            public int Hits;
        }

        public static void AnalyseBatch()
        {
            try
            {
                var report = new StringBuilder();
                report.AppendLine("Chapter 4 Foley sources: measured, not listened to.");
                report.AppendLine("columns: file · ch · Hz · length s · peak dBFS · body RMS dBFS · first 120 ms RMS dBFS · onset ms · body s · hits · zero crossings/s · share of energy under ~250 Hz");
                foreach (string path in AssetDatabase.FindAssets("t:AudioClip", new[] { SourceRoot }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p))
                {
                    var m = Analyse(path);
                    report.AppendLine($"{path.Substring(SourceRoot.Length + 1)} · {m.Channels} · {m.Rate} · {m.Length:F2} · {m.PeakDb:F1} · {m.RmsDb:F1} · {m.AttackDb:F1} · {m.Onset * 1000f:F0} · {m.Body:F2} · {m.Hits} · {m.Zcr:F0} · {m.LowShare:F2}");
                }
                Directory.CreateDirectory("Tools/LauTuBa/QA");
                File.WriteAllText("Tools/LauTuBa/QA/FoleySources.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        // ------------------------------------------------------------------ the recipe

        /// <summary>One sound id: which source takes, how long a one-shot may be, how loud its first 120 ms should sit.</summary>
        private sealed class Recipe
        {
            public string Id;
            public string[] Takes;
            public float MaxSeconds;
            public float AttackDb;
            /// <summary>Optional second recording mixed under each take (wet stone: a step plus a little water).</summary>
            public string[] Layer;
            public float LayerDb;
            /// <summary>Gait sets for footsteps: the lightest takes crouched, the heaviest running.</summary>
            public bool Gaits;
        }

        private static string[] Takes(string pattern, params int[] numbers) => numbers.Select(n => string.Format(pattern, n)).ToArray();

        // Chosen from the measurements (FoleySources.txt) and the packs' own names, not by listening (see §14).
        private static readonly Recipe[] Recipes =
        {
            // Footsteps: every take at the same level (-21 dBFS over its first 120 ms, low enough that none is limited); the
            // game sets the level per gait and floor, following the noise.
            new Recipe { Id = "step_wood", Takes = Takes("kenney_impact/footstep_wood_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.3f, AttackDb = -21f, Gaits = true },
            new Recipe { Id = "step_stone", Takes = Takes("kenney_impact/footstep_concrete_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.2f, AttackDb = -21f, Gaits = true },
            new Recipe { Id = "step_wet", Takes = Takes("kenney_impact/footstep_concrete_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.3f, AttackDb = -21f, Gaits = true,
                         Layer = Takes("oga_mud/mud_{0:00}.ogg", 24, 10, 6, 25, 13), LayerDb = -9f },
            new Recipe { Id = "step_mud", Takes = Takes("oga_mud/mud_{0:00}.ogg", 6, 8, 10, 11, 13, 24, 25), MaxSeconds = 0.35f, AttackDb = -21f, Gaits = true },
            new Recipe { Id = "step_earth", Takes = Takes("kenney_rpg/footstep{0:00}.ogg", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9), MaxSeconds = 0.3f, AttackDb = -21f, Gaits = true },
            // Carpet: the pack has five files but only three different recordings (001 = 002, 003 = 004).
            new Recipe { Id = "step_mat", Takes = Takes("kenney_impact/footstep_carpet_{0:000}.ogg", 0, 1, 3), MaxSeconds = 0.2f, AttackDb = -21f, Gaits = true },
            // Hinged wooden doors. Squeaking metal hinges (qubodup Open02, Open04, Close05) and takes with several separate
            // hits are left out.
            new Recipe { Id = "door_creak_soft", Takes = new[] { "kenney_rpg/creak1.ogg", "kenney_rpg/creak2.ogg", "kenney_rpg/creak3.ogg" }, MaxSeconds = 0.45f, AttackDb = -26f },
            new Recipe { Id = "door_fast", Takes = new[] { "kenney_rpg/doorOpen_1.ogg", "oga_doors/qubodup-DoorOpen01.ogg", "oga_doors/qubodup-DoorOpen05.ogg", "oga_doors/qubodup-DoorOpen06.ogg", "oga_doors/qubodup-DoorOpen08.ogg" }, MaxSeconds = 0.9f, AttackDb = -16f },
            new Recipe { Id = "door_slam", Takes = new[] { "oga_doors/qubodup-DoorClose02.ogg", "oga_doors/qubodup-DoorClose04.ogg", "oga_doors/qubodup-DoorClose07.ogg", "oga_doors/qubodup-DoorClose08.ogg", "kenney_rpg/doorClose_3.ogg", "kenney_rpg/doorClose_4.ogg" }, MaxSeconds = 0.7f, AttackDb = -13f },
            // Sliding paper doors: no recording of the slide itself was found; the leaf meeting the frame is a wooden knock.
            new Recipe { Id = "slide_slam", Takes = Takes("kenney_impact/impactWood_heavy_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.3f, AttackDb = -15f },
            new Recipe { Id = "slide_fast", Takes = Takes("kenney_impact/impactWood_medium_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.3f, AttackDb = -19f },
            // Bolts and latches.
            new Recipe { Id = "latch_drop", Takes = Takes("kenney_impact/impactWood_light_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.25f, AttackDb = -18f },
            new Recipe { Id = "bolt_fast", Takes = Takes("kenney_impact/impactPlank_medium_{0:000}.ogg", 0, 1, 2, 3, 4), MaxSeconds = 0.45f, AttackDb = -16f },
            new Recipe { Id = "lock_open", Takes = new[] { "kenney_rpg/metalLatch.ogg" }, MaxSeconds = 0.3f, AttackDb = -21f },
            new Recipe { Id = "key_turn", Takes = new[] { "kenney_rpg/metalClick.ogg" }, MaxSeconds = 0.4f, AttackDb = -26f },
        };

        /// <summary>
        /// Renders every recipe to edited mono one-shots under Assets/_Game/Audio/Ch4Foley/&lt;id&gt;/, builds Ch4FoleyBank.asset,
        /// writes A/B files (old sound, then the new takes, level-matched) under Tools/LauTuBa/Foley and a report.
        ///   Batch: -executeMethod ThuyKieu.Core.Editor.Ch4Foley.BuildBatch
        /// </summary>
        public static void BuildBatch()
        {
            try { Build(); EditorApplication.Exit(0); }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public const string BankPath = Root + "/Ch4FoleyBank.asset";

        public static void Build()
        {
            var report = new StringBuilder();
            report.AppendLine("Chapter 4 Foley: edited one-shots (measured, not listened to)");
            report.AppendLine("columns: output · from · trimmed start ms · length s · gain dB · peak dBFS · first 120 ms RMS dBFS");
            var groups = new List<ThuyKieu.Presentation.FoleyBank.Group>();
            Directory.CreateDirectory("Tools/LauTuBa/Foley");
            foreach (var recipe in Recipes)
            {
                string folder = Root + "/" + recipe.Id;
                Directory.CreateDirectory(folder);
                var written = new List<(string path, float attack)>();
                int rate = 44100;
                var abNew = new List<float[]>();
                for (int i = 0; i < recipe.Takes.Length; i++)
                {
                    var x = Samples(SourceRoot + "/" + recipe.Takes[i], out rate, out _);
                    var shot = OneShot(x, rate, recipe.MaxSeconds, out int start);
                    float sourceAttack = Rms(shot, 0, rate * 12 / 100);
                    string from = recipe.Takes[i];
                    if (recipe.Layer != null)
                    {
                        var layer = OneShot(Resample(Samples(SourceRoot + "/" + recipe.Layer[i % recipe.Layer.Length], out int layerRate, out _), layerRate, rate), rate, recipe.MaxSeconds, out _);
                        float scale = sourceAttack / Mathf.Max(1e-6f, Rms(layer, 0, rate * 12 / 100)) * Mathf.Pow(10f, recipe.LayerDb / 20f);
                        for (int s = 0; s < shot.Length && s < layer.Length; s++) shot[s] += layer[s] * scale;
                        from += " + " + recipe.Layer[i % recipe.Layer.Length] + $" ({recipe.LayerDb:F0} dB)";
                    }
                    float attack = Rms(shot, 0, rate * 12 / 100);
                    float gain = Mathf.Pow(10f, recipe.AttackDb / 20f) / Mathf.Max(1e-6f, attack);
                    float peak = shot.Max(v => Mathf.Abs(v)) * gain;
                    if (peak > 0.89f) gain *= 0.89f / peak; // keep 1 dB of headroom; such a take ends up quieter (noted)
                    for (int s = 0; s < shot.Length; s++) shot[s] *= gain;
                    string path = $"{folder}/{recipe.Id}_{i + 1:00}.wav";
                    AudioBankBaker.WriteWav(path, shot, rate);
                    written.Add((path, sourceAttack));
                    abNew.Add(shot);
                    report.AppendLine($"{Path.GetFileName(path)} · {from} · {start * 1000f / rate:F0} · {shot.Length / (float)rate:F2} · {Db(gain):F1} · {Db(shot.Max(v => Mathf.Abs(v))):F1} · {Db(Rms(shot, 0, rate * 12 / 100)):F1}");
                }
                AssetDatabase.Refresh();
                foreach (var (path, _) in written) Configure(path);
                var clips = written.Select(w => AssetDatabase.LoadAssetAtPath<AudioClip>(w.path)).ToArray();
                groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = recipe.Id, Takes = clips });
                if (recipe.Gaits && clips.Length >= 3)
                {
                    // Lighter takes for a crouched step, heavier for a running one (by the source's own attack level).
                    var order = Enumerable.Range(0, clips.Length).OrderBy(i => written[i].attack).ToArray();
                    int part = Mathf.Max(2, Mathf.RoundToInt(clips.Length * 0.4f));
                    groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = recipe.Id + ".crouch", Takes = order.Take(part).Select(i => clips[i]).ToArray() });
                    groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = recipe.Id + ".walk", Takes = clips });
                    groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = recipe.Id + ".run", Takes = order.Skip(clips.Length - part).Select(i => clips[i]).ToArray() });
                    report.AppendLine($"  {recipe.Id}: crouch {string.Join(",", order.Take(part).Select(i => i + 1))} · run {string.Join(",", order.Skip(clips.Length - part).Select(i => i + 1))}");
                }
                float oldDb = WriteAB(recipe, abNew, rate);
                // In the game the group plays where the sound it replaces played (same level over the first 120 ms), so the
                // mix with rain and music stays as it was and only the sound itself changes.
                float level = oldDb > -200f ? Mathf.Clamp(Mathf.Pow(10f, (oldDb - recipe.AttackDb) / 20f), 0.35f, 2.5f) : 1f;
                foreach (var group in groups.Where(g => g.Id == recipe.Id || g.Id.StartsWith(recipe.Id + "."))) group.Gain = level;
                report.AppendLine($"  {recipe.Id}: the old sound sits at {(oldDb > -200f ? oldDb.ToString("F1") + " dBFS" : "? (not readable)")} over its first 120 ms; the new takes at {recipe.AttackDb:F0}, played at ×{level:F2} ({Db(level):+0.0;-0.0} dB)");
            }
            var bank = AssetDatabase.LoadAssetAtPath<ThuyKieu.Presentation.FoleyBank>(BankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<ThuyKieu.Presentation.FoleyBank>();
                AssetDatabase.CreateAsset(bank, BankPath);
            }
            // Tú Bà's breath lives in the same bank (built by Ch4Snore from its own recordings).
            Ch4Snore.AddTo(groups, report);
            // So are a thrown pebble's contacts (built by Ch4Pebble from its own recordings).
            Ch4Pebble.AddTo(groups, report);
            bank.Groups = groups.ToArray();
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            Ch3AudioSync.Bank(); // Chapter 3 shares the step and door groups
            File.WriteAllText("Tools/LauTuBa/QA/FoleyBuild.txt", report.ToString(), new UTF8Encoding(false));
            Debug.Log(report.ToString());
        }

        /// <summary>One contact: from just before the hit to the end of its ring (within a maximum), 2 ms fade in, soft fade out.</summary>
        private static float[] OneShot(float[] x, int rate, float maxSeconds, out int start)
        {
            var env = Envelope(x, Mathf.Max(1, rate / 400));
            float peak = env.Max();
            int onset = System.Array.FindIndex(env, e => e >= peak * 0.03f);
            start = Mathf.Max(0, onset - rate / 500);
            int end = System.Array.FindLastIndex(env, e => e >= peak * 0.01f) + rate * 3 / 100;
            end = Mathf.Min(x.Length - 1, Mathf.Min(end, start + Mathf.RoundToInt(maxSeconds * rate)));
            var shot = new float[end - start + 1];
            System.Array.Copy(x, start, shot, 0, shot.Length);
            int fadeIn = rate / 500, fadeOut = Mathf.Min(shot.Length / 4, rate * 25 / 1000);
            for (int i = 0; i < fadeIn && i < shot.Length; i++) shot[i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fadeIn);
            for (int i = 0; i < fadeOut; i++) shot[shot.Length - 1 - i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fadeOut);
            return shot;
        }

        private static float[] Resample(float[] x, int from, int to)
        {
            if (from == to) return x;
            var y = new float[(int)((long)x.Length * to / from)];
            for (int i = 0; i < y.Length; i++)
            {
                float p = i * (float)from / to;
                int a = Mathf.Min(x.Length - 1, (int)p);
                int b = Mathf.Min(x.Length - 1, a + 1);
                y[i] = Mathf.Lerp(x[a], x[b], p - a);
            }
            return y;
        }

        /// <summary>
        /// A/B for listening: four plays of the old sound for this id, a second of silence, then the new takes, both at the
        /// new level. Returns the old sound's own level (first 120 ms, dBFS), or -999 when it cannot be read.
        /// </summary>
        private static float WriteAB(Recipe recipe, List<float[]> takes, int rate)
        {
            var old = ThuyKieu.Presentation.AudioBank.Get(ThuyKieu.Presentation.AudioBank.Sfx, recipe.Id);
            float oldDb = -999f;
            var mix = new List<float>();
            float gap = recipe.Id.StartsWith("step") ? 0.5f : 1.1f;
            float target = Mathf.Pow(10f, recipe.AttackDb / 20f);
            if (old != null)
            {
                // The old clip is read as it is imported (its settings are not touched).
                var y = old.loadType == AudioClipLoadType.DecompressOnLoad && old.LoadAudioData() ? Mono(old) : null;
                if (y != null)
                {
                    y = Resample(y, old.frequency, rate);
                    oldDb = Db(Rms(y, 0, rate * 12 / 100));
                    float g = target / Mathf.Max(1e-6f, Rms(y, 0, rate * 12 / 100));
                    for (int k = 0; k < 4; k++) Append(mix, y, g, gap, rate);
                }
            }
            for (int s = 0; s < rate; s++) mix.Add(0f);
            for (int k = 0; k < Mathf.Max(4, takes.Count); k++) Append(mix, takes[k % takes.Count], 1f, gap, rate);
            AudioBankBaker.WriteWav($"Tools/LauTuBa/Foley/AB_{recipe.Id}.wav", mix.ToArray(), rate);
            return oldDb;
        }

        private static void Append(List<float> mix, float[] clip, float gain, float gap, int rate)
        {
            int length = Mathf.Max(clip.Length, Mathf.RoundToInt(gap * rate));
            for (int i = 0; i < length; i++) mix.Add(i < clip.Length ? Mathf.Clamp(clip[i] * gain, -1f, 1f) : 0f);
        }

        /// <summary>Short effects: decompressed into memory, mono, Vorbis like the rest of the effects.</summary>
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

        /// <summary>Mono samples of an imported clip (channels averaged).</summary>
        public static float[] Samples(string path, out int rate, out int channels)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad || importer.loadInBackground)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
                importer.loadInBackground = false;
                importer.SaveAndReimport();
            }
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            clip.LoadAudioData();
            rate = clip.frequency;
            channels = clip.channels;
            return Mono(clip);
        }

        private static float[] Mono(AudioClip clip)
        {
            var data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);
            var mono = new float[clip.samples];
            for (int i = 0; i < clip.samples; i++)
            {
                float sum = 0f;
                for (int c = 0; c < clip.channels; c++) sum += data[i * clip.channels + c];
                mono[i] = sum / clip.channels;
            }
            return mono;
        }

        public static Measure Analyse(string path)
        {
            var x = Samples(path, out int rate, out int channels);
            var m = new Measure { Path = path, Channels = channels, Rate = rate, Length = x.Length / (float)rate };
            float peak = x.Length > 0 ? x.Max(v => Mathf.Abs(v)) : 0f;
            m.PeakDb = Db(peak);
            if (peak <= 1e-6f) return m;
            // Envelope: 5 ms moving RMS.
            int win = Mathf.Max(1, rate / 200);
            var env = Envelope(x, win);
            float envPeak = env.Max();
            int onset = System.Array.FindIndex(env, e => e >= envPeak * 0.1f);
            int end = System.Array.FindLastIndex(env, e => e >= envPeak * 0.01f);
            m.Onset = onset / (float)rate;
            m.Body = (end - onset) / (float)rate;
            m.RmsDb = Db(Rms(x, onset, end));
            m.AttackDb = Db(Rms(x, onset, Mathf.Min(x.Length - 1, onset + rate * 12 / 100)));
            // Separate hits: envelope rising through 25 % of its peak at least 80 ms after the last one.
            int hits = 0, last = -rate;
            for (int i = 1; i < env.Length; i++)
                if (env[i] >= envPeak * 0.25f && env[i - 1] < envPeak * 0.25f && i - last > rate * 8 / 100) { hits++; last = i; }
            m.Hits = hits;
            int crossings = 0;
            for (int i = onset + 1; i <= end; i++) if ((x[i] >= 0f) != (x[i - 1] >= 0f)) crossings++;
            m.Zcr = crossings / Mathf.Max(0.001f, m.Body);
            // One-pole low-pass at about 250 Hz: how much of the energy is the body of the impact.
            float a = 1f - Mathf.Exp(-2f * Mathf.PI * 250f / rate), lp = 0f, low = 0f, all = 0f;
            for (int i = onset; i <= end; i++) { lp += a * (x[i] - lp); low += lp * lp; all += x[i] * x[i]; }
            m.LowShare = all > 0f ? low / all : 0f;
            return m;
        }

        public static float[] Envelope(float[] x, int window)
        {
            var env = new float[x.Length];
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sum += x[i] * x[i];
                if (i >= window) sum -= x[i - window] * x[i - window];
                env[i] = Mathf.Sqrt((float)System.Math.Max(0, sum) / Mathf.Min(i + 1, window));
            }
            return env;
        }

        public static float Rms(float[] x, int from, int to)
        {
            double sum = 0;
            int n = 0;
            for (int i = Mathf.Max(0, from); i <= Mathf.Min(x.Length - 1, to); i++) { sum += x[i] * x[i]; n++; }
            return n > 0 ? Mathf.Sqrt((float)(sum / n)) : 0f;
        }

        public static float Db(float amplitude) => 20f * Mathf.Log10(Mathf.Max(1e-6f, amplitude));
    }
}

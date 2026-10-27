using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThuyKieu.Gameplay.Chase;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    public static partial class Ch5Foley
    {
        public const string ScorePath = MusicRoot + "/Ch5ChaseScore.asset";
        public const string MusicCredit = "'Juggernaut' by Scott Buckley - released under CC-BY 4.0. www.scottbuckley.com.au";
        private const int LoopBars = 8, BeatsPerBar = 4;
        // In-game level of each loop's RMS (dBFS at full Music volume, before the scene's music level): the old synthesised
        // score sat at −20.8; the levels sit just under, at and just over it, so the rest of the mix keeps its place.
        private static readonly float[] LevelTargetsDb = { -22.5f, -21f, -19.5f };

        /// <summary>
        /// The chase score from Scott Buckley's "Juggernaut" (CC BY 4.0, full mix only, no stems): its tempo and downbeat are
        /// measured; three 8-bar stretches are chosen by loudness (the quietest steady stretch past the intro, the loudest,
        /// one between) and baked into seamless loops (the bar after the loop crossfaded over one beat into its start, so the
        /// wrap is the music's own continuation). Each loop is written stereo, 44.1 kHz, to Audio/Ch5Music/chase_levelN.wav
        /// (in place), and the ChaseScore asset records them, their levels and the tempo.
        /// </summary>
        private static void BuildScore(StringBuilder report)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(MusicSource);
            var settings = importer.defaultSampleSettings;
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad || importer.loadInBackground || importer.forceToMono)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
                importer.loadInBackground = false;
                importer.forceToMono = false;
                importer.SaveAndReimport();
            }
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicSource);
            clip.LoadAudioData();
            int rate = clip.frequency, channels = clip.channels;
            var data = new float[clip.samples * channels];
            clip.GetData(data, 0);
            var mono = new float[clip.samples];
            for (int i = 0; i < clip.samples; i++) { float s = 0f; for (int c = 0; c < channels; c++) s += data[i * channels + c]; mono[i] = s / channels; }

            float bpm = Tempo(mono, rate, out float confidence);
            bpm = RefineTempo(mono, rate, bpm, out double firstBeat);
            double beat = 60.0 / bpm, bar = beat * BeatsPerBar;
            int bars = (int)((clip.samples / (double)rate - firstBeat) / bar);
            var barDb = new float[bars];
            for (int b = 0; b < bars; b++)
            {
                int from = (int)((firstBeat + b * bar) * rate);
                barDb[b] = Ch4Foley.Db(Ch4Foley.Rms(mono, from, from + (int)(bar * rate) - 1));
            }
            report.AppendLine($"score: {MusicCredit}");
            report.AppendLine($"  {clip.samples / (float)rate:F1} s · {channels} ch · {rate} Hz · tempo {bpm:F2} BPM (autocorrelation {confidence:F2}) · first downbeat {firstBeat:F3} s · {bars} bars of {bar:F2} s");
            report.AppendLine("  bar loudness dBFS: " + string.Join(" ", barDb.Select((d, i) => $"{i}:{d:F0}")));

            // Candidate 8-bar stretches past the intro (2 bars) and before the ending (2 bars), with one bar to spare after
            // for the loop's crossfade: their mean loudness and how much it moves from bar to bar.
            var candidates = new List<(int start, float mean, float spread)>();
            for (int b = 2; b + LoopBars + 3 <= bars; b++)
            {
                var window = barDb.Skip(b).Take(LoopBars).ToArray();
                candidates.Add((b, window.Average(), window.Max() - window.Min()));
            }
            if (candidates.Count < 3) throw new System.InvalidOperationException("Juggernaut: not enough bars for three loops");
            var steady = candidates.Where(c => c.spread < 6f).ToList();
            if (steady.Count < 3) steady = candidates;
            var loud = steady.OrderByDescending(c => c.mean).First();
            // The quiet one: not an almost silent bar, and not overlapping the loud one.
            var apart = steady.Where(c => Mathf.Abs(c.start - loud.start) >= LoopBars).ToList();
            var quietPool = apart.Where(c => c.mean > loud.mean - 12f && c.mean > -32f).ToList();
            var quiet = (quietPool.Count > 0 ? quietPool : apart.Count > 0 ? apart : steady).OrderBy(c => c.mean).First();
            float middle = (quiet.mean + loud.mean) * 0.5f;
            var mid = steady.Where(c => Mathf.Abs(c.start - quiet.start) >= LoopBars && Mathf.Abs(c.start - loud.start) >= LoopBars)
                .OrderBy(c => Mathf.Abs(c.mean - middle)).DefaultIfEmpty(steady.OrderBy(c => Mathf.Abs(c.mean - middle)).First()).First();
            var chosen = new[] { quiet, mid, loud };

            Directory.CreateDirectory(MusicRoot);
            var loops = new AudioClip[chosen.Length];
            var gains = new float[chosen.Length];
            for (int level = 0; level < chosen.Length; level++)
            {
                int start = (int)((firstBeat + chosen[level].start * bar) * rate);
                int length = (int)(LoopBars * bar * rate), fade = (int)(beat * rate);
                var loop = new float[length * channels];
                for (int i = 0; i < length; i++)
                {
                    float into = i < fade ? Mathf.Sin(0.5f * Mathf.PI * i / fade) : 1f, outOf = i < fade ? Mathf.Cos(0.5f * Mathf.PI * i / fade) : 0f;
                    for (int c = 0; c < channels; c++)
                    {
                        float now = data[(start + i) * channels + c];
                        float after = i < fade ? data[(start + length + i) * channels + c] : 0f;
                        loop[i * channels + c] = now * into + after * outOf;
                    }
                }
                float peak = loop.Max(v => Mathf.Abs(v));
                if (peak > 0.89f) for (int i = 0; i < loop.Length; i++) loop[i] *= 0.89f / peak;
                string path = $"{MusicRoot}/chase_level{level}.wav";
                AudioBankBaker.WriteWav(path, loop, rate, channels);
                float rms = Ch4Foley.Rms(loop.Where((v, i) => i % channels == 0).ToArray(), 0, length - 1);
                gains[level] = Mathf.Pow(10f, (LevelTargetsDb[level] - Ch4Foley.Db(rms)) / 20f);
                report.AppendLine($"  level {level}: bars {chosen[level].start}–{chosen[level].start + LoopBars - 1} ({start / (float)rate:F2}–{(start + length) / (float)rate:F2} s) · mean {chosen[level].mean:F1} dB · spread {chosen[level].spread:F1} dB · loop RMS {Ch4Foley.Db(rms):F1} dBFS → in-game ×{gains[level]:F2} ({LevelTargetsDb[level]:F1} dBFS)");
            }
            AssetDatabase.Refresh();
            for (int level = 0; level < chosen.Length; level++)
            {
                string path = $"{MusicRoot}/chase_level{level}.wav";
                var loopImporter = (AudioImporter)AssetImporter.GetAtPath(path);
                var loopSettings = loopImporter.defaultSampleSettings;
                loopSettings.loadType = AudioClipLoadType.CompressedInMemory;
                loopSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                loopSettings.quality = 0.7f;
                loopSettings.preloadAudioData = true;
                loopImporter.defaultSampleSettings = loopSettings;
                loopImporter.loadInBackground = false;
                loopImporter.SaveAndReimport();
                loops[level] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            // The source itself back to streaming (it is only read here).
            settings.loadType = AudioClipLoadType.Streaming;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();

            var score = AssetDatabase.LoadAssetAtPath<ChaseScore>(ScorePath);
            if (score == null) { score = ScriptableObject.CreateInstance<ChaseScore>(); AssetDatabase.CreateAsset(score, ScorePath); }
            score.Levels = loops;
            score.Gains = gains;
            score.Bpm = bpm;
            score.BeatsPerBar = BeatsPerBar;
            score.Credit = MusicCredit + "\nCut into three 8-bar loops (bars " + string.Join(", ", chosen.Select(c => c.start + "–" + (c.start + LoopBars - 1))) + "), each crossfaded over one beat at its loop point.";
            EditorUtility.SetDirty(score);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The tempo near the autocorrelation estimate (±4 %, 0.05 BPM steps) and the first downbeat: the grid that lines the
        /// most onset energy up on its beats, the downbeat the beat of the strongest bar position.
        /// </summary>
        private static float RefineTempo(float[] x, int rate, float estimate, out double firstBeat)
        {
            const int hop = 256;
            int frames = x.Length / hop;
            var energy = new float[frames];
            for (int f = 0; f < frames; f++) energy[f] = Ch4Foley.Rms(x, f * hop, f * hop + hop - 1);
            var onset = new float[frames];
            for (int f = 1; f < frames; f++) onset[f] = Mathf.Max(0f, energy[f] - energy[f - 1]);
            float frameRate = rate / (float)hop;
            float bestBpm = estimate, bestScore = -1f;
            double bestPhase = 0;
            for (float bpm = estimate * 0.96f; bpm <= estimate * 1.04f; bpm += 0.05f)
            {
                double period = 60.0 / bpm * frameRate;
                for (int p = 0; p < 24; p++)
                {
                    double phase = period * p / 24.0;
                    float score = 0f;
                    for (double t = phase; t < frames - 1; t += period) score += onset[(int)t];
                    if (score > bestScore) { bestScore = score; bestBpm = bpm; bestPhase = phase; }
                }
            }
            // Of the four beats of a bar, the one carrying the most onset energy starts the bars.
            double beatFrames = 60.0 / bestBpm * frameRate;
            var weight = new float[BeatsPerBar];
            int k = 0;
            for (double t = bestPhase; t < frames - 1; t += beatFrames, k++) weight[k % BeatsPerBar] += onset[(int)t];
            int down = System.Array.IndexOf(weight, weight.Max());
            firstBeat = (bestPhase + down * beatFrames) / frameRate;
            return bestBpm;
        }
    }
}

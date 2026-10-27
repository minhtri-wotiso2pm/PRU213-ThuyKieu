using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    public static partial class Ch5Foley
    {
        /// <summary>
        /// Measures the Chapter 5 sources so the edits can be chosen from numbers (nobody here can listen): every source's
        /// length, level and onsets; for the tree recording a 50 ms envelope with its hits (to find the chops, the creak, the
        /// fall and the impact); for the music the loudness every two seconds and the tempo from the onset envelope.
        ///   Batch: -executeMethod ThuyKieu.Core.Editor.Ch5Foley.AnalyseBatch   (report: Tools/Ch5/QA/Ch5Sources.txt)
        /// </summary>
        public static void AnalyseBatch()
        {
            try
            {
                var report = new StringBuilder();
                report.AppendLine("Chapter 5 sources (measured, not listened to)");
                report.AppendLine("columns: file · channels · rate · length s · peak dBFS · RMS dBFS · first 120 ms dBFS · onset ms · body s · hits · zero crossings/s · share under 250 Hz");
                foreach (string path in Directory.GetFiles(SourceRoot, "*.ogg", SearchOption.AllDirectories).Concat(Directory.GetFiles(Ch4Foley.SourceRoot + "/kenney_rpg", "footstep*.ogg"))
                             .Concat(Directory.GetFiles(Ch4Foley.SourceRoot + "/kenney_impact", "impact*.ogg")).Select(p => p.Replace('\\', '/')).OrderBy(p => p))
                {
                    var m = Ch4Foley.Analyse(path);
                    report.AppendLine($"{path.Substring(path.IndexOf("/Source/") + 8)} · {m.Channels} · {m.Rate} · {m.Length:F2} · {m.PeakDb:F1} · {m.RmsDb:F1} · {m.AttackDb:F1} · {m.Onset * 1000f:F0} · {m.Body:F2} · {m.Hits} · {m.Zcr:F0} · {m.LowShare:F2}");
                }

                report.AppendLine();
                report.AppendLine("== tree fall: 50 ms envelope (dBFS) and hits");
                var tree = Ch4Foley.Samples(TreeSource, out int treeRate, out _);
                Envelope(report, tree, treeRate, 0.05f);

                report.AppendLine();
                report.AppendLine("== music: loudness every 2 s (dBFS RMS, mono sum) and tempo");
                var music = Ch4Foley.Samples(MusicSource, out int musicRate, out _);
                int block = musicRate * 2;
                var line = new StringBuilder();
                for (int i = 0; i + block <= music.Length; i += block)
                    line.Append($"{i / musicRate,3}s {Ch4Foley.Db(Ch4Foley.Rms(music, i, i + block - 1)),6:F1} | ");
                report.AppendLine(line.ToString());
                float bpm = Tempo(music, musicRate, out float confidence);
                report.AppendLine($"length {music.Length / (float)musicRate:F1} s · tempo {bpm:F1} BPM (autocorrelation peak {confidence:F2})");

                Directory.CreateDirectory("Tools/Ch5/QA");
                File.WriteAllText("Tools/Ch5/QA/Ch5Sources.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static void Envelope(StringBuilder report, float[] x, int rate, float step)
        {
            int window = Mathf.RoundToInt(rate * step);
            var line = new StringBuilder();
            for (int i = 0; i + window <= x.Length; i += window)
            {
                line.Append($"{i / (float)rate:F2}:{Ch4Foley.Db(Ch4Foley.Rms(x, i, i + window - 1)):F0} ");
                if (line.Length > 180) { report.AppendLine(line.ToString()); line.Clear(); }
            }
            report.AppendLine(line.ToString());
            var env = Ch4Foley.Envelope(x, Mathf.Max(1, rate / 200));
            float peak = env.Max();
            int last = -rate;
            for (int i = 1; i < env.Length; i++)
                if (env[i] >= peak * 0.2f && env[i - 1] < peak * 0.2f && i - last > rate / 10)
                {
                    last = i;
                    report.AppendLine($"  hit at {i / (float)rate:F3} s · {Ch4Foley.Db(env.Skip(i).Take(rate / 20).Max()):F1} dBFS");
                }
        }

        /// <summary>Beats per minute from the autocorrelation of the onset envelope (80–160 BPM), and how clear the peak is.</summary>
        private static float Tempo(float[] x, int rate, out float confidence)
        {
            const int hop = 512;
            int frames = x.Length / hop;
            var energy = new float[frames];
            for (int f = 0; f < frames; f++) energy[f] = Ch4Foley.Rms(x, f * hop, f * hop + hop - 1);
            var onset = new float[frames];
            for (int f = 1; f < frames; f++) onset[f] = Mathf.Max(0f, energy[f] - energy[f - 1]);
            float frameRate = rate / (float)hop;
            // 80–160 BPM: below it the autocorrelation finds every other beat (half tempo) and the bar lines would drift onto beat two.
            int minLag = Mathf.RoundToInt(frameRate * 60f / 160f), maxLag = Mathf.RoundToInt(frameRate * 60f / 80f);
            float best = 0f, zero = 0f;
            int bestLag = minLag;
            for (int f = 0; f < frames; f++) zero += onset[f] * onset[f];
            for (int lag = minLag; lag <= maxLag; lag++)
            {
                float sum = 0f;
                for (int f = lag; f < frames; f++) sum += onset[f] * onset[f - lag];
                if (sum > best) { best = sum; bestLag = lag; }
            }
            confidence = zero > 0f ? best / zero : 0f;
            return 60f * frameRate / bestLag;
        }
    }
}

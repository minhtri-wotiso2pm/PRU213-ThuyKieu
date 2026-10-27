using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThuyKieu.Presentation;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    public static partial class Ch5Foley
    {
        private const int Rate = 44100;
        private const string Kenney = "Assets/_Game/Audio/Ch4Foley/Source/kenney_impact/";
        private const string KenneyRpg = "Assets/_Game/Audio/Ch4Foley/Source/kenney_rpg/";
        private const string Gravel = SourceRoot + "/ali6868_gravel/";
        public const string BankPath = "Assets/_Game/Audio/Ch4Foley/Ch5FoleyBank.asset";

        /// <summary>
        /// Renders Chapter 5's one-shots (measured, not listened to) under Audio/Ch5Foley/&lt;id&gt;/ and then the bank and the
        /// score: the forest earth with a little grit in it (the gravel takes the player approved, a step each, never the
        /// preview loop), landings and push-offs by surface, trips, culm knocks, the guard's heavier tread, the falling
        /// bamboo in three beats (no axe), the bridge's stake, rope and deck. Report: Tools/Ch5/QA/Ch5FoleyBuild.txt.
        ///   Batch: -executeMethod ThuyKieu.Core.Editor.Ch5Foley.BuildBatch
        /// </summary>
        public static void BuildBatch()
        {
            try { Build(); EditorApplication.Exit(0); }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public static void Build()
        {
            var report = new StringBuilder();
            report.AppendLine("Chapter 5 Foley: edited one-shots (measured, not listened to)");
            report.AppendLine("columns: output · made from · length s · peak dBFS · first 120 ms RMS dBFS");
            var earth = Enumerable.Range(0, 10).Select(i => Shot(KenneyRpg + $"footstep{i:00}.ogg", 0.3f)).ToList();
            var grit = Enumerable.Range(0, 10).Select(i => Shot(Gravel + $"{i}.ogg", 0.3f)).ToList();
            var wood = Enumerable.Range(0, 5).Select(i => Shot(Kenney + $"footstep_wood_{i:000}.ogg", 0.3f)).ToList();
            var stone = Enumerable.Range(0, 5).Select(i => Shot(Kenney + $"footstep_concrete_{i:000}.ogg", 0.2f)).ToList();
            float[] Impact(string name, int i, float seconds) => Shot(Kenney + $"{name}_{i:000}.ogg", seconds);
            // The earth takes ordered light to heavy by their own attack: crouched steps from the light end, the guard's from the heavy end.
            var byWeight = Enumerable.Range(0, earth.Count).OrderBy(i => Ch4Foley.Rms(earth[i], 0, Rate * 12 / 100)).ToArray();

            // Her running steps: the earth contact, and a little grit under it (−9 dB), each take with a different grit take.
            Render(report, "step_earth", -21f, Enumerable.Range(0, 10).Select(i => (Layer(earth[i], grit[(i * 3 + 1) % 10], 0.006f, -9f), $"footstep{i:00} + gravel {(i * 3 + 1) % 10} (−9 dB)")));
            Render(report, "step_earth.crouch", -25f, byWeight.Take(4).Select(i => (Layer(earth[i], grit[(i * 7 + 2) % 10], 0.006f, -13f), $"footstep{i:00} (light) + gravel {(i * 7 + 2) % 10} (−13 dB)")));
            // The guard: the heavy half of the takes with more grit, louder (pitched down in the game).
            Render(report, "guard_step", -19f, byWeight.Skip(4).Select(i => (Layer(earth[i], grit[(i * 3 + 5) % 10], 0.004f, -6f), $"footstep{i:00} (heavy) + gravel {(i * 3 + 5) % 10} (−6 dB)")));
            // Push-off: a short scuff of the same ground, quieter than a step.
            Render(report, "jump_earth", -24f, Enumerable.Range(0, 5).Select(i => (Cut(Layer(earth[byWeight[9 - i]], grit[(i * 3) % 10], 0.003f, -6f), 0.16f), $"footstep{byWeight[9 - i]:00} first 0.16 s + gravel {(i * 3) % 10} (−6 dB)")));
            Render(report, "jump_wood", -24f, Enumerable.Range(0, 5).Select(i => (Cut(wood[i], 0.14f), $"footstep_wood_{i:000} first 0.14 s")));
            Render(report, "jump_stone", -24f, Enumerable.Range(0, 5).Select(i => (Cut(stone[i], 0.12f), $"footstep_concrete_{i:000} first 0.12 s")));
            // Landing: both feet, the second 45 ms after the first and a little lighter, the grit settling; louder than a step.
            Render(report, "land_earth", -17f, Enumerable.Range(0, 6).Select(i => (Layer(Layer(earth[byWeight[9 - i]], earth[byWeight[i]], 0.045f, -3f), grit[(i * 3 + 2) % 10], 0.01f, -6f),
                $"footstep{byWeight[9 - i]:00} + footstep{byWeight[i]:00} at 45 ms (−3 dB) + gravel {(i * 3 + 2) % 10} (−6 dB)")));
            Render(report, "land_wood", -17f, Enumerable.Range(0, 5).Select(i => (Layer(Layer(wood[i], wood[(i + 2) % 5], 0.04f, -3f), Impact("impactPlank_medium", i, 0.35f), 0.0f, -8f),
                $"footstep_wood_{i:000} + footstep_wood_{(i + 2) % 5:000} at 40 ms (−3 dB) + impactPlank_medium_{i:000} (−8 dB)")));
            Render(report, "land_stone", -17f, Enumerable.Range(0, 5).Select(i => (Layer(stone[i], stone[(i + 2) % 5], 0.04f, -3f), $"footstep_concrete_{i:000} + footstep_concrete_{(i + 2) % 5:000} at 40 ms (−3 dB)")));
            // A trip: the foot catching and the body checked (a soft heavy impact) with the ground under it.
            Render(report, "trip_earth", -18f, Enumerable.Range(0, 5).Select(i => (Layer(Layer(Impact("impactSoft_heavy", i, 0.35f), earth[byWeight[9 - i]], 0.02f, -2f), grit[(i * 3 + 4) % 10], 0.03f, -4f),
                $"impactSoft_heavy_{i:000} + footstep{byWeight[9 - i]:00} at 20 ms (−2 dB) + gravel {(i * 3 + 4) % 10} at 30 ms (−4 dB)")));
            Render(report, "trip_wood", -18f, Enumerable.Range(0, 5).Select(i => (Layer(Impact("impactWood_light", i, 0.3f), wood[i], 0.02f, -2f), $"impactWood_light_{i:000} + footstep_wood_{i:000} at 20 ms (−2 dB)")));
            Render(report, "trip_stone", -18f, Enumerable.Range(0, 5).Select(i => (Layer(Impact("impactSoft_medium", i, 0.3f), stone[i], 0.02f, -2f), $"impactSoft_medium_{i:000} + footstep_concrete_{i:000} at 20 ms (−2 dB)")));
            // A culm run into: the hollow knock of wood, raised a little for a hollow stem (+15 %).
            Render(report, "culm_knock", -18f, Enumerable.Range(0, 5).Select(i => (Pitch(Impact("impactWood_medium", i, 0.4f), 1.15f), $"impactWood_medium_{i:000} pitched ×1.15")));
            // The guard thrown against the near bank as the deck goes.
            Render(report, "guard_land", -15f, Enumerable.Range(0, 3).Select(i => (Layer(Impact("impactSoft_heavy", i + 1, 0.4f), Pitch(earth[byWeight[9 - i]], 0.9f), 0.015f, -2f),
                $"impactSoft_heavy_{i + 1:000} + footstep{byWeight[9 - i]:00} ×0.9 at 15 ms (−2 dB)")));

            Tree(report);

            // The bridge: her heel on the rotten stake; the deck swinging down on its ropes (creaks pitched down); the deck
            // meeting the bank; the rope rails taking her weight on the planks.
            Render(report, "stake_kick", -15f, Enumerable.Range(0, 3).Select(i => (Layer(Impact("impactWood_heavy", i, 0.45f), Impact("impactSoft_medium", i + 1, 0.3f), 0.01f, -4f),
                $"impactWood_heavy_{i:000} + impactSoft_medium_{i + 1:000} at 10 ms (−4 dB)")));
            Render(report, "deck_swing", -20f, new[] { 1, 2 }.Select(i => (Pitch(Shot(KenneyRpg + $"creak{i}.ogg", 1.2f), 0.78f), $"creak{i} pitched ×0.78")), sustained: true);
            Render(report, "deck_hit", -13f, Enumerable.Range(0, 3).Select(i => (Layer(Impact("impactPlank_medium", i, 0.45f), Impact("impactWood_heavy", i + 2, 0.45f), 0.03f, -2f),
                $"impactPlank_medium_{i:000} + impactWood_heavy_{i + 2:000} at 30 ms (−2 dB)")));
            Render(report, "rope_strain", -27f, new[] { 1, 2, 3 }.Select(i => (Pitch(Shot(KenneyRpg + $"creak{i}.ogg", 0.9f), 0.9f), $"creak{i} pitched ×0.9")), sustained: true);

            Directory.CreateDirectory("Tools/Ch5/QA");
            AssetDatabase.Refresh();
            foreach (string folder in Directory.GetDirectories(Root).Where(d => !d.EndsWith("Source")))
                foreach (string path in Directory.GetFiles(folder, "*.wav")) Ch4Foley.Configure(path.Replace('\\', '/'));
            Compose(report);
            BuildScore(report);
            File.WriteAllText("Tools/Ch5/QA/Ch5FoleyBuild.txt", report.ToString(), new UTF8Encoding(false));
            Debug.Log(report.ToString());
        }

        // ------------------------------------------------------------------ the bank

        /// <summary>
        /// The Chapter 5 bank (same asset and GUID as before): the planks and stone of the Chapter 4 steps (wood also for
        /// the guard on the bridge), and every group rendered above, by folder. Run again, it gives the same bank: it is made
        /// from what is on disk, never added to, so Apply never doubles or drops a group.
        /// </summary>
        public static FoleyBank Compose(StringBuilder report = null)
        {
            var source = AssetDatabase.LoadAssetAtPath<FoleyBank>(Ch4Foley.BankPath);
            var groups = new List<FoleyBank.Group>();
            if (source != null)
                foreach (var g in source.Groups.Where(g => g.Id.StartsWith("step_wood") || g.Id.StartsWith("step_stone")))
                    groups.Add(new FoleyBank.Group { Id = g.Id, Takes = g.Takes.ToArray(), Gain = g.Gain });
            var woodSteps = groups.FirstOrDefault(g => g.Id == "step_wood");
            if (woodSteps != null) groups.Add(new FoleyBank.Group { Id = "guard_step_wood", Takes = woodSteps.Takes.ToArray(), Gain = woodSteps.Gain * 1.2f });
            if (Directory.Exists(Root))
                foreach (string folder in Directory.GetDirectories(Root).Select(d => d.Replace('\\', '/')).Where(d => !d.EndsWith("/Source")).OrderBy(d => d))
                {
                    var clips = Directory.GetFiles(folder, "*.wav").OrderBy(p => p).Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\', '/'))).Where(c => c != null).ToArray();
                    if (clips.Length == 0) continue;
                    string id = Path.GetFileName(folder);
                    groups.Add(new FoleyBank.Group { Id = id, Takes = clips, Gain = 1f });
                    // Running is the only gait on the run, walking for the odd slow moment: both are the full set.
                    if (id == "step_earth")
                    {
                        groups.Add(new FoleyBank.Group { Id = "step_earth.run", Takes = clips, Gain = 1f });
                        groups.Add(new FoleyBank.Group { Id = "step_earth.walk", Takes = clips, Gain = 1f });
                    }
                }
            var bank = AssetDatabase.LoadAssetAtPath<FoleyBank>(BankPath);
            if (bank == null) { bank = ScriptableObject.CreateInstance<FoleyBank>(); AssetDatabase.CreateAsset(bank, BankPath); }
            bank.Groups = groups.OrderBy(g => g.Id).ToArray();
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            report?.AppendLine($"bank {BankPath}: {string.Join(", ", bank.Groups.Select(g => g.Id + " ×" + g.Takes.Length))}");
            return bank;
        }

        // ------------------------------------------------------------------ editing

        /// <summary>One contact from a source file: mono at 44.1 kHz, from just before the hit to its ring (within a maximum).</summary>
        private static float[] Shot(string path, float maxSeconds)
        {
            var x = Ch4Foley.Samples(path, out int rate, out _);
            x = Ch4Foley.Resample(x, rate, Rate);
            return Ch4Foley.OneShot(x, Rate, maxSeconds, out _);
        }

        /// <summary><paramref name="b"/> laid under <paramref name="a"/>, <paramref name="delay"/> s later, at <paramref name="db"/> relative to a's attack.</summary>
        private static float[] Layer(float[] a, float[] b, float delay, float db)
        {
            int offset = Mathf.RoundToInt(delay * Rate);
            float scale = Ch4Foley.Rms(a, 0, Rate * 12 / 100) / Mathf.Max(1e-6f, Ch4Foley.Rms(b, 0, Rate * 12 / 100)) * Mathf.Pow(10f, db / 20f);
            var y = new float[Mathf.Max(a.Length, offset + b.Length)];
            for (int i = 0; i < a.Length; i++) y[i] = a[i];
            for (int i = 0; i < b.Length; i++) y[offset + i] += b[i] * scale;
            return y;
        }

        /// <summary>The first <paramref name="seconds"/> of a shot, faded out over its last 20 ms.</summary>
        private static float[] Cut(float[] x, float seconds) => Fade(x.Take(Mathf.Min(x.Length, Mathf.RoundToInt(seconds * Rate))).ToArray(), 0.002f, 0.02f);

        /// <summary>Speed change (pitch and length together, as a slower or faster recording).</summary>
        private static float[] Pitch(float[] x, float ratio) => Ch4Foley.Resample(x, Mathf.RoundToInt(Rate * ratio), Rate);

        private static float[] Fade(float[] x, float inSeconds, float outSeconds)
        {
            int fadeIn = Mathf.Min(x.Length / 2, Mathf.RoundToInt(inSeconds * Rate)), fadeOut = Mathf.Min(x.Length / 2, Mathf.RoundToInt(outSeconds * Rate));
            for (int i = 0; i < fadeIn; i++) x[i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fadeIn);
            for (int i = 0; i < fadeOut; i++) x[x.Length - 1 - i] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * i / fadeOut);
            return x;
        }

        /// <summary>
        /// Writes each take of a group at <paramref name="attackDb"/> over its first 120 ms, or over all of it when
        /// <paramref name="sustained"/> (1 dB of peak headroom kept; no
        /// peak normalising), replacing the group's folder content in place (same file names, so the same GUIDs).
        /// </summary>
        private static void Render(StringBuilder report, string id, float attackDb, IEnumerable<(float[] shot, string from)> takes, bool sustained = false)
        {
            string folder = Root + "/" + id;
            Directory.CreateDirectory(folder);
            var list = takes.ToList();
            foreach (string stale in Directory.GetFiles(folder, "*.wav").Where(p => int.TryParse(Path.GetFileNameWithoutExtension(p).Split('_').Last(), out int n) && n > list.Count))
                AssetDatabase.DeleteAsset(stale.Replace('\\', '/'));
            for (int i = 0; i < list.Count; i++)
            {
                var shot = list[i].shot;
                // A contact by its first 120 ms; a creak, a rush or a rustle (sustained) by its whole length.
                float attack = Ch4Foley.Rms(shot, 0, sustained ? shot.Length - 1 : Rate * 12 / 100);
                float gain = Mathf.Pow(10f, attackDb / 20f) / Mathf.Max(1e-6f, attack);
                float peak = shot.Max(v => Mathf.Abs(v)) * gain;
                if (peak > 0.89f) gain *= 0.89f / peak;
                var y = shot.Select(v => v * gain).ToArray();
                string path = $"{folder}/{id}_{i + 1:00}.wav";
                AudioBankBaker.WriteWav(path, y, Rate);
                report.AppendLine($"{Path.GetFileName(path)} · {list[i].from} · {y.Length / (float)Rate:F2} · {Ch4Foley.Db(y.Max(v => Mathf.Abs(v))):F1} · {Ch4Foley.Db(Ch4Foley.Rms(y, 0, Rate * 12 / 100)):F1}");
            }
        }
    }
}

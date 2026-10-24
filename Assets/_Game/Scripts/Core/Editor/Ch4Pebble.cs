using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// A thrown pebble's contacts for Chapter 4 (pebble_hard, pebble_wood, pebble_soft), cut from recordings of real stones
    /// (sources and licences in Assets/_Game/Audio/Ch4Foley/CREDITS.md). Each take is ONE contact: the game plays the
    /// first landing and each bounce as its own take, quieter as the pebble slows, so no take carries a ready-made run of
    /// bounces.
    ///   Measure the candidates: -executeMethod ThuyKieu.Core.Editor.Ch4Pebble.AnalyseBatch (Tools/LauTuBa/QA/PebbleSources.txt)
    ///   Build the groups into the Chapter 4 bank: -executeMethod ThuyKieu.Core.Editor.Ch4Pebble.BuildBatch
    /// Chosen by measurement (single sharp contacts, brightness, ring), not by listening.
    /// </summary>
    public static class Ch4Pebble
    {
        public const string Folder = Ch4Foley.SourceRoot + "/pebble";

        public struct Hit
        {
            public float Time, PeakDb, Ring, Zcr;
        }

        public static void AnalyseBatch()
        {
            try
            {
                var report = new StringBuilder();
                report.AppendLine("Pebble candidates: measured, not listened to.");
                report.AppendLine("file · ch · Hz · length s · peak dBFS · hits; then each hit: time s · peak dBFS · ring ms (to -30 dB or the next hit) · zero crossings/s over its first 40 ms");
                var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { Folder }).Select(AssetDatabase.GUIDToAssetPath).ToList();
                paths.AddRange(AssetDatabase.FindAssets("t:AudioClip", new[] { Ch4Foley.SourceRoot + "/kenney_impact" }).Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.Contains("impactSoft") || p.Contains("footstep_grass")));
                foreach (string path in paths.OrderBy(p => p))
                {
                    var x = Ch4Foley.Samples(path, out int rate, out int channels);
                    var hits = Hits(x, rate);
                    float peak = x.Length > 0 ? x.Max(v => Mathf.Abs(v)) : 0f;
                    report.AppendLine($"{path.Substring(Ch4Foley.SourceRoot.Length + 1)} · {channels} · {rate} · {x.Length / (float)rate:F2} · {Ch4Foley.Db(peak):F1} · {hits.Count}");
                    foreach (var h in hits.Take(14)) report.AppendLine($"    {h.Time:F3} · {h.PeakDb:F1} · {h.Ring * 1000f:F0} · {h.Zcr:F0}");
                }
                Directory.CreateDirectory("Tools/LauTuBa/QA");
                File.WriteAllText("Tools/LauTuBa/QA/PebbleSources.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        // ------------------------------------------------------------------ the recipe

        /// <summary>One contact cut from a recording: the file and roughly when the contact starts (the exact onset is found).</summary>
        private struct Cut { public string File; public float At; public Cut(string file, float at) { File = file; At = at; } }

        private sealed class Recipe
        {
            public string Id;
            public Cut[] Cuts;
            public float MaxSeconds;
            /// <summary>Level of each take over its first 40 ms (dBFS).</summary>
            public float AttackDb;
        }

        // Chosen from PebbleSources.txt: single sharp contacts (ring under ~60 ms), bright (many zero crossings), no second
        // contact inside the cut. Not listened to.
        private static readonly Recipe[] Recipes =
        {
            // A small stone on stone or brick (the wet courtyard, the hall, the gate apron): a stone bouncing on stone
            // (Commons, CC BY-SA 4.0) and small stones dropped (rubberduck, CC0).
            new Recipe { Id = "pebble_hard", MaxSeconds = 0.14f, AttackDb = -18f, Cuts = new[]
            {
                new Cut("pebble/Stone-dropping-6843.wav", 0.051f), new Cut("pebble/Stone-dropping-6843.wav", 0.251f), new Cut("pebble/Stone-dropping-6843.wav", 0.334f),
                new Cut("pebble/sfx100v2_stones_01.ogg", 0.020f), new Cut("pebble/sfx100v2_stones_02.ogg", 0.024f), new Cut("pebble/sfx100v2_stones_03.ogg", 0.003f),
                new Cut("pebble/bfh1_rock_falling_04.ogg", 0.012f), new Cut("pebble/bfh1_rock_falling_08.ogg", 0.003f), new Cut("pebble/bfh1_rock_hit_01.ogg", 0.001f),
            } },
            // Boards, doors, pillars: no recording of a pebble on wood could be downloaded (the Freesound take needs a login);
            // the two brightest single wood hits of the CC0 packs stand in (an object on wood, not known to be a stone).
            new Recipe { Id = "pebble_wood", MaxSeconds = 0.12f, AttackDb = -20f, Cuts = new[]
            {
                new Cut("pebble/bfh1_wood_hit_03.ogg", 0.007f), new Cut("pebble/sfx100v2_wood_hit_03.ogg", 0.017f),
            } },
            // Mats, earth, mud: no recording of a pebble on soft ground; Kenney's soft single impacts (CC0), kept quiet,
            // stand in for the dull tap.
            new Recipe { Id = "pebble_soft", MaxSeconds = 0.1f, AttackDb = -30f, Cuts = new[]
            {
                new Cut("kenney_impact/impactSoft_medium_000.ogg", 0.001f), new Cut("kenney_impact/impactSoft_medium_002.ogg", 0.001f), new Cut("kenney_impact/impactSoft_medium_003.ogg", 0.001f),
            } },
        };

        public static void BuildBatch()
        {
            try
            {
                var bank = AssetDatabase.LoadAssetAtPath<ThuyKieu.Presentation.FoleyBank>(Ch4Foley.BankPath);
                if (bank == null) throw new System.Exception(Ch4Foley.BankPath + " missing: run Ch4Foley.BuildBatch first");
                var report = new StringBuilder();
                var groups = bank.Groups.Where(g => !g.Id.StartsWith("pebble_")).ToList();
                AddTo(groups, report);
                bank.Groups = groups.ToArray();
                EditorUtility.SetDirty(bank);
                AssetDatabase.SaveAssets();
                File.WriteAllText("Tools/LauTuBa/QA/PebbleBuild.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>Renders the pebble one-shots under Assets/_Game/Audio/Ch4Foley/&lt;id&gt;/ and adds their groups (also called from <see cref="Ch4Foley.Build"/>).</summary>
        public static void AddTo(List<ThuyKieu.Presentation.FoleyBank.Group> groups, StringBuilder report)
        {
            report.AppendLine("Pebble contacts (measured, not listened to): output · from · onset s · length s · gain dB · peak dBFS · first 40 ms RMS dBFS");
            foreach (var recipe in Recipes)
            {
                string folder = Ch4Foley.Root + "/" + recipe.Id;
                Directory.CreateDirectory(folder);
                var paths = new List<string>();
                for (int i = 0; i < recipe.Cuts.Length; i++)
                {
                    var cut = recipe.Cuts[i];
                    var x = Ch4Foley.Samples(Ch4Foley.SourceRoot + "/" + cut.File, out int rate, out _);
                    var hits = Hits(x, rate);
                    // The contact nearest the noted time, and where the next one starts (the cut ends before it).
                    int nearest = 0;
                    for (int h = 1; h < hits.Count; h++) if (Mathf.Abs(hits[h].Time - cut.At) < Mathf.Abs(hits[nearest].Time - cut.At)) nearest = h;
                    if (hits.Count == 0 || Mathf.Abs(hits[nearest].Time - cut.At) > 0.02f) throw new System.Exception($"{cut.File}: no contact near {cut.At:F3} s");
                    float onset = hits[nearest].Time;
                    float until = nearest + 1 < hits.Count ? hits[nearest + 1].Time - 0.005f : x.Length / (float)rate;
                    int start = Mathf.Max(0, Mathf.RoundToInt((onset - 0.003f) * rate));
                    int end = Mathf.Min(x.Length, Mathf.RoundToInt(Mathf.Min(until, onset + recipe.MaxSeconds) * rate));
                    var shot = new float[Mathf.Max(1, end - start)];
                    System.Array.Copy(x, start, shot, 0, shot.Length);
                    int fadeIn = rate / 500, fadeOut = Mathf.Min(shot.Length / 3, rate * 25 / 1000);
                    for (int k = 0; k < fadeIn && k < shot.Length; k++) shot[k] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * k / fadeIn);
                    for (int k = 0; k < fadeOut; k++) shot[shot.Length - 1 - k] *= 0.5f - 0.5f * Mathf.Cos(Mathf.PI * k / fadeOut);
                    float attack = Ch4Foley.Rms(shot, 0, rate * 4 / 100);
                    float gain = Mathf.Pow(10f, recipe.AttackDb / 20f) / Mathf.Max(1e-6f, attack);
                    float peak = shot.Max(v => Mathf.Abs(v)) * gain;
                    if (peak > 0.89f) gain *= 0.89f / peak; // 1 dB of headroom
                    for (int k = 0; k < shot.Length; k++) shot[k] *= gain;
                    string path = $"{folder}/{recipe.Id}_{i + 1:00}.wav";
                    AudioBankBaker.WriteWav(path, shot, rate);
                    paths.Add(path);
                    report.AppendLine($"{Path.GetFileName(path)} · {cut.File} · {onset:F3} · {shot.Length / (float)rate:F3} · {Ch4Foley.Db(gain):F1} · {Ch4Foley.Db(shot.Max(v => Mathf.Abs(v))):F1} · {Ch4Foley.Db(Ch4Foley.Rms(shot, 0, rate * 4 / 100)):F1}");
                }
                AssetDatabase.Refresh();
                foreach (string path in paths) Configure(path);
                groups.Add(new ThuyKieu.Presentation.FoleyBank.Group { Id = recipe.Id, Takes = paths.Select(AssetDatabase.LoadAssetAtPath<AudioClip>).ToArray(), Gain = 1f });
            }
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
        /// Separate contacts: the 2 ms envelope rising through a third of the loudest part near it (within 25 dB of the file's
        /// peak), at least 40 ms after the last one.
        /// </summary>
        public static List<Hit> Hits(float[] x, int rate)
        {
            var hits = new List<Hit>();
            if (x.Length == 0) return hits;
            var env = Ch4Foley.Envelope(x, Mathf.Max(1, rate / 500));
            float filePeak = env.Max();
            float floor = filePeak * 0.056f; // -25 dB
            int gap = rate * 4 / 100, last = -gap;
            for (int i = 1; i < env.Length; i++)
            {
                if (env[i] < floor || env[i - 1] >= floor && i - last < gap) continue;
                if (i - last < gap) continue;
                // A rise: the local peak over the next 15 ms.
                int end = Mathf.Min(env.Length - 1, i + rate * 15 / 1000);
                float local = 0f;
                int at = i;
                for (int k = i; k <= end; k++) if (env[k] > local) { local = env[k]; at = k; }
                if (env[i - 1] > local * 0.5f) continue; // not a fresh rise
                last = i;
                int ring = at;
                while (ring < env.Length - 1 && env[ring] > local * 0.0316f && (ring - at) < rate) ring++;
                int crossings = 0, n = Mathf.Min(x.Length - 1, i + rate * 4 / 100);
                for (int k = i + 1; k <= n; k++) if ((x[k] >= 0f) != (x[k - 1] >= 0f)) crossings++;
                hits.Add(new Hit { Time = i / (float)rate, PeakDb = Ch4Foley.Db(local), Ring = (ring - at) / (float)rate, Zcr = crossings / 0.04f });
            }
            return hits;
        }
    }
}

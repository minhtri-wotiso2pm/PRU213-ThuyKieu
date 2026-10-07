using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Dialogue.Editor
{
    /// <summary>
    /// Compiles Main.ink and plays thousands of random walkthroughs from each chapter entry.
    /// Reports stuck runs, unknown tags/speakers and explore hubs mixing world and UI choices.
    /// Batch mode: -executeMethod ThuyKieu.Dialogue.Editor.InkStoryValidator.RunBatch
    /// </summary>
    public static class InkStoryValidator
    {
        public const string ReportPath = "Tools/Ink/InkValidationReport.txt";
        private static readonly HashSet<string> KnownKeys = new HashSet<string>
        { "chapter", "scene", "music", "sfx", "speaker", "emotion", "camera", "objective", "clue", "gameplay", "timer", "fx" };
        private static readonly HashSet<string> KnownSpeakers = new HashSet<string>
        { "Narrator", "Kieu", "NoiTam", "KieuThat", "TrangSach", "MeKieu", "MaGiamSinh", "TuyTung", "TuBa", "MaKieu", "CoHong", "CoLam", "NguoiCanh",
          "KimTrong", "BaHangNuoc", "ThuLai", "ChuHieuBac", "GiaNhan",
          "HocSinh", "Ngan", "CoGiao", "VuongOng", "VuongQuan", "QuanSai", "QuanHuyen", "NhaDich", "ChiLam" };

        /// <summary>Story entry knots with their chapter numbers (0 = prologue, 8 = epilogue). Chapter 1 starts at the root.</summary>
        internal static readonly (string Knot, int Chapter)[] Entries =
        {
            ("prologue", 0), ("chapter_1", 1), ("chapter_2", 2), ("chapter_3", 3), ("chapter_4", 4), ("chapter_5", 5),
            ("chapter_6", 6), ("chapter_7", 7), ("epilogue", 8)
        };

        /// <summary>A fresh story positioned at an entry, with random outcomes for the chapters before it.</summary>
        internal static Ink.Runtime.Story Start(string json, string knot, int chapter, System.Random random)
        {
            var story = new Ink.Runtime.Story(json);
            if (knot == "chapter_1") return story;
            RandomisePriorChapters(story, chapter, random);
            story.ChoosePathString(knot);
            return story;
        }

        /// <summary>The variable summarised per entry in the report.</summary>
        private static string Tracked(string entry) => entry == "chapter_3" ? "tra_loi_kieu_that" : entry == "chapter_6" ? "c6_ho_so"
            : entry == "chapter_7" ? "c7_ket" : entry == "prologue" ? "pl_che_kieu" : null;

        [MenuItem("ThuyKieu/Dialogue/Validate Ink branches")]
        public static void RunMenu()
        {
            bool ok = Validate(out string report);
            Debug.Log(report);
            EditorUtility.DisplayDialog("Ink validation", ok ? "PASS" : "FAIL — xem Console / " + ReportPath, "OK");
        }

        public static void RunBatch()
        {
            bool ok;
            string report;
            try { ok = Validate(out report); }
            catch (Exception exception) { ok = false; report = "EXCEPTION " + exception; }
            Debug.Log(report);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static bool Validate(out string report)
        {
            InkStoryCompiler.Compile();
            string json = File.ReadAllText(InkStoryCompiler.Output, Encoding.UTF8);
            var log = new StringBuilder();
            var problems = new List<string>();
            var random = new System.Random(213);
            var gameplay = new SortedSet<string>();
            var explore = new SortedSet<string>();
            var cameras = new SortedSet<string>();
            var scenes = new SortedSet<string>();
            var clues = new SortedSet<string>();
            foreach (var (entry, chapterNumber) in Entries)
            {
                int runs = 1500, maxLines = 0, minLines = int.MaxValue, maxSuspicion = 0;
                var answers = new Dictionary<int, int>();
                for (int run = 0; run < runs; run++)
                {
                    var story = Start(json, entry, chapterNumber, random);
                    int lines = 0, guard = 0;
                    while (guard++ < 4000)
                    {
                        string[] lastTags = new string[0];
                        while (story.canContinue)
                        {
                            story.Continue();
                            lines++;
                            lastTags = story.currentTags.ToArray();
                            foreach (string tag in lastTags) CheckTag(tag, problems, gameplay, cameras, scenes, clues);
                            foreach (string tag in lastTags) SimulateSequence(tag.Trim(), story, random);
                            int suspicion = (int)story.variablesState["nghi_ngo"];
                            if (suspicion < 0 || suspicion > 100) problems.Add("nghi_ngo out of range: " + suspicion + " at " + story.state.currentPathString);
                            if ((int)story.variablesState["tien"] < 0) problems.Add("tien negative at " + story.state.currentPathString);
                            int dossier = (int)story.variablesState["c6_ho_so"];
                            if (dossier < 0 || dossier > 4) problems.Add("c6_ho_so out of range: " + dossier);
                            if (lastTags.Any(t => t.Trim() == "chapter:8"))
                            {
                                int ending = (int)story.variablesState["c7_ket"];
                                if (ending < 1 || ending > 3) problems.Add("c7_ket not decided before the epilogue: " + ending);
                            }
                            maxSuspicion = Math.Max(maxSuspicion, suspicion);
                        }
                        var choices = story.currentChoices;
                        if (choices.Count == 0) break;
                        int exploreCount = choices.Count(c => c.tags != null && c.tags.Any(t => t.StartsWith("explore:")));
                        if (exploreCount != 0 && exploreCount != choices.Count)
                            problems.Add("Mixed explore/UI choices at " + story.state.currentPathString);
                        foreach (var choice in choices)
                            if (choice.tags != null)
                                foreach (string tag in choice.tags)
                                {
                                    if (tag.StartsWith("explore:")) explore.Add(tag.Substring(8));
                                    else if (tag != "timeout") problems.Add("Unknown choice tag " + tag);
                                }
                        if (lastTags.Any(t => t.StartsWith("timer:")) && !choices.Any(c => c.tags != null && c.tags.Contains("timeout")))
                            problems.Add("Timed choice without #timeout at " + story.state.currentPathString);
                        story.ChooseChoiceIndex(random.Next(choices.Count));
                    }
                    if (story.canContinue || story.currentChoices.Count > 0) problems.Add(entry + " run " + run + " exceeded guard");
                    string tracked = Tracked(entry);
                    if (tracked != null)
                    {
                        // Prologue: opinion of Kiều; 3: answer to the real Kiều; 6: dossier; 7: ending tone.
                        int answer = (int)story.variablesState[tracked];
                        answers[answer] = answers.TryGetValue(answer, out int count) ? count + 1 : 1;
                    }
                    if (entry == "chapter_4" && !(bool)story.variablesState["c4_khe_uoc_mg"]) problems.Add("chapter_4 ended without the Mã–Tú Bà contract");
                    maxLines = Math.Max(maxLines, lines);
                    minLines = Math.Min(minLines, lines);
                }
                log.AppendLine($"{entry}: {runs} runs, lines {minLines}-{maxLines}, max nghi_ngo {maxSuspicion}" +
                               (answers.Count > 0 ? ", " + Tracked(entry) + " " + string.Join(" ", answers.OrderBy(a => a.Key).Select(a => a.Key + ":" + a.Value)) : ""));
            }
            log.AppendLine("gameplay: " + string.Join(", ", gameplay));
            log.AppendLine("explore: " + string.Join(", ", explore));
            log.AppendLine("cameras: " + string.Join(", ", cameras));
            log.AppendLine("scenes: " + string.Join(", ", scenes));
            log.AppendLine("clues: " + string.Join(", ", clues));
            var distinct = problems.Distinct().ToList();
            log.Insert(0, (distinct.Count == 0 ? "INK VALIDATION PASS" : "INK VALIDATION FAIL") + "\n");
            foreach (string problem in distinct.Take(50)) log.AppendLine("PROBLEM " + problem);
            report = log.ToString();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report, new UTF8Encoding(false));
            return distinct.Count == 0;
        }

        /// <summary>Stands in for the Unity sequences that write story variables.</summary>
        internal static void SimulateSequence(string tag, Ink.Runtime.Story story, System.Random random)
        {
            var v = story.variablesState;
            switch (tag)
            {
                case "gameplay:deduction_c2": v["suy_luan"] = random.Next(1, 4); break;
                case "gameplay:sneak_out": v["c5_lang_le"] = random.Next(0, 4); break;
                case "gameplay:chase_alley": v["c5_duong"] = random.Next(1, 3); v["c5_xo_do"] = random.Next(0, 5); break;
                case "gameplay:chase_forest":
                    int qte = random.Next(0, 4);
                    v["c5_qte"] = qte;
                    v["c5_co_lam_thoat"] = (bool)v["c4_ru_co_lam"] && qte >= 2;
                    break;
                case "gameplay:doi_chieu": v["c6_doi_chieu"] = random.Next(0, 4); break;
                case "gameplay:trial_1": case "gameplay:trial_2": case "gameplay:trial_3":
                    int points = random.Next(1, tag.EndsWith("3") ? 5 : 4);
                    v["c7_r" + tag[tag.Length - 1]] = points;
                    v["c7_diem"] = (int)v["c7_diem"] + points;
                    v["c7_long_tin"] = random.Next(10, 101);
                    break;
            }
        }

        /// <summary>Chapters played from their own entry start with random outcomes of the chapters before them.</summary>
        internal static void RandomisePriorChapters(Ink.Runtime.Story story, int chapter, System.Random random)
        {
            var v = story.variablesState;
            if (chapter > 0) v["pl_che_kieu"] = random.Next(1, 4);
            if (chapter > 2) { v["giu_mau_giay"] = random.Next(2) == 0; v["mm_nghe_len"] = random.Next(2) == 0; v["doc_ky_khe"] = random.Next(2) == 0; }
            if (chapter > 3) { v["c3_khe_uoc"] = random.Next(2) == 0; v["c3_so_no"] = random.Next(2) == 0; }
            if (chapter > 4)
            {
                v["c4_ru_co_lam"] = random.Next(2) == 0;
                v["c4_tien"] = random.Next(0, 3);
                v["tien"] = (int)v["c4_tien"] == 2 ? 450 : (int)v["c4_tien"] == 1 ? 5 : 0;
                v["c4_van_tu"] = random.Next(0, 3);
                v["c4_canh_giac"] = random.Next(0, 3);
                v["ev_khe_uoc"] = true;
                v["c4_khe_uoc_mg"] = true;
                v["ev_tien"] = (int)v["c4_tien"] == 2;
                v["ev_giay_no"] = v["c4_so_no"] = random.Next(2) == 0;
            }
            if (chapter > 5)
            {
                int qte = random.Next(0, 4);
                v["c5_qte"] = qte;
                v["c5_co_lam_thoat"] = (bool)v["c4_ru_co_lam"] && qte >= 2;
            }
            if (chapter > 6)
            {
                v["tra_loi_kieu_that"] = random.Next(1, 4);
                v["kt_tin_tuong"] = random.Next(0, 7);
                bool lam = (bool)v["c5_co_lam_thoat"] && random.Next(2) == 0;
                v["nc_co_lam"] = lam;
                v["nc_ba_nuoc"] = random.Next(2) == 0;
                v["nc_thu_lai"] = random.Next(2) == 0;
                v["ev_nhan_chung"] = (lam ? 1 : 0) + ((bool)v["nc_ba_nuoc"] ? 1 : 0) + ((bool)v["nc_thu_lai"] ? 1 : 0);
                v["c6_so_bac"] = random.Next(2) == 0;
                if ((bool)v["c6_so_bac"]) v["ev_tien"] = true;
                v["c6_ho_so"] = 1 + ((int)v["ev_nhan_chung"] > 0 ? 1 : 0) + ((bool)v["ev_giay_no"] ? 1 : 0) + ((bool)v["ev_tien"] ? 1 : 0);
            }
            if (chapter > 7) v["c7_ket"] = random.Next(1, 4);
        }

        private static void CheckTag(string raw, List<string> problems, ISet<string> gameplay, ISet<string> cameras, ISet<string> scenes, ISet<string> clues)
        {
            string tag = raw.Trim();
            int colon = tag.IndexOf(':');
            string key = colon < 0 ? tag : tag.Substring(0, colon);
            string value = colon < 0 ? string.Empty : tag.Substring(colon + 1).Trim();
            if (!KnownKeys.Contains(key)) problems.Add("Unknown tag " + tag);
            if (key == "speaker" && !KnownSpeakers.Contains(value)) problems.Add("Unknown speaker " + value);
            if (key == "gameplay") gameplay.Add(value);
            if (key == "camera") cameras.Add(value);
            if (key == "scene") scenes.Add(value);
            if (key == "clue") clues.Add(value);
        }
    }
}

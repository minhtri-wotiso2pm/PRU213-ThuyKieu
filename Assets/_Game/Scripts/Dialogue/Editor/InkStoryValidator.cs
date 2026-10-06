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
        { "chapter", "scene", "music", "sfx", "speaker", "emotion", "camera", "objective", "clue", "gameplay", "timer" };
        private static readonly HashSet<string> KnownSpeakers = new HashSet<string>
        { "Narrator", "Kieu", "NoiTam", "KieuThat", "TrangSach", "MeKieu", "MaGiamSinh", "TuyTung", "TuBa", "MaKieu", "CoHong", "CoLam", "NguoiCanh" };

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
            foreach (string entry in new[] { "chapter_1", "chapter_2", "chapter_3" })
            {
                int runs = 1500, maxLines = 0, minLines = int.MaxValue, maxSuspicion = 0;
                var answers = new Dictionary<int, int>();
                for (int run = 0; run < runs; run++)
                {
                    var story = new Ink.Runtime.Story(json);
                    if (entry != "chapter_1") story.ChoosePathString(entry);
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
                            foreach (string tag in lastTags)
                                if (tag.Trim() == "gameplay:deduction_c2") story.variablesState["suy_luan"] = random.Next(1, 4);
                            int suspicion = (int)story.variablesState["nghi_ngo"];
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
                    if (entry == "chapter_3")
                    {
                        int answer = (int)story.variablesState["tra_loi_kieu_that"];
                        answers[answer] = answers.TryGetValue(answer, out int count) ? count + 1 : 1;
                    }
                    maxLines = Math.Max(maxLines, lines);
                    minLines = Math.Min(minLines, lines);
                }
                log.AppendLine($"{entry}: {runs} runs, lines {minLines}-{maxLines}, max nghi_ngo {maxSuspicion}" +
                               (answers.Count > 0 ? ", tra_loi_kieu_that " + string.Join(" ", answers.OrderBy(a => a.Key).Select(a => a.Key + ":" + a.Value)) : ""));
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

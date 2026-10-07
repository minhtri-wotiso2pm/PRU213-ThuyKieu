using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Dialogue.Editor
{
    /// <summary>
    /// Lists every voiced line as Tools/Voice/voice_manifest.json for Tools/Voice/generate_voice.py.
    /// Ink lines are collected by random walkthroughs of each chapter (tracking the speaker exactly as
    /// DialogueManager does); barks are read from the gameplay scripts and scene setup code.
    /// Batch mode: -executeMethod ThuyKieu.Dialogue.Editor.VoiceManifestExporter.RunBatch
    /// </summary>
    public static class VoiceManifestExporter
    {
        public const string ManifestPath = "Tools/Voice/voice_manifest.json";
        private const int RunsPerChapter = 2500;
        private static readonly Regex BarkCall = new Regex("(?:ShowBark|Bark)\\(\"(\\w+)\",\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
        private static readonly Regex GlanceLines = new Regex("_glanceLines\",\\s*new\\[\\]\\s*\\{([^}]*)\\}");
        private static readonly Regex Quoted = new Regex("\"((?:[^\"\\\\]|\\\\.)*)\"");
        private static readonly string[] Glancers = { "TuBa", "MaGiamSinh", "TuyTung" };
        // Shout lists picked at random at runtime (ChaseSequence.Shouts → the pursuing servants).
        private static readonly Regex ShoutList = new Regex(@"Shouts\s*=\s*\{([^}]*)\}");

        [MenuItem("ThuyKieu/Dialogue/Export voice manifest")]
        public static void RunMenu() { Debug.Log(Export()); }

        public static void RunBatch()
        {
            try { Debug.Log(Export()); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        public static string Export()
        {
            InkStoryCompiler.Compile();
            string json = File.ReadAllText(InkStoryCompiler.Output, Encoding.UTF8);
            var lines = new Dictionary<string, (string Speaker, string Text, string Source)>();
            var random = new System.Random(7213);
            foreach (string entry in new[] { "chapter_1", "chapter_2", "chapter_3", "chapter_4", "chapter_5", "chapter_6" })
                for (int run = 0; run < RunsPerChapter; run++)
                    Walk(json, entry, random, lines);

            foreach (string file in Directory.GetFiles("Assets/_Game/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                string code = File.ReadAllText(file, Encoding.UTF8);
                foreach (Match match in BarkCall.Matches(code))
                    Add(lines, match.Groups[1].Value, Unescape(match.Groups[2].Value), "bark");
                foreach (Match match in ShoutList.Matches(code))
                    foreach (Match quoted in Quoted.Matches(match.Groups[1].Value))
                        Add(lines, "GiaNhan", Unescape(quoted.Groups[1].Value), "bark");
                foreach (Match match in GlanceLines.Matches(code))
                    foreach (Match quoted in Quoted.Matches(match.Groups[1].Value))
                        foreach (string speaker in Glancers)
                            Add(lines, speaker, Unescape(quoted.Groups[1].Value), "glance");
            }

            var builder = new StringBuilder("[\n");
            var ordered = lines.OrderBy(l => l.Value.Source).ThenBy(l => l.Value.Speaker).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                var (speaker, text, source) = ordered[i].Value;
                builder.Append("  {\"key\": \"").Append(ordered[i].Key)
                    .Append("\", \"speaker\": \"").Append(speaker)
                    .Append("\", \"source\": \"").Append(source)
                    .Append("\", \"text\": \"").Append(EscapeJson(text)).Append("\"}")
                    .Append(i < ordered.Count - 1 ? ",\n" : "\n");
            }
            builder.Append("]\n");
            Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath));
            File.WriteAllText(ManifestPath, builder.ToString(), new UTF8Encoding(false));
            var perSpeaker = ordered.GroupBy(l => l.Value.Speaker).Select(g => g.Key + "=" + g.Count());
            return "Voice manifest: " + ordered.Count + " lines (" + string.Join(", ", perSpeaker) + ") → " + ManifestPath;
        }

        /// <summary>One random playthrough. A new chapter scene resets the speaker to the narrator, as SetInkStory does.</summary>
        private static void Walk(string json, string entry, System.Random random, Dictionary<string, (string, string, string)> lines)
        {
            var story = new Ink.Runtime.Story(json);
            if (entry != "chapter_1") { InkStoryValidator.RandomisePriorChapters(story, entry, random); story.ChoosePathString(entry); }
            string speaker = "Narrator";
            for (int guard = 0; guard < 4000; guard++)
            {
                while (story.canContinue)
                {
                    string text = story.Continue().Trim();
                    var tags = story.currentTags;
                    foreach (string raw in tags)
                    {
                        string tag = raw.Trim();
                        if (tag.StartsWith("chapter:")) speaker = "Narrator";
                        InkStoryValidator.SimulateSequence(tag, story, random);
                    }
                    // DialogueManager skips blank lines without reading their tags.
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    foreach (string raw in tags)
                        if (raw.Trim().StartsWith("speaker:")) speaker = raw.Trim().Substring(8).Trim();
                    Add(lines, speaker, text, entry);
                }
                if (story.currentChoices.Count == 0) return;
                story.ChooseChoiceIndex(random.Next(story.currentChoices.Count));
            }
        }

        private static void Add(Dictionary<string, (string, string, string)> lines, string speaker, string text, string source)
        {
            string spoken = VoiceLine.Normalize(text);
            if (spoken.Length == 0 || !spoken.Any(char.IsLetterOrDigit)) return;
            string key = VoiceLine.Key(speaker, text);
            if (!lines.ContainsKey(key)) lines[key] = (speaker, spoken, source);
        }

        private static string Unescape(string literal) => Regex.Unescape(literal);

        private static string EscapeJson(string text)
        {
            var builder = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '"' || c == '\\') builder.Append('\\').Append(c);
                else if (c < 32) builder.Append("\\u").Append(((int)c).ToString("x4"));
                else builder.Append(c);
            }
            return builder.ToString();
        }
    }
}

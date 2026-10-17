using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ThuyKieu.Presentation;
using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Writes every ProceduralAudio id to a WAV under Resources/StoryAudio so the game never synthesises audio while playing
    /// (a 40-second crowd bed took over a second on the main thread). Runs before each Windows build.
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.AudioBankBaker.BakeBatch
    /// </summary>
    public static class AudioBankBaker
    {
        public const string Folder = "Assets/_Game/Audio/Resources/" + AudioBank.ResourceRoot;
        private const string Source = "Assets/_Game/Scripts/Presentation/ProceduralAudio.cs";
        private static readonly Regex Case = new Regex("case \"([a-z0-9_]+)\"");

        [MenuItem("ThuyKieu/Audio/Bake procedural audio")]
        public static void Bake()
        {
            string code = File.ReadAllText(Source, Encoding.UTF8);
            int written = 0, kept = 0;
            foreach (var (kind, method, next) in new[] { (AudioBank.Music, "Music(", "Ambience("), (AudioBank.Ambience, "Ambience(", "Sfx("), (AudioBank.Sfx, "Sfx(", "// ---") })
            {
                foreach (string id in Ids(code, method, next))
                {
                    string path = $"{Folder}/{kind}/{id}.wav";
                    if (File.Exists(path)) { kept++; continue; } // ids are deterministic; delete a file to bake it again
                    var clip = AudioBank.Synthesise(kind)(id);
                    if (clip == null) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    WriteWav(path, clip);
                    UnityEngine.Object.DestroyImmediate(clip);
                    written++;
                }
            }
            AssetDatabase.Refresh();
            int configured = Configure(Folder);
            Debug.Log($"[AudioBank] baked {written} clips, kept {kept}, import settings on {configured} → {Folder}");
        }

        public static void BakeBatch()
        {
            try { Bake(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static string[] Ids(string code, string method, string next)
        {
            int start = code.IndexOf("public static AudioClip " + method, StringComparison.Ordinal);
            int end = code.IndexOf(next, start + method.Length, StringComparison.Ordinal);
            return Case.Matches(code.Substring(start, end - start)).Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToArray();
        }

        /// <summary>Music and ambience stream from disk (no decode stall on load); short effects stay in memory.</summary>
        private static int Configure(string folder)
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is AudioImporter importer)) continue;
                bool longClip = path.Contains("/" + AudioBank.Music + "/") || path.Contains("/" + AudioBank.Ambience + "/");
                var settings = importer.defaultSampleSettings;
                var wanted = settings;
                wanted.loadType = longClip ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                wanted.compressionFormat = AudioCompressionFormat.Vorbis;
                wanted.quality = longClip ? 0.5f : 0.7f;
                if (importer.forceToMono && wanted.Equals(settings)) continue;
                importer.forceToMono = true;
                importer.defaultSampleSettings = wanted;
                importer.SaveAndReimport();
                count++;
            }
            return count;
        }

        private static void WriteWav(string path, AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                int bytes = samples.Length * 2;
                writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + bytes); writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)clip.channels);
                writer.Write(clip.frequency); writer.Write(clip.frequency * clip.channels * 2); writer.Write((short)(clip.channels * 2)); writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(bytes);
                foreach (float sample in samples) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * 32767f));
            }
        }
    }
}

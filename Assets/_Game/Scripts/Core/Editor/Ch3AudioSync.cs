using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Chapter 3 footsteps and doors brought in line with Chapter 4 (docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md §17).
    ///   Audit: -executeMethod ThuyKieu.Core.Editor.Ch3AudioSync.AuditBatch   (report: Tools/LauTuBa/QA/Ch3AudioAudit.txt)
    /// The audit lists, for both generated scenes, what decides what the player hears: the scene's Foley bank, her footstep
    /// component, every surface tag (by floor, solid or covering) and every door's serialized settings, Ch3 next to Ch4.
    /// </summary>
    public static class Ch3AudioSync
    {
        public static void AuditBatch()
        {
            try
            {
                var report = new StringBuilder();
                var doors = new Dictionary<string, Dictionary<string, string>>();
                foreach (var (label, path) in new[] { ("Ch3", LauTuBaSetup.Chapter3Path), ("Ch4", LauTuBaSetup.Chapter4Path) })
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    report.AppendLine($"== {label} {path}");
                    foreach (var audio in Object.FindObjectsByType<StoryAudio>(FindObjectsInactive.Include))
                    {
                        var so = new SerializedObject(audio);
                        var bank = so.FindProperty("_foley").objectReferenceValue as FoleyBank;
                        report.AppendLine($"  StoryAudio {audio.name}: Foley bank {(bank != null ? AssetDatabase.GetAssetPath(bank) + " (" + string.Join(", ", bank.Groups.Select(g => g.Id)) + ")" : "none")} · player for old steps {(so.FindProperty("_player").objectReferenceValue != null)}");
                    }
                    foreach (var steps in Object.FindObjectsByType<FirstPersonFootsteps>(FindObjectsInactive.Include))
                        report.AppendLine($"  FirstPersonFootsteps on {steps.name}: {Fields(steps)}");
                    report.AppendLine($"  ActorFootsteps: {Object.FindObjectsByType<ActorFootsteps>(FindObjectsInactive.Include).Length}");
                    var tags = Object.FindObjectsByType<SurfaceTag>(FindObjectsInactive.Include);
                    foreach (var group in tags.GroupBy(t => (t.Surface, new SerializedObject(t).FindProperty("_patch").boolValue)).OrderBy(g => g.Key.Surface))
                        report.AppendLine($"  SurfaceTag {group.Key.Surface}{(group.Key.boolValue ? " (covering)" : "")}: {group.Count()} · {string.Join(", ", group.Select(t => t.name + (t.Carry != SurfaceTag.Loudness(t.Surface) ? $" (carries ×{t.Carry:0.##})" : "")).Distinct().Take(12))}");
                    var mine = new Dictionary<string, string>();
                    foreach (var door in Object.FindObjectsByType<HouseDoor>(FindObjectsInactive.Include).OrderBy(d => d.name))
                    {
                        string key = Hierarchy(door.transform);
                        mine[key] = Fields(door);
                    }
                    doors[label] = mine;
                    report.AppendLine($"  HouseDoor: {mine.Count}");
                }
                report.AppendLine("== Door settings that differ (Ch3 | Ch4)");
                foreach (var key in doors["Ch3"].Keys.Union(doors["Ch4"].Keys).OrderBy(k => k))
                {
                    doors["Ch3"].TryGetValue(key, out string a);
                    doors["Ch4"].TryGetValue(key, out string b);
                    if (a == b) continue;
                    report.AppendLine($"  {key}\n    Ch3 {a ?? "-"}\n    Ch4 {b ?? "-"}");
                }
                Directory.CreateDirectory("Tools/LauTuBa/QA");
                File.WriteAllText("Tools/LauTuBa/QA/Ch3AudioAudit.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        // ------------------------------------------------------------------ the bank

        public const string BankPath = Ch4Foley.Root + "/Ch3FoleyBank.asset";

        /// <summary>Footsteps, doors, latches, bolts and locks: what Chapter 3 shares with Chapter 4 (not her snoring).</summary>
        private static readonly string[] SharedPrefixes = { "step_", "door_", "slide_", "latch_", "bolt_", "lock_", "key_" };

        /// <summary>
        /// Chapter 3's Foley bank: the Chapter 4 bank's step and door groups, the same recorded clips (no copies, same GUIDs),
        /// takes and levels; Tú Bà's breath and anything else of Chapter 4's own left out. Refreshed from the Chapter 4 bank
        /// each time (Ch4Foley.Build calls it), so the two never drift apart.
        /// </summary>
        public static FoleyBank Bank()
        {
            var source = AssetDatabase.LoadAssetAtPath<FoleyBank>(Ch4Foley.BankPath);
            if (source == null) return null;
            var bank = AssetDatabase.LoadAssetAtPath<FoleyBank>(BankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<FoleyBank>();
                AssetDatabase.CreateAsset(bank, BankPath);
            }
            bank.Groups = source.Groups.Where(g => SharedPrefixes.Any(p => g.Id.StartsWith(p)))
                .Select(g => new FoleyBank.Group { Id = g.Id, Takes = g.Takes.ToArray(), Gain = g.Gain }).ToArray();
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
            return bank;
        }

        // ------------------------------------------------------------------ the scene

        /// <summary>
        /// Chapter 3's sound settings on the open scene: its StoryAudio plays the shared bank, and the floors that sound like
        /// Chapter 4's (rugs and mats, the muddy alley) still carry to Chapter 3's watchers exactly as before this sync (a rug
        /// as the floor beneath it, the alley as the wet stone it was tagged), since Chapter 3's gameplay is not changed yet.
        /// Used by the scene builder (LauTuBaSetup.CreateChapter3) and by the patch of the existing scene.
        /// </summary>
        public static string Apply(StoryAudio audio)
        {
            var log = new StringBuilder();
            Physics.SyncTransforms();
            var bank = Bank();
            var so = new SerializedObject(audio);
            so.FindProperty("_foley").objectReferenceValue = bank;
            so.ApplyModifiedPropertiesWithoutUndo();
            log.AppendLine($"  StoryAudio bank: {(bank != null ? BankPath + " (" + bank.Groups.Length + " groups)" : "MISSING (run Ch4Foley.BuildBatch)")}");
            foreach (var tag in Object.FindObjectsByType<SurfaceTag>(FindObjectsInactive.Include))
            {
                var t = new SerializedObject(tag);
                bool patch = t.FindProperty("_patch").boolValue;
                float carry = -1f;
                if (patch) carry = UnderCarry(tag);
                else if (tag.Surface == Surface.Mud && tag.name == "Alley") carry = SurfaceTag.Loudness(Surface.WetStone);
                if (carry < 0f) continue;
                t.FindProperty("_carry").floatValue = carry;
                t.ApplyModifiedPropertiesWithoutUndo();
                log.AppendLine($"  {tag.name} ({tag.Surface}{(patch ? ", covering" : "")}): sounds as {tag.Surface}, carries ×{carry:0.##} as before");
            }
            return log.ToString();
        }

        /// <summary>How far a step carried on the floor under a covering (its tag, or boards when untagged).</summary>
        private static float UnderCarry(SurfaceTag covering)
        {
            var renderers = covering.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 1f;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            foreach (var hit in Physics.RaycastAll(bounds.center + Vector3.up * 0.3f, Vector3.down, 1.5f, 1 << 8, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
            {
                var tag = hit.collider.GetComponentInParent<SurfaceTag>();
                if (tag == covering) continue;
                return tag != null ? tag.Carry : 1f;
            }
            return 1f;
        }

        /// <summary>
        /// Patches the existing Chapter 3 scene in place (no rebuild of its architecture): the floor tags the current house
        /// builder gives (read from the Chapter 4 scene, same builder) added where the same object exists in Chapter 3, then
        /// <see cref="Apply"/>, then saved.   Batch: -executeMethod ThuyKieu.Core.Editor.Ch3AudioSync.PatchBatch
        /// (report: Tools/LauTuBa/QA/Ch3AudioPatch.txt)
        /// </summary>
        public static void PatchBatch()
        {
            try
            {
                var report = new StringBuilder("Chapter 3 audio patch\n");
                EditorSceneManager.OpenScene(LauTuBaSetup.Chapter4Path, OpenSceneMode.Single);
                var wanted = Object.FindObjectsByType<SurfaceTag>(FindObjectsInactive.Include)
                    .Select(t => (path: Hierarchy(t.transform), centre: Centre(t.transform), surface: t.Surface, patch: new SerializedObject(t).FindProperty("_patch").boolValue)).ToList();
                var scene = EditorSceneManager.OpenScene(LauTuBaSetup.Chapter3Path, OpenSceneMode.Single);
                var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).GroupBy(Hierarchy).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var w in wanted)
                {
                    if (!all.TryGetValue(w.path, out var candidates)) { report.AppendLine($"  not in Ch3: {w.path} ({w.surface})"); continue; }
                    var target = candidates.OrderBy(c => Vector3.Distance(Centre(c), w.centre)).First();
                    if (Vector3.Distance(Centre(target), w.centre) > 0.5f) { report.AppendLine($"  not in Ch3 at that place: {w.path} ({w.surface})"); continue; }
                    var tag = target.GetComponent<SurfaceTag>();
                    bool added = tag == null;
                    if (added) tag = target.gameObject.AddComponent<SurfaceTag>();
                    var t = new SerializedObject(tag);
                    if (!added && tag.Surface == w.surface && t.FindProperty("_patch").boolValue == w.patch) continue;
                    string before = added ? "untagged" : tag.Surface.ToString();
                    t.FindProperty("_surface").enumValueIndex = (int)w.surface;
                    t.FindProperty("_patch").boolValue = w.patch;
                    t.ApplyModifiedPropertiesWithoutUndo();
                    report.AppendLine($"  {w.path}: {before} → {w.surface}{(w.patch ? " (covering)" : "")}");
                }
                var audio = Object.FindObjectsByType<StoryAudio>(FindObjectsInactive.Include).Single();
                report.Append(Apply(audio));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Directory.CreateDirectory("Tools/LauTuBa/QA");
                File.WriteAllText("Tools/LauTuBa/QA/Ch3AudioPatch.txt", report.ToString(), new UTF8Encoding(false));
                Debug.Log(report.ToString());
                EditorApplication.Exit(0);
            }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static Vector3 Centre(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return t.position;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.center;
        }

        private static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;

        /// <summary>A component's own serialized values (references by name), for comparing two scenes.</summary>
        private static string Fields(Object component)
        {
            var so = new SerializedObject(component);
            var it = so.GetIterator();
            var parts = new List<string>();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.name == "m_Script") continue;
                string value;
                switch (it.propertyType)
                {
                    case SerializedPropertyType.Float: value = it.floatValue.ToString("0.###"); break;
                    case SerializedPropertyType.Integer: value = it.intValue.ToString(); break;
                    case SerializedPropertyType.Boolean: value = it.boolValue.ToString(); break;
                    case SerializedPropertyType.Enum: value = it.enumValueIndex >= 0 && it.enumValueIndex < it.enumNames.Length ? it.enumNames[it.enumValueIndex] : it.enumValueIndex.ToString(); break;
                    case SerializedPropertyType.String: value = it.stringValue; break;
                    case SerializedPropertyType.LayerMask: value = it.intValue.ToString(); break;
                    case SerializedPropertyType.ObjectReference: value = it.objectReferenceValue != null ? it.objectReferenceValue.name : "null"; break;
                    case SerializedPropertyType.Vector3: value = it.vector3Value.ToString("F2"); break;
                    default: value = it.propertyType.ToString(); break;
                }
                parts.Add(it.name + "=" + value);
            }
            return string.Join(" ", parts);
        }
    }
}

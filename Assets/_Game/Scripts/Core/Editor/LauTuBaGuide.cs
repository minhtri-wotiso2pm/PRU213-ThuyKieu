using System;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Gameplay.Chapters.LauTuBa;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Chapter 4's objective guidance (<see cref="Chapter04Guidance"/> and its <see cref="GuideGlow"/> cues) added to the
    /// night scene as it stands: nothing else is rebuilt, so every object, reference and GUID already in the scene stays.
    /// Running it again replaces what it made before, never a second set. The full rebuild (LauTuBaSetup) runs it too.
    /// The cues sit on the house's own pieces: the office's paper window (corridor side), the crack of door B on Tú Bà's
    /// side, the rings and bolts of the doors into the office and of the back door, the cabinet's brass lock, the kitchen's
    /// paper window. Quads only: no collider, no Light, no shadow.
    ///   Menu: ThuyKieu ▸ Lầu Tú Bà ▸ Install Chapter 4 objective guidance
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.LauTuBaGuide.InstallBatch
    /// </summary>
    public static class LauTuBaGuide
    {
        private const string RootName = "ObjectiveGuidance";
        private const string MaterialPath = LauTuBaKit.Folder + "/LTB_GuideGlow.mat";
        private static readonly Color Lamplight = new Color(1f, 0.72f, 0.42f);
        private static readonly Color Gleam = new Color(1f, 0.82f, 0.58f);
        private static readonly Color Embers = new Color(1f, 0.5f, 0.24f);

        [MenuItem("ThuyKieu/Lầu Tú Bà/Install Chapter 4 objective guidance")]
        public static void InstallMenu()
        {
            var scene = EditorSceneManager.OpenScene(LauTuBaSetup.Chapter4Path);
            Debug.Log(Install());
            EditorSceneManager.SaveScene(scene);
        }

        public static void InstallBatch()
        {
            try
            {
                var scene = EditorSceneManager.OpenScene(LauTuBaSetup.Chapter4Path);
                Debug.Log(Install());
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>Into the open Chapter 4 scene (its Chapter04LauWorld must be wired). Returns a one-line summary.</summary>
        public static string Install()
        {
            var world = Object.FindAnyObjectByType<Chapter04LauWorld>(FindObjectsInactive.Include);
            if (world == null) throw new InvalidOperationException("[LauTuBaGuide] no Chapter04LauWorld in the open scene");
            Remove();
            var fields = new SerializedObject(world);
            T Ref<T>(string field) where T : Object
            {
                var value = fields.FindProperty(field)?.objectReferenceValue as T;
                if (value == null) throw new InvalidOperationException("[LauTuBaGuide] Chapter04LauWorld." + field + " is not set");
                return value;
            }
            var innerDoor = Ref<HouseDoor>("_innerDoor");
            var officeDoor = Ref<HouseDoor>("_officeDoor");
            var backDoor = Ref<HouseDoor>("_backDoor");
            var material = GlowMaterial();

            // The office lamp seen from the corridor: warm paper between the slats.
            var officeWindow = Named("OfficeWindow", new Vector3(13.6f, LauTuBaKit.Floor + 1.42f, 11f));
            var paper = PaperGlow("OfficeWindow", officeWindow, Lamplight, 0.55f, 0f, material);
            // The same light through the crack on the latch side of door B, on Tú Bà's side, along the jamb's inner edge.
            float side = Side(innerDoor, true);
            float width = LeafWidth(innerDoor);
            var crack = Glow("DoorBCrack", innerDoor.transform, new Vector3(side * 0.15f, LauTuBaKit.DoorHeight * 0.47f, width - 0.095f), Quaternion.Euler(0f, 90f, 0f),
                new Vector2(0.022f, LauTuBaKit.DoorHeight * 0.82f), Lamplight, 0.9f, new Vector2(0.9f, 0.35f), false, false, material);
            var doors = new List<GuideGlow>();
            doors.AddRange(Hardware("DoorB", innerDoor, side, material));
            doors.AddRange(Hardware("OfficeDoor", officeDoor, -Side(officeDoor, true), material, false));
            // Inside: the brass lock of the cabinet.
            var cabinetDoor = Named("BrassCabinet", new Vector3(20.62f, LauTuBaKit.Floor, 15.2f)).Find("Door");
            var brass = cabinetDoor != null ? cabinetDoor.Find("Lock") : null;
            if (brass == null) throw new InvalidOperationException("[LauTuBaGuide] BrassCabinet/Door/Lock not found");
            var lockGlow = Glow("CabinetLock", cabinetDoor, brass.localPosition + Vector3.forward * (brass.localScale.z * 0.5f + 0.012f), Quaternion.identity,
                Vector2.one * 0.17f, Gleam, 0.6f, Vector2.one, true, true, material);
            // With the contract: the stove's glow on the kitchen paper, a gleam on the back door's ring and bolt.
            var kitchenWindow = Named("KitchenWindow", new Vector3(-18.3f, LauTuBaKit.Floor + 1.42f, 11f));
            var kitchen = new List<GuideGlow> { PaperGlow("KitchenWindow", kitchenWindow, Embers, 0.3f, -0.12f, material) };
            kitchen.AddRange(Hardware("BackDoor", backDoor, Side(backDoor, true), material));

            var root = new GameObject(RootName);
            root.transform.SetParent(world.transform, false);
            var guidance = root.AddComponent<Chapter04Guidance>();
            Set(guidance, "_director", Ref<Object>("_director"));
            Set(guidance, "_world", world);
            Set(guidance, "_inventory", Ref<Object>("_inventory"));
            Set(guidance, "_keyTheft", Ref<Object>("_keyTheft"));
            Set(guidance, "_navMesh", Ref<Object>("_navMesh"));
            Set(guidance, "_officeLamp", Ref<Object>("_officeLamp"));
            Set(guidance, "_innerDoor", innerDoor);
            // The office (x 8–15) and the archive behind the lattice (x 15–21), between the corridor face and the north wall.
            Set(guidance, "_officeMin", new Vector3(8.05f, LauTuBaKit.Floor - 0.5f, 11.05f));
            Set(guidance, "_officeMax", new Vector3(20.95f, LauTuBaKit.Floor + 3f, 18.95f));
            Set(guidance, "_officeLight", new Object[] { paper });
            Set(guidance, "_doorCrack", crack);
            Set(guidance, "_officeDoors", doors.Cast<Object>().ToArray());
            Set(guidance, "_cabinet", new Object[] { lockGlow });
            Set(guidance, "_kitchen", kitchen.Cast<Object>().ToArray());
            Set(guidance, "_hairpin", Ref<Component>("_hairpin").transform);
            Set(guidance, "_kieuDoor", Ref<HouseDoor>("_kieuDoor").transform);
            Set(guidance, "_tuBaBed", Ref<Transform>("_sapFeet"));
            Set(guidance, "_cabinetFront", Ref<Transform>("_cabinetFront"));
            Set(guidance, "_backDoor", backDoor.transform);
            Set(world, "_guidance", guidance);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            int count = Object.FindObjectsByType<GuideGlow>(FindObjectsInactive.Include).Length;
            return $"[LauTuBaGuide] Chapter 4 guidance installed: {count} cues (office {1 + 1 + doors.Count}, cabinet 1, kitchen {kitchen.Count})";
        }

        /// <summary>
        /// The open scene's guidance as installed: one guidance, linked from the chapter, every reference set, each cue used
        /// once (no stray or doubled cues), none with a collider or a light.
        /// </summary>
        public static IEnumerable<(bool ok, string what)> Validate()
        {
            var all = Object.FindObjectsByType<Chapter04Guidance>(FindObjectsInactive.Include);
            yield return (all.Length == 1, "objective guidance: " + all.Length + " in the scene (1 expected)");
            if (all.Length != 1) yield break;
            var guidance = all[0];
            var world = Object.FindAnyObjectByType<Chapter04LauWorld>(FindObjectsInactive.Include);
            yield return (world != null && new SerializedObject(world).FindProperty("_guidance").objectReferenceValue == guidance, "the chapter is linked to its guidance");
            var fields = new SerializedObject(guidance);
            var missing = new List<string>();
            var used = new List<Object>();
            var property = fields.GetIterator();
            for (bool enter = true; property.NextVisible(enter); enter = false)
            {
                if (property.name == "m_Script") continue;
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (property.objectReferenceValue == null) missing.Add(property.name);
                    else used.Add(property.objectReferenceValue);
                }
                else if (property.isArray && property.arrayElementType.StartsWith("PPtr"))
                {
                    if (property.arraySize == 0) missing.Add(property.name + "[]");
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i).objectReferenceValue;
                        if (element == null) missing.Add(property.name + "[" + i + "]");
                        else used.Add(element);
                    }
                }
            }
            yield return (missing.Count == 0, "guidance references set" + (missing.Count == 0 ? "" : ": missing " + string.Join(", ", missing)));
            var glows = Object.FindObjectsByType<GuideGlow>(FindObjectsInactive.Include);
            var cues = used.OfType<GuideGlow>().ToList();
            bool once = glows.All(g => cues.Count(c => c == g) == 1) && cues.Count == glows.Length;
            yield return (once && glows.Length >= 6, $"cues: {glows.Length} in the scene, each used once {once}");
            bool bare = glows.All(g => g.GetComponentInChildren<Collider>(true) == null && g.GetComponentInChildren<Light>(true) == null);
            yield return (bare, "cues have no collider and no light");
        }

        /// <summary>Takes out every cue and guidance object a previous install made (and the world's link to it).</summary>
        private static void Remove()
        {
            foreach (var glow in Object.FindObjectsByType<GuideGlow>(FindObjectsInactive.Include)) Object.DestroyImmediate(glow.gameObject);
            foreach (var old in Object.FindObjectsByType<Chapter04Guidance>(FindObjectsInactive.Include)) Object.DestroyImmediate(old.gameObject);
        }

        private static Material GlowMaterial()
        {
            var shader = Shader.Find("ThuyKieu/GuideGlow");
            if (shader == null) throw new InvalidOperationException("[LauTuBaGuide] shader ThuyKieu/GuideGlow missing (Assets/_Game/Shaders/GuideGlow.shader)");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, MaterialPath); }
            material.shader = shader;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>The scene's object of that name nearest where the builder puts it (a name alone may repeat).</summary>
        private static Transform Named(string name, Vector3 near)
        {
            var found = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t => t.name == name)
                .OrderBy(t => (t.position - near).sqrMagnitude).FirstOrDefault();
            if (found == null || (found.position - near).sqrMagnitude > 4f) throw new InvalidOperationException("[LauTuBaGuide] " + name + " not found near " + near);
            return found;
        }

        /// <summary>The leaf's side (±1 along the door's local X) where its lock side point is: the room it is shut from.</summary>
        private static float Side(HouseDoor door, bool lockSide)
        {
            var point = new SerializedObject(door).FindProperty("_lockSide").objectReferenceValue as Transform;
            if (point == null) throw new InvalidOperationException("[LauTuBaGuide] " + door.name + " has no lock side");
            float side = door.transform.InverseTransformPoint(point.position).x < 0f ? -1f : 1f;
            return lockSide ? side : -side;
        }

        private static float LeafWidth(HouseDoor door)
        {
            var panel = door.transform.Find("Leaf/Panel");
            return panel != null ? panel.localScale.z + 0.02f : 1.2f;
        }

        /// <summary>Glowing paper over a lattice window's paper, on the side its slats face (behind the slats, before the paper).</summary>
        private static GuideGlow PaperGlow(string name, Transform window, Color color, float intensity, float lift, Material material)
        {
            var sheet = window.Find("Paper");
            Vector2 size = sheet != null ? new Vector2(sheet.localScale.x, sheet.localScale.y) : new Vector2(1.5f, 1.1f);
            return Glow(name, window, new Vector3(0f, lift * size.y * 0.5f, 0.012f), Quaternion.identity, size - Vector2.one * 0.06f, color, intensity,
                new Vector2(0.35f, 0.4f), false, false, material);
        }

        /// <summary>Gleams on a door's ring and bolt knob, on one face of the leaf (they swing with it).</summary>
        private static IEnumerable<GuideGlow> Hardware(string name, HouseDoor door, float side, Material material, bool bolt = true)
        {
            var leaf = door.transform.Find("Leaf");
            if (leaf == null) yield break;
            foreach (var part in leaf.Cast<Transform>().ToList())
            {
                bool ring = part.name == "Ring", knob = bolt && part.name == "BoltKnob";
                if (!(ring || knob) || Mathf.Sign(part.localPosition.x) != side) continue;
                Vector3 at = part.localPosition + Vector3.right * side * (part.localScale.x * 0.5f + 0.02f);
                yield return Glow(name + (ring ? "Ring" : "Bolt"), leaf, at, Quaternion.identity, Vector2.one * (ring ? 0.1f : 0.085f), Gleam, ring ? 0.55f : 0.5f,
                    Vector2.one, true, true, material);
            }
        }

        private static GuideGlow Glow(string name, Transform parent, Vector3 local, Quaternion rotation, Vector2 size, Color color, float intensity,
            Vector2 edge, bool round, bool faceCamera, Material material)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Guide_" + name;
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = local;
            quad.transform.localRotation = rotation;
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            GameObjectUtility.SetStaticEditorFlags(quad, 0);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            renderer.enabled = false;
            var glow = quad.AddComponent<GuideGlow>();
            var serialized = new SerializedObject(glow);
            serialized.FindProperty("_color").colorValue = color;
            serialized.FindProperty("_intensity").floatValue = intensity;
            serialized.FindProperty("_edge").vector2Value = edge;
            serialized.FindProperty("_round").boolValue = round;
            serialized.FindProperty("_faceCamera").boolValue = faceCamera;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return glow;
        }
    }
}

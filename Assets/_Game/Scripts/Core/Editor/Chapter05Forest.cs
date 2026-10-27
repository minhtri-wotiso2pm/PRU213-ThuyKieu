using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The Chapter 5 bamboo chase: the fixed course from the edge of the forest to the temple path, its ground and bamboo
    /// (made at load from the course, see <see cref="ForestGround"/> and <see cref="BambooGrove"/>), the obstacles with their
    /// colliders and NavMesh links, the falling culms, the plank bridge, the temple gate, and the chase sequence wired to the
    /// chapter's director, pursuer and Cô Lam.
    ///
    /// Used two ways, with the same result: by <see cref="Chapter05Setup.Create"/> when the whole scene is built, and by
    /// <see cref="Apply"/>, which rebuilds only the forest part inside the existing Chapter05_Escape scene (same file and
    /// GUID, the yard, town and hand-made changes such as the guard rigs left as they are). The builder owns exactly the
    /// "BambooChase" root under World, the forest cameras, the chase sequence object, the chase components on the first
    /// pursuer and Cô Lam, and the scene's night look; everything else is left alone. Running it twice gives the same
    /// scene (it logs a census of what it owns before and after), and its assets (obstacle meshes, materials, textures)
    /// are updated in place, keeping their GUIDs. It also sets Chapter 4 to hand straight over into the run.
    ///   Menu: ThuyKieu ▸ Chapter 5 ▸ Rebuild bamboo chase (in place)
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.Chapter05Forest.ApplyBatch
    /// </summary>
    public static partial class Chapter05Forest
    {
        public const string Folder = "Assets/_Game/Materials/Chapter05Forest";
        public const string RootName = "BambooChase";

        /// <summary>What the forest is wired to in the chapter scene.</summary>
        internal struct Refs
        {
            public Transform World, Sequences;
            public StoryDirector Director;
            public PlayerMovement Player;
            public PlayerInteraction Interaction;
            public ThirdPersonCameraController Camera;
            public StoryAudio Audio;
            public ScreenFx Fx;
            public StoryHUD Hud;
            public Chapter05World Chapter;
            public StoryActor[] Pursuers;
            public Follower Companion;
            public NavMeshSurface TownNav;
            /// <summary>Sở Khanh, who may still stand at the edge of the bamboo when the run begins.</summary>
            public StoryActor SoKhanh;
        }

        [MenuItem("ThuyKieu/Chapter 5/Rebuild bamboo chase (in place)")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new System.InvalidOperationException("Exit Play Mode first.");
            LinkChapter4();
            var scene = EditorSceneManager.OpenScene(Chapter05Setup.ScenePath, OpenSceneMode.Single);
            var refs = Find();
            string before = Census(refs);
            RemoveOld(refs);
            Build(refs);
            string after = Census(refs);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new System.InvalidOperationException("Could not save " + Chapter05Setup.ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Chapter05Forest] census before: " + before);
            Debug.Log("[Chapter05Forest] census after:  " + after + (before == after ? "  (unchanged)" : ""));
            Debug.Log("[Chapter05Forest] Bamboo chase rebuilt in " + Chapter05Setup.ScenePath + " · scene GUID " + AssetDatabase.AssetPathToGUID(Chapter05Setup.ScenePath));
        }

        /// <summary>
        /// Chapter 4 ends at the back door and hands straight over into the run (its director's <c>_seamlessNext</c>), set in
        /// the Chapter 4 scene in place; saved only when it was not set yet.
        /// </summary>
        internal static void LinkChapter4()
        {
            if (!System.IO.File.Exists(LauTuBaSetup.Chapter4Path)) return;
            var scene = EditorSceneManager.OpenScene(LauTuBaSetup.Chapter4Path, OpenSceneMode.Single);
            var director = Object.FindAnyObjectByType<StoryDirector>(FindObjectsInactive.Include);
            if (director == null) throw new System.InvalidOperationException(LauTuBaSetup.Chapter4Path + " has no StoryDirector.");
            var so = new SerializedObject(director);
            var seamless = so.FindProperty("_seamlessNext");
            if (seamless.boolValue) return;
            seamless.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new System.InvalidOperationException("Could not save " + LauTuBaSetup.Chapter4Path);
            Debug.Log("[Chapter05Forest] Chapter 4 now hands straight over into Chapter 5.");
        }

        /// <summary>What the builder owns in the open scene, counted (to show a second run changes nothing).</summary>
        private static string Census(Refs refs)
        {
            var root = refs.World.Find(RootName);
            var parts = new List<string>();
            if (root != null)
            {
                parts.Add("objects " + root.GetComponentsInChildren<Transform>(true).Length);
                parts.Add("colliders " + root.GetComponentsInChildren<Collider>(true).Length);
                parts.Add("renderers " + root.GetComponentsInChildren<Renderer>(true).Length);
                parts.Add("lights " + root.GetComponentsInChildren<Light>(true).Length);
                parts.Add("links " + root.GetComponentsInChildren<NavMeshLink>(true).Length);
                parts.Add("surfaces " + root.GetComponentsInChildren<NavMeshSurface>(true).Length);
                parts.Add("grounds " + root.GetComponentsInChildren<ForestGround>(true).Length);
                parts.Add("groves " + root.GetComponentsInChildren<BambooGrove>(true).Length);
                parts.Add("particles " + root.GetComponentsInChildren<ParticleSystem>(true).Length);
                parts.Add("audio " + root.GetComponentsInChildren<AudioSource>(true).Length);
            }
            else parts.Add("no forest");
            parts.Add("forest roots " + refs.World.Cast<Transform>().Count(t => t.name == RootName));
            parts.Add("cameras " + refs.World.Cast<Transform>().Count(t => t.name.StartsWith("CAM_") || t.name.StartsWith("LOOK_Forest")));
            parts.Add("sequences " + string.Join("+", refs.Sequences.Cast<Transform>().Select(t => t.name).OrderBy(n => n)));
            parts.Add("pursuer chase parts " + refs.Pursuers.Where(p => p != null).Sum(p => p.GetComponents<ChasePursuer>().Length + p.GetComponents<ChaseTraverser>().Length));
            parts.Add("Lam chase parts " + (refs.Companion == null ? 0 : refs.Companion.GetComponents<ChaseRunner>().Length + refs.Companion.GetComponents<ChaseTraverser>().Length));
            parts.Add("scene roots " + refs.World.gameObject.scene.rootCount);
            parts.Add("assets " + AssetDatabase.FindAssets("", new[] { Folder }).Length);
            return string.Join(" · ", parts);
        }

        public static void ApplyBatch()
        {
            try { Apply(); EditorApplication.Exit(0); }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        /// <summary>The chapter's systems in the open scene.</summary>
        private static Refs Find()
        {
            var director = Object.FindAnyObjectByType<StoryDirector>(FindObjectsInactive.Include);
            var chapter = Object.FindAnyObjectByType<Chapter05World>(FindObjectsInactive.Include);
            if (director == null || chapter == null) throw new System.InvalidOperationException("Chapter05_Escape has no StoryDirector / Chapter05World.");
            var data = new SerializedObject(chapter);
            var pursuers = data.FindProperty("_pursuers");
            var list = new List<StoryActor>();
            for (int i = 0; i < pursuers.arraySize; i++) list.Add((StoryActor)pursuers.GetArrayElementAtIndex(i).objectReferenceValue);
            var world = GameObject.Find("World");
            var sequences = director.transform.Find("Sequences");
            return new Refs
            {
                World = world != null ? world.transform : null,
                Sequences = sequences,
                Director = director,
                Player = director.Player,
                Interaction = director.Player.GetComponent<PlayerInteraction>(),
                Camera = director.CameraController,
                Audio = director.Audio,
                Fx = director.Fx,
                Hud = director.Hud,
                Chapter = chapter,
                Pursuers = list.ToArray(),
                Companion = (Follower)data.FindProperty("_companion").objectReferenceValue,
                TownNav = (NavMeshSurface)data.FindProperty("_navMesh").objectReferenceValue,
                SoKhanh = (StoryActor)data.FindProperty("_soKhanh").objectReferenceValue
            };
        }

        /// <summary>
        /// The parts of the old Chapter 5 this replaces: the cinematic forest rail and its temple, the yard sneak and alley
        /// chase sequences the story no longer calls, and their cameras; plus any earlier build of this forest.
        /// </summary>
        private static void RemoveOld(Refs refs)
        {
            string[] worldParts = { "BambooForest", RootName, "ForestChaseCamera", "CAM_Wide_BambooEdge", "CAM_Hero_Kieu_Rain", "CAM_Wide_TempleGate", "LOOK_Forest" };
            foreach (string name in worldParts)
            {
                var part = refs.World.Find(name);
                if (part != null) Object.DestroyImmediate(part.gameObject);
            }
            foreach (var look in refs.World.Cast<Transform>().Where(t => t.name.StartsWith("LOOK_Forest")).ToList()) Object.DestroyImmediate(look.gameObject);
            foreach (string id in new[] { "tutorial_escape", "sneak_out", "chase_alley", "chase_forest", "tutorial_forest" })
            {
                var sequence = refs.Sequences.Find(id);
                if (sequence != null) Object.DestroyImmediate(sequence.gameObject);
            }
            foreach (var pursuer in refs.Pursuers)
            {
                if (pursuer == null) continue;
                foreach (var old in pursuer.GetComponents<ChasePursuer>()) Object.DestroyImmediate(old);
                foreach (var old in pursuer.GetComponents<ChaseTraverser>()) Object.DestroyImmediate(old);
            }
            if (refs.Companion != null)
            {
                foreach (var old in refs.Companion.GetComponents<ChaseRunner>()) Object.DestroyImmediate(old);
                foreach (var old in refs.Companion.GetComponents<ChaseTraverser>()) Object.DestroyImmediate(old);
            }
        }

        /// <summary>Builds the forest under the scene's World and wires the chase into the chapter.</summary>
        internal static void Build(Refs refs)
        {
            System.IO.Directory.CreateDirectory(Folder);
            string previous = AssetFolder;
            AssetFolder = Folder;
            try
            {
                var layout = Layout.Make();
                var root = new GameObject(RootName).transform;
                root.SetParent(refs.World, false);
                var nav = root.gameObject.AddComponent<NavMeshSurface>();
                nav.collectObjects = CollectObjects.Children;
                nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                nav.layerMask = 1 << EnvironmentLayer;

                var ground = Ground(root, layout, nav);
                var course = Course(root, layout, refs);
                var bridge = Bridge(course.transform, layout, refs);
                Obstacles(course.transform, layout, refs, ground);
                Temple(root, layout, ground, out var rainSpot);
                Grove(root, layout, ground);
                Dressing(root, layout, ground, MoonForward(refs));
                Cameras(refs, layout, rainSpot);
                Wire(refs, layout, course, ground, bridge, rainSpot);
                Mood(refs);
                BakeNav(nav, ground);
            }
            finally { AssetFolder = previous; }
        }

        /// <summary>
        /// The forest's NavMesh, baked here from the same ground the scene grows at load (its hidden colliders exist in the
        /// Editor too; the bamboo and the floor past the banks are left out) and kept as one asset updated in place, so
        /// the chapter opens without building it.
        /// </summary>
        private static void BakeNav(NavMeshSurface nav, ForestGround ground)
        {
            Physics.SyncTransforms();
            nav.BuildNavMesh();
            var built = nav.navMeshData;
            string path = Folder + "/C5F_ForestNavMesh.asset";
            var asset = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if (asset == null) { built.name = "C5F_ForestNavMesh"; AssetDatabase.CreateAsset(built, path); asset = built; }
            else
            {
                EditorUtility.CopySerialized(built, asset);
                asset.name = "C5F_ForestNavMesh";
                EditorUtility.SetDirty(asset);
                Object.DestroyImmediate(built);
            }
            nav.RemoveData();
            nav.navMeshData = asset;
            nav.AddData();
            Set(ground, "_baked", asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Chapter05Forest] NavMesh baked into {path} ({new System.IO.FileInfo(path).Length / 1024} KB)");
        }

        // ------------------------------------------------------------------ wiring

        private static void Wire(Refs refs, Layout layout, ChaseCourse course, ForestGround ground, DropBridge bridge, Transform rainSpot)
        {
            // The pursuer: the first of the servants, with the torch; the others stay at the house.
            var main = refs.Pursuers[0];
            var traverser = main.gameObject.AddComponent<ChaseTraverser>();
            var pursuer = main.gameObject.AddComponent<ChasePursuer>();
            Set(pursuer, "_audio", refs.Audio);
            Set(pursuer, "_hud", refs.Hud);
            _ = traverser;
            ChaseRunner runner = null;
            if (refs.Companion != null)
            {
                refs.Companion.gameObject.AddComponent<ChaseTraverser>();
                runner = refs.Companion.gameObject.AddComponent<ChaseRunner>();
            }

            var go = new GameObject("chase_forest");
            go.transform.SetParent(refs.Sequences, false);
            var catchScene = go.AddComponent<CatchScene>();
            Set(catchScene, "_cameraController", refs.Camera);
            Set(catchScene, "_player", refs.Player);
            Set(catchScene, "_audio", refs.Audio);
            Set(catchScene, "_fx", refs.Fx);
            Set(catchScene, "_hud", refs.Hud);
            Set(catchScene, "_ending", (int)CatchScene.Ending.Drag);
            var chase = go.AddComponent<BambooChaseSequence>();
            Set(chase, "_id", "chase_forest");
            Set(chase, "_course", course);
            Set(chase, "_ground", ground);
            Set(chase, "_pursuer", pursuer);
            // Off the course while she runs: the other servants (they stayed at the house) and Sở Khanh (gone on his horse).
            Set(chase, "_others", refs.Pursuers.Skip(1).Append(refs.SoKhanh).Where(a => a != null).Cast<Object>().ToArray());
            Set(chase, "_companion", runner);
            Set(chase, "_bridge", bridge);
            Set(chase, "_catch", catchScene);
            // The chapter opens on this run (Ink "# run:chase_forest"): no title card, no modal tutorial; the controls are
            // the small line the run shows while it plays.
            Set(refs.Director, "_openWithCard", false);

            // Her steps by surface during the chase (off outside it: the yard keeps the old steps).
            if (!refs.Player.TryGetComponent<FirstPersonFootsteps>(out var steps)) steps = refs.Player.gameObject.AddComponent<FirstPersonFootsteps>();
            Set(steps, "_audio", refs.Audio);
            steps.enabled = false;
            // Chapter 5's own bank: rebuilt from what Ch5Foley rendered (and the Chapter 4 planks and stone) every time, the
            // same asset and GUID; nothing is appended, so a second Apply gives the same groups.
            Set(refs.Audio, "_foley", Ch5Foley.Compose());

            // Her body's weight in the view while she runs, in step with her feet.
            var motion = go.AddComponent<ChaseCameraMotion>();
            Set(motion, "_camera", refs.Camera);
            Set(motion, "_player", refs.Player);
            Set(motion, "_steps", steps);
            motion.enabled = false;
            Set(chase, "_motion", motion);
            // The score in levels of pressure: calm-tense from the start, harder once the ways meet, the top for the sprint
            // (the last straight before the bridge); at the scene's own music level.
            var music = go.AddComponent<ChaseMusic>();
            Set(music, "_score", AssetDatabase.LoadAssetAtPath<ChaseScore>(Ch5Foley.ScorePath));
            Set(music, "_volume", new SerializedObject(refs.Audio).FindProperty("_musicVolume").floatValue);
            Set(chase, "_music", music);
            Set(chase, "_pressureAt", new[] { layout.MergeAt, layout.BridgeNear - 70f });

            Set(refs.Chapter, "_forestEdge", course.StartPoint);
            Set(refs.Chapter, "_rainSpot", rainSpot);
            // The town keeps its own NavMesh built at start, now only over the yard and the streets (the forest builds its own).
            if (refs.TownNav != null)
            {
                refs.TownNav.collectObjects = CollectObjects.Volume;
                refs.TownNav.center = new Vector3(46f, 1f, 12f);
                refs.TownNav.size = new Vector3(92f, 8f, 48f);
                EditorUtility.SetDirty(refs.TownNav);
            }
        }

        private static void Cameras(Refs refs, Layout layout, Transform rainSpot)
        {
            Vector3 edge = layout.MoonAt(layout.StartAt - 10f), into = layout.MoonAt(layout.StartAt + 14f);
            Vector3 side = layout.RightAt(layout.StartAt - 10f);
            Look("CAM_Wide_BambooEdge", edge + Vector3.up * 2.3f - side * 2.5f, into + Vector3.up * 1.2f, refs.World);
            Look("CAM_Hero_Kieu_Rain", rainSpot.position + rainSpot.right * 2.1f + rainSpot.forward * -0.9f + Vector3.up * 1.45f, rainSpot.position + Vector3.up * 1.4f, refs.World);
            Vector3 gate = layout.TempleGate;
            Look("CAM_Wide_TempleGate", rainSpot.position - rainSpot.forward * 6f + Vector3.up * 1.7f, gate + Vector3.up * 2.2f, refs.World);
            // Review viewpoints for StoryLookCapture (not used in play): the way a player sees each beat.
            float[] stops = { layout.StartAt + 2f, layout.Of("J1") - 14f, layout.Of("D1") - 10f, layout.ForkAt - 18f, layout.Of("G1") - 12f, layout.MergeAt + 2f, layout.Of("Q1") - 9f, layout.BridgeNear - 30f, layout.BridgeNear - 4f };
            for (int i = 0; i < stops.Length; i++)
            {
                Vector3 at = layout.MoonAt(stops[i]) + Vector3.up * 1.56f;
                Look("LOOK_Forest" + i, at, layout.MoonAt(stops[i] + 12f) + Vector3.up * 1.2f, refs.World);
            }
        }

        private static void Look(string name, Vector3 position, Vector3 target, Transform parent)
        {
            Point(name, position, 0, parent).rotation = Quaternion.LookRotation(target - position);
        }
    }
}

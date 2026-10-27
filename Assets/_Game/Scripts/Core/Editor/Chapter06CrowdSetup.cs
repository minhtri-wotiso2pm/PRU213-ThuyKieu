using System;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using NavigationMesh = UnityEngine.AI.NavMesh;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static class Chapter06CrowdSetup
    {
        private static string Model(int id) => "Assets/_Game/Animations/animationNPC" + id + (id == 2 ? "/Untitled_rigged_walk_interaction.blend" : "/Untitled2_rigged_walk_interaction.blend");

        private static RuntimeAnimatorController Controller(int id)
        {
            string path = Model(id);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing pedestrian model: " + path);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            if (clips.Length != 1) throw new InvalidOperationException("Expected one pedestrian animation per model.");
            if (importer.animationType != ModelImporterAnimationType.Generic || importer.clipAnimations.Length == 0 || !clips[0].loopTime || !clips[0].lockRootPositionXZ || importer.importCameras || importer.importLights)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.importAnimation = true;
                importer.importCameras = importer.importLights = false;
                foreach (var item in clips)
                {
                    item.loopTime = item.loopPose = true;
                    item.lockRootRotation = item.lockRootHeightY = item.lockRootPositionXZ = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
            var walking = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(clip => !clip.name.StartsWith("__preview__"));
            string folder = StoryAnimationSetup.CharacterFolder + "/NPC" + id + "Street";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(folder + ".controller");
            if (controller != null) return controller;
            var idle = new AnimationClip { name = "NPC" + id + "StreetIdle" };
            foreach (var binding in AnimationUtility.GetCurveBindings(walking))
                AnimationUtility.SetEditorCurve(idle, binding, AnimationCurve.Constant(0f, 1f, AnimationUtility.GetEditorCurve(walking, binding).Evaluate(0f)));
            AssetDatabase.CreateAsset(idle, folder + "Idle.anim");
            controller = AnimatorController.CreateAnimatorControllerAtPath(folder + ".controller");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;
            var rest = machine.AddState("Idle"); rest.motion = idle;
            var walk = machine.AddState("Walking"); walk.motion = walking;
            machine.defaultState = rest;
            var start = rest.AddTransition(walk);
            start.hasExitTime = false; start.duration = 0.15f;
            start.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");
            var stop = walk.AddTransition(rest);
            stop.hasExitTime = false; stop.duration = 0.15f;
            stop.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static Vector3[] Route(int index)
        {
            float west = -28f + index * 1.2f, east = 27f - index * 1.1f;
            var route = new[] { new Vector3(west, 0.05f, 16.5f), new Vector3(east, 0.05f, 16.5f),
                new Vector3(east, 0.05f, 19f), new Vector3(west, 0.05f, 19f) };
            if (index % 2 == 1) Array.Reverse(route);
            return route;
        }

        private static void Configure(StoryActor actor, StoryDirector director, int index)
        {
            var route = Route(index);
            actor.transform.position = route[index % route.Length];
            var pedestrian = actor.GetComponent<StreetPedestrian>();
            if (pedestrian == null) pedestrian = actor.gameObject.AddComponent<StreetPedestrian>();
            Set(pedestrian, "_director", director);
            var data = new SerializedObject(pedestrian);
            var points = data.FindProperty("_route");
            points.arraySize = route.Length;
            for (int i = 0; i < route.Length; i++) points.GetArrayElementAtIndex(i).vector3Value = route[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            Set(pedestrian, "_startIndex", index % route.Length);
            Set(pedestrian, "_speed", 1f + index % 3 * 0.1f);
            var agent = actor.GetComponent<NavMeshAgent>();
            agent.radius = 0.25f;
            agent.avoidancePriority = 55 + index;
        }

        internal static void Create(Transform world, StoryDirector director)
        {
            for (int i = 0; i < 7; i++)
            {
                int id = i % 2 == 0 ? 2 : 4;
                var actor = RiggedActor("NguoiDan" + i, "NguoiDan", Model(id), "Assets/_Game/Art/Characters/NPC" + id,
                    id == 2 ? 1.58f : 1.65f, Route(i)[i % 4], 90f, Color.white, Controller(id), false);
                actor.transform.SetParent(world, true);
                Configure(actor, director, i);
            }
        }

        [MenuItem("ThuyKieu/Chapter 6/Use NPC2 and NPC4 pedestrians")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            var scene = SceneManager.GetSceneByPath(Chapter06Setup.ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(Chapter06Setup.ScenePath, OpenSceneMode.Additive);
            var roots = scene.GetRootGameObjects();
            var director = roots.SelectMany(root => root.GetComponentsInChildren<StoryDirector>(true)).Single();
            var crowd = roots.SelectMany(root => root.GetComponentsInChildren<StoryActor>(true)).Where(actor => actor.SpeakerId == "NguoiDan").OrderBy(actor => actor.name).ToArray();
            if (crowd.Length != 7) throw new InvalidOperationException("Expected seven street extras.");
            for (int i = 0; i < crowd.Length; i++)
            {
                int id = i % 2 == 0 ? 2 : 4;
                var actor = crowd[i];
                string name = "StreetNPC" + id;
                var visual = actor.transform.Find(name);
                if (visual == null)
                {
                    var data = new SerializedObject(actor);
                    var oldAnimator = (Animator)data.FindProperty("_animator").objectReferenceValue;
                    var body = Body(actor.transform, name, Model(id), "Assets/_Game/Art/Characters/NPC" + id,
                        id == 2 ? 1.58f : 1.65f, Color.white, Controller(id), false);
                    var animator = body.GetComponentInChildren<Animator>(true);
                    if (animator == null || body.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0) throw new InvalidOperationException("Missing pedestrian rig.");
                    if (oldAnimator != null) oldAnimator.gameObject.SetActive(false);
                    body.SetActive(true);
                    data.FindProperty("_animator").objectReferenceValue = animator;
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
                Configure(actor, director, i);
            }
            Validate(scene, crowd);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Chapter 6 pedestrians.");
            AssetDatabase.SaveAssets();
            Debug.Log("Chapter06 pedestrians applied: seven NPC2/NPC4 walkers, connected street routes, pause during dialogue.");
        }

        private static void Validate(Scene scene, StoryActor[] crowd)
        {
            var nav = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>(true)).Single();
            nav.BuildNavMesh();
            for (int i = 0; i < crowd.Length; i++)
            {
                var route = Route(i);
                for (int j = 0; j < route.Length; j++)
                {
                    var path = new NavMeshPath();
                    if (!NavigationMesh.SamplePosition(route[j], out var from, 2f, NavigationMesh.AllAreas)
                        || !NavigationMesh.SamplePosition(route[(j + 1) % route.Length], out var to, 2f, NavigationMesh.AllAreas)
                        || !NavigationMesh.CalculatePath(from.position, to.position, NavigationMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                        throw new InvalidOperationException("Pedestrian route is blocked: " + crowd[i].name + " segment " + j);
                }
            }
            Debug.Log("Pedestrian navigation checks passed: all 28 route segments are connected.");
            foreach (int id in new[] { 2, 4 }) ValidateAnimation(id);
        }

        private static void ValidateAnimation(int id)
        {
            var model = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model(id)));
            var baked = new Mesh();
            try
            {
                var skin = model.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skin == null || skin.bones.Length == 0 || skin.bones.Any(bone => bone == null)) throw new InvalidOperationException("Missing NPC" + id + " skin weights.");
                var clip = AssetDatabase.LoadAllAssetsAtPath(Model(id)).OfType<AnimationClip>().Single(item => !item.name.StartsWith("__preview__"));
                clip.SampleAnimation(model, 0f);
                skin.BakeMesh(baked);
                var first = baked.vertices;
                clip.SampleAnimation(model, clip.length * 0.35f);
                skin.BakeMesh(baked);
                var second = baked.vertices;
                float motion = 0f;
                for (int i = 0; i < first.Length; i++) motion = Mathf.Max(motion, (first[i] - second[i]).magnitude);
                if (motion < 0.0001f) throw new InvalidOperationException("NPC" + id + " walking animation does not deform its model.");
                Debug.Log("NPC" + id + " walking animation check passed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(baked); UnityEngine.Object.DestroyImmediate(model); }
        }
    }
}

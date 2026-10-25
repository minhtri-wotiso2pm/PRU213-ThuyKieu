using System;
using System.Linq;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Gameplay.Sequences;
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
    public static class Chapter05DogSetup
    {
        private const string Art = "Assets/_Game/Art/Characters/concho/";
        private const string Animations = "Assets/_Game/Animations/animationconcho/";
        private const string MeshPath = Art + "ConChoSkinned.asset";
        private const string ControllerPath = Animations + "ConCho.controller";

        private static AnimationClip Clip(string name)
        {
            string path = Animations + name + ".blend";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            if (importer.animationType != ModelImporterAnimationType.Generic || importer.clipAnimations.Length == 0 || !clips[0].loopTime)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                foreach (var clip in clips)
                {
                    clip.loopTime = clip.loopPose = true;
                    clip.lockRootRotation = clip.lockRootHeightY = clip.lockRootPositionXZ = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(clip => !clip.name.StartsWith("__preview__"));
        }

        private static AnimatorController Controller()
        {
            var walk = Clip("Dog_Walk");
            var run = Clip("Dog_Run");
            string idlePath = Animations + "ConChoIdle.anim";
            var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
            if (idle == null)
            {
                idle = new AnimationClip { name = "ConChoIdle" };
                foreach (var binding in AnimationUtility.GetCurveBindings(walk))
                {
                    float value = AnimationUtility.GetEditorCurve(walk, binding).Evaluate(0f);
                    AnimationUtility.SetEditorCurve(idle, binding, AnimationCurve.Constant(0f, 1f, value));
                }
                AssetDatabase.CreateAsset(idle, idlePath);
            }
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }
            if (!controller.parameters.Any(parameter => parameter.name == "Speed")) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(item => item.state).FirstOrDefault(item => item.name == "Locomotion") ?? machine.AddState("Locomotion");
            var blend = state.motion as BlendTree;
            if (blend == null)
            {
                blend = new BlendTree { name = "DogLocomotion", blendParameter = "Speed", blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
                AssetDatabase.AddObjectToAsset(blend, controller);
                state.motion = blend;
            }
            blend.children = new[] { new ChildMotion { motion = idle, threshold = 0f, timeScale = 1f },
                new ChildMotion { motion = walk, threshold = 1.5f, timeScale = 1f }, new ChildMotion { motion = run, threshold = 3.2f, timeScale = 1f } };
            controller.layers[0].stateMachine.defaultState = state;
            EditorUtility.SetDirty(blend);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        // Transfer the supplied dog's surface to the animation rig, preserving its UVs and texture.
        private static Mesh Skin(GameObject rig, SkinnedMeshRenderer skin)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (existing != null) return existing;
            var source = skin.sharedMesh;
            var art = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "6724082f954391ac7c2cb36dfb1d9792.obj");
            var filter = art.GetComponentInChildren<MeshFilter>();
            var mesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
            mesh.name = "ConChoSkinned";
            var sourceToRoot = rig.transform.worldToLocalMatrix * skin.transform.localToWorldMatrix;
            var artToRoot = art.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var sourcePoints = source.vertices.Select(sourceToRoot.MultiplyPoint3x4).ToArray();
            var points = mesh.vertices.Select(artToRoot.MultiplyPoint3x4).ToArray();
            Bounds sourceBounds = new Bounds(sourcePoints[0], Vector3.zero), targetBounds = new Bounds(points[0], Vector3.zero);
            foreach (var point in sourcePoints) sourceBounds.Encapsulate(point);
            foreach (var point in points) targetBounds.Encapsulate(point);
            var scale = new Vector3(sourceBounds.size.x / targetBounds.size.x, sourceBounds.size.y / targetBounds.size.y, sourceBounds.size.z / targetBounds.size.z);
            var rootToSource = sourceToRoot.inverse;
            var weights = source.boneWeights;
            var lookup = new NearestVertices(sourcePoints);
            var transferred = new BoneWeight[points.Length];
            try
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if (i % 4096 == 0 && EditorUtility.DisplayCancelableProgressBar("Prepare dog model", "Transferring skin weights", (float)i / points.Length))
                        throw new OperationCanceledException("Dog skin preparation cancelled.");
                    Vector3 point = sourceBounds.center + Vector3.Scale(points[i] - targetBounds.center, scale);
                    lookup.Find(point, out int a, out int b, out int c);
                    float da = (sourcePoints[a] - point).sqrMagnitude;
                    float db = (sourcePoints[b] - point).sqrMagnitude;
                    float dc = (sourcePoints[c] - point).sqrMagnitude;
                    var influence = new Dictionary<int, float>();
                    Add(weights[a], 1f / Mathf.Max(da, 0.00001f));
                    Add(weights[b], 1f / Mathf.Max(db, 0.00001f));
                    Add(weights[c], 1f / Mathf.Max(dc, 0.00001f));
                    var top = influence.OrderByDescending(pair => pair.Value).Take(4).ToArray();
                    float total = top.Sum(pair => pair.Value);
                    var result = new BoneWeight();
                    for (int k = 0; k < top.Length; k++)
                    {
                        float value = top[k].Value / total;
                        switch (k)
                        {
                            case 0: result.boneIndex0 = top[k].Key; result.weight0 = value; break;
                            case 1: result.boneIndex1 = top[k].Key; result.weight1 = value; break;
                            case 2: result.boneIndex2 = top[k].Key; result.weight2 = value; break;
                            case 3: result.boneIndex3 = top[k].Key; result.weight3 = value; break;
                        }
                    }
                    transferred[i] = result;
                    points[i] = rootToSource.MultiplyPoint3x4(point);
                    void Add(BoneWeight weight, float factor)
                    {
                        Accumulate(weight.boneIndex0, weight.weight0 * factor);
                        Accumulate(weight.boneIndex1, weight.weight1 * factor);
                        Accumulate(weight.boneIndex2, weight.weight2 * factor);
                        Accumulate(weight.boneIndex3, weight.weight3 * factor);
                    }
                    void Accumulate(int index, float amount)
                    {
                        if (amount <= 0f) return;
                        influence[index] = influence.TryGetValue(index, out float value) ? value + amount : amount;
                    }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            mesh.vertices = points;
            mesh.boneWeights = transferred;
            mesh.bindposes = source.bindposes;
            mesh.RecalculateBounds();
            if (mesh.normals.Length == points.Length)
            {
                var normalTransform = (rootToSource * Matrix4x4.Scale(scale) * artToRoot).inverse.transpose;
                mesh.normals = mesh.normals.Select(normal => normalTransform.MultiplyVector(normal).normalized).ToArray();
            }
            else mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            AssetDatabase.CreateAsset(mesh, MeshPath);
            return mesh;
        }

        private sealed class NearestVertices
        {
            private readonly Vector3[] _points;
            private readonly int[] _order, _left, _right;
            private readonly IComparer<int>[] _comparers;
            private readonly int _root;

            public NearestVertices(Vector3[] points)
            {
                _points = points;
                _order = Enumerable.Range(0, points.Length).ToArray();
                _left = new int[points.Length]; _right = new int[points.Length];
                _comparers = Enumerable.Range(0, 3).Select(axis => Comparer<int>.Create((a, b) => points[a][axis].CompareTo(points[b][axis]))).ToArray();
                _root = Build(0, points.Length, 0);
            }

            private int Build(int start, int count, int depth)
            {
                if (count == 0) return -1;
                Array.Sort(_order, start, count, _comparers[depth % 3]);
                int middle = start + count / 2;
                _left[middle] = Build(start, middle - start, depth + 1);
                _right[middle] = Build(middle + 1, start + count - middle - 1, depth + 1);
                return middle;
            }

            public void Find(Vector3 target, out int first, out int second, out int third)
            {
                int a = 0, b = 0, c = 0;
                float da = float.MaxValue, db = float.MaxValue, dc = float.MaxValue;
                Search(_root, 0);
                first = a; second = b; third = c;
                void Search(int node, int depth)
                {
                    if (node < 0) return;
                    int index = _order[node];
                    float distance = (_points[index] - target).sqrMagnitude;
                    if (distance < da) { dc = db; c = b; db = da; b = a; da = distance; a = index; }
                    else if (distance < db) { dc = db; c = b; db = distance; b = index; }
                    else if (distance < dc) { dc = distance; c = index; }
                    float delta = target[depth % 3] - _points[index][depth % 3];
                    Search(delta < 0 ? _left[node] : _right[node], depth + 1);
                    if (delta * delta < dc) Search(delta < 0 ? _right[node] : _left[node], depth + 1);
                }
            }
        }

        internal static DogPursuer ConfigureDog(DogSentry sentry)
        {
            var root = sentry.transform;
            var dog = root.GetComponent<DogPursuer>();
            var controller = Controller();
            if (dog != null) return dog;
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Animations + "Dog_Run.blend"));
            visual.name = "ConChoVisual";
            var skin = visual.GetComponentInChildren<SkinnedMeshRenderer>();
            try { skin.sharedMesh = Skin(visual, skin); }
            catch { UnityEngine.Object.DestroyImmediate(visual); throw; }
            skin.localBounds = skin.sharedMesh.bounds;
            skin.updateWhenOffscreen = true;
            foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>()) renderer.enabled = false;
            var material = Material("ConChoStory", Color.white);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "texture_pbr_20250901.png"));
            material.SetFloat("_Smoothness", 0.25f);
            skin.sharedMaterials = Enumerable.Repeat(material, skin.sharedMesh.subMeshCount).ToArray();
            Bounds bounds = skin.bounds;
            float scale = 0.8f / bounds.size.y;
            visual.transform.localScale *= scale;
            visual.transform.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
            foreach (Transform child in root) child.gameObject.SetActive(false);
            visual.transform.SetParent(root, false);
            var animator = visual.GetComponent<Animator>();
            if (animator == null) animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var agent = root.gameObject.AddComponent<NavMeshAgent>();
            agent.radius = 0.22f; agent.height = 0.8f; agent.baseOffset = 0f;
            agent.speed = 3.8f; agent.angularSpeed = 300f; agent.acceleration = 8f; agent.stoppingDistance = 0.4f;
            agent.enabled = false;
            dog = root.gameObject.AddComponent<DogPursuer>();
            Set(dog, "_animator", animator);
            var data = new SerializedObject(sentry);
            Set(dog, "_audio", data.FindProperty("_audio").objectReferenceValue);
            Set(sentry, "_head", visual.transform);
            return dog;
        }

        [MenuItem("ThuyKieu/Chapter 5/Use dog model and chase")]
        public static void Apply()
        {
            var scene = SceneManager.GetSceneByPath(Chapter05Setup.ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(Chapter05Setup.ScenePath, OpenSceneMode.Additive);
            var sentry = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DogSentry>(true)).Single();
            var dog = ConfigureDog(sentry);
            foreach (var sequence in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StorySequence>(true)))
                if (sequence is ChaseSequence || sequence is ForestRunSequence)
                {
                    Set(sequence, "_dog", dog);
                    Set(sequence, "_pursuitSpeedMultiplier", 0.85f);
                }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Chapter 5 dog.");
            AssetDatabase.SaveAssets();
            ValidateDog(dog, scene);
            Debug.Log("Chapter05 dog applied: requested OBJ skinned to generic rig, walk/run clips, alley and forest chase; guards 15% slower.");
        }

        private static void ValidateDog(DogPursuer dog, Scene scene)
        {
            var nav = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>(true)).Single();
            nav.BuildNavMesh();
            var chase = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ChaseSequence>(true)).Single();
            var data = new SerializedObject(chase);
            var starts = data.FindProperty("_pursuerStarts");
            var finish = (BoxCollider)data.FindProperty("_finish").objectReferenceValue;
            for (int i = 0; i < starts.arraySize; i++)
            {
                var start = (Transform)starts.GetArrayElementAtIndex(i).objectReferenceValue;
                var path = new NavMeshPath();
                if (!NavigationMesh.SamplePosition(start.position, out var from, 3f, NavigationMesh.AllAreas)
                    || !NavigationMesh.SamplePosition(finish.bounds.center, out var to, 5f, NavigationMesh.AllAreas)
                    || !NavigationMesh.CalculatePath(from.position, to.position, NavigationMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Incomplete escape navigation path from " + start.name);
            }
            var forest = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ForestRunSequence>(true)).Single();
            var forestData = new SerializedObject(forest);
            var rails = forestData.FindProperty("_rail");
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(ForestRunSequence).GetMethod("Measure", flags).Invoke(forest, null);
            var spawnAt = typeof(ForestRunSequence).GetMethod("PursuerStart", flags);
            float gap = forestData.FindProperty("_startGap").floatValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 start = (Vector3)spawnAt.Invoke(forest, new object[] { -gap - (i < 3 ? i * 1.3f : 0f), i == 3 ? 0.8f : 0f });
                for (int j = 0; j < rails.arraySize; j++)
                {
                    var waypoint = (Transform)rails.GetArrayElementAtIndex(j).objectReferenceValue;
                    var path = new NavMeshPath();
                    if (!NavigationMesh.SamplePosition(start, out var from, 3f, NavigationMesh.AllAreas)
                        || !NavigationMesh.SamplePosition(waypoint.position, out var to, 2f, NavigationMesh.AllAreas)
                        || !NavigationMesh.CalculatePath(from.position, to.position, NavigationMesh.AllAreas, path)
                        || path.status != NavMeshPathStatus.PathComplete)
                        throw new InvalidOperationException("Forest chase path blocked for pursuer " + i + " at waypoint " + j);
                }
            }
            Debug.Log("Forest chase validation passed: guards and dog reach all forest waypoints from their spawn positions.");
            var preview = new PreviewRenderUtility();
            try
            {
                var clone = UnityEngine.Object.Instantiate(dog.transform.Find("ConChoVisual").gameObject);
                clone.transform.position = Vector3.zero;
                clone.transform.rotation = Quaternion.identity;
                preview.AddSingleGO(clone);
                var skin = clone.GetComponentInChildren<SkinnedMeshRenderer>();
                var clip = Clip("Dog_Run");
                var baked = new Mesh();
                clip.SampleAnimation(clone, 0f);
                skin.BakeMesh(baked);
                var first = baked.vertices;
                clip.SampleAnimation(clone, clip.length * 0.35f);
                skin.BakeMesh(baked);
                var second = baked.vertices;
                float movement = 0f;
                for (int i = 0; i < first.Length; i++) movement = Mathf.Max(movement, (first[i] - second[i]).magnitude);
                if (movement < 0.001f) throw new InvalidOperationException("Dog run clip does not deform the supplied model.");
                var bounds = skin.bounds;
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 100f;
                preview.camera.fieldOfView = 30f;
                preview.camera.transform.position = bounds.center + new Vector3(1.6f, 0.9f, 1.8f) * bounds.size.magnitude;
                preview.camera.transform.LookAt(bounds.center);
                preview.lights[0].intensity = 1.3f;
                preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                preview.lights[1].intensity = 0.7f;
                preview.BeginStaticPreview(new Rect(0, 0, 512, 512));
                preview.Render(true);
                var picture = preview.EndStaticPreview();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ThuyKieu-dog-skinned.png"), picture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(baked);
                UnityEngine.Object.DestroyImmediate(picture);
                Debug.Log("Dog validation passed: 3 complete escape paths; skinned run pose changes; preview captured.");
            }
            finally { preview.Cleanup(); }
        }
    }
}

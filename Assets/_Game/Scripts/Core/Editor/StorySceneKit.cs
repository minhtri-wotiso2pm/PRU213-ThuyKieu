using System;
using System.IO;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue;
using ThuyKieu.Dialogue.Editor;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

namespace ThuyKieu.Core.Editor
{
    /// <summary>Shared editor helpers that assemble story chapter scenes from code.</summary>
    public static class StorySceneKit
    {
        public const int EnvironmentLayer = 8;
        public const string UIPrefabPath = "Assets/_Game/Prefabs/UI/StoryCanvas.prefab";
        public const string MaterialFolder = "Assets/_Game/Materials/Story";
        /// <summary>Where generated materials, meshes and textures go. The Chapter 1 upgrade points it at its own folder,
        /// because the story folder is deleted on every Chapter 2/3 rebuild.</summary>
        public static string AssetFolder { get; set; } = MaterialFolder;
        public const string FontPath = "Assets/_Game/UI/Fonts/ChapterVietnamese.asset";

        public struct Systems
        {
            public GameObject Root;
            public DialogueManager Dialogue;
            public StoryDirector Director;
            public PlayerMovement Player;
            public PlayerInteraction Interaction;
            public ThirdPersonCameraController Camera;
            public Camera CameraComponent;
            public ObserveMode Observe;
            public StoryHUD Hud;
            public GameObject Canvas;
            public DialogueCameraRig Shots;
            public ScreenFx Fx;
            public StoryAudio Audio;
        }

        // ------------------------------------------------------------------ scene

        public static UnityEngine.SceneManagement.Scene NewScene(string path)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before scene setup.");
            if (File.Exists(path)) throw new InvalidOperationException(path + " exists; use targeted edits instead of replacing it.");
            InkStoryCompiler.Compile();
            PrepareFont();
            Directory.CreateDirectory(MaterialFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            return scene;
        }

        public static void Save(UnityEngine.SceneManagement.Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Player, camera, dialogue, director, UI and presentation systems wired together.</summary>
        public static Systems CreateSystems(int chapter, string title, string entryKnot, string nextScene, Vector3 playerPosition, float playerYaw, string ambience)
        {
            var s = new Systems();
            s.Root = new GameObject("StorySystems");
            s.Dialogue = s.Root.AddComponent<DialogueManager>();
            s.Director = s.Root.AddComponent<StoryDirector>();
            var autopilot = s.Root.AddComponent<StoryAutopilot>();
            Set(autopilot, "_director", s.Director);
            s.Root.AddComponent<DialogueCursorController>();
            new GameObject("InkSession").AddComponent<InkStatePersistence>();

            var playerObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player/ThuyKieu.prefab"));
            playerObject.name = "ThuyKieu";
            playerObject.transform.SetPositionAndRotation(playerPosition, Quaternion.Euler(0, playerYaw, 0));
            s.Player = playerObject.GetComponent<PlayerMovement>();
            s.Interaction = playerObject.GetComponent<PlayerInteraction>();
            if (s.Interaction == null) s.Interaction = playerObject.AddComponent<PlayerInteraction>();
            var playerAnimator = playerObject.GetComponentInChildren<Animator>();
            playerAnimator.runtimeAnimatorController = StoryAnimationSetup.KieuController();
            playerAnimator.applyRootMotion = false;
            var kieuActor = playerObject.AddComponent<StoryActor>();
            Set(kieuActor, "_speakerId", "Kieu");
            Set(kieuActor, "_animator", playerAnimator);
            Set(kieuActor, "_headHeight", 1.5f);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            s.CameraComponent = cameraObject.AddComponent<Camera>();
            s.CameraComponent.clearFlags = CameraClearFlags.SolidColor;
            s.CameraComponent.nearClipPlane = 0.08f;
            s.CameraComponent.farClipPlane = 260f;
            s.CameraComponent.fieldOfView = 58f;
            var urp = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            urp.renderPostProcessing = true;
            urp.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.SetPositionAndRotation(playerPosition + Quaternion.Euler(0, playerYaw, 0) * new Vector3(0, 2.4f, -3.8f), Quaternion.Euler(14, playerYaw, 0));
            s.Camera = cameraObject.AddComponent<ThirdPersonCameraController>();
            Set(s.Camera, "target", playerObject.transform.Find("CameraTarget"));
            Set(s.Camera, "distance", 3.4f);
            Set(s.Camera, "_shoulder", 0.42f);
            Set(s.Camera, "_fov", 55f);
            Set(s.Camera, "_dialogueFov", 40f);
            Set(s.Camera, "minPitch", -25f);
            Set(s.Camera, "maxPitch", 55f);
            Set(s.Camera, "followSmoothTime", 0.08f);
            Set(s.Camera, "_collisionMask", 1 << EnvironmentLayer);
            Set(s.Player, "cameraTransform", cameraObject.transform);
            s.Fx = cameraObject.AddComponent<ScreenFx>();
            Set(s.Fx, "_camera", s.CameraComponent);

            var canvas = (GameObject)PrefabUtility.InstantiatePrefab(UIPrefab());
            s.Canvas = canvas;
            s.Hud = canvas.GetComponent<StoryHUD>();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            s.Observe = cameraObject.AddComponent<ObserveMode>();
            Set(s.Observe, "_camera", s.CameraComponent);
            Set(s.Observe, "_player", s.Player);
            Set(s.Observe, "_hud", s.Hud);

            var shotsObject = new GameObject("DialogueCameraRig");
            shotsObject.transform.SetParent(s.Root.transform, false);
            s.Shots = shotsObject.AddComponent<DialogueCameraRig>();
            Set(s.Shots, "_director", s.Director);
            Set(s.Shots, "_camera", s.Camera);
            Set(s.Shots, "_obstruction", 1 << EnvironmentLayer);
            Set(s.Shots, "_lens", 1.45f);

            var audioObject = new GameObject("StoryAudio");
            audioObject.transform.SetParent(s.Root.transform, false);
            s.Audio = audioObject.AddComponent<StoryAudio>();
            Set(s.Audio, "_player", s.Player);
            Set(s.Audio, "_startAmbience", ambience);

            Set(s.Director, "_compiledInk", AssetDatabase.LoadAssetAtPath<TextAsset>(InkStoryCompiler.Output));
            Set(s.Director, "_chapterNumber", chapter);
            Set(s.Director, "_chapterTitle", title);
            Set(s.Director, "_entryKnot", entryKnot);
            Set(s.Director, "_nextScene", nextScene);
            Set(s.Director, "_dialogue", s.Dialogue);
            Set(s.Director, "_player", s.Player);
            Set(s.Director, "_interaction", s.Interaction);
            Set(s.Director, "_camera", s.Camera);
            Set(s.Director, "_hud", s.Hud);
            Set(s.Director, "_shots", s.Shots);
            Set(s.Director, "_fx", s.Fx);
            Set(s.Director, "_audio", s.Audio);
            return s;
        }

        public static NavMeshSurface NavMesh(Transform parent)
        {
            var go = new GameObject("NavMeshSurface");
            go.transform.SetParent(parent, false);
            var surface = go.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << EnvironmentLayer;
            return surface;
        }

        public static GameObject UIPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath);
            if (existing != null) return existing;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var root = StoryUIBuilder.Build(font);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, UIPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        // The shared TMP font is dynamic: glyphs are added at runtime, so the asset is only checked, never rewritten.
        private static void PrepareFont()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath) == null)
                throw new InvalidOperationException("Missing " + FontPath + " (created by the Chapter 1 setup).");
        }

        // ------------------------------------------------------------------ characters

        /// <summary>
        /// Skinned bounds are padded, so <see cref="Fit"/> can leave a standing body hovering 0.1–0.2 m: moves
        /// <paramref name="mover"/> down (or up) until the lowest baked vertex of <paramref name="model"/> meets the floor.
        /// </summary>
        public static void GroundFeet(Transform mover, GameObject model, float floorY)
        {
            float lowest = float.MaxValue;
            var mesh = new Mesh();
            foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.BakeMesh(mesh, true);
                foreach (var vertex in mesh.vertices) lowest = Mathf.Min(lowest, skinned.transform.TransformPoint(vertex).y);
            }
            UnityEngine.Object.DestroyImmediate(mesh);
            if (lowest < float.MaxValue) mover.position += Vector3.up * (floorY - lowest);
        }

        public static StoryActor RiggedActor(string name, string speakerId, string fbxPath, string textureFolder, float height, Vector3 position, float yaw, Color tint, RuntimeAnimatorController controller, bool humanoid = true)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (humanoid && importer != null && importer.animationType != ModelImporterAnimationType.Human)
            { importer.animationType = ModelImporterAnimationType.Human; importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.SaveAndReimport(); }
            var root = new GameObject(name);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath));
            model.transform.SetParent(root.transform, false);
            Fit(model, height);
            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null && !humanoid) animator = model.AddComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            ApplyTexture(model, name, textureFolder, tint);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            var actor = FinishActor(root, speakerId, animator, null, height);
            return actor;
        }

        /// <summary>
        /// A character with its own Mixamo rig and clips: Art/Characters/&lt;id&gt;/Rigged/&lt;id&gt;_Rigged.fbx with its texture,
        /// animated by the shared story controller overridden with Animations/&lt;id&gt; clips.
        /// </summary>
        public static StoryActor CharacterActor(string name, string speakerId, string id, float height, Vector3 position, float yaw)
        {
            string folder = "Assets/_Game/Art/Characters/" + id + "/Rigged";
            return RiggedActor(name, speakerId, folder + "/" + id + "_Rigged.fbx", folder, height, position, yaw, Color.white, StoryAnimationSetup.CharacterController(id));
        }

        /// <summary>
        /// A second, swappable body under an existing character root (the modern student inside Kiều's player object):
        /// the rigged model fitted to height, tinted and animated by the given controller. Starts hidden.
        /// </summary>
        public static GameObject Body(Transform root, string name, string fbxPath, string textureFolder, float height, Color tint, RuntimeAnimatorController controller, bool humanoid = true)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (humanoid && importer != null && importer.animationType != ModelImporterAnimationType.Human)
            { importer.animationType = ModelImporterAnimationType.Human; importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.SaveAndReimport(); }
            // Fit at the world origin (Fit works in world space), then hang the body under its root, cancelling any
            // scale on the root so the body keeps its fitted height.
            var holder = new GameObject(name);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath));
            model.transform.SetParent(holder.transform, false);
            Fit(model, height);
            // Humanoid playback already puts the feet on the Animator's origin; Fit's lift (from the bind-pose mesh
            // bottom) would leave the body floating above the CharacterController.
            var lifted = model.transform.localPosition;
            if (humanoid) model.transform.localPosition = new Vector3(lifted.x, 0f, lifted.z);
            holder.transform.SetParent(root, false);
            Vector3 rootScale = root.lossyScale;
            holder.transform.localScale = new Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z);
            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null && !humanoid) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            ApplyTexture(model, name, textureFolder, tint);
            holder.SetActive(false);
            return holder;
        }

        public static StoryActor StaticActor(string name, string speakerId, string objPath, string texturePath, float height, Vector3 position, float yaw, float relaxArms = 0f)
        {
            var importer = AssetImporter.GetAtPath(objPath) as ModelImporter;
            if (importer != null && (importer.materialImportMode != ModelImporterMaterialImportMode.None || importer.importAnimation))
            { importer.materialImportMode = ModelImporterMaterialImportMode.None; importer.importAnimation = false; importer.SaveAndReimport(); }
            var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter != null && textureImporter.maxTextureSize > 2048) { textureImporter.maxTextureSize = 2048; textureImporter.SaveAndReimport(); }
            var root = new GameObject(name);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(objPath));
            model.transform.SetParent(visual.transform, false);
            if (relaxArms > 0f) RelaxArms(model, AssetFolder + "/" + name + "_Relaxed.asset", relaxArms);
            Fit(model, height);
            var material = Material(name + "Static", Color.white);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", 0.25f);
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            return FinishActor(root, speakerId, null, visual.transform, height);
        }

        /// <summary>
        /// A modern-day character from <see cref="ModernCharacterImport"/> (static posed mesh, own material with normal map).
        /// <paramref name="seat"/> set: the model is seated, so it is placed with its hips on the bench behind that seat point
        /// (bench top <paramref name="benchHeight"/>, <paramref name="benchBehind"/> metres behind) instead of standing on it.
        /// </summary>
        public static StoryActor ModernActor(string name, string speakerId, string model, float height, Vector3 position, float yaw, Color tint,
            bool seat = false, float benchHeight = 0.46f, float benchBehind = 0.3f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModernCharacterImport.PrefabPath(model));
            if (prefab == null) throw new System.IO.FileNotFoundException("Run ThuyKieu > Tools > Import modern characters first", ModernCharacterImport.PrefabPath(model));
            var root = new GameObject(name);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(visual.transform, false);
            Fit(instance, height);
            if (tint != Color.white)
            {
                var renderer = instance.GetComponentInChildren<Renderer>();
                string path = AssetFolder + "/" + model + "_" + ColorUtility.ToHtmlStringRGB(tint) + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(renderer.sharedMaterial); material.SetColor("_BaseColor", tint); AssetDatabase.CreateAsset(material, path); }
                renderer.sharedMaterial = material;
            }
            if (seat)
            {
                // Hips = the lowest point of the back third of the seated mesh; slide them onto the bench.
                var mesh = instance.GetComponentInChildren<MeshFilter>().sharedMesh;
                var b = mesh.bounds;
                float hips = b.max.y, back = b.min.z + b.size.z * 0.3f;
                foreach (var v in mesh.vertices) if (v.z < back && v.y > b.min.y + b.size.y * 0.15f) hips = Mathf.Min(hips, v.y);
                float scale = height / b.size.y;
                float lift = Mathf.Clamp(benchHeight - (hips - b.min.y) * scale, -0.06f, 0.14f);
                float hipsForward = (b.min.z + b.size.z * 0.15f - b.center.z) * scale;
                visual.transform.localPosition = new Vector3(0f, lift, -benchBehind - hipsForward);
            }
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            var actor = FinishActor(root, speakerId, null, visual.transform, height);
            // A seated pose never walks; without this the agent would snap the actor onto the nearest NavMesh (a desk top).
            if (seat) UnityEngine.Object.DestroyImmediate(root.GetComponent<NavMeshAgent>());
            return actor;
        }


        /// <summary>
        /// Placeholder posing for unrigged T/A-pose meshes: rotates everything outside the shoulders down
        /// around the shoulder joints (smoothly blended) so the arms hang. Saved as a separate mesh asset;
        /// the source model is untouched. Replace with a rigged model when available.
        /// </summary>
        private const string RelaxedVersion = "_v2"; // bump to regenerate cached relaxed meshes

        private static void RelaxArms(GameObject model, string assetPath, float angle)
        {
            var filters = model.GetComponentsInChildren<MeshFilter>();
            if (filters.Length == 0) return;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing != null && existing.name.EndsWith(RelaxedVersion)) { foreach (var f in filters) f.sharedMesh = existing; return; }
            var source = filters[0].sharedMesh;
            var vertices = source.vertices;
            // Rotate the source normals with their vertices. Recalculating them instead splits shading along every
            // UV seam (a dark line down the face).
            var normals = source.normals;
            bool hasNormals = normals != null && normals.Length == vertices.Length;
            var bounds = source.bounds;
            float height = bounds.size.y;
            float cx = bounds.center.x;
            float shoulderY = bounds.min.y + height * 0.8f;
            float shoulder = height * 0.13f;
            float blend = height * 0.07f;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                float side = Mathf.Sign(v.x - cx);
                float lateral = Mathf.Abs(v.x - cx);
                if (lateral <= shoulder || v.y < bounds.min.y + height * 0.45f) continue;
                float weight = Mathf.SmoothStep(0f, 1f, (lateral - shoulder) / blend);
                var pivot = new Vector3(cx + side * shoulder, shoulderY, v.z);
                var turn = Quaternion.Euler(0, 0, -side * angle * weight);
                vertices[i] = turn * (v - pivot) + pivot;
                if (hasNormals) normals[i] = turn * normals[i];
            }
            var mesh = UnityEngine.Object.Instantiate(source);
            mesh.name = System.IO.Path.GetFileNameWithoutExtension(assetPath) + RelaxedVersion;
            mesh.vertices = vertices;
            if (hasNormals) mesh.normals = normals; else mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (existing != null)
            {
                // Keep the asset (and its GUID) and replace its data.
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            else AssetDatabase.CreateAsset(mesh, assetPath);
            foreach (var f in filters) f.sharedMesh = mesh;
        }

        private static StoryActor FinishActor(GameObject root, string speakerId, Animator animator, Transform staticVisual, float height)
        {
            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = height; collider.center = Vector3.up * height / 2; collider.radius = 0.3f;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.3f; agent.height = height; agent.speed = 1.5f; agent.angularSpeed = 300f; agent.acceleration = 6f;
            agent.stoppingDistance = 0.2f; agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            agent.enabled = false;
            var actor = root.AddComponent<StoryActor>();
            Set(actor, "_speakerId", speakerId);
            Set(actor, "_animator", animator);
            Set(actor, "_staticVisual", staticVisual);
            Set(actor, "_headHeight", height * 0.92f);
            return actor;
        }

        private static void Fit(GameObject model, float height)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            float scale = height / bounds.size.y;
            model.transform.localScale *= scale;
            model.transform.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
        }

        private static void ApplyTexture(GameObject model, string name, string textureFolder, Color tint)
        {
            string texturePath = textureFolder + "/texture_pbr_20250901.png";
            if (AssetImporter.GetAtPath(texturePath) is TextureImporter textureImporter && textureImporter.maxTextureSize > 2048) { textureImporter.maxTextureSize = 2048; textureImporter.SaveAndReimport(); }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var material = Material(name + "Story", tint);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", 0.25f);
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }

        // ------------------------------------------------------------------ interaction

        /// <summary>Makes an unscaled anchor (or an actor) resolve an #explore choice. Anchors get a trigger volume.</summary>
        public static ExploreTarget Explore(GameObject go, StoryDirector director, string id, string label, Vector3 triggerSize, bool startHidden = false)
        {
            if (go.GetComponent<StoryActor>() == null)
            {
                var box = go.GetComponent<BoxCollider>();
                if (box == null) box = go.AddComponent<BoxCollider>();
                go.layer = 2; // Ignore Raycast: interaction volumes never block cameras or sight lines.
                box.isTrigger = true;
                box.size = triggerSize;
                box.center = Vector3.up * triggerSize.y * 0.5f;
            }
            var target = go.AddComponent<ExploreTarget>();
            Set(target, "_director", director);
            Set(target, "_exploreId", id);
            Set(target, "_label", label);
            Set(target, "_startHidden", startHidden);
            var actor = go.GetComponent<StoryActor>();
            var marker = Marker(go.transform, actor != null ? go.GetComponent<CapsuleCollider>().height + 0.28f : 1.3f);
            Set(target, "_highlight", marker);
            return target;
        }

        /// <summary>Small floating golden diamond shown while an interaction is on offer.</summary>
        public static GameObject Marker(Transform parent, float height)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "InteractMarker";
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = Vector3.up * height;
            marker.transform.localRotation = Quaternion.Euler(45, 0, 45);
            marker.transform.localScale = Vector3.one * 0.075f;
            marker.GetComponent<Renderer>().sharedMaterial = Emissive("StoryMarker", new Color(1f, 0.72f, 0.3f), 1.1f);
            marker.AddComponent<MarkerSpin>();
            marker.SetActive(false);
            return marker;
        }

        // ------------------------------------------------------------------ geometry & materials

        public static GameObject Block(string name, Vector3 position, Vector3 scale, Material material, Transform parent = null, bool solid = true, float yaw = 0f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.layer = solid ? EnvironmentLayer : 0;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.isStatic = solid;
            return go;
        }

        public static GameObject Cylinder(string name, Vector3 position, float radius, float height, Material material, Transform parent = null, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.layer = solid ? EnvironmentLayer : 0;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position + Vector3.up * height * 0.5f;
            go.transform.localScale = new Vector3(radius * 2, height * 0.5f, radius * 2);
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>Gabled tiled roof made of two slanted slabs and a ridge.</summary>
        public static GameObject Roof(string name, Vector3 centre, float width, float depth, float pitch, Material tiles, Material ridge, Transform parent, bool alongX = true)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = centre;
            float half = (alongX ? depth : width) * 0.5f;
            float slab = half / Mathf.Cos(pitch * Mathf.Deg2Rad) + 0.3f;
            float length = (alongX ? width : depth) + 0.8f;
            for (int side = -1; side <= 1; side += 2)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = "Slope";
                piece.layer = EnvironmentLayer;
                piece.transform.SetParent(root.transform, false);
                Vector3 offset = alongX ? new Vector3(0, -Mathf.Sin(pitch * Mathf.Deg2Rad) * slab * 0.5f, side * half * 0.5f)
                                        : new Vector3(side * half * 0.5f, -Mathf.Sin(pitch * Mathf.Deg2Rad) * slab * 0.5f, 0);
                piece.transform.localPosition = offset;
                piece.transform.localRotation = alongX ? Quaternion.Euler(side * pitch, 0, 0) : Quaternion.Euler(0, 0, -side * pitch);
                piece.transform.localScale = alongX ? new Vector3(length, 0.16f, slab) : new Vector3(slab, 0.16f, length);
                piece.GetComponent<Renderer>().sharedMaterial = tiles;
            }
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Ridge";
            top.layer = EnvironmentLayer;
            top.transform.SetParent(root.transform, false);
            top.transform.localPosition = Vector3.up * 0.06f;
            top.transform.localScale = alongX ? new Vector3(length + 0.5f, 0.22f, 0.3f) : new Vector3(0.3f, 0.22f, length + 0.5f);
            top.GetComponent<Renderer>().sharedMaterial = ridge;

            // Details that make it read as a Vietnamese roof: fascia boards along the eaves,
            // ridge ends that curl upwards and lifted eave corners (đầu đao).
            float rad = pitch * Mathf.Deg2Rad;
            float eaveAcross = half * 0.5f + Mathf.Cos(rad) * slab * 0.5f;
            float eaveY = -Mathf.Sin(rad) * slab;
            float halfLength = length * 0.5f;
            Vector3 Local(float along, float y, float across) => alongX ? new Vector3(along, y, across) : new Vector3(across, y, along);
            Vector3 Size(float along, float y, float across) => alongX ? new Vector3(along, y, across) : new Vector3(across, y, along);
            Quaternion Tilt(float angle) => alongX ? Quaternion.Euler(0, 0, angle) : Quaternion.Euler(-angle, 0, 0);
            for (int side = -1; side <= 1; side += 2)
            {
                RoofPart(root.transform, "Fascia", Local(0, eaveY - 0.02f, side * eaveAcross), Size(length, 0.12f, 0.06f), Quaternion.identity, ridge);
                for (int end = -1; end <= 1; end += 2)
                    RoofPart(root.transform, "EaveCorner", Local(end * (halfLength - 0.15f), eaveY + 0.08f, side * (eaveAcross - 0.1f)),
                        Size(0.75f, 0.1f, 0.32f), Tilt(end * 16f), ridge);
            }
            for (int end = -1; end <= 1; end += 2)
            {
                RoofPart(root.transform, "RidgeCurl", Local(end * (halfLength + 0.35f), 0.2f, 0), Size(0.55f, 0.18f, 0.28f), Tilt(end * 28f), ridge);
                RoofPart(root.transform, "RidgeFin", Local(end * (halfLength + 0.55f), 0.42f, 0), Size(0.12f, 0.34f, 0.2f), Tilt(end * 12f), ridge);
            }
            return root;
        }

        /// <summary>Hanging paper lantern with an optional warm point light.</summary>
        public static GameObject Lantern(string name, Vector3 position, Color color, Transform parent, float lightRange = 6f, float intensity = 2.2f)
        {
            // The root is the hook the lantern hangs from, so LanternSway swings it like a pendulum.
            const float drop = 0.42f;
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position + Vector3.up * drop;
            // Silk over a candle reads deep red; a modest emission keeps the colour under filmic tonemapping.
            Color silk = new Color(color.r * 0.62f, color.g * 0.2f, color.b * 0.2f);
            var glow = Emissive("Lantern_" + ColorUtility.ToHtmlStringRGB(color), silk, 1.15f);
            var frame = Material("LanternFrame", new Color(0.1f, 0.05f, 0.03f), 0.4f);
            var gold = Material("LanternGold", new Color(0.75f, 0.52f, 0.2f), 0.6f);
            LanternPart(PrimitiveType.Sphere, "Body", new Vector3(0, -drop, 0), new Vector3(0.38f, 0.44f, 0.38f), glow, root.transform);
            foreach (float y in new[] { -0.12f, 0f, 0.12f })
            {
                float r = 0.38f * Mathf.Sqrt(1f - (y / 0.22f) * (y / 0.22f)) + 0.012f;
                LanternPart(PrimitiveType.Cylinder, "Rib", new Vector3(0, -drop + y, 0), new Vector3(r, 0.006f, r), frame, root.transform);
            }
            LanternPart(PrimitiveType.Cylinder, "Cap", new Vector3(0, -drop + 0.23f, 0), new Vector3(0.2f, 0.035f, 0.2f), frame, root.transform);
            LanternPart(PrimitiveType.Cylinder, "Base", new Vector3(0, -drop - 0.23f, 0), new Vector3(0.2f, 0.035f, 0.2f), frame, root.transform);
            LanternPart(PrimitiveType.Cylinder, "Cord", new Vector3(0, -drop * 0.5f + 0.06f, 0), new Vector3(0.012f, drop * 0.5f - 0.06f, 0.012f), frame, root.transform);
            LanternPart(PrimitiveType.Sphere, "Knot", new Vector3(0, -drop - 0.3f, 0), Vector3.one * 0.05f, gold, root.transform);
            LanternPart(PrimitiveType.Cylinder, "Tassel", new Vector3(0, -drop - 0.44f, 0), new Vector3(0.05f, 0.12f, 0.05f), Material("LanternTassel", new Color(0.6f, 0.06f, 0.04f), 0.3f), root.transform);
            root.AddComponent<ThuyKieu.Environment.LanternSway>();
            if (lightRange > 0f)
            {
                var lightObject = new GameObject("Light");
                lightObject.transform.SetParent(root.transform, false);
                lightObject.transform.localPosition = new Vector3(0, -drop, 0);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Color.Lerp(color, new Color(1f, 0.7f, 0.4f), 0.5f);
                light.range = lightRange;
                light.intensity = intensity;
                light.shadows = LightShadows.None;
            }
            return root;
        }

        private static void RoofPart(Transform root, string name, Vector3 local, Vector3 scale, Quaternion rotation, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(root, false);
            part.transform.localPosition = local;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void LanternPart(PrimitiveType type, string name, Vector3 local, Vector3 scale, Material material, Transform parent)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = local;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            part.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static GameObject Prop(string prefabPath, Vector3 position, float yaw, float scale, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogWarning("[StoryKit] Missing prefab " + prefabPath); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            go.transform.localScale *= scale;
            return go;
        }

        public static Material Material(string name, Color color, float smoothness = 0.15f)
        {
            string path = AssetFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static Material Emissive(string name, Color color, float intensity)
        {
            string path = AssetFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static Light Sun(string name, Color color, float intensity, Vector3 angles)
        {
            var go = new GameObject(name);
            go.transform.rotation = Quaternion.Euler(angles);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.8f;
            return light;
        }

        public static Transform Point(string name, Vector3 position, float yaw, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            return go.transform;
        }

        public static T Child<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        public static void Set(UnityEngine.Object target, string property, object value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            if (field == null) throw new InvalidOperationException(target.GetType().Name + ": " + property);
            switch (value)
            {
                case null: field.objectReferenceValue = null; break;
                case UnityEngine.Object unityObject: field.objectReferenceValue = unityObject; break;
                case string text: field.stringValue = text; break;
                case float number: field.floatValue = number; break;
                case bool flag: field.boolValue = flag; break;
                case int integer:
                    if (field.propertyType == SerializedPropertyType.LayerMask) field.intValue = integer;
                    else if (field.propertyType == SerializedPropertyType.Enum) field.enumValueIndex = integer;
                    else field.intValue = integer;
                    break;
                case Color color: field.colorValue = color; break;
                case Vector3 vector: field.vector3Value = vector; break;
                case UnityEngine.Object[] array:
                    field.arraySize = array.Length;
                    for (int i = 0; i < array.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                    break;
                case string[] lines:
                    field.arraySize = lines.Length;
                    for (int i = 0; i < lines.Length; i++) field.GetArrayElementAtIndex(i).stringValue = lines[i];
                    break;
                default: throw new InvalidOperationException("Unsupported value for " + property);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

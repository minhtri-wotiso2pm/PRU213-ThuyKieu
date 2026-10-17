using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Scene-0-only integration of the rigged Mixamo classroom characters.
    ///
    /// Converts the six classroom animation FBX to Humanoid (their source bytes are never rewritten; only .meta
    /// import settings and the generated Avatar/clip names), builds one AnimatorController + material per character,
    /// then swaps each classroom <see cref="StoryActor"/>'s static <c>Visual</c> for the rigged model *in place*
    /// (adding a <c>RiggedBody</c> child and wiring <c>_animator</c>). The original static visual is only disabled,
    /// never deleted, so the change is reversible.
    ///
    /// Safe by construction: it refuses to run unless <c>Chapter00_Prologue</c> is the active scene, is re-runnable
    /// (no duplicate bodies/controllers), and saves only that scene.
    ///
    ///   Menu: ThuyKieu/Tools/Integrate rigged classroom characters (Scene 0)
    ///   Menu: ThuyKieu/Tools/Revert rigged classroom characters (Scene 0)
    /// </summary>
    public static class ClassroomRiggedIntegration
    {
        public const string ScenePath = "Assets/_Game/Scenes/Chapter00_Prologue.unity";
        public const string ControllerFolder = "Assets/_Game/Animations/ClassroomRigged";
        public const string MaterialFolder = "Assets/_Game/Art/Characters/ClassroomRigged";

        private const string BodyName = "RiggedBody";
        private const string VisualName = "Visual";

        private const string TeacherFbx = "Assets/_Game/Animations/animationthaygiao/idle.fbx";
        private const string BoyFbx = "Assets/_Game/Animations/animationhocsinhnam/seat.fbx";
        private const string GirlFbx = "Assets/_Game/Animations/animationhocsinhnu/seat.fbx";
        private const string HeroIdleFbx = "Assets/_Game/Animations/animationnuchinh/idle.fbx";
        private const string HeroWalkFbx = "Assets/_Game/Animations/animationnuchinh/walk.fbx";
        private const string HeroSitFbx = "Assets/_Game/Animations/animationnuchinh/seat.fbx";
        private const string SharedWalkFbx = "Assets/_Game/Animations/Shared/Walking.fbx";

        private const string TeacherTex = "Assets/_Game/Art/Characters/thaygiao";
        private const string BoyTex = "Assets/_Game/Art/Characters/hocsinhnam";
        private const string GirlTex = "Assets/_Game/Art/Characters/hocsinhnu";
        private const string HeroTex = "Assets/_Game/Art/Characters/nuchinh";

        private enum Role { Teacher, Boy, Girl, Hero }

        // ------------------------------------------------------------------ entry points

        [MenuItem("ThuyKieu/Tools/Integrate rigged classroom characters (Scene 0)")]
        public static void Run()
        {
            RequireActiveScene();
            EnsureFolder(ControllerFolder);
            EnsureFolder(MaterialFolder);

            ConfigureHumanoid(TeacherFbx, "Idle", loop: true, sitting: false, copyFrom: null);
            ConfigureHumanoid(BoyFbx, "Sitting", loop: true, sitting: true, copyFrom: null);
            ConfigureHumanoid(GirlFbx, "Sitting", loop: true, sitting: true, copyFrom: null);
            ConfigureHumanoid(HeroIdleFbx, "Idle", loop: true, sitting: false, copyFrom: null);
            var heroAvatar = LoadSubAsset<Avatar>(HeroIdleFbx);
            ConfigureHumanoid(HeroWalkFbx, "Walking", loop: true, sitting: false, copyFrom: heroAvatar);
            ConfigureHumanoid(HeroSitFbx, "Sitting", loop: true, sitting: true, copyFrom: heroAvatar);

            var teacherMat = UpsertLit(MaterialFolder + "/ThayGiao.mat", TeacherTex);
            var boyMat = UpsertLit(MaterialFolder + "/HocSinhNam.mat", BoyTex);
            var girlMat = UpsertLit(MaterialFolder + "/HocSinhNu.mat", GirlTex);
            var heroMat = UpsertLit(MaterialFolder + "/NuChinh.mat", HeroTex);

            var teacherCtrl = UpsertTeacherController(RequireClip(TeacherFbx, "Idle"), RequireClip(SharedWalkFbx, "Walking"));
            var boyCtrl = UpsertSeatedController(ControllerFolder + "/HocSinhNam.controller", "Sitting", RequireClip(BoyFbx, "Sitting"));
            var girlCtrl = UpsertSeatedController(ControllerFolder + "/HocSinhNu.controller", "Sitting", RequireClip(GirlFbx, "Sitting"));
            // The dedicated hero walk take bends the knees like a crouch on this avatar.
            // The shared humanoid walk retargets cleanly and keeps both feet under the body.
            var heroCtrl = UpsertHeroController(RequireClip(HeroIdleFbx, "Idle"), RequireClip(SharedWalkFbx, "Walking"), RequireClip(HeroSitFbx, "Sitting"));

            var targets = FindTargets();
            if (targets.Count == 0) throw new InvalidOperationException("No classroom StoryActors found in " + ScenePath + " (is the scene open?).");

            foreach (var actor in targets)
            {
                var role = RoleOf(actor);
                switch (role)
                {
                    case Role.Teacher: Integrate(actor, role, TeacherFbx, teacherCtrl, teacherMat); break;
                    case Role.Boy: Integrate(actor, role, BoyFbx, boyCtrl, boyMat); break;
                    case Role.Girl: Integrate(actor, role, GirlFbx, girlCtrl, girlMat); break;
                    case Role.Hero: Integrate(actor, role, HeroIdleFbx, heroCtrl, heroMat); break;
                }
            }

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[ClassroomRigged] Integrated " + targets.Count + " classroom actors into " + ScenePath + ".");
        }

        [MenuItem("ThuyKieu/Tools/Revert rigged classroom characters (Scene 0)")]
        public static void Revert()
        {
            RequireActiveScene();
            foreach (var actor in FindTargets())
            {
                var root = actor.transform;
                var body = root.Find(BodyName);
                if (body != null) UnityEngine.Object.DestroyImmediate(body.gameObject);
                var visual = root.Find(VisualName);
                if (visual != null) visual.gameObject.SetActive(true);
                StorySceneKit.Set(actor, "_animator", null);
                StorySceneKit.Set(actor, "_staticVisual", visual);
            }
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ClassroomRigged] Reverted classroom actors in " + ScenePath + ".");
        }

        /// <summary>Read-only summary of what <see cref="Run"/> would do. Safe to call in any scene.</summary>
        public static string Preview()
        {
            var sb = new System.Text.StringBuilder();
            var active = EditorSceneManager.GetActiveScene();
            sb.AppendLine("active scene: " + active.path);
            foreach (var actor in FindTargets())
            {
                var role = RoleOf(actor);
                var staticVisual = GetObject<Transform>(actor, "_staticVisual");
                var animatorProp = GetObject<Animator>(actor, "_animator");
                sb.AppendLine(actor.name + " speaker=" + actor.SpeakerId + " role=" + role
                    + " staticVisual=" + (staticVisual != null ? staticVisual.name : "null")
                    + " animator=" + (animatorProp != null ? animatorProp.name : "null")
                    + " alreadyRigged=" + (actor.transform.Find(BodyName) != null));
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ scene targets

        private static void RequireActiveScene()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.path != ScenePath)
                throw new InvalidOperationException("Refusing to run: active scene is '" + active.path + "', expected '" + ScenePath + "'. Open that scene first.");
            if (Application.isPlaying)
                throw new InvalidOperationException("Refusing to run: exit Play Mode first.");
        }

        private static List<StoryActor> FindTargets()
        {
            var list = new List<StoryActor>();
            foreach (var actor in UnityEngine.Object.FindObjectsByType<StoryActor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (actor.gameObject.scene.path != ScenePath) continue;
                string speaker = actor.SpeakerId;
                if (speaker == "ThayGiao" || speaker == "Ngan" || speaker == "BanHoc") list.Add(actor);
            }
            return list;
        }

        private static Role RoleOf(StoryActor actor)
        {
            switch (actor.SpeakerId)
            {
                case "ThayGiao": return Role.Teacher;
                case "Ngan": return Role.Hero;
            }
            // BanHoc*: the boy and girl classmates share a speaker id, so resolve the model from the static visual.
            string model = StaticModelName(actor);
            return model != null && model.Contains("NuSinhToxic") ? Role.Girl : Role.Boy;
        }

        private static string StaticModelName(StoryActor actor)
        {
            var visual = GetObject<Transform>(actor, "_staticVisual");
            if (visual != null && visual.childCount > 0) return visual.GetChild(0).name;
            var renderer = actor.GetComponentInChildren<Renderer>(true);
            return renderer != null && renderer.sharedMaterial != null ? renderer.sharedMaterial.name : null;
        }

        // ------------------------------------------------------------------ integration

        private static void Integrate(StoryActor actor, Role role, string fbxPath, RuntimeAnimatorController controller, Material material)
        {
            var root = actor.transform;
            var staticVisual = GetObject<Transform>(actor, "_staticVisual");
            float height = ColliderHeight(root, actor);

            var holder = root.Find(BodyName);
            GameObject model;
            if (holder == null)
            {
                holder = new GameObject(BodyName).transform;
                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbx == null) throw new InvalidOperationException("Missing FBX " + fbxPath);
                model = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                model.transform.SetParent(holder, false);
                Fit(model, height);
                holder.SetParent(root, false);
                Vector3 rootScale = root.lossyScale; // cancel any parent scale so the fitted height survives
                holder.localScale = new Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z);
                if (staticVisual != null) AlignToStaticVisual(holder, model, staticVisual);
            }
            else
            {
                if (holder.childCount == 0) throw new InvalidOperationException(actor.name + ": existing " + BodyName + " has no model.");
                model = holder.GetChild(0).gameObject;
            }

            var animator = model.GetComponentInChildren<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.avatar = LoadSubAsset<Avatar>(fbxPath);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, renderer.sharedMaterials.Length)).ToArray();

            StorySceneKit.Set(actor, "_animator", animator);
            StorySceneKit.Set(actor, "_staticVisual", null);
            if (staticVisual != null) staticVisual.gameObject.SetActive(false);
        }

        private static float ColliderHeight(Transform root, StoryActor actor)
        {
            var capsule = root.GetComponent<CapsuleCollider>();
            if (capsule != null && capsule.height > 0.2f) return capsule.height;
            float head = GetFloat(actor, "_headHeight");
            return head > 0.2f ? head / 0.92f : 1.6f;
        }

        /// <summary>Fits the model to the given height at the world origin (mirrors StorySceneKit.Fit).</summary>
        private static void Fit(GameObject model, float height)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            float scale = height / bounds.size.y;
            model.transform.localScale *= scale;
            model.transform.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);
        }

        /// <summary>Slides the rigged body so it occupies the same volume (XZ centre, floor) as the static mesh it replaces.</summary>
        private static void AlignToStaticVisual(Transform holder, GameObject model, Transform staticVisual)
        {
            var staticRenderer = staticVisual.GetComponentInChildren<Renderer>(true);
            if (staticRenderer == null) return;
            Bounds target = staticRenderer.bounds;
            Bounds body = BoundsOf(model);
            holder.position += new Vector3(target.center.x - body.center.x, target.min.y - body.min.y, target.center.z - body.center.z);
        }

        private static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        // ------------------------------------------------------------------ FBX import

        private static void ConfigureHumanoid(string path, string clipName, bool loop, bool sitting, Avatar copyFrom)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing FBX " + path);

            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            if (copyFrom != null) { importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther; importer.sourceAvatar = copyFrom; }
            else { importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.sourceAvatar = null; }

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0) throw new InvalidOperationException("No animation take in " + path);
            foreach (var clip in clips)
            {
                clip.name = clipName;
                clip.loopTime = loop;
                clip.loopPose = loop;
                clip.lockRootRotation = true;
                clip.lockRootPositionXZ = true;
                clip.lockRootHeightY = !sitting; // a sitting pose lowers the hips; locking Y would stand it up
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionXZ = true;
                clip.heightFromFeet = !sitting;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            if (copyFrom == null && AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault() == null)
                throw new InvalidOperationException(path + " did not generate a Humanoid Avatar.");
        }

        private static AnimationClip RequireClip(string fbxPath, string clipName)
        {
            var clip = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => c.name == clipName && !c.name.StartsWith("__preview__"));
            if (clip == null) throw new InvalidOperationException("Clip '" + clipName + "' missing on " + fbxPath);
            if (!clip.isHumanMotion) throw new InvalidOperationException(fbxPath + " clip is not Humanoid. Avatar mapping failed.");
            return clip;
        }

        // ------------------------------------------------------------------ controllers

        private static AnimatorController UpsertTeacherController(AnimationClip idle, AnimationClip walk)
        {
            var controller = UpsertController(ControllerFolder + "/ThayGiao.controller");
            EnsureParam(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureSitParams(controller, defaultSitting: false);

            var machine = controller.layers[0].stateMachine;
            var idleState = FindOrAddState(machine, "Idle");
            var walkState = FindOrAddState(machine, "Walking");
            idleState.motion = idle;
            walkState.motion = walk;
            machine.defaultState = idleState;
            ClearTransitions(idleState);
            ClearTransitions(walkState);

            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false; toWalk.duration = 0.15f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            toWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSitting");
            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false; toIdle.duration = 0.15f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        /// <summary>A permanently seated classmate: one looping seated state, plus the sit contract the game calls.</summary>
        private static AnimatorController UpsertSeatedController(string path, string stateName, AnimationClip clip)
        {
            var controller = UpsertController(path);
            EnsureSitParams(controller, defaultSitting: true);

            var machine = controller.layers[0].stateMachine;
            var state = FindOrAddState(machine, stateName);
            state.motion = clip;
            machine.defaultState = state;
            ClearTransitions(state);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorController UpsertHeroController(AnimationClip idle, AnimationClip walk, AnimationClip sit)
        {
            var controller = UpsertController(ControllerFolder + "/NuChinh.controller");
            EnsureParam(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureSitParams(controller, defaultSitting: false);

            var machine = controller.layers[0].stateMachine;
            var idleState = FindOrAddState(machine, "Idle");
            var walkState = FindOrAddState(machine, "Walking");
            var sitState = FindOrAddState(machine, "Sitting");
            idleState.motion = idle;
            walkState.motion = walk;
            sitState.motion = sit;
            machine.defaultState = idleState;
            ClearTransitions(idleState);
            ClearTransitions(walkState);
            ClearTransitions(sitState);
            ClearAnyStateTransitions(machine);

            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false; toWalk.duration = 0.15f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");
            toWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSitting");

            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false; toIdle.duration = 0.15f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

            // StoryActor.Sit(true) sets IsSitting + the Sit trigger; Sit(false) fires the Stand trigger.
            var down = machine.AddAnyStateTransition(sitState);
            down.hasExitTime = false; down.duration = 0.2f; down.canTransitionToSelf = false;
            down.AddCondition(AnimatorConditionMode.If, 0, "Sit");
            down.AddCondition(AnimatorConditionMode.If, 0, "IsSitting");

            var up = sitState.AddTransition(idleState);
            up.hasExitTime = false; up.duration = 0.2f;
            up.AddCondition(AnimatorConditionMode.If, 0, "Stand");
            up.AddCondition(AnimatorConditionMode.If, 0, "IsSitting");

            // Clears IsSitting once the actor has finished standing (same behaviour the shared controllers use).
            var finishStanding = sitState.AddStateMachineBehaviour<DialogueGestureReturn>();
            finishStanding.Configure(0, true);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorController UpsertController(string path)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            return controller;
        }

        private static void EnsureSitParams(AnimatorController controller, bool defaultSitting)
        {
            EnsureParam(controller, "IsSitting", AnimatorControllerParameterType.Bool);
            EnsureParam(controller, "Sit", AnimatorControllerParameterType.Trigger);
            EnsureParam(controller, "Stand", AnimatorControllerParameterType.Trigger);
            var parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == "IsSitting") parameters[i].defaultBool = defaultSitting;
            controller.parameters = parameters;
        }

        private static void EnsureParam(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (controller.parameters.Any(p => p.name == name)) return;
            controller.AddParameter(name, type);
        }

        private static AnimatorState FindOrAddState(AnimatorStateMachine machine, string name)
        {
            foreach (var child in machine.states)
                if (child.state.name == name) return child.state;
            return machine.AddState(name);
        }

        private static void ClearTransitions(AnimatorState state)
        {
            foreach (var transition in state.transitions.ToArray()) state.RemoveTransition(transition);
        }

        private static void ClearAnyStateTransitions(AnimatorStateMachine machine)
        {
            foreach (var transition in machine.anyStateTransitions.ToArray()) machine.RemoveAnyStateTransition(transition);
        }

        // ------------------------------------------------------------------ materials

        private static Material UpsertLit(string path, string textureFolder)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            string albedo = textureFolder + "/texture_pbr_20250901.png";
            string normal = textureFolder + "/texture_pbr_20250901_normal.png";
            CapTexture(albedo);
            MarkNormalMap(normal);
            material.shader = shader;
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
            var bump = AssetDatabase.LoadAssetAtPath<Texture2D>(normal);
            material.SetTexture("_BumpMap", bump);
            if (bump != null) material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CapTexture(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.maxTextureSize > 2048)
            {
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        private static void MarkNormalMap(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        // ------------------------------------------------------------------ serialized helpers

        private static T GetObject<T>(UnityEngine.Object target, string property) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            return field != null ? field.objectReferenceValue as T : null;
        }

        private static float GetFloat(UnityEngine.Object target, string property)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            return field != null ? field.floatValue : 0f;
        }

        private static T LoadSubAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().FirstOrDefault();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) throw new InvalidOperationException("Bad folder " + path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

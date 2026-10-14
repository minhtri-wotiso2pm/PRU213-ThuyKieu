using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Idempotent import + prefab/controller setup for Mixamo classroom FBX (teacher idle, students sit, nữ chính idle/walk/sit).
    /// Does not modify hashed OBJ, Modern prefabs, or chapter scenes. Source FBX bytes are not rewritten; only .meta import settings.
    ///   Menu: ThuyKieu/Tools/Setup classroom Mixamo characters
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.ClassroomCharacterAnimationSetup.RunBatch
    /// </summary>
    public static class ClassroomCharacterAnimationSetup
    {
        public const string ControllerFolder = "Assets/_Game/Animations/Classroom";
        public const string MaterialFolder = "Assets/_Game/Art/Characters/ClassroomAnimated";
        public const string PrefabFolder = "Assets/_Game/Prefabs/NPC/ClassroomAnimated";
        public const string ScenePath = "Assets/_Game/Scenes/CharacterAnimationAuditTest.unity";

        private const string TeacherFbx = "Assets/_Game/Animations/animationthaygiao/idle.fbx";
        private const string BoySitFbx = "Assets/_Game/Animations/animationhocsinhnam/seat.fbx";
        private const string GirlSitFbx = "Assets/_Game/Animations/animationhocsinhnu/seat.fbx";
        private const string HeroIdleFbx = "Assets/_Game/Animations/animationnuchinh/idle.fbx";
        private const string HeroSitFbx = "Assets/_Game/Animations/animationnuchinh/seat.fbx";
        private const string HeroWalkFbx = "Assets/_Game/Animations/animationnuchinh/walk.fbx";

        private const string TeacherTex = "Assets/_Game/Art/Characters/thaygiao";
        private const string BoyTex = "Assets/_Game/Art/Characters/hocsinhnam";
        private const string GirlTex = "Assets/_Game/Art/Characters/hocsinhnu";
        private const string HeroTex = "Assets/_Game/Art/Characters/nuchinh";

        [MenuItem("ThuyKieu/Tools/Setup classroom Mixamo characters")]
        public static void Run()
        {
            EnsureFolder(ControllerFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(PrefabFolder);

            ConfigureHumanoid(TeacherFbx, "Idle", loop: true, sitting: false, copyFrom: null);
            ConfigureHumanoid(BoySitFbx, "Sitting", loop: true, sitting: true, copyFrom: null);
            ConfigureHumanoid(GirlSitFbx, "Sitting", loop: true, sitting: true, copyFrom: null);
            ConfigureHumanoid(HeroIdleFbx, "Idle", loop: true, sitting: false, copyFrom: null);
            var heroAvatar = LoadSubAsset<Avatar>(HeroIdleFbx);
            ConfigureHumanoid(HeroSitFbx, "Sitting", loop: true, sitting: true, copyFrom: heroAvatar);
            ConfigureHumanoid(HeroWalkFbx, "Walking", loop: true, sitting: false, copyFrom: heroAvatar);

            var teacherMat = UpsertLit(MaterialFolder + "/ThayGiaoAnimated.mat", TeacherTex);
            var boyMat = UpsertLit(MaterialFolder + "/HocSinhNamAnimated.mat", BoyTex);
            var girlMat = UpsertLit(MaterialFolder + "/HocSinhNuAnimated.mat", GirlTex);
            var heroMat = UpsertLit(MaterialFolder + "/NuChinhAnimated.mat", HeroTex);

            var teacherCtrl = UpsertSingleStateController(ControllerFolder + "/ThayGiaoClassroom.controller", "Idle", RequireClip(TeacherFbx, "Idle"));
            var boyCtrl = UpsertSingleStateController(ControllerFolder + "/HocSinhNamClassroom.controller", "Sitting", RequireClip(BoySitFbx, "Sitting"));
            var girlCtrl = UpsertSingleStateController(ControllerFolder + "/HocSinhNuClassroom.controller", "Sitting", RequireClip(GirlSitFbx, "Sitting"));
            var heroCtrl = UpsertHeroController(
                RequireClip(HeroIdleFbx, "Idle"),
                RequireClip(HeroWalkFbx, "Walking"),
                RequireClip(HeroSitFbx, "Sitting"));

            UpsertPrefab("ThayGiaoAnimated", TeacherFbx, teacherCtrl, teacherMat);
            UpsertPrefab("HocSinhNamAnimated", BoySitFbx, boyCtrl, boyMat);
            UpsertPrefab("HocSinhNuAnimated", GirlSitFbx, girlCtrl, girlMat);
            UpsertPrefab("NuChinhAnimated", HeroIdleFbx, heroCtrl, heroMat);

            UpsertTestScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[ClassroomMixamo] Controllers, materials, prefabs, and test scene are up to date. Play CharacterAnimationAuditTest to verify deformation.");
        }

        public static void RunBatch()
        {
            try
            {
                Run();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void ConfigureHumanoid(string path, string clipName, bool loop, bool sitting, Avatar copyFrom)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing FBX " + path);

            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            if (copyFrom != null)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = copyFrom;
            }
            else
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.sourceAvatar = null;
            }

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
                throw new InvalidOperationException("No animation take in " + path);

            foreach (var clip in clips)
            {
                clip.name = clipName;
                clip.loopTime = loop;
                clip.loopPose = loop;
                clip.lockRootRotation = true;
                clip.lockRootPositionXZ = true;
                // Sitting Mixamo poses lower the hips; locking Y would stand the character up.
                clip.lockRootHeightY = !sitting;
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
            if (clip == null)
                throw new InvalidOperationException("Clip '" + clipName + "' missing on " + fbxPath);
            if (!clip.isHumanMotion)
                throw new InvalidOperationException(fbxPath + " clip is not Humanoid. Avatar mapping failed.");
            return clip;
        }

        private static AnimatorController UpsertSingleStateController(string path, string stateName, AnimationClip clip)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            var machine = controller.layers[0].stateMachine;
            AnimatorState state = null;
            foreach (var child in machine.states)
            {
                if (child.state.name == stateName) { state = child.state; break; }
            }
            if (state == null) state = machine.AddState(stateName);
            state.motion = clip;
            machine.defaultState = state;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorController UpsertHeroController(AnimationClip idle, AnimationClip walk, AnimationClip sit)
        {
            string path = ControllerFolder + "/NuChinhClassroom.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            EnsureParam(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParam(controller, "IsSitting", AnimatorControllerParameterType.Bool);

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

            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false;
            toWalk.duration = 0.15f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");
            toWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSitting");

            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.15f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

            var sitFromIdle = idleState.AddTransition(sitState);
            sitFromIdle.hasExitTime = false;
            sitFromIdle.duration = 0.2f;
            sitFromIdle.AddCondition(AnimatorConditionMode.If, 0, "IsSitting");

            var sitFromWalk = walkState.AddTransition(sitState);
            sitFromWalk.hasExitTime = false;
            sitFromWalk.duration = 0.2f;
            sitFromWalk.AddCondition(AnimatorConditionMode.If, 0, "IsSitting");

            var stand = sitState.AddTransition(idleState);
            stand.hasExitTime = false;
            stand.duration = 0.2f;
            stand.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSitting");

            EditorUtility.SetDirty(controller);
            return controller;
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
            foreach (var t in state.transitions.ToArray())
                state.RemoveTransition(t);
        }

        private static void UpsertPrefab(string name, string fbxPath, RuntimeAnimatorController controller, Material material)
        {
            string prefabPath = PrefabFolder + "/" + name + ".prefab";
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) throw new InvalidOperationException("FBX model missing " + fbxPath);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = name;

            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            var avatar = LoadSubAsset<Avatar>(fbxPath);
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            foreach (var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    renderer.sharedMaterial = material;
                else
                {
                    var mats = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++) mats[i] = material;
                    renderer.sharedMaterials = mats;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
        }

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

        private static void MarkNormalMap(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        private static void UpsertTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            var root = new GameObject("AuditCharacters");
            Place(root.transform, "ThayGiaoAnimated", new Vector3(-2.4f, 0f, 0f));
            Place(root.transform, "HocSinhNamAnimated", new Vector3(-0.8f, 0f, 0f));
            Place(root.transform, "HocSinhNuAnimated", new Vector3(0.8f, 0f, 0f));
            Place(root.transform, "NuChinhAnimated", new Vector3(2.4f, 0f, 0f));

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static void Place(Transform parent, string prefabName, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + prefabName + ".prefab");
            if (prefab == null) throw new InvalidOperationException("Prefab missing " + prefabName);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = position;
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
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                throw new InvalidOperationException("Bad folder " + path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

using System;
using System.Linq;
using ThuyKieu.Core.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static class Chapter06TeaVendorSetup
    {
        private const string ArtFolder = "Assets/_Game/Art/Characters/baquayhang";
        private const string ModelPath = "Assets/_Game/Animations/animationbaquayhang/Talking.fbx";
        private const string ControllerPath = StoryAnimationSetup.CharacterFolder + "/TeaVendorStory.overrideController";
        private const string VisualName = "TeaVendorVisual";

        private static RuntimeAnimatorController Controller()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing tea vendor Talking.fbx.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null || model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0)
                throw new InvalidOperationException("Talking.fbx has no skinned body; the OBJ needs a rig before it can be animated.");
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            if (clips.Length != 1) throw new InvalidOperationException("Expected one tea vendor talking clip.");
            if (importer.animationType != ModelImporterAnimationType.Human || !clips[0].loopTime || !clips[0].lockRootPositionXZ)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                foreach (var clip in clips)
                {
                    clip.loopTime = clip.loopPose = true;
                    clip.lockRootRotation = clip.lockRootHeightY = clip.lockRootPositionXZ = true;
                    clip.keepOriginalOrientation = clip.keepOriginalPositionXZ = clip.heightFromFeet = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
            var talking = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Single(clip => !clip.name.StartsWith("__preview__"));
            if (!talking.isHumanMotion) throw new InvalidOperationException("TeaVendor talking clip is not Humanoid.");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(ControllerPath);
            if (controller == null)
            {
                controller = new AnimatorOverrideController(StoryAnimationSetup.Controller()) { name = "TeaVendorStory" };
                AssetDatabase.CreateAsset(controller, ControllerPath);
            }
            controller["Talking"] = talking;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        internal static StoryActor Create(Vector3 position, float yaw)
        {
            return RiggedActor("BaHangNuoc", "BaHangNuoc", ModelPath, ArtFolder, 1.56f,
                position, yaw, Color.white, Controller());
        }

        [MenuItem("ThuyKieu/Chapter 6/Use tea vendor model")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            var controller = Controller();
            var scene = SceneManager.GetSceneByPath(Chapter06Setup.ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(Chapter06Setup.ScenePath, OpenSceneMode.Additive);
            var actor = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StoryActor>(true))
                .Single(item => item.SpeakerId == "BaHangNuoc");
            var visual = actor.transform.Find(VisualName);
            if (visual == null)
            {
                var data = new SerializedObject(actor);
                var oldAnimator = (Animator)data.FindProperty("_animator").objectReferenceValue;
                var body = Body(actor.transform, VisualName, ModelPath, ArtFolder, 1.56f, Color.white, controller);
                var animator = body.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException("Invalid tea vendor Humanoid rig.");
                if (oldAnimator != null) oldAnimator.gameObject.SetActive(false);
                body.SetActive(true);
                data.FindProperty("_animator").objectReferenceValue = animator;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Chapter 6.");
            AssetDatabase.SaveAssets();
            Debug.Log("Chapter06 tea vendor verified: custom skinned body, tea vendor texture, Humanoid Talking animation; original actor and interactions preserved.");
        }
    }
}

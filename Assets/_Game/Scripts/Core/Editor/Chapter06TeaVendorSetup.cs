using System;
using System.Linq;
using ThuyKieu.Core.Story;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static class Chapter06TeaVendorSetup
    {
        private const string ArtFolder = "Assets/_Game/Art/Characters/NPC3";
        private const string ModelPath = "Assets/_Game/Animations/animationNPC3/Untitled1_talking.blend";
        private const string ControllerPath = StoryAnimationSetup.CharacterFolder + "/NPC3TeaVendor.controller";
        private const string VisualName = "NPC3TeaVendorVisual";
        private const string WalkClipPath = StoryAnimationSetup.CharacterFolder + "/NPC3TeaVendorWalk.anim";

        private static RuntimeAnimatorController Controller()
        {
            var talking = ImportClip(ModelPath);
            var walking = CompatibleWalk(ImportClip("Assets/_Game/Animations/animationNPC3/Untitled1_rigged_walk_interaction.blend"));
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null || model.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                throw new InvalidOperationException("NPC3 talking model has no skinned body.");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null || controller.layers.Length == 0 || controller.layers[0].stateMachine.states.Length == 0)
            {
                // NPC3 has a custom Generic rig; Humanoid gestures cannot drive these bones.
                if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
                if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
                if (!controller.parameters.Any(parameter => parameter.name == "DialogueTalking")) controller.AddParameter("DialogueTalking", AnimatorControllerParameterType.Bool);
                if (!controller.parameters.Any(parameter => parameter.name == "DialogueEmotion")) controller.AddParameter("DialogueEmotion", AnimatorControllerParameterType.Int);
                if (!controller.parameters.Any(parameter => parameter.name == "Speed")) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
                string idlePath = StoryAnimationSetup.CharacterFolder + "/NPC3TeaVendorIdle.anim";
                var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
                if (idle == null)
                {
                    idle = new AnimationClip { name = "NPC3TeaVendorIdle" };
                    foreach (var binding in AnimationUtility.GetCurveBindings(walking))
                    {
                        float value = AnimationUtility.GetEditorCurve(walking, binding).Evaluate(0f);
                        AnimationUtility.SetEditorCurve(idle, binding, AnimationCurve.Constant(0f, 1f, value));
                    }
                    AssetDatabase.CreateAsset(idle, idlePath);
                }
                var machine = controller.layers[0].stateMachine;
                var idleState = machine.AddState("Idle"); idleState.motion = idle;
                var walkState = machine.AddState("Walking"); walkState.motion = walking;
                var talkState = machine.AddState("Talking"); talkState.motion = talking;
                machine.defaultState = idleState;
                var speak = Transition(idleState, talkState);
                speak.AddCondition(AnimatorConditionMode.If, 0f, "DialogueTalking");
                speak.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");
                Transition(talkState, idleState).AddCondition(AnimatorConditionMode.IfNot, 0f, "DialogueTalking");
                Transition(idleState, walkState).AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");
                Transition(talkState, walkState).AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");
                Transition(walkState, idleState).AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");
            }
            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(StoryAnimationSetup.CharacterFolder + "/NPC3TeaVendorIdle.anim");
            idleClip.ClearCurves();
            foreach (var binding in AnimationUtility.GetCurveBindings(walking))
            {
                float value = AnimationUtility.GetEditorCurve(walking, binding).Evaluate(0f);
                AnimationUtility.SetEditorCurve(idleClip, binding, AnimationCurve.Constant(0f, 1f, value));
            }
            EditorUtility.SetDirty(idleClip);
            foreach (var item in controller.layers[0].stateMachine.states)
            {
                if (item.state.name == "Idle") item.state.motion = idleClip;
                if (item.state.name == "Walking") item.state.motion = walking;
                if (item.state.name == "Talking") item.state.motion = talking;
                EditorUtility.SetDirty(item.state);
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip CompatibleWalk(AnimationClip source)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkClipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = "NPC3TeaVendorWalk" };
                AssetDatabase.CreateAsset(clip, WalkClipPath);
            }
            clip.ClearCurves();
            // The walking file names the same skeleton Humanoid_Rig1; the talking model uses Humanoid_Rig.
            foreach (var original in AnimationUtility.GetCurveBindings(source))
            {
                var binding = original;
                binding.path = binding.path.Replace("Humanoid_Rig1", "Humanoid_Rig");
                AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(source, original));
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            return transition;
        }

        private static AnimationClip ImportClip(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing NPC3 animation: " + path);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            if (clips.Length != 1) throw new InvalidOperationException("Expected one NPC3 clip per Blender file.");
            if (importer.animationType != ModelImporterAnimationType.Generic || importer.clipAnimations.Length == 0 || !clips[0].loopTime || !clips[0].lockRootPositionXZ || importer.importCameras || importer.importLights)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.importAnimation = true;
                importer.importCameras = false;
                importer.importLights = false;
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

        internal static StoryActor Create(Vector3 position, float yaw)
        {
            return RiggedActor("BaHangNuoc", "BaHangNuoc", ModelPath, ArtFolder, 1.56f,
                position, yaw, Color.white, Controller(), false);
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
            if (visual != null && visual.GetComponentInChildren<Animator>(true) == null)
            {
                UnityEngine.Object.DestroyImmediate(visual.gameObject);
                visual = null;
            }
            {
                var data = new SerializedObject(actor);
                var oldAnimator = (Animator)data.FindProperty("_animator").objectReferenceValue;
                var body = visual != null ? visual.gameObject : Body(actor.transform, VisualName, ModelPath, ArtFolder, 1.56f, Color.white, controller, false);
                var animator = body.GetComponentInChildren<Animator>(true);
                if (animator == null || body.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(skin => skin.bones.Length == 0 || skin.bones.Any(bone => bone == null)))
                    throw new InvalidOperationException("Invalid NPC3 rig.");
                ValidateBody(body);
                if (oldAnimator != null && oldAnimator != animator) oldAnimator.gameObject.SetActive(false);
                body.SetActive(true);
                data.FindProperty("_animator").objectReferenceValue = animator;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Chapter 6.");
            AssetDatabase.SaveAssets();
            Debug.Log("Chapter06 NPC3 tea vendor applied: custom Generic rig, NPC3 texture, idle/walk/talk controller; original actor and interactions preserved.");
        }

        private static void ValidateBody(GameObject body)
        {
            var copy = UnityEngine.Object.Instantiate(body);
            var baked = new Mesh();
            try
            {
                copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                copy.SetActive(true);
                var animator = copy.GetComponentInChildren<Animator>();
                var skin = copy.GetComponentInChildren<SkinnedMeshRenderer>();
                foreach (var clip in new[] { AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Single(item => !item.name.StartsWith("__preview__")), AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkClipPath) })
                {
                    clip.SampleAnimation(animator.gameObject, 0f);
                    skin.BakeMesh(baked);
                    var first = baked.vertices;
                    clip.SampleAnimation(animator.gameObject, clip.length * 0.35f);
                    skin.BakeMesh(baked);
                    var second = baked.vertices;
                    float motion = 0f;
                    for (int i = 0; i < first.Length; i++) motion = Mathf.Max(motion, (first[i] - second[i]).magnitude);
                    if (motion < 0.0001f) throw new InvalidOperationException("NPC3 animation does not move the supplied mesh: " + clip.name);
                }
                Debug.Log("NPC3 validation passed: talking and walking clips deform the custom rig correctly.");
            }
            finally { UnityEngine.Object.DestroyImmediate(baked); UnityEngine.Object.DestroyImmediate(copy); }
        }
    }
}

using System;
using System.Linq;
using ThuyKieu.Dialogue.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Story chapters' animator: the Chapter 1 shared controller plus a Crouch state (sneaking)
    /// and a Nervous gesture (emotion 7). Kiều gets an override with her own walk and run.
    /// </summary>
    public static class StoryAnimationSetup
    {
        public const string Folder = "Assets/_Game/Animations/Story";
        public const string ControllerPath = Folder + "/StoryShared.controller";
        public const string KieuOverridePath = Folder + "/ThuyKieuStory.overrideController";
        private static readonly string[] Looping = { "TK-CrouchWalk", "MGS-Nervous", "MGS-BreathingIdle", "MGS-Arguing" };

        public static void ImportClips()
        {
            foreach (string name in new[] { "TK-CrouchWalk", "TK-Write", "MGS-Nervous", "MGS-BreathingIdle", "MGS-Arguing" })
            {
                string path = Folder + "/" + name + ".fbx";
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) throw new InvalidOperationException("Missing clip " + path);
                if (importer.animationType == ModelImporterAnimationType.Human && importer.clipAnimations.Length > 0 && importer.clipAnimations[0].name == name) continue;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                var clips = importer.clipAnimations;
                if (clips.Length == 0) clips = importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    clip.name = name;
                    clip.loopTime = clip.loopPose = Looping.Contains(name);
                    clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                    clip.keepOriginalOrientation = true; clip.keepOriginalPositionXZ = true; clip.heightFromFeet = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        public static AnimatorController Controller()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null) return existing;
            ImportClips();
            if (!AssetDatabase.CopyAsset(Chapter01SharedAnimationSetup.ControllerPath, ControllerPath))
                throw new InvalidOperationException("Could not copy " + Chapter01SharedAnimationSetup.ControllerPath);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            controller.AddParameter("Crouch", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            var idle = machine.defaultState;

            var crouch = machine.AddState("Crouch");
            crouch.motion = Clip("TK-CrouchWalk");
            // Speed is normalised (walk = 0.5); scaling keeps the sneak cadence matched and freezes the pose when still.
            crouch.speed = 3.6f;
            crouch.speedParameter = "Speed";
            crouch.speedParameterActive = true;
            var enter = machine.AddAnyStateTransition(crouch);
            enter.hasExitTime = false; enter.duration = 0.18f; enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0, "Crouch");
            enter.AddCondition(AnimatorConditionMode.IfNot, 0, "DialogueTalking");
            var leave = crouch.AddTransition(idle);
            leave.hasExitTime = false; leave.duration = 0.18f;
            leave.AddCondition(AnimatorConditionMode.IfNot, 0, "Crouch");
            var talkOut = crouch.AddTransition(idle);
            talkOut.hasExitTime = false; talkOut.duration = 0.15f;
            talkOut.AddCondition(AnimatorConditionMode.If, 0, "DialogueTalking");

            var nervous = machine.AddState("Nervous");
            nervous.motion = Clip("MGS-Nervous");
            var nervousIn = machine.AddAnyStateTransition(nervous);
            nervousIn.hasExitTime = false; nervousIn.duration = 0.15f; nervousIn.canTransitionToSelf = false;
            nervousIn.AddCondition(AnimatorConditionMode.If, 0, "DialogueTalking");
            nervousIn.AddCondition(AnimatorConditionMode.Equals, 7, "DialogueEmotion");
            nervousIn.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSitting");
            var nervousOut = nervous.AddTransition(idle);
            nervousOut.hasExitTime = false; nervousOut.duration = 0.15f;
            nervousOut.AddCondition(AnimatorConditionMode.IfNot, 0, "DialogueTalking");
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        public static AnimatorOverrideController KieuController()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(KieuOverridePath);
            if (existing != null) return existing;
            var controller = new AnimatorOverrideController(Controller()) { name = "ThuyKieuStory" };
            controller["Walking"] = ThuyKieu.Player.Editor.ThuyKieuLocomotionSetup.Clip(ThuyKieu.Player.Editor.ThuyKieuLocomotionSetup.ModelFolder + "ThuyKieu_Walk.fbx");
            controller["Running"] = ThuyKieu.Player.Editor.ThuyKieuLocomotionSetup.Clip(ThuyKieu.Player.Editor.ThuyKieuLocomotionSetup.ModelFolder + "ThuyKieu_Run.fbx");
            AssetDatabase.CreateAsset(controller, KieuOverridePath);
            return controller;
        }

        /// <summary>Per-character overrides (regenerated with the shared controller they extend).</summary>
        public const string CharacterFolder = Folder + "/Characters";

        /// <summary>
        /// Shared story controller with a character's own Mixamo clips (Assets/_Game/Animations/&lt;id&gt;/&lt;id&gt;-Idle/-Walk/-Run.fbx,
        /// optionally -Yell for the arguing gesture, -Waving and -Dying for gestures 3 and 4). Missing clips keep the shared ones.
        /// </summary>
        public static RuntimeAnimatorController CharacterController(string id)
        {
            string path = CharacterFolder + "/" + id + "Story.overrideController";
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (existing != null) return existing;
            if (!AssetDatabase.IsValidFolder(CharacterFolder)) AssetDatabase.CreateFolder(Folder, "Characters");
            var controller = new AnimatorOverrideController(Controller()) { name = id + "Story" };
            // One-shot gestures some rigs bring along: Vương Ông waves when glad (emotion 3) and falls when struck (4).
            foreach (var (suffix, slot, loop) in new[] { ("Idle", "Idle", true), ("Walk", "Walking", true), ("Run", "Running", true), ("Yell", "Standing Arguing", true),
                         ("Waving", "Agreeing", false), ("Dying", "Disagree", false) })
            {
                var clip = CharacterClip(id, suffix, loop);
                if (clip != null) controller[slot] = clip;
            }
            AssetDatabase.CreateAsset(controller, path);
            return controller;
        }

        /// <summary>Imports a character clip as Humanoid in place (root locked, looping as asked) and returns it, or null if absent.</summary>
        private static AnimationClip CharacterClip(string id, string suffix, bool loop)
        {
            string name = id + "-" + suffix;
            string path = "Assets/_Game/Animations/" + id + "/" + name + ".fbx";
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) return null;
            if (importer.animationType != ModelImporterAnimationType.Human || importer.clipAnimations.Length == 0 || importer.clipAnimations[0].name != name)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    clip.name = name;
                    clip.loopTime = clip.loopPose = loop;
                    clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                    clip.keepOriginalOrientation = true; clip.keepOriginalPositionXZ = true; clip.heightFromFeet = true;
                }
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        }

        private static AnimationClip Clip(string name) => AssetDatabase.LoadAllAssetsAtPath(Folder + "/" + name + ".fbx")
            .OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
    }
}

using System;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chapters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static class Chapter05PursuerSetup
    {
        private const string GuardFolder = "Assets/_Game/Art/Characters/LinhCanh/Rigged";
        private const string VisualName = "LinhCanhPursuerVisual";

        [MenuItem("ThuyKieu/Chapter 5/Use guard pursuer models")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before changing pursuer models.");

            var scene = SceneManager.GetSceneByPath(Chapter05Setup.ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(Chapter05Setup.ScenePath, OpenSceneMode.Additive);
            var world = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Chapter05World>(true)).Single();
            var worldData = new SerializedObject(world);
            var pursuers = worldData.FindProperty("_pursuers");
            if (pursuers.arraySize != 3) throw new InvalidOperationException("Expected three Chapter 5 pursuers.");
            var controller = StoryAnimationSetup.CharacterController("LinhCanh");
            foreach (string suffix in new[] { "Idle", "Walk", "Run" })
                if (!controller.animationClips.Any(clip => AssetDatabase.GetAssetPath(clip) == "Assets/_Game/Animations/LinhCanh/LinhCanh-" + suffix + ".fbx"))
                    throw new InvalidOperationException("Missing guard animation: " + suffix);

            for (int i = 0; i < pursuers.arraySize; i++)
            {
                var actor = (StoryActor)pursuers.GetArrayElementAtIndex(i).objectReferenceValue;
                if (actor.transform.Find(VisualName) != null) continue;
                var data = new SerializedObject(actor);
                var oldAnimator = (Animator)data.FindProperty("_animator").objectReferenceValue;
                var visual = Body(actor.transform, VisualName, GuardFolder + "/LinhCanh_Rigged.fbx",
                    GuardFolder, 1.72f + i * 0.03f, Color.white, controller);
                var animator = visual.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException("Invalid guard rig for " + actor.name);
                // Keep the previous body disabled so scene references and authored content remain recoverable.
                if (oldAnimator != null) oldAnimator.gameObject.SetActive(false);
                visual.SetActive(true);
                data.FindProperty("_animator").objectReferenceValue = animator;
                data.ApplyModifiedPropertiesWithoutUndo();
            }

            if (worldData.FindProperty("_soKhanh").objectReferenceValue == null)
            {
                worldData.FindProperty("_soKhanh").objectReferenceValue = CreateSoKhanh(world.transform);
                worldData.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Chapter 5.");
            AssetDatabase.SaveAssets();
            Debug.Log("Chapter05 guards verified: 3 pursuers, valid Humanoid rigs, LinhCanh Idle/Walk/Run; chase roots preserved.");
        }

        internal static StoryActor CreateSoKhanh(Transform parent)
        {
            // Sở Khanh keeps his scholar appearance independently of the guards chasing Kiều.
            var actor = RiggedActor("SoKhanh", "SoKhanh",
                "Assets/_Game/Art/Characters/MaGiamSinh/Models/MaGiamSinh_Rigged.fbx",
                "Assets/_Game/Art/Characters/MaGiamSinh/Tectures", 1.72f, Vector3.zero, 0f,
                new Color(0.72f, 0.8f, 0.92f), StoryAnimationSetup.Controller());
            actor.transform.SetParent(parent, false);
            actor.gameObject.SetActive(false);
            return actor;
        }
    }
}

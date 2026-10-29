using System;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Editor;
using ThuyKieu.Core.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Reproduction helper, outside Assets. Copy into an isolated project's Assets/Editor before running.
public static class UpgradeTeaVendor
{
    private const string Model = "Assets/_Game/Art/Characters/BaHangNuoc/Models/BaHangNuoc_Rigged.fbx";
    private const string Textures = "Assets/_Game/Art/Characters/BaHangNuoc/Textures";

    public static void Run()
    {
        try
        {
            RemoveBatchSearchCallback();
            var scene = EditorSceneManager.OpenScene(Chapter02Setup.ScenePath, OpenSceneMode.Single);
            var actor = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StoryActor>(true))
                .Single(candidate => candidate.SpeakerId == "BaHangNuoc");
            var oldAnimator = actor.GetComponentInChildren<Animator>(true);
            Require(oldAnimator != null, "Existing vendor animator is missing");
            Require(oldAnimator.runtimeAnimatorController != null, "Existing vendor controller is missing");
            var oldVisual = oldAnimator.transform;
            while (oldVisual.parent != actor.transform) oldVisual = oldVisual.parent;

            var preserved = new Dictionary<Component, string>();
            foreach (var component in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Component>(true)))
            {
                if (component == null || component == actor || component == actor.transform || component.transform.IsChildOf(oldVisual)) continue;
                preserved.Add(component, EditorJsonUtility.ToJson(component));
            }

            StorySceneKit.AssetFolder = StorySceneKit.MaterialFolder;
            var temporary = StorySceneKit.RiggedActor("BaHangNuoc", "BaHangNuoc", Model, Textures, 1.56f,
                actor.transform.position, actor.transform.eulerAngles.y, Color.white, oldAnimator.runtimeAnimatorController);
            var newVisual = temporary.transform.GetChild(0);
            newVisual.SetParent(actor.transform, true);
            var newAnimator = newVisual.GetComponentInChildren<Animator>(true);
            StorySceneKit.GroundFeet(newAnimator.transform, newVisual.gameObject, actor.transform.position.y);
            if (oldAnimator.GetComponent<AnimatorRootAnchor>() != null)
                newAnimator.gameObject.AddComponent<AnimatorRootAnchor>();
            StorySceneKit.Set(actor, "_animator", newAnimator);
            Object.DestroyImmediate(oldVisual.gameObject);
            Object.DestroyImmediate(temporary.gameObject);

            foreach (var entry in preserved)
                Require(entry.Key != null && EditorJsonUtility.ToJson(entry.Key) == entry.Value,
                    "Unrelated scene component changed: " + (entry.Key != null ? entry.Key.name : "destroyed"));
            Debug.Log("[GiaThuanMerge] Preserved " + preserved.Count + " unrelated scene components byte-for-byte in Editor serialization");
            ValidateVendor(actor);
            Require(EditorSceneManager.SaveScene(scene), "Could not save Chapter 2");
            AssetDatabase.SaveAssets();
            scene = EditorSceneManager.OpenScene(Chapter02Setup.ScenePath, OpenSceneMode.Single);
            actor = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StoryActor>(true))
                .Single(candidate => candidate.SpeakerId == "BaHangNuoc");
            ValidateVendor(actor);
            foreach (var transform in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0,
                    "Missing script on " + transform.name);

            var vanModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Characters/ThuyVan/Models/ThuyVan_Rigged.fbx");
            Require(vanModel != null && vanModel.GetComponentInChildren<SkinnedMeshRenderer>(true) != null,
                "Thuy Van model inherited from dev is missing");
            Debug.Log("[GiaThuanMerge] PASS: vendor survives scene reload, humanoid avatar/controller/textures resolve, Thuy Van model resolves, no missing scene scripts");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void ValidateVendor(StoryActor actor)
    {
        var animator = actor.GetComponentInChildren<Animator>(true);
        Require(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
            "Vendor humanoid avatar is invalid");
        Require(animator.runtimeAnimatorController != null, "Vendor controller is missing");
        Require(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(animator.gameObject) == Model,
            "Vendor still uses the previous model");
        var serialized = new SerializedObject(actor);
        Require(serialized.FindProperty("_animator").objectReferenceValue == animator, "StoryActor animator link is stale");
        var skins = actor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Require(skins.Length > 0, "Vendor has no skinned mesh renderer");
        foreach (var renderer in skins)
        {
            Require(renderer.sharedMesh != null && renderer.bones.All(bone => bone != null), "Vendor skin has missing mesh or bones");
            foreach (var material in renderer.sharedMaterials)
                Require(material != null && AssetDatabase.GetAssetPath(material.GetTexture("_BaseMap")) == Textures + "/texture_pbr_20250901.png",
                    "Vendor base texture does not point at the new model's texture");
        }
        Debug.Log("[GiaThuanMerge] Vendor avatar valid, head height " + actor.HeadHeight.ToString("F3") + " m");
    }

    public static void Playtest()
    {
        RemoveBatchSearchCallback();
        StoryBuild.PlaytestBatch();
    }

    private static void RemoveBatchSearchCallback()
    {
        foreach (Delegate callback in EditorApplication.delayCall?.GetInvocationList() ?? Array.Empty<Delegate>())
            if (callback.Method.Name == "IndexationOnStartup" && callback.Method.DeclaringType?.FullName == "UnityEditor.Search.SearchInit")
                EditorApplication.delayCall -= (EditorApplication.CallbackFunction)callback;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

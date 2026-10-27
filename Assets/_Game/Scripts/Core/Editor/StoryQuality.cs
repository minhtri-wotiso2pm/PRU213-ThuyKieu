using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The PC quality preset's frame cost. It rendered at twice the screen resolution with 8× MSAA (16 times the pixel
    /// work of a plain frame, for edges already smooth at 4×): now the screen's own resolution with 4× MSAA, which halves
    /// the frame time in every chapter (StoryPerfProbe -perfQuick, Editor 1080p). Shadows, SSAO and post stay as they were:
    /// lowering them saved half a millisecond for a visible loss. Cut-out leaves keep smooth edges through alpha to
    /// coverage (their materials' _AlphaToMask).
    ///   Menu: ThuyKieu ▸ Tools ▸ PC quality: native resolution, MSAA 4×
    ///   Batch: -executeMethod ThuyKieu.Core.Editor.StoryQuality.ApplyPcBatch
    /// </summary>
    public static class StoryQuality
    {
        public const string PcAssetPath = "Assets/Settings/PC_RPAsset.asset";

        [MenuItem("ThuyKieu/Tools/PC quality: native resolution, MSAA 4x")]
        public static void ApplyPc()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PcAssetPath);
            if (asset == null) { Debug.LogError("[StoryQuality] Missing " + PcAssetPath); return; }
            float scale = asset.renderScale;
            int msaa = asset.msaaSampleCount;
            asset.renderScale = 1f;
            asset.msaaSampleCount = 4;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[StoryQuality] PC preset: render scale {scale} → {asset.renderScale}, MSAA {msaa}× → {asset.msaaSampleCount}×");
        }

        public static void ApplyPcBatch()
        {
            ApplyPc();
            EditorApplication.Exit(0);
        }
    }
}

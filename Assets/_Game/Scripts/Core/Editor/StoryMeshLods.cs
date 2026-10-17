using UnityEditor;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// The generated environment models (trees, props, architecture) carry hundreds of thousands of triangles each.
    /// Turning on Unity's automatic Mesh LOD makes distant copies draw a decimated version of the same mesh,
    /// which is what keeps the tree-lined Chapter 2 road at a playable frame rate. Safe to run repeatedly.
    /// </summary>
    public static class StoryMeshLods
    {
        private static readonly string[] Folders =
        {
            "Assets/_Game/Art/Environment/Vegetation", "Assets/_Game/Art/Environment/Props", "Assets/_Game/Art/Environment/Architecture"
        };

        [MenuItem("ThuyKieu/Tools/Generate mesh LODs for environment models")]
        public static void Apply()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", Folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not ModelImporter importer || importer.generateMeshLods) continue;
                importer.generateMeshLods = true;
                importer.SaveAndReimport();
                Debug.Log("[MeshLods] " + path);
            }
        }

        public static void ApplyBatch()
        {
            try { Apply(); EditorApplication.Exit(0); }
            catch (System.Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
    }
}

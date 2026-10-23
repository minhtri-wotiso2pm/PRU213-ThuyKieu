using System.Collections.Generic;
using System.Text;
using ThuyKieu.Core.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Stores where each rigged story character's mouth is (Resources/MouthSpots) for <see cref="ActorMouth"/>.
    /// The painted lips were read off front close-ups of each face (the table below); the baker finds the skin
    /// surface there on the posed mesh and records the spot relative to the head bone, so it rides every animation.
    /// The cartoon classroom extras are static meshes with mouths already painted open, so they are left out.
    /// </summary>
    public static class MouthSpotBaker
    {
        private const string AssetPath = "Assets/_Game/Resources/MouthSpots.asset";
        private const float SkinOffset = 0.0025f, OpeningOfLips = 0.7f;

        public static readonly string[] StoryScenes =
        {
            "Assets/_Game/Scenes/Chapter00_Prologue.unity",
            "Assets/_Game/Scenes/Chapter01_GiaBien.unity",
            "Assets/_Game/Scenes/Chapter02_RoadToMaGiamSinh.unity"
        };

        // Avatar → centre of the painted lips in the actor's frame (cm across to the actor's right, cm up from the head
        // bone) and the lips' width in cm.
        private static readonly Dictionary<string, Vector3> Lips = new Dictionary<string, Vector3>
        {
            { "ThuyKieu_WalkAvatar", new Vector3(0f, 0f, 2.2f) },
            { "idleAvatar", new Vector3(0.4f, 0.75f, 3f) },       // the modern student Kiều wakes up as
            { "seatAvatar", new Vector3(0f, 3.1f, 4.2f) },        // Ngân at her desk
            { "MeThuyKieu_RiggedAvatar", new Vector3(0f, 3.1f, 3.8f) },
            { "MaGiamSinh_RiggedAvatar", new Vector3(0f, 2.9f, 3.7f) },
            { "VuongOng_RiggedAvatar", new Vector3(0f, -0.25f, 4f) }, // under the moustache
            { "LinhCanh_RiggedAvatar", new Vector3(0.4f, 4.75f, 3.7f) }
        };

        [MenuItem("ThuyKieu/Story/Bake Mouth Spots (Chapters 0–2)")]
        private static void BakeMenu() => Debug.Log(Bake(StoryScenes));

        /// <summary>Opens each scene alongside the current one (never saving it), measures its actors, and returns a report.</summary>
        public static string Bake(IEnumerable<string> scenePaths)
        {
            var spots = AssetDatabase.LoadAssetAtPath<MouthSpots>(AssetPath);
            if (spots == null)
            {
                spots = ScriptableObject.CreateInstance<MouthSpots>();
                AssetDatabase.CreateAsset(spots, AssetPath);
            }
            var report = new StringBuilder();
            var done = new HashSet<string>();
            foreach (string path in scenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool opened = !scene.isLoaded;
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                spots.AddScene(scene.name);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var actor in root.GetComponentsInChildren<StoryActor>(true))
                        foreach (var face in actor.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                            Measure(actor, face, spots, done, report);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
            EditorUtility.SetDirty(spots);
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        private static void Measure(StoryActor actor, SkinnedMeshRenderer face, MouthSpots spots, HashSet<string> done, StringBuilder report)
        {
            string key = MouthSpots.KeyFor(face);
            var animator = face.GetComponentInParent<Animator>(true);
            if (key == null || done.Contains(key) || animator == null || animator.avatar == null) return;
            if (!Lips.TryGetValue(animator.avatar.name, out Vector3 lips)) return;
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;

            // The actor's frame: x to its right, y up, z out of its face.
            var frame = Matrix4x4.TRS(actor.transform.position, actor.transform.rotation, Vector3.one);
            var toFrame = frame.inverse;
            Vector3 bone = toFrame.MultiplyPoint3x4(head.position);
            float x = bone.x + lips.x * 0.01f, y = bone.y + lips.y * 0.01f;

            // The skin surface over the lips: the furthest-forward vertex right there.
            float depth = float.MinValue;
            foreach (var world in WorldVertices(face))
            {
                Vector3 p = toFrame.MultiplyPoint3x4(world);
                if (Mathf.Abs(p.x - x) < 0.01f && Mathf.Abs(p.y - y) < 0.008f) depth = Mathf.Max(depth, p.z);
            }
            if (depth == float.MinValue) { report.AppendLine($"{actor.name} {key}: no skin at the marked lips, skipped"); return; }

            Vector3 spotWorld = frame.MultiplyPoint3x4(new Vector3(x, y, depth + SkinOffset));
            spots.Set(new MouthSpots.Spot
            {
                Key = key,
                OnHeadBone = true,
                Position = head.InverseTransformPoint(spotWorld),
                Rotation = Quaternion.Inverse(head.rotation) * actor.transform.rotation,
                Width = lips.z * 0.01f * OpeningOfLips / Mathf.Max(head.lossyScale.x, 1e-6f)
            });
            done.Add(key);
            report.AppendLine($"{actor.name} ({actor.SpeakerId}) {key}: lips {depth - bone.z:F3} m in front of the head bone");
        }

        private static Vector3[] WorldVertices(SkinnedMeshRenderer face)
        {
            // Without useScale the bones' scale (where these rigs carry their import scale) is already applied:
            // only the renderer's position and rotation remain.
            var baked = new Mesh();
            face.BakeMesh(baked, false);
            var vertices = baked.vertices;
            Object.DestroyImmediate(baked);
            for (int i = 0; i < vertices.Length; i++) vertices[i] = face.transform.position + face.transform.rotation * vertices[i];
            return vertices;
        }
    }
}

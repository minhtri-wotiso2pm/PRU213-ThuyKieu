using System.Collections.Generic;
using ThuyKieu.Dialogue;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// Opens a mouth on an actor's face while they speak, following the loudness of their recorded voice (or a
    /// syllable rhythm for lines with no recording). The models have their lips painted on, so this lays a small dark
    /// opening over the painted lips at the spot baked into <see cref="MouthSpots"/>, riding the head bone.
    /// </summary>
    public class ActorMouth : MonoBehaviour
    {
        private const float MaxOpening = 0.45f; // height of the open mouth as a fraction of its width
        private const float OpenSpeed = 22f, CloseSpeed = 14f;

        private static Mesh _mesh;
        private static Material _material;

        private readonly List<(Transform mouth, Renderer renderer, Renderer face, float width)> _mouths = new List<(Transform, Renderer, Renderer, float)>();
        private StoryActor _actor;
        private float _open;

        /// <summary>Opens the mouth fully regardless of speech, for lining spots up in the editor.</summary>
        public static bool ForceOpen { get; set; }

        public static void Attach(StoryActor actor)
        {
            if (!MouthSpots.Covers(actor.gameObject.scene.name)) return;
            var mouth = actor.gameObject.AddComponent<ActorMouth>();
            mouth._actor = actor;
            mouth.Build();
            if (mouth._mouths.Count == 0) Destroy(mouth);
        }

        private void Build()
        {
            foreach (var face in GetComponentsInChildren<Renderer>(true))
            {
                if (!MouthSpots.TryGet(MouthSpots.KeyFor(face), out var spot)) continue;
                Transform parent = face.transform;
                if (spot.OnHeadBone)
                {
                    var animator = face.GetComponentInParent<Animator>(true);
                    parent = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
                    if (parent == null) continue;
                }
                var mouth = new GameObject("Mouth", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                mouth.SetParent(parent, false);
                mouth.localPosition = spot.Position;
                mouth.localRotation = spot.Rotation;
                mouth.localScale = Vector3.zero;
                mouth.GetComponent<MeshFilter>().sharedMesh = SharedMesh();
                var renderer = mouth.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = SharedMaterial();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.enabled = false;
                _mouths.Add((mouth, renderer, face, spot.Width));
            }
        }

        private void LateUpdate()
        {
            float target = ForceOpen ? 1f : VoicePlayer.MouthLevel(_actor.SpeakerId);
            _open = Mathf.MoveTowards(_open, target, (target > _open ? OpenSpeed : CloseSpeed) * Time.deltaTime);
            foreach (var (mouth, renderer, face, width) in _mouths)
            {
                bool show = _open > 0.04f && face.enabled && face.gameObject.activeInHierarchy;
                renderer.enabled = show;
                if (show) mouth.localScale = new Vector3(width * Mathf.Lerp(0.85f, 1f, _open), width * MaxOpening * _open, width);
            }
        }

        /// <summary>An oval disc of unit width, its edges curving back like lips wrapping round the face.</summary>
        private static Mesh SharedMesh()
        {
            if (_mesh != null) return _mesh;
            const int segments = 28;
            var vertices = new Vector3[segments + 1];
            var uv = new Vector2[segments + 1];
            var triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Cos(angle) * 0.5f, y = Mathf.Sin(angle) * 0.5f;
                vertices[i + 1] = new Vector3(x, y, -0.2f * (x * 2f) * (x * 2f));
                uv[i + 1] = new Vector2(0.5f + x, 0.5f + y);
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            _mesh = new Mesh { name = "ActorMouth", vertices = vertices, uv = uv, triangles = triangles };
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            return _mesh;
        }

        private static Material SharedMaterial()
        {
            if (_material != null) return _material;
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_Cull", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetTexture("_BaseMap", Paint());
            material.SetColor("_BaseColor", Color.white);
            _material = material;
            return material;
        }

        /// <summary>A dark mouth with a hint of tongue low down and soft lip-coloured edges.</summary>
        private static Texture2D Paint()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Color edge = new Color(0.42f, 0.14f, 0.13f), inside = new Color(0.07f, 0.02f, 0.025f), tongue = new Color(0.36f, 0.09f, 0.1f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v);
                    Color color = Color.Lerp(inside, edge, Ramp(0.78f, 0.98f, r));
                    color = Color.Lerp(color, tongue, Mathf.Clamp01(-v - 0.45f) * (1f - Mathf.Abs(u)) * 0.8f);
                    color.a = 1f - Ramp(0.88f, 1f, r);
                    texture.SetPixel(x, y, color);
                }
            texture.Apply();
            return texture;
        }

        /// <summary>0 below <paramref name="from"/>, 1 above <paramref name="to"/>, eased in between.</summary>
        private static float Ramp(float from, float to, float value) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
    }
}

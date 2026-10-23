using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// A figure made of light: a rigged humanoid drawn as faint glowing glass, with motes streaming off its skeleton.
    /// It gathers out of the air and scatters as drifting peach petals. The motes follow the bones, so the model's
    /// mesh does not need to be readable.
    /// </summary>
    public class LightApparition : MonoBehaviour
    {
        private const float MotesPerSecond = 520f, GlassAlpha = 0.13f;
        private static readonly Color MoteColor = new Color(1f, 0.78f, 0.45f), PetalColor = new Color(1f, 0.5f, 0.64f);

        // Segments of the body the motes are scattered along: (from, to, thickness). The two hips-to-shin pieces
        // fill out the scholar's long robe.
        private static readonly (HumanBodyBones from, HumanBodyBones to, float radius)[] Limbs =
        {
            (HumanBodyBones.Hips, HumanBodyBones.Spine, 0.13f), (HumanBodyBones.Spine, HumanBodyBones.Chest, 0.14f),
            (HumanBodyBones.Chest, HumanBodyBones.Neck, 0.13f), (HumanBodyBones.Neck, HumanBodyBones.Head, 0.05f),
            (HumanBodyBones.Head, HumanBodyBones.Head, 0.1f), (HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm, 0.07f),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, 0.06f), (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, 0.07f),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, 0.06f), (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, 0.07f),
            (HumanBodyBones.Hips, HumanBodyBones.LeftLowerLeg, 0.17f), (HumanBodyBones.Hips, HumanBodyBones.RightLowerLeg, 0.17f),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, 0.14f), (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, 0.14f)
        };

        private Transform[,] _bones;
        private Transform _upperArm, _hand;
        private Vector3 _offerTarget;
        private float _offerWeight;
        private Material _glass;
        private Color _glassBase, _glassGlow;
        private ParticleSystem _motes, _petals;
        private Light _glow;
        private float _presence, _gathering, _emitDebt;

        public static LightApparition Create(GameObject model, RuntimeAnimatorController controller, Vector3 position, Quaternion rotation, float height)
        {
            var root = new GameObject(model.name + "_Apparition");
            var apparition = root.AddComponent<LightApparition>();
            apparition.Build(model, controller, height);
            root.transform.SetPositionAndRotation(position, rotation);
            return apparition;
        }

        /// <summary>Motes drift in from the air around him and settle into his shape.</summary>
        public IEnumerator Gather(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                _presence = Mathf.SmoothStep(0f, 1f, t / duration);
                _gathering = 1f - _presence;
                yield return null;
            }
            _presence = 1f;
            _gathering = 0f;
        }

        /// <summary>Drifts a few steps to <paramref name="to"/> (on the ground), the way a figure of light would walk.</summary>
        public IEnumerator Glide(Vector3 to, float duration)
        {
            Vector3 from = transform.position;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            transform.position = to;
        }

        /// <summary>Raises his right hand towards <paramref name="target"/> (0 = arm as animated, 1 = reaching it).</summary>
        public void Offer(Vector3 target, float weight)
        {
            _offerTarget = target;
            _offerWeight = Mathf.Clamp01(weight);
        }

        /// <summary>
        /// He comes apart into peach petals that first circle once round <paramref name="centre"/> (whoever stood before
        /// him) as they rise, then are carried off towards <paramref name="away"/>; then the object goes.
        /// </summary>
        public IEnumerator Scatter(Vector3 away, float duration, Vector3 centre)
        {
            _petals.transform.position = centre;
            var swirl = _petals.velocityOverLifetime;
            swirl.enabled = true;
            swirl.space = ParticleSystemSimulationSpace.Local;
            swirl.orbitalY = new ParticleSystem.MinMaxCurve(1.6f);
            swirl.radial = new ParticleSystem.MinMaxCurve(0.08f);
            swirl.y = new ParticleSystem.MinMaxCurve(0.25f);
            yield return Scatter(away, duration);
        }

        /// <summary>He comes apart into peach petals carried off towards <paramref name="away"/>, then the object goes.</summary>
        public IEnumerator Scatter(Vector3 away, float duration)
        {
            away = Vector3.ProjectOnPlane(away, Vector3.up).normalized;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                _presence = 1f - Mathf.SmoothStep(0f, 1f, k);
                int petals = Mathf.RoundToInt(Mathf.Lerp(14f, 2f, k) * 60f * Time.deltaTime);
                for (int i = 0; i < petals; i++) EmitPetal(away);
                yield return null;
            }
            _presence = 0f;
            Destroy(gameObject, _petals.main.startLifetime.constantMax + 0.5f);
        }

        private void Build(GameObject model, RuntimeAnimatorController controller, float height)
        {
            // Built at the origin and posed by Create afterwards: the fit below works in world space.
            var body = Instantiate(model, transform).transform;
            body.localPosition = Vector3.zero;
            body.localRotation = Quaternion.identity;
            var renderers = body.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            float scale = height / bounds.size.y;
            body.localScale *= scale;
            body.localPosition = new Vector3(-bounds.center.x * scale, -bounds.min.y * scale, -bounds.center.z * scale);

            var animator = body.GetComponentInChildren<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            _upperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _bones = new Transform[Limbs.Length, 2];
            for (int i = 0; i < Limbs.Length; i++)
            {
                _bones[i, 0] = animator.GetBoneTransform(Limbs[i].from);
                _bones[i, 1] = animator.GetBoneTransform(Limbs[i].to);
            }

            _glassBase = new Color(0.86f, 0.92f, 1f, 0f);
            _glassGlow = new Color(1f, 0.72f, 0.42f) * 0.55f;
            _glass = Glass(_glassBase, _glassGlow);
            foreach (var renderer in body.GetComponentsInChildren<Renderer>())
            {
                var materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++) materials[i] = _glass;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;
            }

            // His light falls on whoever stands before him and on the wall behind.
            _glow = new GameObject("Glow").AddComponent<Light>();
            _glow.transform.SetParent(transform, false);
            _glow.transform.localPosition = new Vector3(0f, height * 0.65f, 0.35f);
            _glow.type = LightType.Point;
            _glow.color = new Color(1f, 0.82f, 0.6f);
            _glow.range = 3.5f;
            _glow.intensity = 0f;
            _glow.shadows = LightShadows.None;

            _motes = Particles("Motes", Dot(), MoteColor * 3f, true, 4000);
            var moteFade = _motes.colorOverLifetime;
            moteFade.enabled = true;
            moteFade.color = Fade(0.15f, 0.6f);

            _petals = Particles("Petals", Petal(), PetalColor, false, 1500);
            var main = _petals.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var spin = _petals.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            var noise = _petals.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.3f;
            var petalFade = _petals.colorOverLifetime;
            petalFade.enabled = true;
            petalFade.color = Fade(0.05f, 0.7f);
        }

        private void Update()
        {
            if (_glass == null) return;
            Color glass = _glassBase;
            glass.a = _presence * GlassAlpha;
            _glass.SetColor("_BaseColor", glass);
            _glass.SetColor("_EmissionColor", _glassGlow * _presence);
            _glow.intensity = _presence * 2.2f;

            _emitDebt += MotesPerSecond * _presence * Time.deltaTime;
            for (; _emitDebt >= 1f; _emitDebt -= 1f) EmitMote();
        }

        // After the animation: swing the whole right arm from the shoulder so the hand points at the offered spot.
        private void LateUpdate()
        {
            if (_offerWeight <= 0f || _upperArm == null || _hand == null) return;
            Vector3 current = _hand.position - _upperArm.position;
            Vector3 wanted = _offerTarget - _upperArm.position;
            Quaternion turn = Quaternion.FromToRotation(current, wanted);
            _upperArm.rotation = Quaternion.Slerp(Quaternion.identity, turn, _offerWeight) * _upperArm.rotation;
        }

        private void EmitMote()
        {
            Vector3 at = PointOnBody(out float radius);
            var emit = new ParticleSystem.EmitParams
            {
                startLifetime = Random.Range(0.6f, 1.4f),
                startSize = Random.Range(0.008f, 0.022f),
                startColor = Color.white
            };
            if (_gathering > 0.01f)
            {
                // While he is still forming, motes come in from further out and fly to their place on him.
                Vector3 from = at + Random.insideUnitSphere * (0.25f + 1.2f * _gathering);
                emit.position = from;
                emit.velocity = (at - from) / emit.startLifetime * 0.9f;
            }
            else
            {
                emit.position = at + Random.insideUnitSphere * radius * 0.2f;
                emit.velocity = Vector3.up * Random.Range(0.02f, 0.14f) + Random.insideUnitSphere * 0.03f;
            }
            _motes.Emit(emit, 1);
        }

        private void EmitPetal(Vector3 away)
        {
            var emit = new ParticleSystem.EmitParams
            {
                position = PointOnBody(out _),
                velocity = away * Random.Range(0.5f, 1.3f) + Vector3.up * Random.Range(0.2f, 0.7f) + Random.insideUnitSphere * 0.25f,
                startSize = Random.Range(0.03f, 0.055f),
                startColor = Color.Lerp(Color.white, new Color(1f, 0.7f, 0.8f), Random.value)
            };
            _petals.Emit(emit, 1);
        }

        private Vector3 PointOnBody(out float radius)
        {
            int i = Random.Range(0, Limbs.Length);
            radius = Limbs[i].radius;
            Transform a = _bones[i, 0], b = _bones[i, 1];
            if (a == null || b == null) return transform.position + Vector3.up * Random.Range(0.1f, 1.6f);
            return Vector3.Lerp(a.position, b.position, Random.value) + Random.insideUnitSphere * radius;
        }

        private ParticleSystem Particles(string name, Texture2D texture, Color tint, bool additive, int max)
        {
            var host = new GameObject(name);
            host.transform.SetParent(transform, false);
            var system = host.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.playOnAwake = false;
            main.loop = true; // keeps simulating the hand-emitted particles for as long as he exists
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = max;
            main.startSpeed = 0f;
            var emission = system.emission;
            emission.enabled = false;
            var shape = system.shape;
            shape.enabled = false;
            var renderer = host.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = ParticleMaterial(texture, tint, additive);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            system.Play();
            return system;
        }

        private static Gradient Fade(float fadeIn, float fadeOut)
        {
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeIn), new GradientAlphaKey(1f, fadeOut), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

        private static Material Glass(Color color, Color glow)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Transparent(material, false);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.8f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", glow);
            return material;
        }

        private static Material ParticleMaterial(Texture2D texture, Color tint, bool additive)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            Transparent(material, additive);
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", tint);
            return material;
        }

        private static void Transparent(Material material, bool additive)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        /// <summary>A soft round glow.</summary>
        private static Texture2D Dot()
        {
            return Paint(32, (x, y) =>
            {
                float r = Mathf.Clamp01(new Vector2(x, y).magnitude);
                return (1f - r) * (1f - r);
            });
        }

        /// <summary>A petal: a soft oval, narrow at the stem, with a small notch at the tip.</summary>
        private static Texture2D Petal()
        {
            return Paint(32, (x, y) =>
            {
                float width = Mathf.Lerp(0.25f, 0.65f, (y + 1f) * 0.5f);
                float d = new Vector2(x / width, y).magnitude;
                float notch = y > 0.75f && Mathf.Abs(x) < 0.08f ? 0f : 1f;
                return Mathf.Clamp01((1f - d) * 6f) * notch;
            });
        }

        private static Texture2D Paint(int size, System.Func<float, float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha(u, v)));
                }
            texture.Apply();
            return texture;
        }
    }
}

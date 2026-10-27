using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.Chase;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    public static partial class Chapter05Forest
    {
        private const float JumpRadius = 0.23f, DuckClearance = 1.45f;

        // ------------------------------------------------------------------ obstacles

        private static void Obstacles(Transform course, Layout layout, Refs refs, ForestGround ground)
        {
            var random = new System.Random(505);
            foreach (var mark in layout.Marks)
            {
                int way = mark.Way == 2 ? 2 : 1;
                switch (mark.Kind)
                {
                    case ChaseCourse.ObstacleKind.Jump: Log(course, layout, ground, mark, way, random); break;
                    case ChaseCourse.ObstacleKind.Duck: Arch(course, layout, ground, mark, way, random); break;
                    case ChaseCourse.ObstacleKind.Hazard: Falling(course, layout, ground, mark, refs, random); break;
                    case ChaseCourse.ObstacleKind.Gap: Ditch(course, layout, ground, mark, way, random); break;
                    case ChaseCourse.ObstacleKind.Squeeze: Squeeze(course, layout, ground, mark, random); break;
                    case ChaseCourse.ObstacleKind.Dodge: Dodge(course, layout, ground, mark, way, random); break;
                }
            }
            Divider(course, layout, ground, random);
        }

        private static Vector3 OnGround(ForestGround ground, Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);

        /// <summary>
        /// A fallen culm across the way, thick and old, a thinner one beside it and its dead leaves: a jump (the top is at
        /// about 0.43 m, well over the step a running foot takes). Its collider is the culm, end to end into both banks.
        /// </summary>
        private static void Log(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, int way, System.Random random)
        {
            Vector3 c = OnGround(ground, layout.At(way, mark.S)), dir = layout.DirAt(way, mark.S);
            float half = layout.HalfAt(way, mark.S), span = 2f * half + 3.4f;
            var root = new GameObject("Log_" + mark.Id);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(c, Quaternion.LookRotation(dir));
            root.layer = EnvironmentLayer;
            root.isStatic = true;
            var collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 0;
            collider.radius = JumpRadius;
            collider.height = span;
            collider.center = new Vector3(0f, 0.2f, 0f);
            var b = new BambooMesh.Builder(2);
            float skew = ((float)random.NextDouble() - 0.5f) * 0.12f;
            BambooMesh.Culm(b, 0, new Vector3(-span * 0.5f, 0.21f, -skew * span * 0.5f), new Vector3(1f, 0f, skew).normalized, span, 0.235f, 0.06f, Vector3.up, 10, 0.9f, 0.42f);
            BambooMesh.Culm(b, 0, new Vector3(-span * 0.45f, 0.08f, 0.24f), new Vector3(1f, 0.02f, -0.08f).normalized, span * 0.85f, 0.07f, 0.1f, Vector3.forward, 7, 1.2f, 0.4f);
            for (int i = 0; i < 6; i++)
            {
                var at = new Vector3(span * 0.5f - 0.4f - (float)random.NextDouble() * 1.6f, 0.3f + (float)random.NextDouble() * 0.25f, ((float)random.NextDouble() - 0.5f) * 0.6f);
                var outward = new Vector3((float)random.NextDouble() - 0.5f, 0f, (float)random.NextDouble() - 0.5f);
                BambooMesh.Spray(b, 1, at, outward, 0.9f, 0.6f, 0.9f, random.Next(2) == 0);
            }
            Visual(root.transform, "C5F_Log_" + mark.Id, b, new[] { CulmMaterial(2), DryLeafMaterial() }, true);
            Link("Vault_" + mark.Id, parent, OnGround(ground, c - dir * 1.1f), OnGround(ground, c + dir * 1.1f), Mathf.Max(0.6f, 2f * half - 0.8f), ChaseLink.Kind.Vault, 0.75f);
        }

        /// <summary>
        /// Culms leaning across the way from bank to bank, their undersides at 1.45 m with leaves hanging lower: standing she
        /// runs into them (her head is at 1.55), ducking she passes (1.08). The collider is the band the culms fill.
        /// </summary>
        private static void Arch(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, int way, System.Random random)
        {
            Vector3 c = OnGround(ground, layout.At(way, mark.S)), dir = layout.DirAt(way, mark.S);
            float half = layout.HalfAt(way, mark.S), span = 2f * half + 3.2f;
            var root = new GameObject("Low_" + mark.Id);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(c, Quaternion.LookRotation(dir));
            root.layer = EnvironmentLayer;
            root.isStatic = true;
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, DuckClearance + 0.45f, 0f);
            box.size = new Vector3(span, 0.9f, mark.Length);
            var b = new BambooMesh.Builder(2);
            // Culms bent over by the wind from one bank, crowns caught in the other: each slopes down across the way at its
            // own angle, the lowest underside over the way still at 1.45 m.
            int culms = 5;
            for (int i = 0; i < culms; i++)
            {
                float z = -mark.Length * 0.42f + mark.Length * 0.84f * i / (culms - 1);
                float r = 0.045f + (float)random.NextDouble() * 0.03f;
                bool fromLeft = i % 2 == 0 ^ random.Next(3) == 0;
                float high = DuckClearance + 0.75f + (float)random.NextDouble() * 0.5f;
                float low = DuckClearance + r + 0.04f + (float)random.NextDouble() * 0.12f;
                float drift = ((float)random.NextDouble() - 0.5f) * 1.4f;
                // Its height over the far edge of the way is the lowest it gets above the running floor.
                float overEdge = Mathf.InverseLerp(-span * 0.5f, span * 0.5f, half);
                float footY = high, tipY = (low - high * (1f - overEdge)) / Mathf.Max(0.01f, overEdge);
                var foot = new Vector3(fromLeft ? -span * 0.5f : span * 0.5f, footY, z - drift * 0.5f);
                var tip = new Vector3(fromLeft ? span * 0.5f : -span * 0.5f, tipY, z + drift * 0.5f);
                var axis = (tip - foot).normalized;
                BambooMesh.Culm(b, 0, foot, axis, Vector3.Distance(foot, tip), r, 0f, Vector3.up, 7, 1f, 0.4f, (float)random.NextDouble());
                for (int k = 0; k < 4; k++)
                {
                    float t = 0.2f + 0.6f * (float)random.NextDouble();
                    var at = foot + axis * span * t;
                    BambooMesh.Spray(b, 1, at, new Vector3((float)random.NextDouble() - 0.5f, 0f, (float)random.NextDouble() - 0.5f), 0.55f, 0.45f, 1.6f, random.Next(2) == 0);
                }
            }
            Visual(root.transform, "C5F_Low_" + mark.Id, b, new[] { CulmMaterial(0), LeafMaterial() }, true);
            float reach = mark.Length * 0.5f + 0.8f;
            Link("Duck_" + mark.Id, parent, OnGround(ground, c - dir * reach), OnGround(ground, c + dir * reach), Mathf.Max(0.6f, 2f * half - 0.8f), ChaseLink.Kind.Duck, 0f);
        }

        /// <summary>Where a falling culm crosses the middle of the way once down (the pilot's mark, the pursuer's link).</summary>
        private static Vector3 HazardCrossing(Layout layout, Layout.Mark mark)
        {
            HazardPose(layout, mark, out var foot, out var fallen, out _);
            Vector3 flat = new Vector3(fallen.x, 0f, fallen.z).normalized;
            Vector3 right = layout.RightAt(mark.S) * (mark.Side < 0 ? -1f : 1f), centre = layout.MoonAt(mark.S);
            float across = Vector3.Dot(foot - centre, right);
            float t = across / Mathf.Max(0.1f, Vector3.Dot(flat, -right));
            return new Vector3(foot.x, centre.y, foot.z) + flat * t;
        }

        /// <summary>A falling culm stands at the edge of the way on its mark's side and falls across, a little forward.</summary>
        private static void HazardPose(Layout layout, Layout.Mark mark, out Vector3 foot, out Vector3 fallen, out Vector3 upright)
        {
            Vector3 dir = layout.DirAt(1, mark.S), right = layout.RightAt(mark.S) * (mark.Side < 0 ? -1f : 1f);
            float half = layout.HalfAt(1, mark.S);
            foot = layout.MoonAt(mark.S - 1.2f) + right * (half - 0.15f);
            Vector3 flat = (-right + dir * 0.35f).normalized;
            fallen = (flat + Vector3.up * Mathf.Tan(8f * Mathf.Deg2Rad)).normalized;
            upright = (Vector3.up - right * 0.14f).normalized;
        }

        /// <summary>The middle of what blocks a sidestep (the side of the way it fills), as the course and the pilot see it.</summary>
        private static Vector3 DodgeCentre(Layout layout, Layout.Mark mark)
        {
            int way = mark.Way == 2 ? 2 : 1;
            return layout.At(way, mark.S) + layout.RightAt(way, mark.S) * (mark.Side < 0 ? -1f : 1f) * layout.HalfAt(way, mark.S) * 0.45f;
        }

        /// <summary>
        /// A sidestep: a mossy boulder come down off the bank with broken culms standing round it, filling one side of the
        /// way to 0.3 m past the middle, so 1.2–1.4 m is left on the other side. Read from afar by its pale rock and the cue
        /// light behind it; run into, it stops her like a culm. Its colliders are on the ground layer, so the NavMesh goes round
        /// it and the pursuer and Cô Lam take the open side as she does.
        /// </summary>
        private static void Dodge(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, int way, System.Random random)
        {
            Vector3 c = OnGround(ground, layout.At(way, mark.S)), dir = layout.DirAt(way, mark.S);
            Vector3 right = layout.RightAt(way, mark.S) * (mark.Side < 0 ? -1f : 1f);
            float half = layout.HalfAt(way, mark.S);
            const float past = 0.3f;
            var root = new GameObject("Dodge_" + mark.Id).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(c, Quaternion.LookRotation(dir));
            float yaw = Quaternion.LookRotation(dir).eulerAngles.y;
            var rock = ScanRock(c + right * half * 0.6f, new Vector3(1.9f, 1.2f, mark.Length), yaw + random.Next(-12, 12), root, random.Next(4), true);
            rock.isStatic = true;
            SurfaceTag.Add(rock, Surface.Stone);
            // Its inner face (below head height) exactly 0.3 m past the middle of the way.
            var mesh = rock.GetComponent<MeshFilter>().sharedMesh;
            float inner = float.MaxValue;
            foreach (var v in mesh.vertices)
            {
                Vector3 world = rock.transform.TransformPoint(v);
                if (world.y - c.y > 1.2f) continue;
                inner = Mathf.Min(inner, Vector3.Dot(world - c, right));
            }
            rock.transform.position += right * (-past - inner) + Vector3.down * 0.2f;
            // Broken culms on the bank side of the rock, before and after it: dead, snapped, solid.
            var b = new BambooMesh.Builder(1);
            for (int i = 0; i < 4; i++)
            {
                float along = (i < 2 ? -1f : 1f) * (mark.Length * 0.5f + 0.15f + (float)random.NextDouble() * 0.35f);
                float outward = half * (0.25f + 0.5f * (float)random.NextDouble());
                Vector3 foot = OnGround(ground, c + dir * along + right * outward) - c + Vector3.down * 0.05f;
                Vector3 local = Quaternion.Inverse(root.rotation) * foot;
                var axis = (Vector3.up + new Vector3((float)random.NextDouble() - 0.5f, 0f, (float)random.NextDouble() - 0.5f) * 0.25f).normalized;
                float length = 0.9f + (float)random.NextDouble() * 0.9f, radius = 0.05f + (float)random.NextDouble() * 0.03f;
                Vector3 localAxis = Quaternion.Inverse(root.rotation) * axis;
                BambooMesh.Culm(b, 0, local, localAxis, length, radius, 0.02f, Vector3.right, 7, 0.5f, 0.4f, (float)random.NextDouble(), 0.9f);
                var stub = new GameObject("Stub");
                stub.transform.SetParent(root, false);
                stub.layer = EnvironmentLayer;
                stub.transform.localPosition = local + localAxis * length * 0.5f;
                stub.transform.localRotation = Quaternion.FromToRotation(Vector3.up, localAxis);
                var capsule = stub.AddComponent<CapsuleCollider>();
                capsule.radius = radius;
                capsule.height = length;
            }
            Visual(root, "C5F_Dodge_" + mark.Id, b, new[] { CulmMaterial(2) }, true);
        }

        /// <summary>A rotten culm at the edge of the way that cracks, sways and falls across it as she comes (see <see cref="FallingBamboo"/>).</summary>
        private static void Falling(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, Refs refs, System.Random random)
        {
            HazardPose(layout, mark, out var foot, out var fallen, out var upright);
            foot = OnGround(ground, foot);
            const float length = 9.5f, radius = 0.2f;
            var root = new GameObject("Falling_" + mark.Id);
            root.transform.SetParent(parent, false);
            root.transform.position = foot;
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root.transform, false);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.up, upright);
            var body = pivot.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.None;
            pivot.gameObject.layer = EnvironmentLayer;
            var capsule = pivot.gameObject.AddComponent<CapsuleCollider>();
            capsule.direction = 1;
            capsule.radius = radius;
            capsule.height = length;
            capsule.center = new Vector3(0f, length * 0.5f, 0f);
            var b = new BambooMesh.Builder(2);
            BambooMesh.Culm(b, 0, Vector3.zero, Vector3.up, length, 0.17f, 0.25f, Vector3.right, 9, 1.1f, 0.42f);
            for (int i = 0; i < 10; i++)
            {
                float t = 0.55f + 0.45f * i / 10f;
                var at = BambooMesh.Along(Vector3.zero, Vector3.up, length, 0.25f, Vector3.right, t);
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                BambooMesh.Spray(b, 1, at, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)), 1.2f, 0.8f, 0.5f, random.Next(2) == 0);
            }
            Visual(pivot, "C5F_Falling_" + mark.Id, b, new[] { CulmMaterial(1), LeafMaterial() }, true);
            var cutGo = new GameObject("Cut");
            cutGo.transform.SetParent(pivot, false);
            cutGo.transform.localPosition = new Vector3(0f, length * 0.32f, 0f);
            var cut = cutGo.AddComponent<NavMeshObstacle>();
            cut.shape = NavMeshObstacleShape.Box;
            cut.size = new Vector3(0.5f, length * 0.6f, 0.5f);
            cut.carving = true;
            cut.carveOnlyStationary = false;
            cut.enabled = false;
            Vector3 cross = OnGround(ground, HazardCrossing(layout, mark));
            Vector3 dir = layout.DirAt(1, mark.S);
            float half = layout.HalfAt(1, mark.S);
            var link = Link("Vault_" + mark.Id, parent, OnGround(ground, cross - dir * 1.2f), OnGround(ground, cross + dir * 1.2f), Mathf.Max(0.6f, 2f * half - 0.8f), ChaseLink.Kind.Vault, 0.8f);
            link.enabled = false;
            // Her steps on the roots 16 m before set it going: about 3 s before she reaches it at a full run, 1.95 s to fall.
            var zone = Zone("Crack_" + mark.Id, OnGround(ground, layout.MoonAt(mark.S - 16f)), layout.DirAt(1, mark.S - 16f), new Vector3(2f * half + 1f, 3f, 2.5f), root.transform);
            var falling = root.AddComponent<FallingBamboo>();
            Set(falling, "_player", refs.Player);
            Set(falling, "_audio", refs.Audio);
            Set(falling, "_fx", refs.Fx);
            Set(falling, "_pivot", pivot);
            Set(falling, "_fallenEuler", Quaternion.FromToRotation(Vector3.up, fallen).eulerAngles);
            Set(falling, "_length", length);
            Set(falling, "_radius", radius);
            Set(falling, "_zone", zone);
            Set(falling, "_cut", cut);
            Set(falling, "_links", new Object[] { link });
        }

        /// <summary>A ditch cut by the stream's run-off: stones and water at the bottom; the pursuer leaps it like her.</summary>
        private static void Ditch(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, int way, System.Random random)
        {
            Vector3 c = layout.At(way, mark.S), dir = layout.DirAt(way, mark.S), right = layout.RightAt(way, mark.S);
            float half = layout.HalfAt(way, mark.S);
            var root = new GameObject("Ditch_" + mark.Id).transform;
            root.SetParent(parent, false);
            for (int i = 0; i < 5; i++)
            {
                Vector3 at = c + right * (((float)random.NextDouble() - 0.5f) * 6f) + Vector3.down * 2.3f;
                ScanRock(OnGround(ground, at), new Vector3(0.6f, 0.4f, 0.5f) * (0.6f + (float)random.NextDouble()), (float)random.NextDouble() * 360f, root, i);
            }
            var water = StoryPropKit.Puddle(OnGround(ground, c) + Vector3.up * 0.05f, new Vector2(mark.Length * 0.6f, 7f), Quaternion.LookRotation(dir).eulerAngles.y, root);
            water.name = "DitchWater";
            float reach = mark.Length * 0.5f + 0.8f;
            Link("Leap_" + mark.Id, parent, OnGround(ground, c - dir * reach), OnGround(ground, c + dir * reach), Mathf.Max(0.6f, 2f * half - 0.8f), ChaseLink.Kind.Leap, 0.55f);
        }

        /// <summary>Two mossy boulders pinching the way to 1.5 m, a little off centre: a line to hold, or a shoulder into stone.</summary>
        private static void Squeeze(Transform parent, Layout layout, ForestGround ground, Layout.Mark mark, System.Random random)
        {
            Vector3 c = OnGround(ground, layout.MoonAt(mark.S)), right = layout.RightAt(mark.S);
            float yaw = Quaternion.LookRotation(layout.DirAt(1, mark.S)).eulerAngles.y;
            var root = new GameObject("Squeeze_" + mark.Id).transform;
            root.SetParent(parent, false);
            const float offset = 0.3f, gap = 0.75f;
            foreach (int side in new[] { -1, 1 })
            {
                var rock = ScanRock(c + right * side * 2f, new Vector3(2.6f, 2f, 3.2f), yaw + random.Next(-10, 10), root, side > 0 ? 1 : 2, true);
                rock.isStatic = true;
                // Its inner face exactly at the edge of the gap.
                var mesh = rock.GetComponent<MeshFilter>().sharedMesh;
                float inner = side > 0 ? float.MaxValue : float.MinValue;
                foreach (var v in mesh.vertices)
                {
                    Vector3 world = rock.transform.TransformPoint(v);
                    if (world.y - c.y > 1.8f) continue;
                    float across = Vector3.Dot(world - c, right);
                    inner = side > 0 ? Mathf.Min(inner, across) : Mathf.Max(inner, across);
                }
                float want = offset + side * gap;
                rock.transform.position += right * (want - inner);
                rock.transform.position += Vector3.down * 0.25f;
            }
        }

        /// <summary>Where the ways part: a boulder and a dead clump between them, so the choice is seen from 20 m and made in the open.</summary>
        private static void Divider(Transform parent, Layout layout, ForestGround ground, System.Random random)
        {
            Vector3 split = (layout.MoonAt(layout.ForkAt + 14f) + layout.At(2, layout.ForkAt + 14f)) * 0.5f;
            var root = new GameObject("Fork").transform;
            root.SetParent(parent, false);
            var rock = ScanRock(OnGround(ground, split) + Vector3.down * 0.2f, new Vector3(1.6f, 1.4f, 2.2f), 30f, root, 3, true);
            rock.isStatic = true;
            var b = new BambooMesh.Builder(2);
            for (int i = 0; i < 5; i++)
            {
                Vector3 foot = OnGround(ground, split + new Vector3(((float)random.NextDouble() - 0.5f) * 1.2f, 0f, ((float)random.NextDouble() - 0.5f) * 1.2f)) - split;
                BambooMesh.Culm(b, 0, foot + Vector3.up * 0.2f, (Vector3.up + new Vector3((float)random.NextDouble() - 0.5f, 0f, (float)random.NextDouble() - 0.5f) * 0.3f).normalized,
                    1.5f + (float)random.NextDouble() * 1.5f, 0.06f, 0.1f, Vector3.right, 6, 0.8f, 0.4f);
            }
            var holder = new GameObject("DeadClump").transform;
            holder.SetParent(root, false);
            holder.position = split;
            Visual(holder, "C5F_ForkClump", b, new[] { CulmMaterial(2), DryLeafMaterial() }, false);
        }

        private static NavMeshLink Link(string name, Transform parent, Vector3 a, Vector3 b, float width, ChaseLink.Kind kind, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Vector3 flat = b - a;
            flat.y = 0f;
            go.transform.SetPositionAndRotation((a + b) * 0.5f, Quaternion.LookRotation(flat));
            var link = go.AddComponent<NavMeshLink>();
            link.startPoint = go.transform.InverseTransformPoint(a);
            link.endPoint = go.transform.InverseTransformPoint(b);
            link.width = width;
            link.bidirectional = true;
            link.autoUpdate = false;
            var chase = go.AddComponent<ChaseLink>();
            Set(chase, "_kind", (int)kind);
            Set(chase, "_height", height);
            return link;
        }

        // ------------------------------------------------------------------ the bridge

        /// <summary>
        /// The plank bridge over the ravine: its deck hangs from a hinge at the near lip and has rope rails (with colliders,
        /// so a strafe does not drop her in); the far end is tied to one stake at the corner the path turns, straight ahead of
        /// her as she steps off. The pursuer runs over on one link that goes when the deck does.
        /// </summary>
        private static DropBridge Bridge(Transform parent, Layout layout, Refs refs)
        {
            Vector3 near = layout.MoonAt(layout.BridgeNear), dir = layout.DirAt(1, layout.BridgeNear);
            float span = layout.BridgeFar - layout.BridgeNear + 1.2f;
            var root = new GameObject("Bridge");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(near, Quaternion.LookRotation(dir));
            var hinge = new GameObject("Deck").transform;
            hinge.SetParent(root.transform, false);
            hinge.localPosition = new Vector3(0f, 0f, -0.6f);
            var planks = StoryArtKit.Surface("C5F_Planks", "planks_brown_10", new Color(0.46f, 0.36f, 0.27f), 1.2f, 0.42f, 1.2f, 0.4f, 0.4f, Folder);
            var rope = Material("C5F_Rope", new Color(0.32f, 0.26f, 0.18f));
            var walk = LocalBox("Walk", hinge, new Vector3(0f, -0.06f, span * 0.5f), new Vector3(1.3f, 0.12f, span), null);
            walk.AddComponent<BoxCollider>();
            walk.AddComponent<SurfaceTag>();
            foreach (float x in new[] { -0.72f, 0.72f })
            {
                LocalBox("Rail", hinge, new Vector3(x, 0.45f, span * 0.5f), new Vector3(0.06f, 0.9f, span), null).AddComponent<BoxCollider>();
                LocalBox("Beam", hinge, new Vector3(x * 0.85f, -0.16f, span * 0.5f), new Vector3(0.12f, 0.14f, span), planks);
            }
            var random = new System.Random(9);
            for (float z = 0.15f; z < span; z += 0.31f)
                LocalBox("Plank", hinge, new Vector3(((float)random.NextDouble() - 0.5f) * 0.06f, -0.03f, z), new Vector3(1.32f, 0.05f, 0.27f), planks).transform.localRotation =
                    Quaternion.Euler(((float)random.NextDouble() - 0.5f) * 2f, ((float)random.NextDouble() - 0.5f) * 4f, 0f);
            for (float z = 0f; z <= span + 0.01f; z += span / 6f)
                foreach (float x in new[] { -0.72f, 0.72f })
                    LocalBox("Post", hinge, new Vector3(x, 0.45f, z), new Vector3(0.07f, 0.95f, 0.07f), planks);
            foreach (float x in new[] { -0.72f, 0.72f })
            {
                Vector3 a = hinge.TransformPoint(new Vector3(x, 0.9f, 0f)), b = hinge.TransformPoint(new Vector3(x, 0.9f, span));
                StoryArtKit.Rope(a, b, 0.35f, hinge);
                StoryArtKit.Rope(hinge.TransformPoint(new Vector3(x, 0.5f, 0f)), hinge.TransformPoint(new Vector3(x, 0.5f, span)), 0.3f, hinge);
            }
            foreach (float x in new[] { -0.85f, 0.85f })
                Cylinder("NearStake", near + Quaternion.LookRotation(dir) * new Vector3(x, -0.2f, -0.9f), 0.09f, 1.4f, planks, root.transform, false);

            // The far stake, at the outer edge of the corner, and the ropes tying the deck's far end to it.
            Vector3 stakeAt = layout.MoonAt(layout.Corner) + layout.RightAt(layout.Corner) * (layout.HalfAt(1, layout.Corner) - 0.05f);
            var stake = new GameObject("Stake").transform;
            stake.SetParent(root.transform, false);
            stake.SetPositionAndRotation(stakeAt + Vector3.down * 0.15f, Quaternion.LookRotation(dir));
            var post = Cylinder("StakePost", stakeAt + Vector3.down * 0.15f, 0.1f, 1.25f, Material("C5F_StakeWood", new Color(0.22f, 0.17f, 0.12f), 0.35f), stake, false);
            _ = post;
            Vector3 farEnd = hinge.TransformPoint(new Vector3(0.72f, 0.5f, span));
            StoryArtKit.Rope(farEnd, stakeAt + Vector3.up * 0.9f, 0.4f, hinge);
            StoryArtKit.Rope(hinge.TransformPoint(new Vector3(-0.72f, 0.5f, span)), stakeAt + Vector3.up * 0.8f, 0.5f, hinge);
            var reach = new GameObject("StakeReach");
            reach.transform.SetParent(root.transform, false);
            reach.transform.position = stakeAt + Vector3.up * 0.7f;
            reach.layer = 2;
            var reachBox = reach.AddComponent<BoxCollider>();
            reachBox.isTrigger = true;
            reachBox.size = new Vector3(0.9f, 1.4f, 0.9f);

            // The temple's lanterns on posts at the top of the far bank, where its path begins: the warm light seen from
            // across the ravine all down the last straight, telling her where the way goes on.
            foreach (float side in new[] { 1.6f, 3.4f })
            {
                Vector3 at = layout.MoonAt(layout.Corner + 1f) + layout.RightAt(layout.Corner + 1f) * side;
                Cylinder("TemplePathPost", at + Vector3.down * 0.2f, 0.07f, 2.6f, planks, root.transform, false);
                Lantern("TemplePathLantern", at + Vector3.up * 2.3f, new Color(1f, 0.55f, 0.25f), root.transform, side < 2f ? 8f : 0f, 2.2f);
            }
            var farBank = Zone("FarBank", layout.MoonAt(layout.BridgeFar + 4.5f), layout.DirAt(1, layout.BridgeFar + 2f), new Vector3(5f, 3f, 9f), root.transform);
            var crossing = Link("Crossing", root.transform, layout.MoonAt(layout.BridgeNear - 1.4f), layout.MoonAt(layout.BridgeFar + 1.4f), 0.8f, ChaseLink.Kind.Run, 0f);

            var bridge = root.AddComponent<DropBridge>();
            Set(bridge, "_player", refs.Player);
            Set(bridge, "_audio", refs.Audio);
            Set(bridge, "_fx", refs.Fx);
            Set(bridge, "_deck", hinge);
            Set(bridge, "_deckDownEuler", new Vector3(68f, 0f, 0f));
            Set(bridge, "_stake", stake);
            Set(bridge, "_stakeDownEuler", new Vector3(-62f, 0f, 0f));
            Set(bridge, "_farBank", farBank);
            Set(bridge, "_crossing", crossing);
            return bridge;
        }

        private static GameObject LocalBox(string name, Transform parent, Vector3 local, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = size;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            else Object.DestroyImmediate(go.GetComponent<Renderer>());
            if (material == null) Object.DestroyImmediate(go.GetComponent<MeshFilter>());
            return go;
        }

        // ------------------------------------------------------------------ the temple

        /// <summary>The gate of Bạch Vân temple at the top of the path: steps, red pillars, tiled roof, lanterns seen from the ravine.</summary>
        private static void Temple(Transform parent, Layout layout, ForestGround ground, out Transform rainSpot)
        {
            var stone = StoryArtKit.Surface("C5F_Stone", "japanese_stone_wall", new Color(0.7f, 0.7f, 0.68f), 1.6f, 0.3f, 1f, 0.4f, 0.5f, Folder);
            var red = StoryArtKit.Surface("C5F_Lacquer", "lacquered_cherry_wood", new Color(0.9f, 0.35f, 0.3f), 1.2f, 0.5f, 0.8f, 0.2f, 0f, Folder);
            var tiles = StoryArtKit.Surface("C5F_Tiles", "clay_roof_tiles_02", new Color(0.5f, 0.38f, 0.36f), 1.4f, 0.45f, 1.2f, 0.35f, 0f, Folder);
            var plaster = StoryArtKit.Surface("C5F_Plaster", "plastered_wall_02", new Color(0.82f, 0.77f, 0.68f), 2.6f, 0.12f, 1.2f, 0.5f, 1f, Folder);
            var ridge = Material("C5F_Ridge", new Color(0.12f, 0.07f, 0.06f));
            Vector3 gate = OnGround(ground, layout.TempleGate);
            var temple = new GameObject("TempleGate").transform;
            temple.SetParent(parent, false);
            // Built facing +x at the origin (the old layout), then turned and moved onto the end of the path.
            Block("TempleSteps", new Vector3(-2f, 0.15f, 0f), new Vector3(3f, 0.3f, 5f), stone, temple);
            Block("TempleSteps2", new Vector3(-1f, 0.45f, 0f), new Vector3(2f, 0.3f, 5f), stone, temple);
            Block("TempleCourt", new Vector3(5f, 0.55f, 0f), new Vector3(12f, 0.2f, 9f), stone, temple);
            foreach (float z in new[] { -1.8f, 1.8f }) StoryArtKit.Pillar("TemplePillar", new Vector3(0f, 0.6f, z), 0.2f, 3.4f, red, stone, temple);
            Roof("TempleGateRoof", new Vector3(0f, 4.4f, 0f), 2.4f, 5.6f, 26f, tiles, ridge, temple, false);
            foreach (float z in new[] { -4.8f, 4.8f })
            {
                Block("TempleWall", new Vector3(0f, 1.6f, z), new Vector3(0.5f, 3.2f, 5.6f), plaster, temple);
                Block("TempleWallCap", new Vector3(0f, 3.28f, z), new Vector3(0.9f, 0.16f, 5.9f), tiles, temple, false);
                Block("TempleWallFooting", new Vector3(0f, 0.25f, z), new Vector3(0.6f, 0.5f, 5.7f), stone, temple, false);
            }
            Lantern("TempleLantern", new Vector3(-0.8f, 3.2f, 0f), new Color(1f, 0.6f, 0.3f), temple, 12f, 2.6f);
            foreach (float z in new[] { -2.9f, 2.9f }) StoryPropKit.StoneLantern(new Vector3(-2.8f, 0f, z), 90f, temple, 5f, 1.4f);
            Block("TempleSign", new Vector3(-0.15f, 3.75f, 0f), new Vector3(0.06f, 0.45f, 1.6f), Material("CoupletRed", new Color(0.45f, 0.05f, 0.04f), 0.55f), temple, false);
            // The hall beyond the gate: lit doors, a roof against the sky, the goal seen from across the ravine.
            Block("TempleHall", new Vector3(9f, 2.2f, 0f), new Vector3(5f, 3.6f, 9f), plaster, temple);
            Roof("TempleHallRoof", new Vector3(9f, 4.8f, 0f), 6.4f, 10.4f, 28f, tiles, ridge, temple, false);
            Block("TempleHallDoor", new Vector3(6.45f, 1.7f, 0f), new Vector3(0.08f, 2.2f, 2.4f), Emissive("C5F_DoorGlow2", new Color(0.55f, 0.28f, 0.12f), 0.9f), temple, false);
            foreach (float z in new[] { -2.6f, 2.6f }) Lantern("HallLantern", new Vector3(6.2f, 3f, z), new Color(1f, 0.5f, 0.25f), temple, 9f, 2.2f);
            temple.SetPositionAndRotation(gate, Quaternion.LookRotation(layout.TempleForward) * Quaternion.Euler(0f, -90f, 0f));
            rainSpot = Point("RainSpot", OnGround(ground, layout.MoonAt(layout.End + 1.5f)) + Vector3.up * 0.05f, 0f, parent);
            rainSpot.rotation = Quaternion.LookRotation(layout.TempleForward);
        }

        // ------------------------------------------------------------------ bamboo, dressing

        /// <summary>Where the canopy opens and cold moonlight reaches the way: the moonlit fork, the runway, the bridge approach, the far bank.</summary>
        private static List<(Vector3 at, float intensity)> MoonGaps(Layout layout, ForestGround ground) => new List<(Vector3, float)>
        {
            (OnGround(ground, layout.MoonAt(layout.Of("G1"))), 1.7f),
            (OnGround(ground, layout.MoonAt(layout.StartAt + 20f)), 1.2f),
            (OnGround(ground, layout.MoonAt(layout.BridgeNear - 12f)), 1.4f),
            (OnGround(ground, layout.MoonAt(layout.BridgeFar + 2f)), 1.1f)
        };

        private static void Grove(Transform parent, Layout layout, ForestGround ground)
        {
            var go = new GameObject("Bamboo");
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            // The culms' colliders stand on the banks, where no agent walks: left out of the NavMesh build.
            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            var grove = go.AddComponent<BambooGrove>();
            var so = new SerializedObject(grove);
            so.FindProperty("_ground").objectReferenceValue = ground;
            var culms = so.FindProperty("_culms");
            culms.arraySize = CulmTints.Length;
            for (int i = 0; i < CulmTints.Length; i++) culms.GetArrayElementAtIndex(i).objectReferenceValue = CulmMaterial(i);
            so.FindProperty("_leaves").objectReferenceValue = LeafMaterial();
            so.FindProperty("_farLeaves").objectReferenceValue = FarLeafMaterial();
            so.FindProperty("_canopy").objectReferenceValue = CanopyMaterial();
            so.FindProperty("_ferns").objectReferenceValue = TuftMaterial();
            so.FindProperty("_litter").objectReferenceValue = LitterMaterial();
            so.FindProperty("_shrubs").objectReferenceValue = ShrubMaterial();
            so.FindProperty("_roots").objectReferenceValue = RootMaterial();
            so.FindProperty("_skyline").objectReferenceValue = SkylineMaterial();
            so.FindProperty("_thicket").objectReferenceValue = ThicketMaterial();
            so.FindProperty("_fernMaterial").objectReferenceValue = FernMaterial();
            var ferns = FernMeshes();
            var fernArray = so.FindProperty("_fernMeshes");
            fernArray.arraySize = ferns.Length;
            for (int i = 0; i < ferns.Length; i++) fernArray.GetArrayElementAtIndex(i).objectReferenceValue = ferns[i];
            Debug.Log($"[Chapter05Forest] ferns: {string.Join(", ", ferns.Select(m => $"{m.name} {m.bounds.size.x:F2}×{m.bounds.size.y:F2}×{m.bounds.size.z:F2} m, {m.vertexCount} vertices, lods {m.lodCount}, z {m.bounds.min.z:F2}..{m.bounds.max.z:F2}"))}");
            // The night wind the bamboo moves in (this scene only).
            go.AddComponent<ForestWind>();
            var clear = new List<(Vector3, float)>();
            foreach (var mark in layout.Marks)
            {
                int way = mark.Way == 2 ? 2 : 1;
                float r = mark.Kind == ChaseCourse.ObstacleKind.Bridge ? 9f : layout.HalfAt(way, mark.S) + 2.2f;
                clear.Add((layout.At(way, mark.S), r));
            }
            clear.Add((layout.TempleGate, 12f));
            clear.Add((layout.MoonAt(layout.Corner), 3.5f));
            clear.Add(((layout.MoonAt(layout.ForkAt + 14f) + layout.At(2, layout.ForkAt + 14f)) * 0.5f, 2.5f));
            WriteClearings(so.FindProperty("_clear"), clear);
            // The sky shows where the moonlight falls, over the ravine (the view out along it) and over the temple court.
            var holes = MoonGaps(layout, ground).Select(g => (g.at, 4.5f)).ToList();
            float mid = (layout.BridgeNear + layout.BridgeFar) * 0.5f;
            holes.Add((layout.MoonAt(mid), 15f));
            holes.Add((layout.TempleGate, 13f));
            WriteClearings(so.FindProperty("_skyHoles"), holes);
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            grove.Build();
            Debug.Log($"[Chapter05Forest] bamboo: {grove.CulmCount} culms · {grove.ColliderCount} colliders · {grove.VertexCount} vertices · {grove.FernCount} ferns · {grove.BuildSeconds:F2} s ({grove.Timing})");
        }

        private static void WriteClearings(SerializedProperty array, List<(Vector3 centre, float radius)> clearings)
        {
            array.arraySize = clearings.Count;
            for (int i = 0; i < clearings.Count; i++)
            {
                array.GetArrayElementAtIndex(i).FindPropertyRelative("Centre").vector3Value = clearings[i].centre;
                array.GetArrayElementAtIndex(i).FindPropertyRelative("Radius").floatValue = clearings[i].radius;
            }
        }

        /// <summary>
        /// Rocks in the banks and on the floor (scanned, see <see cref="Rocks"/>), mist where the ground lies low, fireflies,
        /// moonlight where the canopy opens, a soft patch of moonlight just past each obstacle, the stream below the bridge.
        /// The damp of the way and its small pools are in the ground's own material (nothing laid on top of it).
        /// </summary>
        private static void Dressing(Transform parent, Layout layout, ForestGround ground, Vector3 moonForward)
        {
            var root = new GameObject("Dressing").transform;
            root.SetParent(parent, false);
            var random = new System.Random(31);
            Rocks(root, layout, ground);
            // Mist lies in the low places: the hollows below a traverse, where the floor falls away from the way, thicker
            // there and thin on the way itself; higher, a faint haze now and then; leaves coming down in the wind.
            for (float s = layout.StartAt; s < layout.End; s += 24f)
            {
                Vector3 at = layout.MoonAt(s), right = layout.RightAt(s);
                foreach (int side in new[] { -1, 1 })
                {
                    Vector3 out7 = at + right * side * (layout.HalfAt(1, s) + 7f);
                    float below = at.y - ground.Height(out7.x, out7.z);
                    if (below < 1.2f || ground.InCutAt(out7.x, out7.z)) continue;
                    StoryArtKit.Dust("MistHollow", OnGround(ground, out7) + Vector3.up * 0.6f, new Vector3(14f, 1.2f, 12f), root, new Color(0.5f, 0.56f, 0.6f, Mathf.Lerp(0.08f, 0.13f, Mathf.Clamp01((below - 1.2f) / 3f))), 50);
                }
                if (Mathf.RoundToInt(s / 24f) % 3 == 0) StoryArtKit.Dust("Mist", at + Vector3.up * 0.4f, new Vector3(10f, 0.6f, 12f), root, new Color(0.5f, 0.56f, 0.6f, 0.06f), 30);
                if (Mathf.RoundToInt(s / 24f) % 4 == 1) StoryArtKit.Dust("MistHigh", at + Vector3.up * 3.5f, new Vector3(26f, 3f, 26f), root, new Color(0.45f, 0.5f, 0.55f, 0.06f), 36);
                if (Mathf.RoundToInt(s / 24f) % 2 == 0) StoryArtKit.Leaves("Leaves", at + Vector3.up * 7f, new Vector3(14f, 2f, 14f), root, new Color(0.36f, 0.42f, 0.22f), 18);
            }
            StoryArtKit.Fireflies("Fireflies", layout.At(2, (layout.ThickForkFrom + layout.ThickForkTo) * 0.5f) + Vector3.up, new Vector3(14f, 1.4f, 14f), root, 40);
            float mid = (layout.BridgeNear + layout.BridgeFar) * 0.5f;
            StoryArtKit.Fireflies("Fireflies", layout.MoonAt(mid) + Vector3.down * 4f, new Vector3(16f, 3f, 30f), root, 50);
            StoryArtKit.Dust("RavineMist", layout.MoonAt(mid) + Vector3.down * 6f, new Vector3(14f, 3f, 50f), root, new Color(0.55f, 0.6f, 0.66f, 0.14f), 120);
            var stream = Block("Stream", layout.MoonAt(mid) + Vector3.down * 8.4f, new Vector3(3.5f, 0.05f, 70f), Material("C5F_Water", new Color(0.04f, 0.05f, 0.06f), 0.96f), root, false,
                Quaternion.LookRotation(layout.RightAt(mid)).eulerAngles.y);
            stream.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            foreach (var (at, intensity) in MoonGaps(layout, ground)) MoonShaft(root, at, intensity, moonForward);
            Cues(root, layout, ground, moonForward);
        }

        /// <summary>
        /// A shaft of moonlight through a gap in the canopy: narrow, cold, no shadows, onto the way, coming the way the moon
        /// shines (every patch of moonlight in the forest falls from the same side).
        /// </summary>
        private static void MoonShaft(Transform parent, Vector3 onto, float intensity, Vector3 moonForward)
        {
            var go = new GameObject("MoonShaft");
            go.transform.SetParent(parent, false);
            Vector3 from = onto - moonForward * 12f;
            go.transform.SetPositionAndRotation(from, Quaternion.LookRotation(moonForward));
            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(0.56f, 0.66f, 0.86f);
            light.intensity = intensity;
            light.range = 16f;
            light.spotAngle = 34f;
            light.innerSpotAngle = 12f;
            light.shadows = LightShadows.None;
        }

        /// <summary>
        /// Moonlight just past each obstacle, as if through a small gap in the canopy: the ground beyond is lit, so a fallen
        /// culm or a low bough reads as a dark edge against it from 15–20 m, and a ditch as a lit far lip over a black gap.
        /// Not a row of lamps: a wide, soft pool the colour of the moon, from high up, nothing on the way between them.
        /// </summary>
        private static void Cues(Transform parent, Layout layout, ForestGround ground, Vector3 moonForward)
        {
            var root = new GameObject("ObstacleCues").transform;
            root.SetParent(parent, false);
            foreach (var mark in layout.Marks)
            {
                if (mark.Kind == ChaseCourse.ObstacleKind.Bridge) continue;
                int way = mark.Way == 2 ? 2 : 1;
                float beyond = mark.Kind == ChaseCourse.ObstacleKind.Gap ? mark.Length * 0.5f + 0.8f : mark.Kind == ChaseCourse.ObstacleKind.Duck ? mark.Length * 0.5f + 1.6f : 1.8f;
                float s = mark.Kind == ChaseCourse.ObstacleKind.Hazard ? mark.S + 1.5f : mark.S + beyond;
                // High and wide (3.5 m up, lighting some 8 m of the way, before the obstacle and past it), shifted a little
                // towards the moon's side, so it reads as a pool of moonlight on the earth and not a lamp over the way.
                Vector3 flat = new Vector3(moonForward.x, 0f, moonForward.z);
                Vector3 at = OnGround(ground, layout.At(way, s)) + Vector3.up * (mark.Kind == ChaseCourse.ObstacleKind.Duck ? 2.6f : 3.5f) - flat * 0.8f;
                var go = new GameObject("Cue_" + mark.Id);
                go.transform.SetParent(root, false);
                go.transform.position = at;
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.56f, 0.65f, 0.84f);
                light.intensity = mark.Way == 2 ? 5.6f : 4.8f; // the dark thick way gets a little more
                light.range = mark.Kind == ChaseCourse.ObstacleKind.Gap ? 8.5f : 7.5f;
                light.shadows = LightShadows.None;
            }
        }

        // ------------------------------------------------------------------ the look of the night (this scene only)

        /// <summary>
        /// A damp forest at night: deep olive-black shade that keeps its detail, a cold grey mist in depth, the moon only
        /// where the canopy lets it through (a cookie of leaf shadow on the moonlight, since the leaves themselves cast
        /// none), the torch amber. Darker light, not a darker picture: the exposure stays where obstacles read. Everything set
        /// here belongs to Chapter 5's scene (its render settings, its moon and sky, its grade); nothing shared changes.
        /// </summary>
        /// <summary>Which way the moon shines in this scene (the patches of moonlight in the forest all fall that way).</summary>
        private static Vector3 MoonForward(Refs refs)
        {
            var moon = (Light)new SerializedObject(refs.Chapter).FindProperty("_moon").objectReferenceValue;
            Vector3 forward = moon != null ? moon.transform.forward : new Vector3(0.3f, -0.8f, 0.5f);
            // A low moon would light the trunks from the side more than the ground: the gaps let it down at least this steeply.
            if (forward.y > -0.65f) { forward.y = 0f; forward = (forward.normalized * 0.76f + Vector3.down * 0.65f).normalized; }
            return forward.normalized;
        }

        private static void Mood(Refs refs)
        {
            // A dry night: no rain anywhere in this chapter (the scene as first built carried rain under the camera).
            foreach (var rain in refs.Camera.GetComponentsInChildren<ParticleSystem>(true).Where(p => p.name == "Rain").ToList())
                Object.DestroyImmediate(rain.gameObject);
            // (Colours here are gamma: the project renders in linear, so 0.2 here is about 0.03 of light.)
            // ACES (the shared filmic grade) presses a night this dark into black at the usual exposure: the scene is lit
            // darkly and exposed up into the range where the tonemapper keeps shade detail and still rolls the torch off.
            StoryArtKit.Grade(refs.Fx, 3.3f, 4f, -6f, -4f, new Color(0.4f, 0.44f, 0.43f), new Color(0.9f, 0.68f, 0.44f));
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.2f, 0.235f, 0.26f);
            RenderSettings.ambientEquatorColor = new Color(0.135f, 0.15f, 0.135f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.066f, 0.058f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.085f, 0.097f, 0.105f);
            RenderSettings.fogDensity = 0.022f;
            RenderSettings.reflectionIntensity = 0.35f;
            var sky = RenderSettings.skybox;
            if (sky != null && sky.name.StartsWith("Ch5") && sky.HasProperty("_Exposure")) { sky.SetFloat("_Exposure", 0.22f); EditorUtility.SetDirty(sky); }

            var moon = (Light)new SerializedObject(refs.Chapter).FindProperty("_moon").objectReferenceValue;
            if (moon != null)
            {
                moon.intensity = 0.7f;
                moon.color = new Color(0.58f, 0.66f, 0.86f);
                moon.shadowStrength = 0.85f;
                moon.shadows = LightShadows.Soft;
                moon.cookie = Store("C5F_CanopyCookie", 256, 256, DrawCookie, true, true, false);
                if (!moon.TryGetComponent<UniversalAdditionalLightData>(out var extra)) extra = moon.gameObject.AddComponent<UniversalAdditionalLightData>();
                extra.lightCookieSize = new Vector2(19f, 19f);
                EditorUtility.SetDirty(moon);
                EditorUtility.SetDirty(extra);
            }
            // The dialogue shots after the run (the rain, the temple) light faces a little less than the town did.
            if (refs.Director.Shots != null) Set(refs.Director.Shots, "_keyLight", 1.2f);
            // His torch: the warm light that moves with him over trunks, mud and his own body, falling off with distance.
            var torch = refs.Pursuers[0].transform.Find("Torch");
            var flame = torch != null ? torch.GetComponentInChildren<Light>() : null;
            if (flame != null)
            {
                flame.range = 10f;
                flame.intensity = 2.6f;
                flame.color = new Color(1f, 0.56f, 0.24f);
                EditorUtility.SetDirty(flame);
            }
        }

        // ------------------------------------------------------------------ meshes, materials, textures

        /// <summary>A mesh kept as an asset (the obstacles'); an existing asset is overwritten in place, so its GUID and references stay.</summary>
        private static void Visual(Transform parent, string name, BambooMesh.Builder builder, Material[] materials, bool shadows)
        {
            var mesh = builder.ToMesh(name);
            string path = Folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                existing.name = name;
                Object.DestroyImmediate(mesh);
                mesh = existing;
                EditorUtility.SetDirty(mesh);
            }
            else AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("Visual");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }
}

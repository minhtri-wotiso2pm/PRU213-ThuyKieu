using System.Collections.Generic;
using ThuyKieu.Environment;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static ThuyKieu.Core.Editor.StorySceneKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Building blocks for Tú Bà's house (Chapters 3–4, first person): wall runs with real openings, raised floors with
    /// stone edges, timber trim, ceilings and veranda eaves, railings, steps, doors that open, barred and paper windows.
    /// Everything that blocks the player also blocks sight (Environment layer); trims and details have no colliders.
    /// Generated materials live in their own folder so the Chapter 2–7 rebuild never deletes them.
    /// </summary>
    public static class LauTuBaKit
    {
        public const string Folder = "Assets/_Game/Materials/LauTuBa";
        /// <summary>Height of the raised floors (rooms, verandas, hall) above the courtyard.</summary>
        public const float Floor = 0.35f;
        public const float WallHeight = 3.05f;
        public const float DoorHeight = 2.15f;

        public struct Opening
        {
            public float From, To, Bottom, Top;
            public Opening(float from, float to, float bottom, float top) { From = from; To = to; Bottom = bottom; Top = top; }
            public static Opening Door(float from, float to) => new Opening(from, to, 0f, DoorHeight);
            public static Opening Window(float from, float to, float sill = 0.85f, float top = 2.0f) => new Opening(from, to, sill, top);
        }

        public static Material Plaster, PlasterDirty, Timber, PillarWood, PillarWoodYard, PlinthYard, Handrail, TimberDusty, Lacquer, Planks, FloorBoards, Stone, WetStone, Plinth, Tiles, Ridge,
            Earth, Brick, Paper, PaperLit, Iron, Mud, WallStone, Ceiling, Bamboo;

        /// <summary>Surfaces of the house; <paramref name="night"/> makes the yard and the alley wetter.</summary>
        public static void InitMaterials(bool night)
        {
            System.IO.Directory.CreateDirectory(Folder);
            // Wet-weather variants are separate assets: Chapter 3 (afternoon) and Chapter 4 (rain night) must not share one
            // material whose values depend on which chapter was rebuilt last.
            string wet = night ? "_Night" : "";
            // Warm aged plaster; red-brown timber whose grain follows each piece and whose sheen differs by use (posts and
            // beams dull, floors softly polished by feet, lacquer an even satin coat); stone damp in places, not
            // mirror-glossy; roof tiles at a real tile size. Roughness/AO come from the Poly Haven ARM maps where they exist.
            Plaster = Arm(StoryArtKit.Surface("LTB_Plaster", "plastered_wall_02", new Color(0.86f, 0.77f, 0.62f), 2.4f, 0.3f, 1.0f, 0.55f, 0.6f, Folder), "plastered_wall_02");
            PlasterDirty = Arm(StoryArtKit.Surface("LTB_PlasterDirty", "plastered_wall_02", new Color(0.64f, 0.56f, 0.46f), 2f, 0.3f, 1.2f, 0.7f, 1f, Folder), "plastered_wall_02");
            Timber = Arm(StoryArtKit.Surface("LTB_Timber", "dark_wood", new Color(0.72f, 0.56f, 0.5f), 1.1f, 0.5f, 0.9f, 0.25f, 0.35f, Folder), "dark_wood", true);
            PillarWood = Arm(StoryArtKit.Surface("LTB_PillarWood", "dark_wood", new Color(0.9f, 0.62f, 0.56f), 1.3f, 0.62f, 0.8f, 0.2f, 0.45f, Folder), "dark_wood", true);
            Lacquer = Arm(StoryArtKit.Surface("LTB_Lacquer", "dark_wood", new Color(0.8f, 0.48f, 0.45f), 1.2f, 0.45f, 0.35f, 0.15f, 0.2f, Folder), null, true);
            Planks = Arm(StoryArtKit.Surface("LTB_DoorPlanks", "planks_brown_10", new Color(0.72f, 0.55f, 0.45f), 1.4f, 0.45f, 1.0f, 0.3f, 0.4f, Folder), "planks_brown_10", true);
            FloorBoards = Arm(StoryArtKit.Surface("LTB_FloorBoards", "old_wooden_floor_02", new Color(0.86f, 0.66f, 0.82f), 2.2f, 0.56f, 0.8f, 0.3f, 0.35f, Folder), "old_wooden_floor_02", true);
            Stone = Arm(StoryArtKit.Surface("LTB_Paving" + wet, "monastery_stone_floor", night ? new Color(0.44f, 0.42f, 0.39f) : new Color(0.66f, 0.62f, 0.56f), 3f, night ? 0.8f : 0.7f, 0.3f, 0.45f, 0.6f, Folder), "monastery_stone_floor");
            WetStone = Arm(StoryArtKit.Surface("LTB_PavingWet" + wet, "monastery_stone_floor", new Color(0.4f, 0.37f, 0.34f), 3f, night ? 0.55f : 0.42f, 0.3f, 0.45f, 0.7f, Folder), null);
            Plinth = StoryArtKit.Surface("LTB_Plinth", "japanese_stone_wall", new Color(0.56f, 0.55f, 0.51f), 1.4f, 0.22f, 0.8f, 0.5f, 0.8f, Folder);
            Tiles = StoryArtKit.Surface("LTB_Tiles" + wet, "clay_roof_tiles_02", new Color(0.5f, 0.33f, 0.27f), 2.1f, night ? 0.4f : 0.25f, 1.1f, 0.45f, 0.6f, Folder);
            Ridge = Mat("LTB_Ridge", new Color(0.13f, 0.08f, 0.07f), 0.3f);
            Earth = StoryArtKit.Surface("LTB_Earth", "brown_mud_dry", new Color(0.55f, 0.45f, 0.36f), 2f, 0.12f, 1.2f, 0.5f, 0.8f, Folder);
            Brick = Arm(StoryArtKit.Surface("LTB_BrickFloor", "brick_floor", new Color(0.8f, 0.5f, 0.4f), 1.6f, 0.6f, 0.5f, 0.4f, 0.5f, Folder), "brick_floor");
            Mud = StoryArtKit.Surface("LTB_AlleyMud" + wet, "brown_mud_02", new Color(0.4f, 0.36f, 0.32f), 2f, night ? 0.78f : 0.55f, 1.3f, 0.5f, 0.8f, Folder);
            WallStone = StoryArtKit.Surface("LTB_WallStone", "mossy_rock", new Color(0.5f, 0.52f, 0.48f), 2f, 0.25f, 1.2f, 0.5f, 0.8f, Folder);
            Ceiling = Arm(StoryArtKit.Surface("LTB_Ceiling", "planks_brown_10", new Color(0.56f, 0.42f, 0.36f), 1.6f, 0.4f, 0.8f, 0.35f, 0f, Folder), "planks_brown_10", true);
            // Wear with a cause: the yard-side pillars and kerbs damp at the foot after the rain; handrails polished by hands;
            // shelves in the store and archive under a film of dust on their tops.
            PillarWoodYard = Damp(Arm(StoryArtKit.Surface("LTB_PillarWoodYard", "dark_wood", new Color(0.9f, 0.62f, 0.56f), 1.3f, 0.62f, 0.8f, 0.2f, 0.45f, Folder), "dark_wood", true), Floor + 0.42f);
            PlinthYard = Damp(StoryArtKit.Surface("LTB_PlinthYard", "japanese_stone_wall", new Color(0.56f, 0.55f, 0.51f), 1.4f, 0.22f, 0.8f, 0.5f, 0.8f, Folder), Floor + 0.28f);
            Handrail = Arm(StoryArtKit.Surface("LTB_Handrail", "dark_wood", new Color(0.9f, 0.56f, 0.5f), 1.2f, 0.62f, 0.25f, 0.12f, 0f, Folder), null, true);
            TimberDusty = Dusty(Arm(StoryArtKit.Surface("LTB_TimberDusty", "dark_wood", new Color(0.72f, 0.56f, 0.5f), 1.1f, 0.5f, 0.9f, 0.25f, 0.35f, Folder), "dark_wood", true), 0.7f);
            Paper = Mat("LTB_Paper", new Color(0.82f, 0.75f, 0.62f), 0.1f);
            PaperLit = EmissiveMat("LTB_PaperLit", new Color(0.95f, 0.78f, 0.52f), 0.55f);
            Iron = Mat("LTB_Iron", new Color(0.11f, 0.105f, 0.1f), 0.45f);
            Bamboo = Mat("LTB_Bamboo", new Color(0.52f, 0.42f, 0.24f), 0.3f);
        }

        /// <summary>Vertical faces darker and glossier below world height <paramref name="line"/> (WorldTriplanarLit _DAMP_BASE).</summary>
        private static Material Damp(Material material, float line)
        {
            material.EnableKeyword("_DAMP_BASE");
            material.SetFloat("_ClearCoatMask", line);
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Dust on the upward faces (WorldTriplanarLit _DUST_TOP).</summary>
        public static Material Dusty(Material material, float amount)
        {
            material.EnableKeyword("_DUST_TOP");
            material.SetFloat("_ClearCoatSmoothness", amount);
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// Real roughness and AO from the texture set's Poly Haven ARM map (<paramref name="texture"/> null: none, e.g. a
        /// lacquer coat or a water film whose gloss is even); <c>_Smoothness</c> is then the gloss of the smoothest spot.
        /// <paramref name="grain"/>: the wood grain follows each piece's length instead of the world axes.
        /// </summary>
        private static Material Arm(Material material, string texture, bool grain = false)
        {
            if (texture != null)
            {
                string path = StoryArtKit.TextureRoot + "/" + texture + "/" + texture + "_arm_1k.jpg";
                if (UnityEditor.AssetImporter.GetAtPath(path) is UnityEditor.TextureImporter importer && (importer.sRGBTexture || importer.maxTextureSize != 1024 || importer.anisoLevel != 4))
                {
                    importer.sRGBTexture = false;
                    importer.maxTextureSize = 1024;
                    importer.anisoLevel = 4;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.SaveAndReimport();
                }
                var arm = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (arm == null) throw new System.InvalidOperationException("Missing ARM map " + path);
                material.SetTexture("_MetallicGlossMap", arm);
                material.EnableKeyword("_ARM_MAP");
            }
            else
            {
                material.SetTexture("_MetallicGlossMap", null);
                material.DisableKeyword("_ARM_MAP");
            }
            if (grain) material.EnableKeyword("_GRAIN_FOLLOWS_OBJECT");
            else material.DisableKeyword("_GRAIN_FOLLOWS_OBJECT");
            UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Plain URP Lit material; an existing asset is brought in line with these values (the code is the source).</summary>
        public static Material Mat(string name, Color color, float smoothness = 0.15f)
        {
            string path = Folder + "/" + name + ".mat";
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            // Silks and cottons carry a woven surface (generated, see LauTuBaTextures); their meshes have UVs in metres.
            bool silk = name.StartsWith("LTB_Silk"), cotton = name.StartsWith("LTB_Cotton");
            if (silk || cotton)
            {
                var (weave, weaveNormal) = Weaves(silk);
                material.SetTexture("_BaseMap", weave);
                material.SetTexture("_BumpMap", weaveNormal);
                material.SetTextureScale("_BaseMap", Vector2.one / (silk ? 0.3f : 0.45f));
                material.SetFloat("_BumpScale", silk ? 0.5f : 0.8f);
                material.EnableKeyword("_NORMALMAP");
            }
            material.enableInstancing = true;
            if (create) UnityEditor.AssetDatabase.CreateAsset(material, path);
            else UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        private static readonly Dictionary<bool, (Texture2D, Texture2D)> WeaveCache = new Dictionary<bool, (Texture2D, Texture2D)>();

        private static (Texture2D albedo, Texture2D normal) Weaves(bool silk)
        {
            if (!WeaveCache.TryGetValue(silk, out var weave) || weave.Item1 == null) WeaveCache[silk] = weave = LauTuBaTextures.Weave(silk);
            return weave;
        }

        public static Material EmissiveMat(string name, Color color, float intensity)
        {
            string path = Folder + "/" + name + ".mat";
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.enableInstancing = true;
            if (create) UnityEditor.AssetDatabase.CreateAsset(material, path);
            else UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Flat unlit colour (a thin guide line drawn over the scene, not lit by it).</summary>
        public static Material UnlitMat(string name, Color color)
        {
            string path = Folder + "/" + name + ".mat";
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color);
            if (create) UnityEditor.AssetDatabase.CreateAsset(material, path);
            else UnityEditor.EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------ primitives

        /// <summary>Box between two corners (min/max), solid by default.</summary>
        public static GameObject Box(string name, Vector3 min, Vector3 max, Material material, Transform parent, bool solid = true)
        {
            Vector3 size = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), Mathf.Abs(max.z - min.z));
            if (size.x < 0.001f || size.y < 0.001f || size.z < 0.001f) return null;
            return Block(name, (min + max) * 0.5f, size, material, parent, solid);
        }

        /// <summary>Floor slab whose top is at <paramref name="top"/>, tagged with its surface for footsteps.</summary>
        public static GameObject FloorSlab(string name, float x0, float z0, float x1, float z1, float top, float thickness, Material material, Surface surface, Transform parent)
        {
            var slab = Box(name, new Vector3(x0, top - thickness, z0), new Vector3(x1, top, z1), material, parent);
            slab.AddComponent<SurfaceTag>();
            var serialized = new UnityEditor.SerializedObject(slab.GetComponent<SurfaceTag>());
            serialized.FindProperty("_surface").enumValueIndex = (int)surface;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return slab;
        }

        /// <summary>
        /// A straight wall from <paramref name="a"/> to <paramref name="b"/> (on the floor plane, y ignored), standing on
        /// <paramref name="baseY"/>, with real holes for doors and windows (distances along the wall from a). Each opening
        /// gets a dark timber frame; the wall gets a stone or timber skirting and a top beam.
        /// Returns the wall pieces' root.
        /// </summary>
        public static Transform Wall(string name, Vector2 a, Vector2 b, float baseY, float height, float thickness, Material face, Transform parent,
            Material skirting = null, params Opening[] openings)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Vector2 dir = (b - a).normalized;
            float length = Vector2.Distance(a, b);
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            var sorted = new List<Opening>(openings);
            sorted.Sort((p, q) => p.From.CompareTo(q.From));
            float cursor = 0f;
            void Piece(string pieceName, float from, float to, float bottom, float top)
            {
                if (to - from < 0.01f || top - bottom < 0.01f) return;
                Vector2 centre = a + dir * ((from + to) * 0.5f);
                var go = Block(pieceName, new Vector3(centre.x, baseY + (bottom + top) * 0.5f, centre.y), new Vector3(thickness, top - bottom, to - from), face, root, true, yaw);
                go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            foreach (var opening in sorted)
            {
                Piece("Wall", cursor, opening.From, 0f, height);
                Piece("Sill", opening.From, opening.To, 0f, opening.Bottom);
                Piece("Lintel", opening.From, opening.To, opening.Top, height);
                OpeningFrame(root, a, dir, yaw, baseY, thickness, opening);
                cursor = opening.To;
            }
            Piece("Wall", cursor, length, 0f, height);

            // Dark timber: a beam along the top and posts at both ends, the frame that carries the plaster.
            Trim(root, a, dir, yaw, 0f, length, baseY + height - 0.12f, 0.24f, thickness + 0.06f);
            Post(root, a + dir * 0.09f, baseY, height, thickness + 0.06f, yaw);
            Post(root, b - dir * 0.09f, baseY, height, thickness + 0.06f, yaw);
            if (skirting != null)
            {
                cursor = 0f;
                foreach (var opening in sorted)
                {
                    if (opening.Bottom < 0.01f) { Trim(root, a, dir, yaw, cursor, opening.From, baseY + 0.12f, 0.24f, thickness + 0.04f, skirting); cursor = opening.To; }
                }
                Trim(root, a, dir, yaw, cursor, length, baseY + 0.12f, 0.24f, thickness + 0.04f, skirting);
            }
            return root;
        }

        private static void OpeningFrame(Transform root, Vector2 a, Vector2 dir, float yaw, float baseY, float thickness, Opening opening)
        {
            float w = 0.09f;
            Vector2 left = a + dir * (opening.From + w * 0.5f);
            Vector2 right = a + dir * (opening.To - w * 0.5f);
            float depth = thickness + 0.05f;
            foreach (var p in new[] { left, right })
            {
                var post = Block("Jamb", new Vector3(p.x, baseY + (opening.Bottom + opening.Top) * 0.5f, p.y), new Vector3(depth, opening.Top - opening.Bottom, w), Timber, root, false);
                post.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            Trim(root, a, dir, yaw, opening.From - 0.06f, opening.To + 0.06f, baseY + opening.Top + 0.05f, 0.1f, depth);
            if (opening.Bottom > 0.01f) Trim(root, a, dir, yaw, opening.From - 0.04f, opening.To + 0.04f, baseY + opening.Bottom - 0.03f, 0.06f, depth + 0.04f);
            else Trim(root, a, dir, yaw, opening.From, opening.To, baseY + 0.02f, 0.04f, depth, Plinth); // threshold
        }

        /// <summary>A horizontal strip along the wall line (beam, skirting, threshold); decorative, no collider.</summary>
        public static void Trim(Transform root, Vector2 a, Vector2 dir, float yaw, float from, float to, float y, float height, float depth, Material material = null)
        {
            if (to - from < 0.01f) return;
            Vector2 centre = a + dir * ((from + to) * 0.5f);
            var beam = Block("Trim", new Vector3(centre.x, y, centre.y), new Vector3(depth, height, to - from), material ?? Timber, root, false);
            beam.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public static void Post(Transform root, Vector2 at, float baseY, float height, float depth, float yaw)
        {
            var post = Block("Post", new Vector3(at.x, baseY + height * 0.5f, at.y), new Vector3(depth, height, 0.18f), Timber, root, false);
            post.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        /// <summary>Plank ceiling with cross beams, at <paramref name="y"/>, so looking up indoors shows a room, not a roof's underside.</summary>
        public static void CeilingPanel(string name, float x0, float z0, float x1, float z1, float y, Transform parent, bool beamsAlongX = false)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Box("Boards", new Vector3(x0, y, z0), new Vector3(x1, y + 0.06f, z1), Ceiling, root, false);
            float span = beamsAlongX ? z1 - z0 : x1 - x0;
            int beams = Mathf.Max(1, Mathf.RoundToInt(span / 1.6f));
            for (int i = 1; i <= beams; i++)
            {
                float t = i / (beams + 1f);
                if (beamsAlongX) Box("Beam", new Vector3(x0, y - 0.18f, Mathf.Lerp(z0, z1, t) - 0.08f), new Vector3(x1, y, Mathf.Lerp(z0, z1, t) + 0.08f), Timber, root, false);
                else Box("Beam", new Vector3(Mathf.Lerp(x0, x1, t) - 0.08f, y - 0.18f, z0), new Vector3(Mathf.Lerp(x0, x1, t) + 0.08f, y, z1), Timber, root, false);
            }
        }

        /// <summary>
        /// Underside of a veranda roof seen from below: rafters running out from the wall to the eave beam over the pillars,
        /// boards above them. <paramref name="wall"/> is the high side, <paramref name="eave"/> the low outer edge.
        /// </summary>
        public static void Eave(string name, Vector3 wallStart, Vector3 wallEnd, Vector3 outward, float depth, float drop, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Vector3 along = wallEnd - wallStart;
            float length = along.magnitude;
            Vector3 dir = along / length;
            Vector3 down = Vector3.down * drop;
            Vector3 slopeDir = (outward * depth + down).normalized;
            Vector3 normal = Vector3.Cross(dir, slopeDir);
            if (normal.y < 0f) normal = -normal;
            Quaternion slope = Quaternion.LookRotation(slopeDir, normal);
            float slant = Mathf.Sqrt(depth * depth + drop * drop);
            Vector3 middle = (wallStart + wallEnd) * 0.5f + outward * depth * 0.5f + down * 0.5f;
            // Boards: one slanted slab resting on the rafters.
            var boards = Block("EaveBoards", middle + normal * 0.06f, new Vector3(length + 0.4f, 0.05f, slant + 0.5f), Ceiling, root, false);
            boards.transform.rotation = slope;
            // Rafters every 0.55 m.
            int count = Mathf.Max(2, Mathf.RoundToInt(length / 0.55f));
            for (int i = 0; i <= count; i++)
            {
                Vector3 foot = wallStart + dir * (length * i / count);
                var rafter = Block("Rafter", foot + outward * depth * 0.5f + down * 0.5f - normal * 0.01f, new Vector3(0.07f, 0.09f, slant + 0.45f), Timber, root, false);
                rafter.transform.rotation = slope;
            }
            // Eave beam riding on the pillars, and the fascia with round tile ends hanging over the yard.
            Vector3 eaveLine = (wallStart + wallEnd) * 0.5f + outward * depth + down;
            var beam = Block("EaveBeam", eaveLine - Vector3.up * 0.12f, new Vector3(0.2f, 0.24f, length + 0.4f), Timber, root, false);
            beam.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var fascia = Block("Fascia", eaveLine + outward * 0.42f + Vector3.up * 0.02f, new Vector3(0.06f, 0.18f, length + 0.6f), Ridge, root, false);
            fascia.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            int tiles = Mathf.RoundToInt((length + 0.6f) / 0.24f);
            for (int i = 0; i <= tiles; i++)
            {
                Vector3 p = eaveLine + outward * 0.47f + dir * (-length * 0.5f - 0.3f + (length + 0.6f) * i / tiles) + Vector3.up * 0.05f;
                var end = Cylinder("TileEnd", p, 0.06f, 0.05f, Tiles, root, false);
                end.transform.rotation = Quaternion.LookRotation(Vector3.up, outward) * Quaternion.Euler(90f, 0f, 0f);
                end.transform.position = p;
                end.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// A veranda railing between <paramref name="a"/> and <paramref name="b"/>: a moulded hand rail with a rounded top,
        /// a bottom rail and turned balusters (con tiện) every 15 cm, one mesh per run, grain along each member. A box
        /// collider runs its full height so nobody steps or is pathed over it.
        /// </summary>
        public static void Railing(string name, Vector3 a, Vector3 b, Transform parent, float height = 0.62f)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Vector3 along = b - a;
            float length = along.magnitude;
            root.SetPositionAndRotation(a, Quaternion.LookRotation(along / length, Vector3.up));
            var blocker = new GameObject("Rail");
            blocker.transform.SetParent(root, false);
            blocker.transform.localPosition = new Vector3(0f, (height + 0.035f) * 0.5f, length * 0.5f);
            blocker.layer = EnvironmentLayer;
            blocker.isStatic = true;
            blocker.AddComponent<BoxCollider>().size = new Vector3(0.09f, height + 0.035f, length);
            var visual = new GameObject("Railing");
            visual.transform.SetParent(root, false);
            visual.AddComponent<MeshFilter>().sharedMesh = RailingMesh(length, height);
            visual.AddComponent<MeshRenderer>().sharedMaterials = new[] { Lacquer, Timber, Handrail };
            visual.isStatic = true;
        }

        private static Mesh RailingMesh(float length, float height)
        {
            string key = $"Joinery_Railing_{length:F2}_{height:F2}_v2".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            // Hand rail: flat underside, rounded crown.
            var hand = new List<Vector2> { new Vector2(0.045f, -0.025f), new Vector2(0.045f, 0.0f) };
            for (int i = 1; i < 9; i++)
            {
                float angle = Mathf.PI * i / 9f;
                hand.Add(new Vector2(Mathf.Cos(angle) * 0.045f, Mathf.Sin(angle) * 0.026f));
            }
            hand.Add(new Vector2(-0.045f, 0.0f));
            hand.Add(new Vector2(-0.045f, -0.025f));
            // The hand rail in its own, hand-polished material (submesh 2).
            j.Sweep(new[] { new Vector3(0f, height, 0f), new Vector3(0f, height, length) }, hand.ToArray(), Vector3.up, 2);
            j.Member(new Vector3(0f, 0.11f, 0f), new Vector3(0f, 0.11f, length), 0.07f, 0.05f, 0.01f, Vector3.up);
            // Turned balusters between the rails: a square-ish foot, a vase, a neck, rings under the rail.
            float bottom = 0.135f, span = height - 0.025f - bottom;
            int count = Mathf.Max(2, Mathf.RoundToInt(length / 0.15f));
            var profile = new[]
            {
                new Vector2(0.02f, 0f), new Vector2(0.021f, span * 0.08f), new Vector2(0.014f, span * 0.12f), new Vector2(0.024f, span * 0.3f), new Vector2(0.026f, span * 0.4f),
                new Vector2(0.015f, span * 0.6f), new Vector2(0.012f, span * 0.78f), new Vector2(0.019f, span * 0.84f), new Vector2(0.013f, span * 0.9f), new Vector2(0.02f, span)
            };
            for (int i = 1; i < count; i++) j.Turned(new Vector3(0f, bottom, length * i / count), profile, 8, 1);
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        /// <summary>Stone steps from the yard (y 0) up to a raised floor, <paramref name="width"/> wide, climbing towards <paramref name="up"/>.</summary>
        public static void Steps(string name, Vector3 bottomCentre, Vector3 up, float width, float rise, Transform parent, int count = 2)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Quaternion facing = Quaternion.LookRotation(up, Vector3.up);
            float tread = 0.32f;
            for (int i = 0; i < count; i++)
            {
                float h = rise * (i + 1) / count;
                Vector3 centre = bottomCentre + up * (tread * (i - count + 0.5f)) + Vector3.up * h * 0.5f;
                var step = Block("Step", centre, new Vector3(width, h, tread), Plinth, root);
                step.transform.rotation = facing;
                step.AddComponent<SurfaceTag>();
                var tag = new UnityEditor.SerializedObject(step.GetComponent<SurfaceTag>());
                tag.FindProperty("_surface").enumValueIndex = (int)Surface.Stone;
                tag.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>Iron bars set in a window opening (bars run vertically, set into the wall thickness).</summary>
        public static void Bars(string name, Vector3 centre, float width, float height, float yaw, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(centre, Quaternion.Euler(0f, yaw, 0f));
            int count = Mathf.Max(3, Mathf.RoundToInt(width / 0.13f));
            for (int i = 1; i < count; i++)
            {
                var bar = Block("Bar", Vector3.zero, new Vector3(0.028f, height, 0.028f), Iron, root, false);
                bar.transform.localPosition = new Vector3(-width * 0.5f + width * i / count, 0f, 0f);
                bar.transform.localRotation = Quaternion.identity;
            }
            foreach (float y in new[] { -height * 0.32f, height * 0.32f })
            {
                var cross = Block("BarCross", Vector3.zero, new Vector3(width, 0.03f, 0.02f), Iron, root, false);
                cross.transform.localPosition = new Vector3(0f, y, 0f);
                cross.transform.localRotation = Quaternion.identity;
            }
            // A clear collider: nothing passes, but sight does (bars are thin).
            var block = new GameObject("BarsBlock");
            block.transform.SetParent(root, false);
            block.layer = 2;
            var box = block.AddComponent<BoxCollider>();
            box.size = new Vector3(width, height, 0.08f);
        }

        /// <summary>Paper lattice set into a window opening: lit paper glows when the room behind has a lamp.</summary>
        public static GameObject PaperWindow(string name, Vector3 centre, float width, float height, float yaw, bool lit, Transform parent)
        {
            var window = StoryArtKit.Lattice(name, centre, width, height, yaw, Timber, lit ? LitPaperAt(centre) : Paper, parent);
            // Paper and slats block sight (it is a wall you cannot see through, only a glow and shadows).
            var blocker = new GameObject("Sight");
            blocker.transform.SetParent(window.transform, false);
            blocker.layer = EnvironmentLayer;
            blocker.AddComponent<BoxCollider>().size = new Vector3(width, height, 0.05f);
            return window;
        }

        private static Material[] _litPapers;

        /// <summary>
        /// Lamp-lit paper for a window at <paramref name="at"/>: one of four, each with the lamp standing somewhere else behind
        /// it and burning a little brighter or lower, the paper's fibres showing, darker towards the frame. Neighbouring
        /// windows rarely match, so a row of them does not read as identical flat panels.
        /// </summary>
        private static Material LitPaperAt(Vector3 at)
        {
            if (_litPapers == null || _litPapers[0] == null)
            {
                _litPapers = new Material[4];
                var hot = new[] { new Vector2(0.38f, 0.42f), new Vector2(0.7f, 0.3f), new Vector2(0.5f, 0.62f), new Vector2(0.22f, 0.35f) };
                var spread = new[] { 0.42f, 0.33f, 0.55f, 0.28f };
                var tint = new[] { new Color(0.95f, 0.76f, 0.5f), new Color(1f, 0.72f, 0.44f), new Color(0.92f, 0.78f, 0.56f), new Color(1f, 0.68f, 0.4f) };
                var glow = new[] { 0.6f, 0.5f, 0.44f, 0.32f };
                for (int i = 0; i < 4; i++)
                {
                    var paper = LauTuBaTextures.LitPaper(i + 1, hot[i], spread[i]);
                    var material = EmissiveMat("LTB_PaperLit" + i, tint[i], glow[i]);
                    material.SetTexture("_BaseMap", paper);
                    material.SetTexture("_EmissionMap", paper);
                    material.SetColor("_EmissionColor", tint[i] * glow[i]);
                    UnityEditor.EditorUtility.SetDirty(material);
                    _litPapers[i] = material;
                }
            }
            int pick = Mathf.Abs(Mathf.RoundToInt(at.x * 3.1f + at.z * 1.7f + at.y * 5.3f)) % 4;
            return _litPapers[pick];
        }

        /// <summary>See-through slatted screen (the lattice partition): blocks bodies, not eyes.</summary>
        public static void Slats(string name, Vector3 a, Vector3 b, float bottom, float top, Transform parent, float spacing = 0.11f)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Vector3 along = b - a;
            float length = along.magnitude;
            Quaternion facing = Quaternion.LookRotation(along / length, Vector3.up);
            int count = Mathf.Max(2, Mathf.RoundToInt(length / spacing));
            for (int i = 0; i <= count; i++)
            {
                var slat = Block("Slat", Vector3.Lerp(a, b, i / (float)count) + Vector3.up * ((bottom + top) * 0.5f), new Vector3(0.035f, top - bottom, 0.05f), Timber, root, false);
                slat.transform.rotation = facing;
            }
            var rail = Block("SlatRail", (a + b) * 0.5f + Vector3.up * top, new Vector3(0.08f, 0.08f, length), Timber, root, false);
            rail.transform.rotation = facing;
            var railLow = Block("SlatRail", (a + b) * 0.5f + Vector3.up * bottom, new Vector3(0.08f, 0.08f, length), Timber, root, false);
            railLow.transform.rotation = facing;
            // Bodies stop here; eyes (Environment linecasts) pass between the slats.
            var block = new GameObject("SlatsBlock");
            block.transform.SetParent(root, false);
            block.layer = 2;
            block.transform.SetPositionAndRotation((a + b) * 0.5f + Vector3.up * ((bottom + top) * 0.5f), facing);
            block.AddComponent<BoxCollider>().size = new Vector3(0.08f, top - bottom, length);
            block.AddComponent<NavMeshObstacle>().carving = true;
        }

        // ------------------------------------------------------------------ doors

        /// <summary>
        /// A working door in an opening: a hinged leaf (hinge at <paramref name="hinge"/>, closed leaf pointing along
        /// <paramref name="closedDir"/>) or a sliding panel. The leaf is solid, blocks sight, carves the NavMesh while shut
        /// and is left out of the NavMesh bake so open doors can be walked through.
        /// </summary>
        public static HouseDoor Door(string name, DoorKind kind, Vector3 hinge, Vector3 closedDir, float width, float height, float swing, Material face,
            Transform parent, StoryAudio audio, KieuInventory inventory, string label, DoorLock lockState = DoorLock.None, Transform lockSide = null, bool paper = false, bool panelled = true)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.position = hinge;
            root.rotation = Quaternion.LookRotation(closedDir, Vector3.up);
            var leaf = new GameObject("Leaf").transform;
            leaf.SetParent(root, false);
            float thickness = paper ? 0.05f : 0.07f;
            // The leaf's geometry runs along local +Z from the hinge.
            var panel = Block("Panel", Vector3.zero, new Vector3(thickness, height - 0.02f, width - 0.02f), face, leaf, true);
            panel.transform.localPosition = new Vector3(0f, height * 0.5f, width * 0.5f);
            panel.transform.localRotation = Quaternion.identity;
            panel.isStatic = false;
            panel.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            var obstacle = panel.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.size = Vector3.one;
            if (paper)
            {
                // Frame and lattice over the paper panel.
                foreach (float z in new[] { 0.03f, width - 0.03f })
                    Child(leaf, "Stile", new Vector3(0f, height * 0.5f, z), new Vector3(thickness + 0.03f, height, 0.06f), Timber);
                for (float y = 0.4f; y < height - 0.1f; y += 0.3f) Child(leaf, "Rail", new Vector3(0f, y, width * 0.5f), new Vector3(thickness + 0.02f, 0.025f, width), Timber);
                for (float z = 0.25f; z < width - 0.1f; z += 0.25f) Child(leaf, "Bar", new Vector3(0f, height * 0.5f, z), new Vector3(thickness + 0.02f, height, 0.025f), Timber);
            }
            else
            {
                if (panelled)
                {
                    // Frame and panel (cửa bức bàn): stiles and rails round raised panels, one mesh. The plain panel only
                    // collides and carves the NavMesh.
                    panel.GetComponent<MeshRenderer>().enabled = false;
                    var joinery = new GameObject("Joinery");
                    joinery.transform.SetParent(leaf, false);
                    joinery.AddComponent<MeshFilter>().sharedMesh = DoorLeafMesh(width - 0.02f, height - 0.02f, thickness);
                    // The frame in the house's red-brown timber, the panels in the door's own wood.
                    joinery.AddComponent<MeshRenderer>().sharedMaterials = new[] { Timber, face };
                }
                else
                {
                    // Planks with two battens (a back door).
                    Child(leaf, "Batten", new Vector3(0.045f, 0.45f, width * 0.5f), new Vector3(0.03f, 0.12f, width - 0.1f), Timber);
                    Child(leaf, "Batten", new Vector3(0.045f, height - 0.45f, width * 0.5f), new Vector3(0.03f, 0.12f, width - 0.1f), Timber);
                }
                Child(leaf, "Ring", new Vector3(0.06f, 1.05f, width - 0.14f), new Vector3(0.02f, 0.09f, 0.09f), Iron);
                Child(leaf, "Ring", new Vector3(-0.06f, 1.05f, width - 0.14f), new Vector3(0.02f, 0.09f, 0.09f), Iron);
                // The wooden bolt (then cửa) on the side it is shut from, sliding in two iron staples towards the jamb.
                if (width <= 1.3f)
                {
                    float side = lockSide != null && root.InverseTransformPoint(lockSide.position).x < 0f ? -1f : 1f;
                    float face0 = side * (thickness * 0.5f + 0.02f);
                    Child(leaf, "Bolt", new Vector3(face0, 1.28f, width - 0.2f), new Vector3(0.04f, 0.05f, 0.34f), Timber);
                    foreach (float z in new[] { width - 0.32f, width - 0.1f })
                        Child(leaf, "Staple", new Vector3(face0, 1.28f, z), new Vector3(0.05f, 0.08f, 0.025f), Iron);
                    Child(leaf, "BoltKnob", new Vector3(face0 + side * 0.03f, 1.28f, width - 0.22f), new Vector3(0.03f, 0.03f, 0.03f), Iron);
                }
            }
            if (kind == DoorKind.Hinged)
            {
                // Pin knuckles on the frame and iron straps across both faces of the leaf.
                foreach (float y in new[] { 0.32f, height - 0.32f })
                {
                    Child(root, "Knuckle", new Vector3(0f, y, 0.02f), new Vector3(0.045f, 0.14f, 0.045f), Iron);
                    foreach (float x in new[] { thickness * 0.5f + 0.005f, -thickness * 0.5f - 0.005f })
                        Child(leaf, "Strap", new Vector3(x, y, 0.2f), new Vector3(0.01f, 0.045f, 0.36f), Iron);
                }
            }
            else
            {
                // The track it runs in: a timber head rail with two lips above, a worn runner below.
                float span = width * 2f;
                foreach (float x in new[] { thickness * 0.5f + 0.02f, -thickness * 0.5f - 0.02f })
                    Child(root, "TrackLip", new Vector3(x, height + 0.02f, width * 0.5f - width * 0.47f), new Vector3(0.025f, 0.07f, span), Timber);
                Child(root, "TrackHead", new Vector3(0f, height + 0.065f, width * 0.5f - width * 0.47f), new Vector3(thickness + 0.07f, 0.03f, span), Timber);
                Child(root, "Runner", new Vector3(0f, 0.008f, width * 0.5f - width * 0.47f), new Vector3(thickness + 0.04f, 0.016f, span), Plinth);
                foreach (float x in new[] { thickness * 0.5f + 0.004f, -thickness * 0.5f - 0.004f })
                    Child(leaf, "Pull", new Vector3(x, 1.0f, width - 0.12f), new Vector3(0.008f, 0.12f, 0.035f), Iron);
            }
            var door = root.gameObject.AddComponent<HouseDoor>();
            Set(door, "_kind", (int)kind);
            Set(door, "_leaf", leaf);
            Set(door, "_openAngle", swing);
            Set(door, "_slide", new Vector3(0f, 0f, -width * 0.95f));
            Set(door, "_lock", (int)lockState);
            Set(door, "_lockSide", lockSide);
            Set(door, "_name", label);
            Set(door, "_audio", audio);
            Set(door, "_inventory", inventory);
            return door;
        }

        /// <summary>
        /// A frame-and-panel door leaf in the leaf's space (X through the leaf, Y up, Z from the hinge): two stiles, a
        /// deep bottom rail, a lock rail and a top rail, and in each opening a thin panel with a raised field standing
        /// proud on both faces.
        /// </summary>
        private static Mesh DoorLeafMesh(float width, float height, float thickness)
        {
            string key = $"Joinery_DoorLeaf_{width:F2}_{height:F2}_{thickness:F3}_v2".Replace('.', 'p');
            if (LauTuBaMeshKit.Has(key, out var cached)) return cached;
            var j = new LauTuBaJoinery();
            const float z0 = 0.01f, y0 = 0.01f, top = 0.1f, lockRail = 0.12f, bottom = 0.17f;
            // Stiles wide enough to show past the jambs that cover the leaf's outer 9 cm.
            float stile = Mathf.Min(0.17f, width * 0.15f), lockY = Mathf.Min(1.0f, height * 0.42f);
            foreach (float z in new[] { z0 + stile * 0.5f, z0 + width - stile * 0.5f })
                j.Member(new Vector3(0f, y0, z), new Vector3(0f, y0 + height, z), stile, thickness, 0.008f, Vector3.right);
            foreach (var (y, h) in new[] { (y0 + bottom * 0.5f, bottom), (y0 + lockY, lockRail), (y0 + height - top * 0.5f, top) })
                j.Member(new Vector3(0f, y, z0 + stile - 0.005f), new Vector3(0f, y, z0 + width - stile + 0.005f), h, thickness * 0.92f, 0.006f, Vector3.right);
            float panelWidth = width - 2f * stile + 0.01f;
            foreach (var (low, high) in new[] { (y0 + bottom, y0 + lockY - lockRail * 0.5f), (y0 + lockY + lockRail * 0.5f, y0 + height - top) })
            {
                float panelHeight = high - low + 0.01f;
                // Grain runs up the panels: the outline is laid out height-first and turned upright.
                var place = Matrix4x4.TRS(new Vector3(0f, (low + high) * 0.5f, z0 + width * 0.5f), Quaternion.Euler(0f, -90f, 0f) * Quaternion.Euler(0f, 0f, 90f), Vector3.one);
                j.Board(Outline(panelHeight, panelWidth), thickness * 0.3f, place, 1);
                j.Board(Outline(panelHeight - 0.1f, panelWidth - 0.1f), thickness * 0.82f, place, 1);
            }
            return LauTuBaMeshKit.Keep(key, j.Build(key));
        }

        private static Vector2[] Outline(float width, float height) =>
            new[] { new Vector2(-width * 0.5f, -height * 0.5f), new Vector2(width * 0.5f, -height * 0.5f), new Vector2(width * 0.5f, height * 0.5f), new Vector2(-width * 0.5f, height * 0.5f) };

        private static readonly Dictionary<Material, Material> UvTwins = new Dictionary<Material, Material>();

        /// <summary>
        /// The URP Lit twin of a house wood for meshes with real UVs in metres (joinery): same textures, tile size, tint
        /// and normal strength; a Poly Haven ARM map becomes URP's metallic-smoothness and occlusion maps (repacked, the
        /// same data). Non-triplanar materials are returned as they are.
        /// </summary>
        public static Material UvTwin(Material source)
        {
            if (source == null || source.shader.name != StoryArtKit.TriplanarShader) return source;
            if (UvTwins.TryGetValue(source, out var twin) && twin != null) return twin;
            string path = Folder + "/" + source.name + "_UV.mat";
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = material == null;
            if (create) material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
            material.SetTextureScale("_BaseMap", source.GetTextureScale("_BaseMap"));
            material.SetColor("_BaseColor", source.GetColor("_BaseColor"));
            material.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            material.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", source.GetFloat("_Smoothness"));
            if (source.IsKeywordEnabled("_ARM_MAP") && source.GetTexture("_MetallicGlossMap") is Texture2D arm)
            {
                var packed = LauTuBaTextures.MetallicSmoothness(arm);
                material.SetTexture("_MetallicGlossMap", packed);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap", packed);
                material.EnableKeyword("_OCCLUSIONMAP");
                material.SetFloat("_OcclusionStrength", 1f);
            }
            else
            {
                material.SetTexture("_MetallicGlossMap", null);
                material.DisableKeyword("_METALLICSPECGLOSSMAP");
                material.SetTexture("_OcclusionMap", null);
                material.DisableKeyword("_OCCLUSIONMAP");
            }
            material.enableInstancing = true;
            if (create) UnityEditor.AssetDatabase.CreateAsset(material, path);
            else UnityEditor.EditorUtility.SetDirty(material);
            UvTwins[source] = material;
            return material;
        }

        private static void Child(Transform parent, string name, Vector3 local, Vector3 size, Material material)
        {
            var go = Block(name, Vector3.zero, size, material, parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.identity;
        }

        // ------------------------------------------------------------------ stealth helpers

        public static HideSpot HideSpot(string name, Vector3 inside, float yaw, Vector3 triggerCentre, Vector3 triggerSize, PlayerMovement player, string label, string sfx, Transform parent, Transform shake = null, float peek = 55f)
        {
            var go = new GameObject("Hide_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = triggerCentre;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = triggerSize;
            var spot = go.AddComponent<HideSpot>();
            var point = Point("Inside", inside, yaw, go.transform);
            Set(spot, "_player", player);
            Set(spot, "_inside", point);
            Set(spot, "_label", label);
            Set(spot, "_sfx", sfx);
            Set(spot, "_peekRange", peek);
            if (shake != null) Set(spot, "_shakeVisual", shake);
            return spot;
        }

        public static NoiseZone NoiseZone(string name, Vector3 centre, Vector3 size, float scale, Transform parent)
        {
            var go = new GameObject("Noise_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = centre;
            go.layer = 2;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var zone = go.AddComponent<NoiseZone>();
            Set(zone, "_scale", scale);
            return zone;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue.Editor;
using ThuyKieu.Environment;
using ThuyKieu.Environment.Editor;
using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Interaction;
using ThuyKieu.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using Object = UnityEngine.Object;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Puts the hand-made Chapter01_GiaBien house on the same story systems as the other chapters: StoryDirector,
    /// the shared StoryCanvas (dialogue, HUD, pause, journal, Trang Sách), dialogue camera rig, explore targets,
    /// observation, suspicion and checkpoints. The house, its lights, rain and dressing are kept as they are; only the
    /// systems, the player, the camera and the characters are (re)created, so it is safe to run again — the story
    /// rebuild runs it to re-link the shared UI and animation assets it regenerates.
    ///   -executeMethod ThuyKieu.Core.Editor.Chapter01StoryUpgrade.ApplyBatch
    /// </summary>
    public static class Chapter01StoryUpgrade
    {
        public const string ScenePath = "Assets/_Game/Scenes/Chapter01_GiaBien.unity";
        private const string Characters = "Assets/_Game/Art/Characters/";
        private const string StoryRoot = "Chapter01Story";

        /// <summary>Scene roots owned by the story systems (old Chapter 1 flow or a previous run of this upgrade).</summary>
        private static readonly string[] Replaced =
        {
            "Chapter01Systems", "StorySystems", "InkSession", "ThuyKieu", "PlayerSpawn", "Main Camera", "MeKieu", "MaGiamSinh",
            "TuyTung", "DialogueCanvas", "StoryCanvas", "EventSystem", "DialogueCameraPresets", StoryRoot
        };

        /// <summary>Leftovers of the old Chapter 1 flow inside the house: its door trigger, floating labels and music sources.</summary>
        private static readonly string[] Obsolete = { "MainDoorInteraction", "ContractLabel", "MusicSource", "UISource" };

        private struct Link { public Component Owner; public string Property; public string Root; public string Path; public string Type; }

        [MenuItem("ThuyKieu/Chapter 1/Apply story systems")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before the Chapter 1 upgrade.");
            InkStoryCompiler.Compile();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var roots = scene.GetRootGameObjects();
            var links = CollectLinks(roots);
            // A story rebuild regenerates the shared UI prefab, which leaves its old instance here as a missing prefab.
            foreach (var root in roots)
                if (Replaced.Contains(root.name) || PrefabUtility.IsPrefabAssetMissing(root)) Object.DestroyImmediate(root);
            foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Where(t => Obsolete.Contains(t.name)).ToList())
                if (t != null) Object.DestroyImmediate(t.gameObject);
            foreach (var go in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject))
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            Build();
            RestoreLinks(scene, links);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static void ApplyBatch()
        {
            try { Apply(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }

        private static void Build()
        {
            var s = CreateSystems(1, "Gia biến", "chapter_1", "Chapter02_RoadToMaGiamSinh", new Vector3(0f, 0.05f, -1.8f), 0f, "none");
            s.CameraComponent.farClipPlane = 90f;
            Chapter01LookUpgrade.ApplyCamera(s.CameraComponent);
            Set(s.Shots, "_keyLight", 1.2f);
            // The house has its own rain bed and wood/stone footsteps (Chapter01Audio); StoryAudio plays music and story effects.
            Set(s.Audio, "_player", null);

            // Generated materials go beside the Chapter 1 look assets: the story folder is wiped by every story rebuild.
            AssetFolder = Chapter01LookUpgrade.LookAssets;
            try { BuildStory(s); }
            finally { AssetFolder = MaterialFolder; }
        }

        private static void BuildStory(Systems s)
        {
            var director = s.Director;
            var root = new GameObject(StoryRoot).transform;

            // ------------------------------------------------------------ characters
            var shared = StoryAnimationSetup.Controller();
            var mother = RiggedActor("MeKieu", "MeKieu", Characters + "MeThuyKieu/Models/MeThuyKieu_Rigged.fbx", Characters + "MeThuyKieu/Tectures",
                1.56f, new Vector3(-2.2f, 0f, 1.35f), 115f, Color.white, shared);
            var ma = RiggedActor("MaGiamSinh", "MaGiamSinh", Characters + "MaGiamSinh/Models/MaGiamSinh_Rigged.fbx", Characters + "MaGiamSinh/Tectures",
                1.72f, new Vector3(1.0f, 0f, -1.2f), 310f, Color.white, shared);
            var servant = CharacterActor("TuyTung", "TuyTung", "LinhCanh", 1.72f, new Vector3(2.3f, 0f, -3.4f), 340f);

            // ------------------------------------------------------------ staging
            var staging = Group(root, "Staging");
            // Keep the door-to-table centre aisle readable. The family receives the visitor from the left side while
            // Mã and his attendant occupy the right/porch side, instead of everyone standing on one central line.
            var kieuMeet = Point("KieuMeet", new Vector3(-0.55f, 0.05f, -0.1f), 55f, staging);
            var motherMeet = Point("MotherMeet", new Vector3(-2.2f, 0f, 1.35f), 80f, staging);
            var maMeet = Point("MaMeet", new Vector3(1.25f, 0f, -0.75f), 310f, staging);
            var servantPorch = Point("ServantPorch", new Vector3(2.55f, 0f, -2.8f), 330f, staging);
            var motherTable = Point("MotherTable", new Vector3(-1.95f, 0f, 2.3f), 100f, staging);
            var maTable = Point("MaTable", new Vector3(1.8f, 0f, 2.5f), 250f, staging);
            var tableFocus = Point("TableFocus", new Vector3(0f, 1.0f, 3.0f), 0f, staging);
            var kieuDoor = Point("KieuDoor", new Vector3(0f, 0.05f, -1.9f), 180f, staging);
            var motherDoor = Point("MotherDoor", new Vector3(-1.1f, 0f, -1.4f), 170f, staging);
            var maGate = Point("MaGate", new Vector3(1.9f, 0f, -12.0f), 270f, staging);
            var servantGate = Point("ServantGate", new Vector3(-1.9f, 0f, -12.2f), 90f, staging);

            // ------------------------------------------------------------ cameras
            var cameras = Group(root, "Cameras");
            Shot(cameras, "WS_VuongGia", new Vector3(2.2f, 2.4f, -6.8f), new Vector3(0f, 1.4f, 0.6f));
            Shot(cameras, "Reveal_MaGiamSinh", new Vector3(-1.9f, 1.65f, 1.3f), new Vector3(1.0f, 1.55f, -1.2f));
            Shot(cameras, "Table_Contract", new Vector3(1.1f, 1.75f, 1.35f), new Vector3(0.1f, 0.82f, 2.75f));
            Shot(cameras, "Wide_Courtyard", new Vector3(-1.2f, 2.5f, -2.9f), new Vector3(4.8f, 0.3f, -6.0f));
            Shot(cameras, "Hero_Kieu_Rain", new Vector3(1.0f, 1.9f, -8.4f), new Vector3(0f, 1.2f, -12.4f));
            Shot(cameras, "OTS_Kieu_Cabinet", new Vector3(3.15f, 1.55f, 4.15f), new Vector3(4.8f, 1.02f, 5.58f));
            Shot(cameras, "OTS_Kieu_Altar", new Vector3(-2.15f, 1.62f, 4.28f), new Vector3(0f, 1.18f, 5.95f));
            Shot(cameras, "MCU_MeKieu", new Vector3(1.55f, 1.52f, 4.05f), new Vector3(-0.88f, 1.12f, 3.08f));
            Point("Focus_Cabinet", new Vector3(4.8f, 1.1f, 5.6f), 0f, cameras);
            Point("Focus_Altar", new Vector3(0f, 1.22f, 5.92f), 0f, cameras);

            // ------------------------------------------------------------ props
            var silver = Silver(root);
            SearchedCourtyard(root);
            var contract = ContractOnTable();

            // ------------------------------------------------------------ interactions
            var targets = Group(root, "Interactions");
            Explore(mother.gameObject, director, "c1_me", "Ngồi xuống bên Mẹ Kiều", Vector3.one * 1.9f);
            var farewell = Point("FarewellTarget", mother.transform.position, 0f, mother.transform);
            Explore(farewell.gameObject, director, "c1_me_cuoi", "Ôm Mẹ lần cuối", new Vector3(1.8f, 1.8f, 1.8f));
            Target(targets, director, "CabinetTarget", new Vector3(4.8f, 0f, 4.9f), "c1_tu", "Mở tủ gỗ", new Vector3(1.4f, 1.6f, 1.2f), new Vector3(4.8f, 0.05f, 4.7f), 0f);
            Target(targets, director, "AltarTarget", new Vector3(0f, 0f, 5.14f), "c1_ban_tho", "Thắp hương trên bàn thờ", new Vector3(1.4f, 1.6f, 1.2f), new Vector3(0f, 0.05f, 5.14f), 0f);
            Target(targets, director, "GateKnock", new Vector3(0f, 0f, -12.0f), "c1_cong", "Ra mở cổng", new Vector3(3.2f, 2f, 1.8f), null, 0f);
            Target(targets, director, "GateLeave", new Vector3(0f, 0f, -11.8f), "c1_ra_di", "Bước ra cổng", new Vector3(3.2f, 2f, 1.8f), new Vector3(0f, 0.05f, -11.5f), 180f);
            Target(targets, director, "SilverTarget", new Vector3(0.7f, 0f, 1.95f), "c1_tui_bac", "Nhìn túi bạc", new Vector3(1.1f, 1.4f, 0.9f), new Vector3(0.65f, 0.05f, 1.6f), 0f);
            Target(targets, director, "ContractTarget", new Vector3(-0.35f, 0f, 1.95f), "c1_khe", "Ngồi vào bàn, xem tờ khế", new Vector3(1.0f, 1.4f, 0.9f), new Vector3(-0.25f, 0.05f, 1.6f), 0f);
            Explore(ma.gameObject, director, "c1_hoi", "Đáp lời Mã Giám Sinh", Vector3.one * 1.9f);
            Observable(ma.gameObject, director, "c1_quan_sat", "Quan sát Mã Giám Sinh", ma.transform.position + Vector3.up * 1.55f, 11f);
            var askServant = Explore(servant.gameObject, director, "c1_tuy_tung", "Hỏi chuyện tên tùy tùng", Vector3.one * 1.9f);
            Set(askServant, "_standPoint", Point("ServantAskStand", new Vector3(1.2f, 0.05f, -3.1f), 105f, targets));
            var courtyard = Point("CourtyardObserve", new Vector3(4.8f, 0f, -6.0f), 0f, targets);
            Observable(courtyard.gameObject, director, "c1_san", "Quan sát khoảng sân bị lục soát", new Vector3(4.8f, 0.3f, -6.0f), 14f);

            // ------------------------------------------------------------ world
            var world = s.Root.AddComponent<Chapter01World>();
            Set(world, "_director", director);
            Set(world, "_mother", mother);
            Set(world, "_ma", ma);
            Set(world, "_servant", servant);
            Set(world, "_observe", s.Observe);
            Set(world, "_silver", silver);
            Set(world, "_contract", contract);
            Set(world, "_kieuMeet", kieuMeet);
            Set(world, "_motherMeet", motherMeet);
            Set(world, "_maMeet", maMeet);
            Set(world, "_servantPorch", servantPorch);
            Set(world, "_motherTable", motherTable);
            Set(world, "_maTable", maTable);
            Set(world, "_tableFocus", tableFocus);
            Set(world, "_kieuDoor", kieuDoor);
            Set(world, "_motherDoor", motherDoor);
            Set(world, "_maGate", maGate);
            Set(world, "_servantGate", servantGate);

            // The house's own presentation now follows the story director.
            var atmosphere = Object.FindAnyObjectByType<Chapter01Atmosphere>(FindObjectsInactive.Include);
            if (atmosphere != null) Set(atmosphere, "_director", director);
            var houseAudio = Object.FindAnyObjectByType<Chapter01Audio>(FindObjectsInactive.Include);
            if (houseAudio != null) Set(houseAudio, "_player", s.Player);
            foreach (var door in Object.FindObjectsByType<ProximityDoor>(FindObjectsInactive.Include)) Set(door, "_player", s.Player.transform);

            // ------------------------------------------------------------ sequences
            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            Tutorial(sequences, "tutorial_house",
                "<color=#E8C27A>Đi lại:</color> WASD · <color=#E8C27A>chạy:</color> Shift · <color=#E8C27A>xoay camera:</color> chuột\n" +
                "<color=#E8C27A>E</color>: tương tác với thứ có dấu ◆ vàng.   <color=#E8C27A>Giữ chuột phải</color>: quan sát kỹ một nơi ở xa.\n" +
                "<color=#E8C27A>J</color>: sổ tay manh mối.\n\n" +
                "Trước khi khách đến, hãy tìm hiểu tình cảnh gia đình: ngồi với Mẹ Kiều và xem quanh nhà, quanh sân.",
                "WASD: đi · Shift: chạy · Chuột phải: quan sát · E: tương tác · J: sổ tay");
            Tutorial(sequences, "tutorial_suspicion",
                "<color=#E8C27A>Tìm hiểu Mã Giám Sinh</color> trước khi ký khế — nhưng đừng để hắn sinh nghi.\n\n" +
                "<color=#E8C27A>Giữ chuột phải</color>: quan sát hắn.   <color=#E8C27A>E</color>: đáp lời, xem túi bạc, ra hiên hỏi chuyện.\n" +
                "Thanh <color=#F08080>Nghi ngờ</color> tăng khi bạn nhìn chằm chằm lúc hắn đang nhìn lại, hoặc khi hỏi quá thẳng. Đầy thanh là hắn bỏ đi.\n\n" +
                "Có câu trả lời phải chọn trong <color=#E8C27A>vài giây</color>: hết giờ thì Kiều im lặng.",
                "WASD: đi · Chuột phải: quan sát · E: tương tác · J: sổ tay");
            var document = Child<DocumentSequence>(sequences, "document_khe");
            Set(document, "_id", "document_khe");
            Set(document, "_view", s.Hud.GetComponent<DocumentView>());
            Set(document, "_title", "Tờ hôn ước");
            Set(document, "_body", "Nay họ Vương có con gái là <b>Vương Thúy Kiều</b>, bằng lòng về làm vợ <b>Mã Giám Sinh</b>, người huyện Lâm Thanh.\n\n" +
                "Sính lễ: <b>bốn trăm lạng bạc</b>, giao khi việc thành.\n\n" +
                "<size=82%>Từ nay người nhận có toàn quyền định đoạt nơi ở và công việc của người giao. " +
                "Người giao không được đòi lại, không được kêu ca, không được bỏ đi.</size>\n\n" +
                "Làm bằng, có điểm chỉ.\n— Mã");
            Set(document, "_sealed", true);
            Chapter03Setup.Fx(sequences, "kieu_voice", "spirit", "memory", "none", 1.6f, 0f, 1);
        }

        // ------------------------------------------------------------------ props

        /// <summary>The bag of silver Mã Giám Sinh sets on the table: a cloth pouch and new ingots, each with a red Phúc Ký stamp.</summary>
        private static GameObject Silver(Transform root)
        {
            var silver = Group(root, "SilverOnTable");
            var cloth = Material("Ch1Pouch", new Color(0.62f, 0.42f, 0.24f), 0.1f);
            var metal = Material("Ch1Silver", new Color(0.8f, 0.8f, 0.82f), 0.8f);
            var stamp = Material("Ch1Stamp", new Color(0.6f, 0.06f, 0.04f), 0.2f);
            StoryArtKit.Sphere("Pouch", new Vector3(0.62f, 0.86f, 2.78f), new Vector3(0.2f, 0.15f, 0.17f), cloth, silver);
            for (int i = 0; i < 4; i++)
            {
                var ingot = new Vector3(0.24f + (i % 2) * 0.13f, 0.8f, 2.56f + (i / 2) * 0.09f);
                Block("Ingot", ingot, new Vector3(0.1f, 0.035f, 0.055f), metal, silver, false, 12f * i);
                Block("Stamp", ingot + Vector3.up * 0.019f, new Vector3(0.02f, 0.003f, 0.02f), stamp, silver, false, 12f * i);
            }
            return silver.gameObject;
        }

        /// <summary>What the officials left in the courtyard: an overturned chest, silk trampled in the mud and the ledger by the well.</summary>
        private static void SearchedCourtyard(Transform root)
        {
            var mess = Group(root, "SearchedCourtyard");
            var chest = Material("Ch1Chest", new Color(0.3f, 0.16f, 0.08f), 0.3f);
            Block("ChestOverturned", new Vector3(4.2f, 0.28f, -6.5f), new Vector3(0.9f, 0.55f, 0.6f), chest, mess, false, 25f);
            Block("ChestLid", new Vector3(3.6f, 0.04f, -6.0f), new Vector3(0.92f, 0.06f, 0.62f), chest, mess, false, -30f);
            Block("Ledger", new Vector3(5.2f, 0.03f, -5.55f), new Vector3(0.3f, 0.05f, 0.22f), Material("Ch1Ledger", new Color(0.22f, 0.12f, 0.07f), 0.2f), mess, false, 18f);
            Block("LedgerPages", new Vector3(5.2f, 0.06f, -5.55f), new Vector3(0.27f, 0.012f, 0.2f), Material("Ch1LedgerPaper", new Color(0.7f, 0.64f, 0.5f), 0.4f), mess, false, 18f);
            Block("SilkInMud", new Vector3(4.4f, 0.015f, -5.2f), new Vector3(1.2f, 0.01f, 0.7f), Material("Ch1SilkMud", new Color(0.4f, 0.1f, 0.1f), 0.6f), mess, false, 10f);
            var colours = new[] { new Color(0.55f, 0.08f, 0.08f), new Color(0.15f, 0.2f, 0.4f), new Color(0.85f, 0.78f, 0.6f), new Color(0.3f, 0.45f, 0.3f) };
            var spots = new[] { new Vector3(3.4f, 0f, -5.4f), new Vector3(4.9f, 0f, -6.9f), new Vector3(5.6f, 0f, -6.3f), new Vector3(3.9f, 0f, -7.4f) };
            for (int i = 0; i < spots.Length; i++)
            {
                var bolt = Cylinder("SilkBolt", spots[i], 0.09f, 0.9f, Material("Ch1Silk" + i, colours[i], 0.55f), mess, false);
                bolt.transform.SetPositionAndRotation(spots[i] + Vector3.up * 0.09f, Quaternion.Euler(0f, 35f + i * 50f, 90f));
            }
        }

        /// <summary>Groups the contract sheet, its ink lines and seal so the story can lay it on the table when it is brought out.</summary>
        private static GameObject ContractOnTable()
        {
            var parts = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(t => t.name == "Tờ khế" || t.name.StartsWith("ContractInkLine") || t.name == "ContractSeal").ToList();
            if (parts.Count == 0) throw new InvalidOperationException("Chapter 1 contract sheet (\"Tờ khế\") not found.");
            Transform group = parts[0].parent;
            if (group.name != "ContractOnTable")
            {
                var hall = group;
                group = new GameObject("ContractOnTable").transform;
                group.SetParent(hall, false);
            }
            foreach (var part in parts) if (part.parent != group) part.SetParent(group, true);
            group.gameObject.SetActive(true);
            return group.gameObject;
        }

        // ------------------------------------------------------------------ helpers

        private static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Shot(Transform parent, string id, Vector3 position, Vector3 look)
        {
            var anchor = Point("CAM_" + id, position, 0f, parent);
            anchor.rotation = Quaternion.LookRotation(look - position);
        }

        private static void Target(Transform parent, StoryDirector director, string name, Vector3 position, string id, string label, Vector3 size,
            Vector3? stand, float standYaw)
        {
            var anchor = Point(name, position, 0f, parent);
            var target = Explore(anchor.gameObject, director, id, label, size);
            if (stand.HasValue) Set(target, "_standPoint", Point(name + "Stand", stand.Value, standYaw, parent));
        }

        private static void Observable(GameObject owner, StoryDirector director, string id, string label, Vector3 focus, float maxDistance)
        {
            var observable = owner.AddComponent<ObservableTarget>();
            Set(observable, "_director", director);
            Set(observable, "_exploreId", id);
            Set(observable, "_label", label);
            Set(observable, "_focusPoint", Point("ObserveFocus", focus, 0f, owner.transform));
            Set(observable, "_maxDistance", maxDistance);
        }

        private static void Tutorial(Transform parent, string id, string body, string controls)
        {
            var tutorial = Child<TutorialSequence>(parent, id);
            Set(tutorial, "_id", id);
            Set(tutorial, "_body", body);
            Set(tutorial, "_controlsHint", controls);
        }

        // ------------------------------------------------------------------ references from the house into replaced objects

        /// <summary>
        /// Object references held by components that stay (doors, audio, camera zones…) into the objects about to be
        /// replaced, recorded by root name, child path and component type so they can point at the new objects.
        /// </summary>
        private static List<Link> CollectLinks(GameObject[] roots)
        {
            var links = new List<Link>();
            foreach (var kept in roots.Where(r => !Replaced.Contains(r.name) && !PrefabUtility.IsPrefabAssetMissing(r)))
                foreach (var owner in kept.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (owner == null) continue;
                    var property = new SerializedObject(owner).GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue == null) continue;
                        var target = property.objectReferenceValue;
                        var transform = target is GameObject go ? go.transform : target is Component c ? c.transform : null;
                        if (transform == null || !Replaced.Contains(transform.root.name)) continue;
                        links.Add(new Link
                        {
                            Owner = owner, Property = property.propertyPath, Root = transform.root.name,
                            Path = AnimationUtility.CalculateTransformPath(transform, transform.root), Type = target is GameObject ? null : target.GetType().FullName
                        });
                    }
                }
            return links;
        }

        private static void RestoreLinks(UnityEngine.SceneManagement.Scene scene, List<Link> links)
        {
            foreach (var link in links)
            {
                if (link.Owner == null) continue;
                var root = scene.GetRootGameObjects().FirstOrDefault(r => r.name == link.Root);
                var transform = root == null ? null : link.Path.Length == 0 ? root.transform : root.transform.Find(link.Path);
                Object value = transform == null ? null : link.Type == null ? transform.gameObject
                    : (Object)transform.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().FullName == link.Type);
                var serialized = new SerializedObject(link.Owner);
                var property = serialized.FindProperty(link.Property);
                if (property == null) continue;
                property.objectReferenceValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (value == null) Debug.LogWarning("[Chapter01StoryUpgrade] " + link.Owner.name + "." + link.Property + " lost its link to " + link.Root + "/" + link.Path, link.Owner);
            }
        }
    }
}

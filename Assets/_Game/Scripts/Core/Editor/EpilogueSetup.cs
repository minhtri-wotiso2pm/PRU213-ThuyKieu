using ThuyKieu.Gameplay.Chapters;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.UI;
using UnityEditor;
using UnityEngine;
using static ThuyKieu.Core.Editor.StorySceneKit;
using static ThuyKieu.Core.Editor.StoryArtKit;

namespace ThuyKieu.Core.Editor
{
    /// <summary>Builds Chapter08_Epilogue: the prologue's classroom at sunset, fifteen minutes after the bell.</summary>
    public static class EpilogueSetup
    {
        public const string SceneName = "Chapter08_Epilogue";
        public const string ScenePath = "Assets/_Game/Scenes/" + SceneName + ".unity";
        private const string Characters = "Assets/_Game/Art/Characters/";

        [MenuItem("ThuyKieu/Epilogue/Create playable scene")]
        public static void Create()
        {
            var scene = NewScene(ScenePath);
            var s = CreateSystems(ChapterCatalog.Epilogue, "Trở lại lớp học", "epilogue", ChapterCatalog.MainMenuScene, new Vector3(2.9f, 0.05f, -1.15f), 0f, "classroom_evening");
            Grade(s.Fx, 0.12f, 9f, 4f, 22f, new Color(0.42f, 0.36f, 0.5f), new Color(0.95f, 0.66f, 0.42f));
            UseSky(s.CameraComponent, Sky("EpilogueSky", "evening_road_01_puresky", 0.9f, new Color(0.7f, 0.6f, 0.55f), 120f));
            Set(s.Shots, "_keyLight", 0.7f);
            Set(s.Shots, "_keyColor", new Color(1f, 0.72f, 0.5f));
            var world = new GameObject("World").transform;
            var nav = NavMesh(world);
            var director = s.Director;

            var room = ClassroomKit.Build(world, Vector3.zero, true);
            var look = ClassroomKit.StudentBody(s);
            // Ngân still at the next seat when she wakes (static seated model from ThuyKieuKiemTien).
            ModernActor("Ngan", "Ngan", "NuSinhToxic", 1.3f, room.NganSeat.position, 0f, Color.white, seat: true);
            Set(Explore(Anchor(world, "WindowTarget", room.WindowAnchor.position), director, "ep_window", "Nhìn ra cửa sổ", new Vector3(1.2f, 1.6f, 1.8f)), "_standPoint", room.WindowStand);
            Set(Explore(Anchor(world, "BookTarget", room.Seat.position + new Vector3(0, 0, 0.1f)), director, "ep_book", "Mở lại quyển Truyện Kiều", new Vector3(1.2f, 1.6f, 1.2f)), "_standPoint", room.Seat);

            // Sunset: low warm light raking through the windows, the room tubes off.
            Sun("Sun", new Color(1f, 0.62f, 0.36f), 1.35f, new Vector3(11f, 255f, 0f));
            RenderSettings.ambientLight = new Color(0.56f, 0.46f, 0.44f);
            foreach (float z in new[] { -2f, 2f })
            {
                var fill = new GameObject("SunsetFill");
                fill.transform.SetParent(world, false);
                fill.transform.position = new Vector3(1.5f, 2.6f, z);
                var light = fill.AddComponent<Light>();
                light.type = LightType.Point; light.color = new Color(1f, 0.7f, 0.5f); light.range = 7f; light.intensity = 0.9f; light.shadows = LightShadows.None;
            }
            RenderSettings.fogColor = new Color(0.72f, 0.56f, 0.5f);
            RenderSettings.fogDensity = 0.006f;
            Dust("SunDust", new Vector3(2.5f, 1.6f, 0f), new Vector3(4f, 2.4f, 6f), world, new Color(1f, 0.82f, 0.6f, 0.35f), 120);

            var worldScript = s.Root.AddComponent<EpilogueWorld>();
            Set(worldScript, "_director", director);
            Set(worldScript, "_navMesh", nav);
            Set(worldScript, "_look", look);
            Set(worldScript, "_seat", room.Seat);
            Set(worldScript, "_thankYou", room.ThankYou.gameObject);

            var sequences = new GameObject("Sequences").transform;
            sequences.SetParent(s.Root.transform, false);
            Chapter03Setup.Fx(sequences, "wake_class", "memory", "memory", "epilogue", 2.2f, 0f, 1);
            var tutorial = Child<TutorialSequence>(sequences, "tutorial_epilogue");
            Set(tutorial, "_id", "tutorial_epilogue");
            Set(tutorial, "_body", "Mọi thứ vẫn như cũ: bảng đen, quạt trần, chiếc cặp trên bàn.\n\nQuyển <color=#E8C27A>Truyện Kiều</color> vẫn mở ở đoạn bán mình. Đến bàn và bấm <color=#E8C27A>E</color> khi bạn sẵn sàng.");
            Set(tutorial, "_controlsHint", "WASD: đi · E: tương tác · Space: tiếp lời thoại");
            var book = Child<DocumentSequence>(sequences, "open_book");
            Set(book, "_id", "open_book");
            Set(book, "_view", s.Hud.GetComponent<DocumentView>());
            Set(book, "_title", "TRUYỆN KIỀU — NGUYỄN DU");
            Set(book, "_body", "<i>Duyên hội ngộ, đức cù lao,\nBên tình bên hiếu bên nào nặng hơn?\nĐể lời thệ hải minh sơn,\nLàm con trước phải đền ơn sinh thành.\nQuyết tình nàng mới hạ tình:\n\"Dẽ cho để thiếp bán mình chuộc cha!\"</i>");
            Set(book, "_sealed", false);
            var reveal = Child<RevealSequence>(sequences, "thank_you");
            Set(reveal, "_id", "thank_you");
            Set(reveal, "_text", room.ThankYou);
            var ending = Child<EndingSequence>(sequences, "ending");
            Set(ending, "_id", "ending");
            Set(ending, "_recap", s.Hud.GetComponent<RecapView>());

            Save(scene, ScenePath);
        }


        private static GameObject Anchor(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }
    }
}

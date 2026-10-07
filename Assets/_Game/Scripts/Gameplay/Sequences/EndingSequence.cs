using System.Collections;
using System.Text;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// The very end: the game's title on black, a recap of the player's journey (the ending's tone and the choices that
    /// shaped it, prologue to Chapter 7), then the credits and the main menu.
    /// </summary>
    public class EndingSequence : StorySequence
    {
        [SerializeField] private RecapView _recap;

        public override IEnumerator Run(StoryDirector director)
        {
            if (director.Audio != null) director.Audio.PlayMusic("epilogue");
            director.Hud.SetFadeColor(Color.black);
            yield return director.Hud.Fade(1f, director.Autopilot ? 0f : 1.6f);
            yield return director.Hud.Card("HẾT", "THÚY KIỀU: NGHỊCH MỆNH", false, 4f);
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            _recap.FastMode = director.Autopilot;
            var v = director;
            yield return _recap.Show(Tone(v.GetInt("c7_ket")), Left(v), Right(v));
            StoryAutopilot.Note("ENDING tone " + v.GetInt("c7_ket") + " · trial " + v.GetInt("c7_diem") + " · dossier " + v.GetInt("c6_ho_so"));
            // The story is over: no chapter-complete card, the progress is marked finished (chapter select unlocks).
            director.EndStory();
            if (director.Autopilot) yield break;
            bool closed = false;
            GameShell.ShowCredits(() => closed = true);
            while (!closed) yield return null;
            GameShell.GoToMainMenu();
        }

        private static string Tone(int ending)
        {
            switch (ending)
            {
                case 3: return "<color=#E8C27A>Kết trọn vẹn</color> — công lý đến với Kiều, và cả với những người con gái trước nàng.";
                case 2: return "<color=#E8C27A>Kết ấm áp</color> — Kiều tự do, kẻ buôn người phải đền tội.";
                default: return "<color=#E8C27A>Kết chật vật</color> — thắng nhờ những người dám đứng ra vào phút chót.";
            }
        }

        private static string Left(StoryDirector v)
        {
            var b = new StringBuilder();
            Header(b, "MỞ ĐẦU");
            Item(b, "Bạn từng nghĩ về Kiều: " + Pick(v.GetInt("pl_che_kieu"), "\"Dễ mà, mình sẽ làm tốt hơn.\"", "\"Phải có cách khác.\"", "\"Kiều cam chịu quá.\""));
            Item(b, "Khi quan sai đến bắt cha: " + Pick(v.GetInt("pl_hanh_dong"), "lao ra chắn trước mặt cha", "hỏi tội danh, đòi xem đơn", "đứng yên, ghi nhớ từng gương mặt"));
            Header(b, "CHƯƠNG 1 · GIA BIẾN");
            Item(b, v.GetBool("doc_ky_khe") ? "Đọc kỹ từng chữ trên tờ khế trước khi ký." : "Ký tờ khế mà không đọc kỹ.");
            Header(b, "CHƯƠNG 2 · TRÊN ĐƯỜNG");
            Item(b, "Manh mối thu được: " + v.GetInt("manh_moi") + (v.GetBool("giu_mau_giay") ? " · giữ lại mẩu giấy \"giao hàng\"" : string.Empty));
            Header(b, "CHƯƠNG 3 · TÚ BÀ");
            Item(b, "Trả lời Kiều thật: " + Pick(v.GetInt("tra_loi_kieu_that"), "nhận mình đã sai khi phán xét nàng", "\"ta sẽ không bỏ cuộc\"", "im lặng"));
            Header(b, "CHƯƠNG 4 · KHÔNG CÒN ĐƯỜNG LUI");
            Item(b, "Văn tự bán thân: " + Pick(v.GetInt("c4_van_tu") + 1, "để lại trong tủ", "mang theo làm bằng", "đốt đi"));
            Item(b, "Bạc của Tú Bà: " + Pick(v.GetInt("c4_tien") + 1, "không lấy một đồng", "chỉ lấy ít lộ phí", "mang cả túi bạc có dấu"));
            if (v.GetBool("c4_ru_co_lam")) Item(b, "Rủ chị Lam cùng trốn.");
            return b.ToString();
        }

        private static string Right(StoryDirector v)
        {
            var b = new StringBuilder();
            Header(b, "CHƯƠNG 5 · ĐÀO THOÁT");
            if (v.GetBool("c4_ru_co_lam")) Item(b, v.GetBool("c5_co_lam_thoat") ? "Kéo được chị Lam ra khỏi rừng tre." : "Chị Lam không thoát được cùng bạn.");
            Item(b, "Rượt đuổi trong rừng: " + v.GetInt("c5_qte") + "/3 lần thoát hiểm gọn gàng.");
            Header(b, "CHƯƠNG 6 · KIM TRỌNG");
            int trust = v.GetInt("kt_tin_tuong");
            Item(b, "Kim Trọng tin bạn " + (trust >= 5 ? "trọn vẹn" : trust >= 3 ? "nhiều" : "còn dè dặt") + (v.GetBool("kt_biet_xuyen_khong") ? " · chàng biết bạn đến từ nơi rất xa." : "."));
            Item(b, "Hồ sơ: " + v.GetInt("c6_ho_so") + "/4 loại bằng chứng · " + v.GetInt("ev_nhan_chung") + " nhân chứng.");
            Header(b, "CHƯƠNG 7 · NGHỊCH MỆNH");
            Item(b, "Mâu thuẫn vạch ra trước công đường: " + v.GetInt("c7_diem") + " điểm.");
            Item(b, "Lòng tin của quan huyện lúc kết thúc: " + v.GetInt("c7_long_tin") + "%" + (v.GetInt("c7_that_bai") > 0 ? " · phải làm lại " + v.GetInt("c7_that_bai") + " lần." : "."));
            if (v.GetInt("c7_lui") > 0) Item(b, "Một nhân chứng đã lùi bước trước lời dọa của Tú Bà.");
            else if (v.GetBool("c7_nhan_chung_vung")) Item(b, "Không nhân chứng nào lùi bước.");
            return b.ToString();
        }

        private static string Pick(int index, params string[] options) => index >= 1 && index <= options.Length ? options[index - 1] : "—";
        private static void Header(StringBuilder b, string text)
        {
            if (b.Length > 0) b.Append('\n');
            b.Append("<size=19><color=#E8C27A>").Append(text).Append("</color></size>\n");
        }
        private static void Item(StringBuilder b, string text) => b.Append("  ").Append(text).Append('\n');
    }
}

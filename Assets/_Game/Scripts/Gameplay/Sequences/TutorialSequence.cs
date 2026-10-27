using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Contextual how-to-play card shown the first time a mechanic appears.</summary>
    public class TutorialSequence : StorySequence
    {
        [SerializeField, TextArea(4, 10)] private string _body;
        [SerializeField] private string _controlsHint;

        /// <summary>Lets a chapter refresh legacy serialized tutorial copy after its mechanic changes.</summary>
        public void Configure(string body, string controlsHint)
        {
            _body = body;
            _controlsHint = controlsHint;
        }

        public override IEnumerator Run(StoryDirector director)
        {
            director.Hud.FastMode = director.Autopilot;
            if (!string.IsNullOrEmpty(_controlsHint)) director.Hud.SetControls(_controlsHint);
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            yield return director.Hud.TutorialCard(ConciseBody(director.ChapterNumber));
        }

        // Existing scenes store older tutorial copy; keep the current player-facing wording here.
        private string ConciseBody(int chapter)
        {
            switch (Id)
            {
                case "tutorial_classroom":
                    return "Di chuyển: WASD · Camera: Chuột\nTương tác: E\nTiếp thoại: Space / Click · Chọn: 1–4\nCất Truyện Kiều vào cặp.";
                case "tutorial_room":
                    return "Tương tác: E\nQuan sát: Giữ chuột phải\nSổ tay: J\nSoi gương trước.";
                case "tutorial_house":
                    return chapter == 1
                        ? "Di chuyển: WASD · Chạy: Shift\nTương tác: E · Quan sát: Giữ chuột phải\nSổ tay: J\nKhám phá nhà trước khi mở cổng."
                        : "Tương tác: E · Khom người: Ctrl / C\nKhu cấm: Phòng sổ sách, cửa sổ Tú Bà\nTránh vùng nhìn đỏ; nấp sau vật cản.";
                case "tutorial_suspicion":
                    return "Quan sát túi bạc: Giữ chuột phải\nBuông khi Mã Giám Sinh nhìn lại.\nĐối chất ngay: E";
                case "tutorial_road":
                    return "Đi theo đoàn: WASD\nTương tác: E · Khom người: Ctrl / C\nQuan sát: Giữ chuột phải\nĐừng tụt xa; ngừng nhìn khi hắn quay lại.";
                case "tutorial_lau":
                    return "Nhìn mục tiêu, tương tác: E\nMở cửa khẽ: Giữ E · Khom người: Ctrl\nChạy: Shift (gây tiếng động)\nDãy nhà sau là khu cấm; viền đỏ = bị để ý.";
                case "tutorial_night":
                    return "Khom người: Ctrl / C · Nấp: E\nNém sỏi: Q\nTránh ánh đèn, đi trong bóng tối.\nAn toàn: Phòng mình, hiên phía tây.";
                case "tutorial_dem":
                    return "Khom người: Ctrl · Mở cửa khẽ: Giữ E\nNém sỏi: Q (nhặt thêm cạnh cửa phòng)\nTránh ánh đèn; cắt tầm nhìn trước khi nấp.\nNgáy ngừng = Tú Bà sắp tỉnh.\nAn toàn: Phòng mình.";
                case "tutorial_escape":
                    return "Khom gần chó: Ctrl / C · Nấp: E\nĐẩy cửa: Giữ E\nThả E trước khi thanh tiếng bản lề đầy.\nĐi trong bóng tối, tránh đèn tuần tra.";
                case "tutorial_street":
                    return "Nói chuyện / Nấp: E · Sổ tay: J\nĐưa đúng bằng chứng; sai 2 lần sẽ mất lời khai.\nĐừng chạy hoặc áp sát người của Tú Bà.\nXong 2 nơi: Về thư phòng lập hồ sơ.";
                case "tutorial_gate":
                    return "Tương tác: E · Quan sát: Giữ chuột phải\nĐộng viên nhân chứng, bàn kế với Kim Trọng.\nĐánh trống: Bắt đầu phiên tòa, không quay lại.";
                case "tutorial_trial":
                    return "Chọn lời khai: 1–4 / Click\nHỏi vặn: Q · Đưa bằng chứng: E\nTrang Tiên tri: T\nBằng chứng sai làm giảm lòng tin của quan.";
                case "tutorial_epilogue":
                    return "Di chuyển: WASD\nĐọc Truyện Kiều trên bàn: E";
                default:
                    return _body;
            }
        }
    }
}

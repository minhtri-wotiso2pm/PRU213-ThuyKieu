using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 6, Kim Trọng's desk: the papers Kiều carried out are laid side by side and the player points at the
    /// contradiction — one line on each paper — instead of picking a written answer (which would give it away).
    /// Which papers appear depends on what she kept (văn tự, ledger page, the Chapter 2 note).
    /// Writes the number of contradictions found to "c6_doi_chieu".
    /// </summary>
    public class ComparisonSequence : StorySequence
    {
        [SerializeField] private DossierView _view;

        private const string Betrothal = "TỜ KHẾ HÔN ƯỚC";
        private static readonly string[] BetrothalLines =
        {
            "Lập tháng Tư, tại nhà họ Vương.",
            "Nhà trai: <b>Mã Giám Sinh</b>, người huyện Lâm Thanh,",
            "xin cưới <b>Vương Thúy Kiều</b> làm vợ.",
            "Sính lễ: <b>bốn trăm lạng bạc</b>.",
            "Từ nay nhà trai toàn quyền định đoạt nơi ở của người vợ."
        };
        private const string Deal = "GIAO KÈO MÃ – TÚ BÀ";
        private static readonly string[] DealLines =
        {
            "Lập ngày mùng chín <b>tháng Giêng</b>.",
            "Mã Giám Sinh lo việc \"hỏi vợ\" ở các huyện.",
            "Mỗi người giao đến, Tú Bà trả <b>bốn trăm năm mươi lạng</b>.",
            "<s>Lý thị</s> · <s>Trần thị</s> · <s>Lưu thị</s>",
            "Hai bên điểm chỉ làm bằng."
        };
        private const string Sale = "VĂN TỰ BÁN THÂN";
        private static readonly string[] SaleLines =
        {
            "Người bán: <b>Mã Giám Sinh</b>.",
            "Người mua: <b>Tú Bà</b>, lầu xanh Lâm Tri.",
            "Người bị bán: <b>Vương Thúy Kiều</b>.",
            "Giá: <b>bốn trăm năm mươi lạng</b>.",
            "Người mua toàn quyền định đoạt nơi ở và công việc của người bị bán."
        };

        public override IEnumerator Run(StoryDirector director)
        {
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            if (director.Audio != null) director.Audio.PlayMusic("tension_low");
            bool keptSale = director.GetInt("c4_van_tu") == 1;
            var items = new List<DossierView.Comparison>
            {
                new DossierView.Comparison
                {
                    LeftTitle = Betrothal, LeftLines = BetrothalLines,
                    RightTitle = keptSale ? Sale : Deal, RightLines = keptSale ? SaleLines : DealLines,
                    Prompt = "Mã Giám Sinh thật ra là ai? Chỉ ra dòng nói về hắn trên mỗi tờ.",
                    LeftAnswer = 1, RightAnswer = keptSale ? 0 : 1,
                    Insight = "Cùng một cái tên: trên giấy cưới là \"nhà trai\", trên giấy kia là kẻ đem bán. Cưới để bán.",
                    Hint = "Tìm dòng có tên Mã Giám Sinh ở mỗi tờ: \"nhà trai\" và \"" + (keptSale ? "người bán" : "lo việc hỏi vợ") + "\"."
                },
                new DossierView.Comparison
                {
                    LeftTitle = Betrothal, LeftLines = BetrothalLines, RightTitle = Deal, RightLines = DealLines,
                    Prompt = "Đám cưới đã được tính sẵn từ bao giờ? Chỉ ra hai dòng chứng minh.",
                    LeftAnswer = 0, RightAnswer = 0,
                    Insight = "Giao kèo lập tháng Giêng, hắn hỏi cưới tháng Tư. Mọi thứ đã sắp đặt từ trước.",
                    Hint = "So ngày lập của hai tờ: \"tháng Tư\" và \"tháng Giêng\"."
                }
            };
            if (director.GetBool("ev_giay_no"))
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = "TRANG SỔ NỢ",
                    LeftLines = new[] { "<b>Vương Thúy Kiều</b>", "Nợ tiền mua: <b>bốn trăm năm mươi lạng</b>.", "Áo lụa: ba mươi lạng.", "Son phấn: mười lạng.", "(Chưa trả.)" },
                    RightTitle = Deal, RightLines = DealLines,
                    Prompt = "Món nợ lớn nhất trong sổ từ đâu mà ra? Chỉ ra hai dòng khớp nhau.",
                    LeftAnswer = 1, RightAnswer = 2,
                    Insight = "Kiều phải trả lại giá người ta mua mình. Món nợ ấy không bao giờ trả hết — đó là sợi xích.",
                    Hint = "Con số \"tiền mua\" trong sổ trùng khớp từng đồng với giá trên giao kèo."
                });
            else if (director.GetBool("giu_mau_giay"))
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = "MẨU GIẤY TRÊN ĐƯỜNG",
                    LeftLines = new[] { "\"<b>Bà T.</b> ở Lâm Tri nhận hàng —", "giá bốn trăm năm mươi lạng.\"", "(nhặt được trên đường theo Mã Giám Sinh)" },
                    RightTitle = Deal, RightLines = DealLines,
                    Prompt = "\"Hàng\" mà Bà T. nhận là gì? Chỉ ra hai dòng khớp nhau.",
                    LeftAnswer = 1, RightAnswer = 2,
                    Insight = "Cùng giá, cùng nơi, cùng người nhận. Kiều được gọi là \"hàng\" ngay từ trên đường đi.",
                    Hint = "Giá trong mẩu giấy và giá trên giao kèo giống hệt nhau."
                });
            else
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = Deal, LeftLines = DealLines, RightTitle = Betrothal, RightLines = BetrothalLines,
                    Prompt = "Kiều có phải người đầu tiên không? Chỉ ra dòng trên giao kèo, và dòng trên khế ước nối tiếp nó.",
                    LeftAnswer = 3, RightAnswer = 2,
                    Insight = "Ba cái tên bị gạch là ba lần \"hỏi vợ\" trước Kiều. Không phải một lần lừa — là một nghề.",
                    Hint = "Giao kèo ghi \"mỗi người giao đến\"; những cái tên gạch đi là những người đã giao. Kiều là người kế tiếp."
                });

            int score = 0;
            _view.FastMode = director.Autopilot;
            yield return _view.Compare("ĐỐI CHIẾU GIẤY TỜ", items, s => score = s);
            director.SetInt("c6_doi_chieu", score);
            if (director.Audio != null) director.Audio.PlaySfx(score >= 2 ? "clue" : "suspicion_up");
        }
    }
}

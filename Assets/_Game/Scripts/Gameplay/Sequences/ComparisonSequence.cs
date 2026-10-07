using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 6, Kim Trọng's desk: the papers Kiều carried out are laid side by side and the player points out where
    /// they contradict each other. Which papers appear depends on what she kept (văn tự, ledger page, the Chapter 2 note).
    /// Writes the number of contradictions found to "c6_doi_chieu".
    /// </summary>
    public class ComparisonSequence : StorySequence
    {
        [SerializeField] private DossierView _view;

        private const string Betrothal = "TỜ KHẾ HÔN ƯỚC";
        private const string BetrothalBody = "Lập tháng Tư, tại nhà họ Vương.\n\nNhà trai: <b>Mã Giám Sinh</b>, người huyện Lâm Thanh,\nxin cưới <b>Vương Thúy Kiều</b> làm vợ.\n\nSính lễ: <b>bốn trăm lạng bạc</b>.\nTừ nay nhà trai toàn quyền định đoạt\nnơi ở của người vợ.";
        private const string Deal = "GIAO KÈO MÃ – TÚ BÀ";
        private const string DealBody = "Lập ngày mùng chín <b>tháng Giêng</b>.\n\nMã Giám Sinh lo việc \"hỏi vợ\" ở các huyện.\nMỗi người giao đến, Tú Bà trả\n<b>bốn trăm năm mươi lạng</b>.\n\n<s>Lý thị</s> · <s>Trần thị</s> · <s>Lưu thị</s>";
        private const string Sale = "VĂN TỰ BÁN THÂN";
        private const string SaleBody = "Người bán: <b>Mã Giám Sinh</b>.\nNgười mua: <b>Tú Bà</b>, lầu xanh Lâm Tri.\nNgười bị bán: <b>Vương Thúy Kiều</b>.\n\nGiá: <b>bốn trăm năm mươi lạng</b>.\nNgười mua toàn quyền định đoạt\nnơi ở và công việc của người bị bán.";

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
                    LeftTitle = Betrothal, LeftBody = BetrothalBody,
                    RightTitle = keptSale ? Sale : Deal, RightBody = keptSale ? SaleBody : DealBody,
                    Prompt = "Trên hai tờ giấy này, Mã Giám Sinh là ai?",
                    Options = new[] { "Bên trái là chồng cưới vợ — bên phải là kẻ bán người", "Ở cả hai tờ hắn chỉ là người làm chứng", "Ở cả hai tờ hắn là người mua" },
                    Correct = 0,
                    Insight = "Cùng một cái tên: trên giấy cưới là \"chồng\", trên giấy kia là kẻ đem bán. Cưới để bán.",
                    Hint = "Đọc kỹ dòng có tên Mã Giám Sinh ở mỗi tờ: \"nhà trai\" và \"người bán\" / \"giao người\"."
                },
                new DossierView.Comparison
                {
                    LeftTitle = Betrothal, LeftBody = BetrothalBody, RightTitle = Deal, RightBody = DealBody,
                    Prompt = "Ngày lập của hai tờ giấy cho thấy điều gì?",
                    Options = new[] { "Hắn bàn chuyện bán người với Tú Bà từ trước khi đến hỏi cưới", "Hắn đổi ý bán Kiều sau khi cưới xong", "Tú Bà ép hắn bán sau đám cưới" },
                    Correct = 0,
                    Insight = "Giao kèo lập tháng Giêng, hắn hỏi cưới tháng Tư. Mọi thứ đã sắp đặt từ trước.",
                    Hint = "So \"tháng Giêng\" trên giao kèo với \"tháng Tư\" trên tờ khế hôn ước."
                }
            };
            if (director.GetBool("ev_giay_no"))
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = "TRANG SỔ NỢ", LeftBody = "<b>Vương Thúy Kiều</b>\n\nNợ tiền mua: <b>bốn trăm năm mươi lạng</b>.\nÁo lụa: ba mươi lạng.\nSon phấn: mười lạng.\n\n(Chưa trả.)",
                    RightTitle = Deal, RightBody = DealBody,
                    Prompt = "Món nợ bốn trăm năm mươi lạng trong sổ thực chất là gì?",
                    Options = new[] { "Chính số bạc Tú Bà trả cho Mã — bị chuyển thành nợ của Kiều", "Tiền Kiều vay để chuộc cha", "Tiền áo lụa và son phấn cộng lại" },
                    Correct = 0,
                    Insight = "Kiều phải trả lại giá người ta mua mình. Món nợ ấy không bao giờ trả hết — đó là sợi xích.",
                    Hint = "Con số trong sổ nợ trùng khớp từng đồng với giá trên giao kèo."
                });
            else if (director.GetBool("giu_mau_giay"))
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = "MẨU GIẤY TRÊN ĐƯỜNG", LeftBody = "\"<b>Bà T.</b> ở Lâm Tri nhận hàng —\ngiá bốn trăm năm mươi lạng.\"\n\n(nhặt được trên đường theo Mã Giám Sinh)",
                    RightTitle = Deal, RightBody = DealBody,
                    Prompt = "\"Bà T.\" và \"hàng\" trong mẩu giấy là ai?",
                    Options = new[] { "Tú Bà — và \"hàng\" chính là Kiều", "Một bà buôn vải — hàng là lụa", "Mẹ của Mã Giám Sinh — hàng là sính lễ" },
                    Correct = 0,
                    Insight = "Cùng giá, cùng nơi, cùng người nhận. Kiều được gọi là \"hàng\" ngay từ trên đường đi.",
                    Hint = "Giá trong mẩu giấy và giá trên giao kèo giống hệt nhau."
                });
            else
                items.Add(new DossierView.Comparison
                {
                    LeftTitle = Deal, LeftBody = DealBody, RightTitle = Betrothal, RightBody = BetrothalBody,
                    Prompt = "Ba cái tên bị gạch trên giao kèo nghĩa là gì?",
                    Options = new[] { "Kiều là người thứ tư — đây là một mối buôn lặp đi lặp lại", "Ba người đã trả hết nợ và được về nhà", "Ba người từ chối, nên giao kèo bị hủy" },
                    Correct = 0,
                    Insight = "Ba lần \"hỏi vợ\" trước Kiều. Không phải một lần lừa — là một nghề.",
                    Hint = "Giao kèo ghi \"mỗi người giao đến\". Những cái tên gạch đi là những người đã giao."
                });

            for (int i = 0; i < items.Count; i++) items[i] = Shuffled(items[i]);
            int score = 0;
            _view.FastMode = director.Autopilot;
            yield return _view.Compare("ĐỐI CHIẾU GIẤY TỜ", items, s => score = s);
            director.SetInt("c6_doi_chieu", score);
            if (director.Audio != null) director.Audio.PlaySfx(score >= 2 ? "clue" : "suspicion_up");
        }

        /// <summary>The right answer is written first above; present the options in a random order.</summary>
        private static DossierView.Comparison Shuffled(DossierView.Comparison item)
        {
            var order = new List<int>();
            for (int i = 0; i < item.Options.Length; i++) order.Insert(Random.Range(0, order.Count + 1), i);
            var options = new string[order.Count];
            for (int i = 0; i < order.Count; i++) options[i] = item.Options[order[i]];
            item.Correct = order.IndexOf(item.Correct);
            item.Options = options;
            return item;
        }
    }
}

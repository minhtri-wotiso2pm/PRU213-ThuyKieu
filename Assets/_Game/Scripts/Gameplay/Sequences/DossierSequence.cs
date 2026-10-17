using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Chapter 6 finale: the dossier for the magistrate, its four kinds of evidence shown filled or missing.</summary>
    public class DossierSequence : StorySequence
    {
        [SerializeField] private DossierView _view;

        public override IEnumerator Run(StoryDirector director)
        {
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            int witnesses = director.GetInt("ev_nhan_chung");
            var names = new List<string>();
            if (director.GetBool("nc_ba_nuoc")) names.Add("bà hàng nước");
            if (director.GetBool("nc_thu_lai")) names.Add("thư lại huyện");
            if (director.GetBool("nc_co_lam")) names.Add("chị Lam");
            bool silver = director.GetInt("c4_tien") == 2, ledger = director.GetBool("c6_hieu_bac_xong") && director.GetBool("ev_tien") && !silver;
            var slots = new List<DossierView.Slot>
            {
                new DossierView.Slot { Title = "KHẾ ƯỚC", Filled = director.GetBool("ev_khe_uoc"),
                    Body = "Giao kèo Mã Giám Sinh – Tú Bà, lập từ tháng Giêng." + (director.GetInt("c4_van_tu") == 1 ? "\nKèm văn tự bán thân của Kiều." : string.Empty) },
                new DossierView.Slot { Title = "NHÂN CHỨNG", Filled = witnesses > 0,
                    Body = witnesses > 0 ? witnesses + " người: " + string.Join(", ", names) + "." : "Chưa ai chịu đứng ra làm chứng." },
                new DossierView.Slot { Title = "GIẤY NỢ", Filled = director.GetBool("ev_giay_no"),
                    Body = director.GetBool("ev_giay_no") ? "Trang sổ nợ: \"nợ bốn trăm năm mươi lạng tiền mua\"." : "Trang sổ nợ vẫn nằm trong ngăn bàn của Tú Bà." },
                new DossierView.Slot { Title = "TIỀN BẠC", Filled = director.GetBool("ev_tien"),
                    Body = silver ? "Túi bạc đóng dấu \"T.\" của lầu Lâm Tri." : ledger ? "Sổ hiệu bạc Phúc Ký: Tú Bà giao 450 lạng cho \"Mã công tử\"." : director.GetBool("ev_tien") ? "Bằng chứng tiền bạc." : "Không có dấu vết đồng bạc nào." }
            };
            int filled = director.GetInt("c6_ho_so");
            string verdict = filled >= 4 ? "HỒ SƠ ĐẦY ĐỦ." : filled == 3 ? "THIẾU MỘT MẢNH — VẪN ĐỦ ĐỂ QUAN PHẢI NGHE." : "HỒ SƠ CÒN MỎNG.";
            if (director.Audio != null) director.Audio.PlayMusic("study");
            _view.FastMode = director.Autopilot;
            yield return _view.Summary("HỒ SƠ GỬI HUYỆN ĐƯỜNG", slots, verdict);
            if (director.Audio != null) director.Audio.PlaySfx(filled >= 3 ? "clue" : "paper");
        }
    }
}

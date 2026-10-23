namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// The quick still-image montages that stand in for narration ("# montage:&lt;id&gt;" in Ink plays one before its line).
    /// A panel's shot is a camera anchor id (CAM_&lt;id&gt;) or CU_/MCU_&lt;speaker&gt;; painted art in
    /// Resources/StoryPanels/&lt;montage id&gt;_&lt;panel number&gt; replaces the rendered still.
    /// </summary>
    public static class MontageCatalog
    {
        public sealed class Panel
        {
            public string Shot, Caption;
            public float Seconds = 1.9f;
        }

        public sealed class Montage
        {
            public string Id;
            /// <summary>Black before the first still, so staging started by the line's scene tag has settled.</summary>
            public float Settle;
            public Panel[] Panels;
        }

        public static readonly Montage[] All =
        {
            new Montage
            {
                Id = "montage_pl_mua", Settle = 0.8f, Panels = new[]
                {
                    new Panel { Shot = "Gate", Caption = "Cổng khép lại. Cha và em bị giải lên huyện." },
                    new Panel { Shot = "Wide_Courtyard", Caption = "Chiều ấy, trời đổ mưa." }
                }
            },
            new Montage
            {
                Id = "montage_c1_mo", Settle = 0.3f, Panels = new[]
                {
                    new Panel { Shot = "WS_VuongGia", Caption = "Đêm mưa. Nhà họ Vương." },
                    new Panel { Shot = "Wide_Courtyard", Caption = "Dấu chân quan sai còn lẫn trong bùn." },
                    new Panel { Shot = "CU_MeKieu", Caption = "Cha và em bị giải đi, chỉ vì một lời khai không tên." }
                }
            },
            new Montage
            {
                Id = "montage_c1_sang", Settle = 1.3f, Panels = new[]
                {
                    new Panel { Shot = "Table_Contract", Caption = "Bạc được mang đi ngay trong đêm." },
                    new Panel { Shot = "WS_VuongGia", Caption = "Gần sáng: cha và em được tạm thả." }
                }
            },
            new Montage
            {
                Id = "montage_c2_mo", Settle = 0.3f, Panels = new[]
                {
                    new Panel { Shot = "Wide_Road", Caption = "Đi từ rạng sáng. Giờ nắng đã ngả chiều." },
                    new Panel { Shot = "CU_MaGiamSinh", Caption = "Không kiệu hoa, không kèn trống. Chỉ có hắn, tên tùy tùng và hòm bạc." }
                }
            },
            new Montage
            {
                Id = "montage_c2_lam_tri", Settle = 1.2f, Panels = new[]
                {
                    new Panel { Shot = "Wide_LamTri", Caption = "Nửa đêm. Cổng Lâm Tri hiện ra sau màn sương." },
                    new Panel { Shot = "CU_Kieu", Caption = "Phía xa, đèn lồng đỏ trên một ngôi lầu vẫn còn sáng." }
                }
            },
            new Montage
            {
                Id = "montage_c1_ra_di", Settle = 0.3f, Panels = new[]
                {
                    new Panel { Shot = "Hero_Kieu_Rain", Caption = "Sau lưng: chữ Hiếu." },
                    new Panel { Shot = "CU_MaGiamSinh", Caption = "Trước mặt: một chiếc lồng chưa đóng cửa." }
                }
            }
        };

        public static Montage Get(string id)
        {
            foreach (var montage in All) if (montage.Id == id) return montage;
            return null;
        }
    }
}

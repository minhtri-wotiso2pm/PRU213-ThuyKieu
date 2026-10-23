using System.Collections.Generic;

namespace ThuyKieu.Core.Story
{
    /// <summary>
    /// The Trang Sách cards: verses of Truyện Kiều the player collects and turns as foreknowledge. Each card is an Ink
    /// int variable of the same id (0 none · 1 torn, half a verse · 2 held · 3 used). Turning a card in the story is an
    /// Ink choice tagged #page:&lt;id&gt;; in the courtroom any held card can be turned as a "Tiên tri".
    /// </summary>
    public static class PageCatalog
    {
        public const int None = 0, Torn = 1, Held = 2, Used = 3;

        public sealed class Page
        {
            public string Id, Verse, Half, Hint;
            public int Cost;
            /// <summary>A warning, not a tool: it can never be turned.</summary>
            public bool Unusable;
        }

        public static readonly Page[] All =
        {
            new Page { Id = "ts_tai_menh", Verse = "Trăm năm trong cõi người ta,\nChữ tài chữ mệnh khéo là ghét nhau.", Cost = 5,
                Hint = "Ch2: giả như đã cam phận trước mặt Mã Giám Sinh · Ch7: Tiên tri" },
            new Page { Id = "ts_hieu", Verse = "Duyên hội ngộ, đức cù lao,\nBên tình bên hiếu bên nào nặng hơn?", Cost = 5,
                Hint = "Ch3: trả lời Kiều Thật · Ch6: khiến Kim Trọng tin ngay" },
            new Page { Id = "ts_ma_mat", Verse = "Quá niên trạc ngoại tứ tuần,\nMày râu nhẵn nhụi áo quần bảnh bao.", Half = "Quá niên trạc ngoại tứ tuần...", Cost = 10,
                Hint = "Ch2: tâng bốc cho Mã Giám Sinh tự khai · Ch7: Tiên tri" },
            new Page { Id = "ts_co_ke", Verse = "Cò kè bớt một thêm hai,\nGiờ lâu ngã giá vàng ngoài bốn trăm.", Cost = 10,
                Hint = "Ch2: ghép manh mối ngay · Ch7: Tiên tri" },
            new Page { Id = "ts_tu_ba", Verse = "Thoắt trông nàng đã biết tình,\nChim lồng khôn lẽ cất mình bay cao.", Cost = 10,
                Hint = "Ch4: qua buổi \"dạy quy củ\" mà mụ không nghi" },
            new Page { Id = "ts_hoa_troi", Verse = "Hoa trôi bèo dạt đã đành,\nBiết duyên mình biết phận mình thế thôi.", Cost = 0,
                Hint = "Ch4: thuyết phục chị Lam cùng trốn" },
            new Page { Id = "ts_so_khanh", Verse = "Một chàng vừa trạc thanh xuân,\nHình dung chải chuốt áo khăn dịu dàng.", Half = "Một chàng vừa trạc thanh xuân...", Cost = 5,
                Hint = "Ch5: vạch mặt kẻ hứa đưa nàng đi trốn" },
            new Page { Id = "ts_kim_trong", Verse = "Người quốc sắc, kẻ thiên tài,\nTình trong như đã, mặt ngoài còn e.", Cost = 5,
                Hint = "Ch6: Kim Trọng nhận ra nàng ngay" },
            new Page { Id = "ts_be_dau", Verse = "Trải qua một cuộc bể dâu,\nNhững điều trông thấy mà đau đớn lòng.", Cost = 10,
                Hint = "Ch7: giữ các nhân chứng đứng vững" },
            new Page { Id = "ts_chu_tam", Verse = "Thiện căn ở tại lòng ta,\nChữ tâm kia mới bằng ba chữ tài.", Cost = 0,
                Hint = "Ch7: lời kết lục bát" },
            new Page { Id = "ts_thanh_lau", Verse = "Thanh lâu hai lượt, thanh y hai lần.", Cost = 0, Unusable = true,
                Hint = "Số phận gốc đang kéo về. Không lật được." }
        };

        private static Dictionary<string, Page> _byId;

        public static bool TryGet(string id, out Page page)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, Page>();
                foreach (var p in All) _byId[p.Id] = p;
            }
            return _byId.TryGetValue(id, out page);
        }

        /// <summary>The state of a card in a story (0 when the story does not declare it).</summary>
        public static int State(Ink.Runtime.Story story, string id) => story != null && story.variablesState[id] is int value ? value : 0;
    }
}

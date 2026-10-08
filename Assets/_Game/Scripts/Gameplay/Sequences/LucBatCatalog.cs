using System.Linq;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// The four lục bát exchanges. The player completes the eight-word line: first the rhyme word in position six
    /// (it must rhyme with the sixth word of the six-word line and carry a level tone), then the tone of the whole line,
    /// which should suit the listener. Every offered line keeps the rules (positions 2-4-6-8: bằng-trắc-bằng-bằng,
    /// six and eight in different level tones).
    /// </summary>
    public static class LucBatCatalog
    {
        public sealed class Tone
        {
            public string Label, Line;
        }

        public sealed class Couplet
        {
            public string Id, Variable, Title, Listener, Luc;
            /// <summary>The eight-word line with "{0}" where the rhyme word goes (shown as a gap in the first step).</summary>
            public string BatPattern;
            public string[] Tiles;
            public int Correct;
            /// <summary>Why each tile is wrong (empty for the right one).</summary>
            public string[] Why;
            public Tone[] Tones;
            /// <summary>Index of the tone the listener wants, or -1 when any will do.</summary>
            public int Preferred = -1;
            public float TimeLimit;
        }

        public static readonly Couplet[] All =
        {
            new Couplet
            {
                Id = "lucbat_pl", Variable = "lb_pl", Title = "Giờ Văn · điền chữ còn thiếu", Listener = "ThayGiao",
                Luc = "Trăm năm trong cõi người ta", BatPattern = "Chữ tài chữ mệnh khéo {0} ghét nhau",
                Tiles = new[] { "là", "lại", "thì", "đã" }, Correct = 0,
                Why = new[] { "", "\"lại\" mang thanh trắc — chữ thứ sáu câu tám phải là thanh bằng.", "\"thì\" không vần với \"ta\".", "\"đã\" mang thanh trắc — chữ thứ sáu câu tám phải là thanh bằng." },
                Tones = new[] { new Tone { Label = "Nguyên tác", Line = "Chữ tài chữ mệnh khéo là ghét nhau" } }
            },
            new Couplet
            {
                Id = "lucbat_ch3", Variable = "lb_ch3", Title = "Đêm đầu ở lầu · nói với chị Lam", Listener = "CoLam",
                Luc = "Chị ơi đừng khóc một mình", BatPattern = "__  __  __  __  __  {0}  __  __",
                Tiles = new[] { "lệ", "tình", "thương", "khổ" }, Correct = 1,
                Why = new[] { "\"lệ\" mang thanh trắc.", "", "\"thương\" không vần với \"mình\".", "\"khổ\" mang thanh trắc." },
                Tones = new[]
                {
                    new Tone { Label = "Cứng rắn", Line = "Khóa thân, đâu khóa được tình chúng ta" },
                    new Tone { Label = "Tha thiết", Line = "Có em ở cạnh nghĩa tình chị em" },
                    new Tone { Label = "Khéo léo", Line = "Nói cười cho khéo giữ tình mai sau" }
                },
                Preferred = 1
            },
            new Couplet
            {
                Id = "lucbat_ch6", Variable = "lb_ch6", Title = "Thư phòng · đối thơ với Kim Trọng", Listener = "KimTrong",
                Luc = "Trăng xưa còn nhớ vườn đào", BatPattern = "__  __  __  __  __  {0}  __  __",
                Tiles = new[] { "gửi", "nhau", "trao", "tặng" }, Correct = 2,
                Why = new[] { "\"gửi\" mang thanh trắc.", "\"nhau\" không vần với \"đào\".", "", "\"tặng\" mang thanh trắc." },
                Tones = new[]
                {
                    new Tone { Label = "Tha thiết", Line = "Lòng em vẫn giữ lời trao ngày nào" },
                    new Tone { Label = "Khéo léo", Line = "Thoa vàng ai nhặt ai trao bên tường" },
                    new Tone { Label = "Cứng rắn", Line = "Việc đời chưa vẹn, lời trao đành chờ" }
                },
                Preferred = 1
            },
            new Couplet
            {
                Id = "lucbat_ch7", Variable = "lb_ch7", Title = "Công đường · lời kết trước khi quan tuyên án", Listener = "QuanHuyen",
                Luc = "Thân này đâu phải món hàng", BatPattern = "__  __  __  __  rõ {0}  __  __",
                Tiles = new[] { "rành", "thật", "ràng", "mạch" }, Correct = 2,
                Why = new[] { "\"rành\" vần -anh, không vần với \"hàng\".", "\"thật\" mang thanh trắc.", "", "\"mạch\" mang thanh trắc." },
                Tones = new[]
                {
                    new Tone { Label = "Cứng rắn", Line = "Giấy tờ đã chứng rõ ràng ai gian" },
                    new Tone { Label = "Khéo léo", Line = "Mong quan một chữ rõ ràng cho xong" },
                    new Tone { Label = "Tha thiết", Line = "Xin quan soi tỏ rõ ràng phận con" }
                },
                Preferred = 2, TimeLimit = 20f
            }
        };

        public static string[] Ids => All.Select(c => c.Id).ToArray();

        public static Couplet Get(string id) => All.FirstOrDefault(c => c.Id == id);
    }
}

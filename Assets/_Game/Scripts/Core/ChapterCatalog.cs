using System.Collections.Generic;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>Chapter list used by the main menu, chapter select, the dev console and the "-chapter N" flag.</summary>
    public static class ChapterCatalog
    {
        public const string MainMenuScene = "MainMenu";

        public sealed class Entry
        {
            public int Number;
            public string Title;
            public string Blurb;
            public string Scene;
            /// <summary>Not playable content yet (placeholder screen only).</summary>
            public bool Placeholder;
        }

        public static readonly IReadOnlyList<Entry> All = new[]
        {
            new Entry { Number = 1, Title = "Gia biến", Scene = "Chapter01_GiaBien",
                Blurb = "Nhà họ Vương gặp nạn. Mã Giám Sinh tới hỏi cưới." },
            new Entry { Number = 2, Title = "Trên đường theo Mã Giám Sinh", Scene = "Chapter02_RoadToMaGiamSinh",
                Blurb = "Con đường đất đỏ, và những nghi ngờ đầu tiên." },
            new Entry { Number = 3, Title = "Tú Bà", Scene = "Chapter03_TuBaHouse",
                Blurb = "Một ngôi nhà trông như mái ấm, vận hành như chiếc lồng." },
            new Entry { Number = 4, Title = "Không còn đường lui", Scene = "Chapter04_TuBaNight",
                Blurb = "Đêm ở nhà Tú Bà — tìm lối ra." },
            new Entry { Number = 5, Title = "Đào thoát", Scene = "Chapter05_Escape",
                Blurb = "Ngõ hẻm, chợ đêm và cánh rừng phía sau." },
            new Entry { Number = 6, Title = "Kim Trọng", Scene = "Chapter06_KimTrong",
                Blurb = "Chùa Bạch Vân, thư phòng và ba nhân chứng." },
            new Entry { Number = 7, Title = "Nghịch mệnh", Scene = "Chapter07_Placeholder", Placeholder = true,
                Blurb = "Đang được phát triển." },
        };

        public static Entry Get(int number)
        {
            foreach (var entry in All) if (entry.Number == number) return entry;
            return null;
        }

        public static Entry FindByScene(string scene)
        {
            foreach (var entry in All) if (entry.Scene == scene) return entry;
            return null;
        }

        /// <summary>Accepts "3", "ch3", "chuong3" or a scene name.</summary>
        public static Entry Parse(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            string digits = value.ToLowerInvariant().Replace("chapter", "").Replace("chuong", "").Replace("ch", "").Trim();
            if (int.TryParse(digits, out int number)) return Get(number);
            return FindByScene(value);
        }

        public static bool InBuild(Entry entry) => entry != null && Application.CanStreamedLevelBeLoaded(entry.Scene);
    }
}

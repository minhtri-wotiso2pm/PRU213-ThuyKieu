using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// The endings the player has reached, kept across playthroughs for the main menu's gallery.
    /// Classification mirrors the story: lost case → fate; low bond → won but lost herself; then the Nghịch Mệnh choice,
    /// then the tone of the verdict.
    /// </summary>
    public static class EndingRecord
    {
        private const string Key = "tk.endings";

        public sealed class Ending
        {
            public string Id, Title, Body, Hint;
        }

        public static readonly Ending[] All =
        {
            new Ending { Id = "nghich_menh_cung", Title = "Nghịch Mệnh · Hai linh hồn một thân",
                Body = "Thắng trọn vẹn, và ở lại cùng Kiều. Hai người tự viết tiếp trang sách của mình.",
                Hint = "Thắng trọn vẹn, đồng điệu thật cao với Kiều Thật, rồi chọn ở lại." },
            new Ending { Id = "nghich_menh_chia", Title = "Nghịch Mệnh · Trả lại đời cho nàng",
                Body = "Thắng trọn vẹn, rồi trả thân xác cho Kiều để nàng sống nốt đời mình.",
                Hint = "Thắng trọn vẹn, đồng điệu thật cao với Kiều Thật, rồi chọn chia tay." },
            new Ending { Id = "tron_ven", Title = "Kết trọn vẹn",
                Body = "Lầu xanh bị niêm phong, ba người con gái được tìm về.",
                Hint = "Đủ hồ sơ, đối chất sắc bén, lời kết thuyết phục." },
            new Ending { Id = "am", Title = "Kết ấm áp",
                Body = "Kiều tự do, kẻ buôn người phải đền tội.",
                Hint = "Thắng kiện với điểm phán quyết khá." },
            new Ending { Id = "chat_vat", Title = "Kết chật vật",
                Body = "Suýt thua, thắng nhờ người dám đứng ra bảo lãnh.",
                Hint = "Thắng kiện sát nút." },
            new Ending { Id = "mat_minh", Title = "Thắng mà mất mình",
                Body = "Vụ kiện thắng, nhưng Kiều Thật lặng đi không một lời.",
                Hint = "Thắng, nhưng chưa bao giờ thật sự lắng nghe Kiều Thật." },
            new Ending { Id = "dinh_menh", Title = "Kết Định Mệnh",
                Body = "Thua kiện. Kiều bị trả về lầu xanh, đúng như trang sách.",
                Hint = "Thua kiện trước công đường." }
        };

        /// <summary>Which ending a finished story reached, read from its Ink variables.</summary>
        public static string Classify(Func<string, int> get)
        {
            if (get("c7_ket") == 0) return "dinh_menh";
            if (get("dong_dieu") <= 3) return "mat_minh";
            if (get("ket_hon") == 1) return "nghich_menh_chia";
            if (get("ket_hon") == 2) return "nghich_menh_cung";
            switch (get("c7_ket"))
            {
                case 3: return "tron_ven";
                case 2: return "am";
                default: return "chat_vat";
            }
        }

        public static IReadOnlyCollection<string> Unlocked => Load();

        public static bool IsUnlocked(string id) => Load().Contains(id);

        /// <summary>Records an ending; true when it is new.</summary>
        public static bool Unlock(string id)
        {
            var ids = Load();
            if (!ids.Add(id)) return false;
            PlayerPrefs.SetString(Key, string.Join(",", ids));
            PlayerPrefs.Save();
            return true;
        }

        private static HashSet<string> Load() =>
            new HashSet<string>(PlayerPrefs.GetString(Key, string.Empty).Split(',').Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}

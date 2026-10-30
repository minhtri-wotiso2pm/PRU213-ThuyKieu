using System;
using System.Collections.Generic;
using ThuyKieu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Options screen of the game shell, in four tabs (sound, dialogue, display, controls).
    /// Rows are selected with the mouse or the arrow keys; Left/Right (or the ‹ › buttons) change the value,
    /// which is saved and applied at once through <see cref="GameSettings"/>.
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        private sealed class Option
        {
            public string Label;
            public Func<string> Value;
            public Action<int> Step;
            public TMP_Text ValueText;
            public Button Row;
        }

        private static bool _cleanArmed;
        private static readonly string[] Tabs = { "Âm thanh", "Thoại", "Hình ảnh", "Điều khiển" };
        private static readonly float[] TextSpeeds = { 0.5f, 1f, 1.6f, 2.5f, 4f };
        private static readonly string[] TextSpeedNames = { "Chậm", "Vừa", "Nhanh", "Rất nhanh", "Hiện ngay" };

        private readonly List<List<Option>> _pages = new List<List<Option>>();
        private readonly List<ShellButton> _tabLooks = new List<ShellButton>();
        private readonly List<GameObject> _pageRoots = new List<GameObject>();
        private Button _done;
        private int _tab;
        private Action _onClose;

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Builds the hidden options panel under the shell canvas.</summary>
        public static SettingsView Build(Transform canvas, Func<Transform, string, string, Vector2, Vector2, Action, float, bool, Button> makeButton)
        {
            var root = StoryUIBuilder.Rect(canvas, "Settings", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(root, new Color(0.015f, 0.01f, 0.02f, 0.94f)).raycastTarget = true;
            var view = root.gameObject.AddComponent<SettingsView>();
            var panel = StoryUIBuilder.Panel(root, "Panel", new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.9f), new Color(0.06f, 0.045f, 0.055f, 0.98f), 26);
            StoryUIBuilder.Text(panel, "Title", "CÀI ĐẶT", 40, new Vector2(0.05f, 0.88f), new Vector2(0.5f, 0.97f), StoryUIBuilder.Gold).fontStyle = FontStyles.Bold;
            var hint = StoryUIBuilder.Text(panel, "Hint", "↑↓ chọn · ←→ đổi giá trị · Q/E đổi thẻ · Esc quay lại", 18,
                new Vector2(0.4f, 0.88f), new Vector2(0.96f, 0.97f), new Color(1, 1, 1, 0.5f));
            hint.alignment = TextAlignmentOptions.MidlineRight;
            StoryUIBuilder.Image(StoryUIBuilder.Rect(panel, "Rule", new Vector2(0.05f, 0.87f), new Vector2(0.95f, 0.872f)), new Color(1f, 0.8f, 0.45f, 0.3f));

            for (int i = 0; i < Tabs.Length; i++)
            {
                int index = i;
                float left = 0.05f + i * 0.225f;
                var tab = makeButton(panel, "Tab" + i, Tabs[i], new Vector2(left, 0.77f), new Vector2(left + 0.21f, 0.85f), () => view.ShowTab(index), 26, false);
                view._tabLooks.Add(tab.GetComponent<ShellButton>());
            }

            view.AddPage(panel, makeButton, new List<Option>
            {
                Percent("Âm lượng tổng", () => GameSettings.Master, v => GameSettings.Master = v),
                Percent("Nhạc nền", () => GameSettings.Music, v => GameSettings.Music = v),
                Percent("Hiệu ứng âm thanh", () => GameSettings.Effects, v => GameSettings.Effects = v),
                Percent("Âm thanh môi trường", () => GameSettings.Ambience, v => GameSettings.Ambience = v),
                Percent("Giọng nhân vật", () => GameSettings.Voice, v => GameSettings.Voice = v),
            });
            view.AddPage(panel, makeButton, new List<Option>
            {
                new Option { Label = "Tốc độ chữ", Value = () => TextSpeedNames[NearestSpeed()],
                    Step = d => GameSettings.TextSpeed = TextSpeeds[Mathf.Clamp(NearestSpeed() + d, 0, TextSpeeds.Length - 1)] },
                Toggle("Lồng tiếng", () => GameSettings.VoiceOn, v => GameSettings.VoiceOn = v),
                Toggle("Tự chuyển câu thoại", () => GameSettings.AutoAdvance, v => GameSettings.AutoAdvance = v),
                // "Bắt đầu sạch": forget the Trang Sách pages kept between runs (two presses, so it is never an accident).
                new Option { Label = "Bắt đầu sạch (quên trang đã nhớ)",
                    Value = () => _cleanArmed ? "Bấm lần nữa để xóa" : Core.Story.PageArchive.Ids.Count + " trang",
                    Step = d => { if (!_cleanArmed) { _cleanArmed = true; return; } Core.Story.PageArchive.Clear(); _cleanArmed = false; } },
            });
            view.AddPage(panel, makeButton, new List<Option>
            {
                Percent("Độ sáng", () => GameSettings.Brightness, v => GameSettings.Brightness = v),
                new Option { Label = "Độ phân giải", Value = ResolutionName, Step = d =>
                {
                    var all = GameSettings.Resolutions();
                    int current = GameSettings.ResolutionIndex < 0 ? CurrentResolution(all) : GameSettings.ResolutionIndex;
                    GameSettings.ResolutionIndex = Mathf.Clamp(current + d, 0, all.Length - 1);
                    GameSettings.ApplyDisplay();
                } },
                new Option { Label = "Chế độ hiển thị", Value = () => GameSettings.Fullscreen ? "Toàn màn hình" : "Cửa sổ",
                    Step = d => { GameSettings.Fullscreen = !GameSettings.Fullscreen; GameSettings.ApplyDisplay(); } },
                new Option { Label = "Chất lượng đồ hoạ", Value = () => QualityName(GameSettings.Quality), Step = d =>
                {
                    GameSettings.Quality = Mathf.Clamp(GameSettings.Quality + d, 0, QualitySettings.names.Length - 1);
                    GameSettings.ApplyDisplay();
                } },
                new Option { Label = "V-Sync", Value = () => GameSettings.VSync ? "Bật" : "Tắt",
                    Step = d => { GameSettings.VSync = !GameSettings.VSync; GameSettings.ApplyDisplay(); } },
            });
            view.AddPage(panel, makeButton, new List<Option>
            {
                new Option { Label = "Độ nhạy chuột", Value = () => "× " + GameSettings.MouseSensitivity.ToString("0.00"),
                    Step = d => GameSettings.MouseSensitivity += d * 0.25f },
                Toggle("Đảo trục dọc chuột", () => GameSettings.InvertY, v => GameSettings.InvertY = v),
                new Option { Label = "Rung màn hình", Value = () => GameSettings.ShakeAmount <= 0f ? "Tắt" : GameSettings.ShakeAmount < 0.75f ? "Nhẹ" : "Đầy đủ",
                    Step = d => GameSettings.ShakeAmount = GameSettings.ShakeAmount <= 0f ? (d > 0 ? 0.5f : 0f) : GameSettings.ShakeAmount < 0.75f ? (d > 0 ? 1f : 0f) : (d > 0 ? 1f : 0.5f) },
            });

            makeButton(panel, "Reset", "Khôi phục mặc định", new Vector2(0.05f, 0.04f), new Vector2(0.42f, 0.12f), () => { GameSettings.ResetAll(); view.RefreshValues(); }, 24, false);
            view._done = makeButton(panel, "Done", "Xong", new Vector2(0.62f, 0.04f), new Vector2(0.95f, 0.12f), view.Close, 26, true);
            root.gameObject.SetActive(false);
            return view;
        }

        private void AddPage(Transform panel, Func<Transform, string, string, Vector2, Vector2, Action, float, bool, Button> makeButton, List<Option> options)
        {
            var page = StoryUIBuilder.Rect(panel, "Page" + _pages.Count, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.74f));
            float step = 1f / 5f;
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                float top = 1f - i * step;
                option.Row = makeButton(page, "Row" + i, option.Label, new Vector2(0f, top - step * 0.86f), new Vector2(1f, top), () => Change(option, 1), 27, false);
                var look = option.Row.GetComponent<ShellButton>();
                look.BackgroundColor = new Color(0.1f, 0.075f, 0.085f, 1f);
                look.BackgroundHighlight = new Color(0.3f, 0.21f, 0.11f, 1f);
                look.Setup();
                option.ValueText = StoryUIBuilder.Text(option.Row.transform, "Value", "", 26, new Vector2(0.55f, 0f), new Vector2(0.9f, 1f), Color.white);
                option.ValueText.alignment = TextAlignmentOptions.Center;
                Arrow(option.Row.transform, "Less", "‹", new Vector2(0.5f, 0.1f), new Vector2(0.56f, 0.9f), () => Change(option, -1));
                Arrow(option.Row.transform, "More", "›", new Vector2(0.9f, 0.1f), new Vector2(0.96f, 0.9f), () => Change(option, 1));
            }
            _pages.Add(options);
            _pageRoots.Add(page.gameObject);
        }

        private static void Arrow(Transform row, string name, string glyph, Vector2 min, Vector2 max, Action onClick)
        {
            var rect = StoryUIBuilder.Rect(row, name, min, max);
            var image = StoryUIBuilder.Image(rect, new Color(1, 1, 1, 0.001f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => onClick());
            var text = StoryUIBuilder.Text(rect, "Glyph", glyph, 38, Vector2.zero, Vector2.one, StoryUIBuilder.Gold);
            text.alignment = TextAlignmentOptions.Center;
        }

        public void Open(Action onClose)
        {
            _onClose = onClose;
            gameObject.SetActive(true);
            ShowTab(_tab);
        }

        public void Close()
        {
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            var closed = _onClose;
            _onClose = null;
            closed?.Invoke();
        }

        public void ShowTab(int index)
        {
            _tab = (index + Tabs.Length) % Tabs.Length;
            for (int i = 0; i < _pageRoots.Count; i++) _pageRoots[i].SetActive(i == _tab);
            for (int i = 0; i < _tabLooks.Count; i++)
            {
                _tabLooks[i].LabelColor = i == _tab ? StoryUIBuilder.Gold : new Color(1, 1, 1, 0.6f);
                _tabLooks[i].BackgroundColor = i == _tab ? new Color(0.3f, 0.21f, 0.11f, 1f) : new Color(0, 0, 0, 0f);
                _tabLooks[i].Refresh();
            }
            RefreshValues();
            Select(_pages[_tab][0].Row.gameObject);
        }

        private void RefreshValues()
        {
            foreach (var page in _pages)
                foreach (var option in page) option.ValueText.text = option.Value();
        }

        private void Change(Option option, int direction)
        {
            option.Step(direction);
            option.ValueText.text = option.Value();
            Select(option.Row.gameObject);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { GamePause.ConsumeEscape(); Close(); return; }
            if (keyboard.qKey.wasPressedThisFrame || keyboard.pageUpKey.wasPressedThisFrame) { ShowTab(_tab - 1); return; }
            if (keyboard.eKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame || keyboard.pageDownKey.wasPressedThisFrame) { ShowTab(_tab + 1); return; }
            int direction = keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ? -1
                : keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame ? 1 : 0;
            if (direction == 0) return;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            foreach (var option in _pages[_tab])
                if (option.Row.gameObject == selected) { Change(option, direction); return; }
        }

        private static void Select(GameObject target)
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(target);
        }

        private static Option Percent(string label, Func<float> get, Action<float> set) => new Option
        {
            Label = label,
            Value = () => Mathf.RoundToInt(get() * 100f) + "%",
            Step = d => set(Mathf.Round(get() * 10f + d) / 10f)
        };

        private static Option Toggle(string label, Func<bool> get, Action<bool> set) => new Option
        {
            Label = label,
            Value = () => get() ? "Bật" : "Tắt",
            Step = d => set(!get())
        };

        private static int NearestSpeed()
        {
            int best = 0;
            for (int i = 1; i < TextSpeeds.Length; i++)
                if (Mathf.Abs(TextSpeeds[i] - GameSettings.TextSpeed) < Mathf.Abs(TextSpeeds[best] - GameSettings.TextSpeed)) best = i;
            return best;
        }

        private static int CurrentResolution(Resolution[] all)
        {
            for (int i = 0; i < all.Length; i++) if (all[i].width == Screen.width && all[i].height == Screen.height) return i;
            return all.Length - 1;
        }

        private static string ResolutionName()
        {
            if (Application.isEditor) return Screen.width + " × " + Screen.height + " (Editor)";
            var all = GameSettings.Resolutions();
            int index = GameSettings.ResolutionIndex < 0 ? CurrentResolution(all) : Mathf.Clamp(GameSettings.ResolutionIndex, 0, all.Length - 1);
            return all[index].width + " × " + all[index].height;
        }

        private static string QualityName(int level)
        {
            var names = QualitySettings.names;
            if (names.Length == 0) return "—";
            string name = names[Mathf.Clamp(level, 0, names.Length - 1)];
            switch (name)
            {
                case "Very Low": return "Rất thấp";
                case "Low": return "Thấp";
                case "Medium": return "Trung bình";
                case "High": return "Cao";
                case "Very High": return "Rất cao";
                case "Ultra": return "Tối đa";
                default: return name;
            }
        }
    }
}

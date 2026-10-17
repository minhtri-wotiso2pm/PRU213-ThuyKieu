using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThuyKieu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Test console toggled with ` (backquote) or F1: jump to any chapter or scene, restart, unlock, change time scale.
    /// Text arrives through Keyboard.onTextInput, so it works without an InputField or EventSystem focus.
    /// </summary>
    public class DevConsole : MonoBehaviour
    {
        private const int MaxLines = 14;
        private readonly List<string> _lines = new List<string>();
        private readonly List<string> _history = new List<string>();
        private readonly StringBuilder _input = new StringBuilder();
        private GameObject _root;
        private TMP_Text _log, _prompt;
        private int _historyIndex;
        private int _openedFrame = -1;

        public bool IsOpen => _root != null && _root.activeSelf;
        public System.Action<bool> OpenChanged;

        public static DevConsole Build(Transform parent)
        {
            var root = StoryUIBuilder.Rect(parent, "Console", new Vector2(0f, 0.52f), Vector2.one);
            StoryUIBuilder.Image(root, new Color(0.02f, 0.015f, 0.025f, 0.94f));
            StoryUIBuilder.Image(StoryUIBuilder.Rect(root, "Rule", new Vector2(0f, 0f), new Vector2(1f, 0f)), StoryUIBuilder.Gold).rectTransform.offsetMax = new Vector2(0, 3);
            var title = StoryUIBuilder.Text(root, "Title", "BẢNG LỆNH THỬ NGHIỆM  <size=18><color=#FFFFFF77>gõ help · Enter chạy · mũi tên lên/xuống: lịch sử · Esc / ` đóng</color></size>",
                22, new Vector2(0.015f, 0.88f), new Vector2(0.985f, 0.98f), StoryUIBuilder.Gold);
            title.fontStyle = FontStyles.Bold;
            var log = StoryUIBuilder.Text(root, "Log", "", 21, new Vector2(0.015f, 0.14f), new Vector2(0.985f, 0.87f), new Color(0.86f, 0.86f, 0.82f));
            log.alignment = TextAlignmentOptions.BottomLeft;
            log.richText = true;
            var prompt = StoryUIBuilder.Text(root, "Prompt", "", 25, new Vector2(0.015f, 0.02f), new Vector2(0.985f, 0.13f), Color.white);
            prompt.richText = true;

            var console = root.gameObject.AddComponent<DevConsole>();
            console._root = root.gameObject;
            console._log = log;
            console._prompt = prompt;
            console.Print("<color=#E8C27A>Gõ</color> help <color=#E8C27A>để xem lệnh.</color>  Ví dụ: <b>ch 3</b> · <b>ch 4 keep</b> · <b>restart</b> · <b>menu</b>");
            root.gameObject.SetActive(false);
            return console;
        }

        private void OnEnable() { if (Keyboard.current != null) Keyboard.current.onTextInput += OnText; }
        private void OnDisable() { if (Keyboard.current != null) Keyboard.current.onTextInput -= OnText; }

        public void Toggle() { if (IsOpen) Close(); else Open(); }

        public void Open()
        {
            if (IsOpen) return;
            _openedFrame = Time.frameCount;
            _input.Clear();
            _historyIndex = _history.Count;
            _root.SetActive(true);
            Redraw();
            OpenChanged?.Invoke(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            _root.SetActive(false);
            OpenChanged?.Invoke(false);
        }

        private void OnText(char c)
        {
            // The key that opened the console also produces a character on the same frame.
            if (!IsOpen || Time.frameCount == _openedFrame || c == '`' || c == '~' || char.IsControl(c)) return;
            _input.Append(c);
            Redraw();
        }

        private void Update()
        {
            if (!IsOpen) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.backspaceKey.wasPressedThisFrame && _input.Length > 0) { _input.Length--; Redraw(); }
            if (keyboard.upArrowKey.wasPressedThisFrame && _history.Count > 0) Recall(-1);
            if (keyboard.downArrowKey.wasPressedThisFrame && _history.Count > 0) Recall(1);
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Submit();
        }

        private void Recall(int step)
        {
            _historyIndex = Mathf.Clamp(_historyIndex + step, 0, _history.Count);
            _input.Clear();
            if (_historyIndex < _history.Count) _input.Append(_history[_historyIndex]);
            Redraw();
        }

        private void Redraw()
        {
            bool caret = Mathf.Repeat(Time.unscaledTime, 1f) < 0.6f;
            _prompt.text = "<color=#E8C27A>></color> " + _input + (caret ? "<color=#E8C27A>_</color>" : " ");
            _log.text = string.Join("\n", _lines);
        }

        private void LateUpdate() { if (IsOpen) Redraw(); }

        public void Print(string line)
        {
            _lines.Add(line);
            while (_lines.Count > MaxLines) _lines.RemoveAt(0);
            if (_log != null) _log.text = string.Join("\n", _lines);
        }

        private void Submit()
        {
            string command = _input.ToString().Trim();
            _input.Clear();
            if (command.Length == 0) return;
            _history.Add(command);
            _historyIndex = _history.Count;
            Print("<color=#FFFFFF66>></color> " + command);
            Execute(command);
            Redraw();
        }

        public void Execute(string command)
        {
            string[] parts = command.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            string name = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1] : null;
            switch (name)
            {
                case "help": case "?": case "h":
                    Print("<color=#E8C27A>ch</color> <n> [keep]   chơi chương n từ đầu (keep: giữ lựa chọn đã lưu của chương trước)");
                    Print("<color=#E8C27A>continue</color>         chơi tiếp chương gần nhất");
                    Print("<color=#E8C27A>restart</color>          chơi lại scene hiện tại      <color=#E8C27A>select</color>  mở bảng chọn chương");
                    Print("<color=#E8C27A>scenes</color> · <color=#E8C27A>scene</color> <số|tên>   liệt kê / mở scene bất kỳ trong build");
                    Print("<color=#E8C27A>menu</color> · <color=#E8C27A>unlock</color> · <color=#E8C27A>progress</color> · <color=#E8C27A>reset</color> · <color=#E8C27A>ts</color> <hệ số> · <color=#E8C27A>clear</color> · <color=#E8C27A>quit</color>");
                    break;
                case "ch": case "chapter": case "chuong":
                    var entry = ChapterCatalog.Parse(arg);
                    if (entry == null) { ListChapters(); break; }
                    bool keep = parts.Length > 2 && parts[2].ToLowerInvariant() == "keep";
                    if (ChapterLoader.LoadChapter(entry, keep)) Print("Đang tải " + ChapterCatalog.Label(entry.Number) + " — " + entry.Title + (keep ? " (giữ lựa chọn)" : ""));
                    else Print("<color=#FF8A7A>Không tải được " + entry.Scene + " (chưa có trong Build Settings?)</color>");
                    break;
                case "continue":
                    if (!ChapterLoader.Continue()) Print("<color=#FF8A7A>Chưa có tiến trình để chơi tiếp.</color>");
                    break;
                case "restart": case "r":
                    ChapterLoader.LoadScene(SceneManager.GetActiveScene().name);
                    break;
                case "select":
                    Close();
                    GameShell.ShowChapterSelect(null);
                    break;
                case "menu":
                    if (!ChapterLoader.LoadMainMenu()) Print("<color=#FF8A7A>Scene MainMenu chưa có trong build.</color>");
                    break;
                case "scenes":
                    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                        Print(i + "  " + System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
                    break;
                case "scene":
                    string scene = arg;
                    if (int.TryParse(arg, out int index) && index >= 0 && index < SceneManager.sceneCountInBuildSettings)
                        scene = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index));
                    if (scene == null || !ChapterLoader.LoadScene(scene)) Print("<color=#FF8A7A>Không tìm thấy scene: " + arg + "</color>");
                    break;
                case "unlock":
                    GameProgress.UnlockAll = true;
                    Print("Đã mở khoá mọi chương.");
                    break;
                case "progress":
                    Print("Đã tới chương " + GameProgress.Reached + " · chơi gần nhất: chương " + GameProgress.Last + (GameProgress.UnlockAll ? " · mở khoá toàn bộ" : ""));
                    break;
                case "reset":
                    GameProgress.Reset();
                    Print("Đã xoá tiến trình (PlayerPrefs). File lựa chọn Ink vẫn giữ.");
                    break;
                case "ts": case "timescale":
                    if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale) && scale > 0f)
                    {
                        GameShell.SetResumeTimeScale(scale);
                        Print("Time scale = " + scale.ToString(CultureInfo.InvariantCulture) + " (áp dụng khi đóng bảng lệnh)");
                    }
                    else Print("Cú pháp: ts 2");
                    break;
                case "clear":
                    _lines.Clear();
                    break;
                case "quit": case "exit":
                    ChapterLoader.Quit();
                    break;
                default:
                    Print("<color=#FF8A7A>Lệnh không rõ: " + name + "</color> — gõ help");
                    break;
            }
        }

        private void ListChapters()
        {
            foreach (var chapter in ChapterCatalog.All)
                Print("ch " + chapter.Number + "   " + chapter.Title + (ChapterCatalog.InBuild(chapter) ? "" : "  <color=#FF8A7A>(thiếu trong build)</color>"));
        }
    }
}

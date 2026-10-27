using System.Text;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEngine;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Clue journal (J). Opens only while exploring; pauses player control while open. Long journals turn pages
    /// (←/→ or A/D) and open on the newest page.
    /// </summary>
    public class JournalView : MonoBehaviour
    {
        private GameObject _root;
        private TMP_Text _list, _hint;
        private StoryDirector _director;

        public bool IsOpen => _root.activeSelf;
        /// <summary>Frame the journal closed on, so the same Esc press does not also open the pause menu.</summary>
        public int ClosedFrame { get; private set; } = -1;

        private void Awake()
        {
            _root = transform.Find("Journal").gameObject;
            _list = StoryUIBuilder.Find<TMP_Text>(transform, "Journal/Panel/List");
            _hint = StoryUIBuilder.Find<TMP_Text>(transform, "Journal/Panel/Hint");
            _list.overflowMode = TextOverflowModes.Page;
        }

        private void Start() { _director = FindAnyObjectByType<StoryDirector>(); }

        private void Update()
        {
            if (_director == null) return;
            if (IsOpen && (InputReader.JournalPressedThisFrame || InputReader.CancelPressedThisFrame)) { Close(); return; }
            if (IsOpen) TurnPages();
            if (!IsOpen && InputReader.JournalPressedThisFrame && _director.InExplore && !_director.IsBusy && !PauseMenuView.IsPaused) Open();
        }

        public void Open()
        {
            var builder = new StringBuilder();
            if (_director.Clues.Count == 0) builder.Append("<i>Chưa có manh mối nào. Hãy quan sát, lắng nghe và đặt câu hỏi.</i>");
            string chapter = null;
            foreach (string id in _director.Clues)
            {
                string group = id.StartsWith("pl_") ? "MỞ ĐẦU · PHÒNG KIỀU" : id.StartsWith("c2_") ? "CHƯƠNG 2 · TRÊN ĐƯỜNG" : id.StartsWith("c3_") ? "CHƯƠNG 3 · NHÀ TÚ BÀ" : id.StartsWith("c4_") ? "CHƯƠNG 4 · ĐÊM Ở LẦU TÚ BÀ" : id.StartsWith("c6_") ? "CHƯƠNG 6 · HỒ SƠ" : id.StartsWith("c7_") ? "CHƯƠNG 7 · HUYỆN ĐƯỜNG" : "KHÁC";
                if (group != chapter) { chapter = group; builder.Append("<size=20><color=#8A5A2A>").Append(group).Append("</color></size>\n"); }
                ClueCatalog.TryGet(id, out string title, out string body);
                builder.Append("<b><color=#5A1E0E>").Append(title).Append("</color></b>\n").Append(body).Append("\n\n");
            }
            _list.text = builder.ToString();
            _root.SetActive(true);
            _list.ForceMeshUpdate();
            _list.pageToDisplay = Mathf.Max(1, _list.textInfo.pageCount);
            UpdateHint();
            _director.SetFreeControl(false);
            _director.CameraController.SetCursorLocked(false);
            if (_director.Audio != null) _director.Audio.PlaySfx("paper", 0.6f);
        }

        private void TurnPages()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            int pages = Mathf.Max(1, _list.textInfo.pageCount);
            int page = _list.pageToDisplay;
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) page--;
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) page++;
            // The mouse wheel pages too: rolling down reads on, like scrolling a document.
            var mouse = UnityEngine.InputSystem.Mouse.current;
            float wheel = mouse != null ? mouse.scroll.ReadValue().y : 0f;
            if (wheel < -0.5f) page++;
            if (wheel > 0.5f) page--;
            page = Mathf.Clamp(page, 1, pages);
            if (page == _list.pageToDisplay) return;
            _list.pageToDisplay = page;
            UpdateHint();
        }

        private void UpdateHint()
        {
            if (_hint == null) return;
            int pages = Mathf.Max(1, _list.textInfo.pageCount);
            _hint.text = pages > 1 ? "←/→ / lăn chuột: lật trang (" + _list.pageToDisplay + "/" + pages + ") · J / Esc: đóng sổ tay" : "J / Esc: đóng sổ tay";
        }

        public void Close()
        {
            _root.SetActive(false);
            ClosedFrame = Time.frameCount;
            if (_director.InExplore) _director.SetFreeControl(true);
        }
    }
}

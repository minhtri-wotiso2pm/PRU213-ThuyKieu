using System.Text;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEngine;

namespace ThuyKieu.UI
{
    /// <summary>Clue journal (J). Opens only while exploring; pauses player control while open.</summary>
    public class JournalView : MonoBehaviour
    {
        private GameObject _root;
        private TMP_Text _list;
        private StoryDirector _director;

        public bool IsOpen => _root.activeSelf;
        /// <summary>Frame the journal closed on, so the same Esc press does not also open the pause menu.</summary>
        public int ClosedFrame { get; private set; } = -1;

        private void Awake()
        {
            _root = transform.Find("Journal").gameObject;
            _list = StoryUIBuilder.Find<TMP_Text>(transform, "Journal/Panel/List");
        }

        private void Start() { _director = FindAnyObjectByType<StoryDirector>(); }

        private void Update()
        {
            if (_director == null) return;
            if (IsOpen && (InputReader.JournalPressedThisFrame || InputReader.CancelPressedThisFrame)) { Close(); return; }
            if (!IsOpen && InputReader.JournalPressedThisFrame && _director.InExplore && !_director.IsBusy && !PauseMenuView.IsPaused) Open();
        }

        public void Open()
        {
            var builder = new StringBuilder();
            if (_director.Clues.Count == 0) builder.Append("<i>Chưa có manh mối nào. Hãy quan sát, lắng nghe và đặt câu hỏi.</i>");
            string chapter = null;
            foreach (string id in _director.Clues)
            {
                string group = id.StartsWith("c2_") ? "CHƯƠNG 2 · TRÊN ĐƯỜNG" : id.StartsWith("c3_") ? "CHƯƠNG 3 · NHÀ TÚ BÀ" : id.StartsWith("c4_") ? "CHƯƠNG 4 · ĐÊM Ở LẦU TÚ BÀ" : id.StartsWith("c6_") ? "CHƯƠNG 6 · HỒ SƠ" : "KHÁC";
                if (group != chapter) { chapter = group; builder.Append("<size=20><color=#8A5A2A>").Append(group).Append("</color></size>\n"); }
                ClueCatalog.TryGet(id, out string title, out string body);
                builder.Append("<b><color=#5A1E0E>").Append(title).Append("</color></b>\n").Append(body).Append("\n\n");
            }
            _list.text = builder.ToString();
            _root.SetActive(true);
            _director.SetFreeControl(false);
            _director.CameraController.SetCursorLocked(false);
            if (_director.Audio != null) _director.Audio.PlaySfx("paper", 0.6f);
        }

        public void Close()
        {
            _root.SetActive(false);
            ClosedFrame = Time.frameCount;
            if (_director.InExplore) _director.SetFreeControl(true);
        }
    }
}

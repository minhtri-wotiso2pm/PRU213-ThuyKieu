using System;
using System.Text;
using ThuyKieu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.UI
{
    /// <summary>
    /// "Các kết" from the main menu: every ending, the ones reached with their summary, the rest as a hint of how to
    /// get there. Esc / Enter close it.
    /// </summary>
    public class EndingGalleryView : MonoBehaviour
    {
        private TMP_Text _title, _list;
        private float _shownAt;
        private Action _onClose;

        public bool IsOpen => gameObject.activeSelf;

        public static EndingGalleryView Build(Transform canvas)
        {
            var root = StoryUIBuilder.Rect(canvas, "Endings", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(root, new Color(0.015f, 0.01f, 0.02f, 0.96f)).raycastTarget = true;
            var view = root.gameObject.AddComponent<EndingGalleryView>();
            var panel = StoryUIBuilder.Panel(root, "Panel", new Vector2(0.14f, 0.07f), new Vector2(0.86f, 0.93f), new Color(0.06f, 0.045f, 0.055f, 0.98f), 26);
            view._title = StoryUIBuilder.Text(panel, "Title", "", 40, new Vector2(0.04f, 0.89f), new Vector2(0.96f, 0.98f), StoryUIBuilder.Gold);
            view._title.fontStyle = FontStyles.Bold;
            StoryUIBuilder.Image(StoryUIBuilder.Rect(panel, "Rule", new Vector2(0.04f, 0.885f), new Vector2(0.96f, 0.887f)), new Color(1f, 0.8f, 0.45f, 0.3f));
            view._list = StoryUIBuilder.Text(panel, "List", "", 25, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.87f), Color.white);
            view._list.alignment = TextAlignmentOptions.TopLeft;
            view._list.lineSpacing = 6;
            view._list.richText = true;
            var hint = StoryUIBuilder.Text(panel, "Hint", "Esc / Enter: đóng", 19, new Vector2(0.5f, 0.015f), new Vector2(0.96f, 0.07f), new Color(1, 1, 1, 0.45f));
            hint.alignment = TextAlignmentOptions.Right;
            root.gameObject.SetActive(false);
            return view;
        }

        public void Open(Action onClose)
        {
            _onClose = onClose;
            int reached = 0;
            var b = new StringBuilder();
            foreach (var ending in EndingRecord.All)
            {
                if (EndingRecord.IsUnlocked(ending.Id))
                {
                    reached++;
                    b.Append("<color=#E8C27A><b>").Append(ending.Title).Append("</b></color>\n<size=85%><color=#FFFFFFCC>").Append(ending.Body).Append("</color></size>\n\n");
                }
                else b.Append("<color=#FFFFFF55><b>? ? ?</b></color>\n<size=85%><color=#FFFFFF66>Gợi ý: ").Append(ending.Hint).Append("</color></size>\n\n");
            }
            _title.text = "CÁC KẾT ĐÃ MỞ  <size=60%><color=#FFFFFF88>" + reached + " / " + EndingRecord.All.Length +
                          " · chơi lại để \"viết lại\" — các trang thơ đã nhớ được giữ lại</color></size>";
            _list.text = b.ToString();
            gameObject.SetActive(true);
            _shownAt = Time.unscaledTime;
        }

        public void Close()
        {
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            var closed = _onClose;
            _onClose = null;
            closed?.Invoke();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (Time.unscaledTime - _shownAt < 0.3f) return;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) { GamePause.ConsumeEscape(); Close(); }
            else if ((keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
                     || (mouse != null && mouse.rightButton.wasPressedThisFrame)) Close();
        }
    }
}

using System;
using ThuyKieu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Rolling credits: the team, the course and every borrowed resource with its licence.
    /// Scrolls on its own (hold Space to speed up); Esc / Enter close it.
    /// </summary>
    public class CreditsView : MonoBehaviour
    {
        public const string Text =
            "<size=64><b><color=#E8C27A>THÚY KIỀU</color></b></size>\n<size=30><cspace=0.6em>NGHỊCH MỆNH</cspace></size>\n\n\n" +
            "<size=24><color=#E8C27A>NHÓM THỰC HIỆN</color></size>\n" +
            "<size=22><color=#FFFFFF99>Môn PRU213 · nhóm 6 người</color></size>\n\n" +
            "<b>Ngô Trần Gia Thuận</b>\n<size=22><color=#FFFFFFAA>Trưởng nhóm · Kịch bản & Ink · Tích hợp</color></size>\n\n" +
            "<b>Lê Ngọc Quốc Anh</b>\n<size=22><color=#FFFFFFAA>Lập trình chính (Core)</color></size>\n\n" +
            "<b>Nguyễn Đỗ Bảo Hưng</b>\n<size=22><color=#FFFFFFAA>Nhân vật · Dựng cảnh điện ảnh</color></size>\n\n" +
            "<b>Nguyễn Minh Trí</b>\n<size=22><color=#FFFFFFAA>Thiết kế màn chơi · Gameplay người chơi</color></size>\n\n" +
            "<b>Nguyễn Lê Ngọc Trí</b>\n<size=22><color=#FFFFFFAA>Giao diện · Mini-game</color></size>\n\n" +
            "<b>Hoàng Thái Hoàng</b>\n<size=22><color=#FFFFFFAA>Kiểm thử · Build · Git · Âm thanh · Công cụ</color></size>\n\n\n" +
            "<size=24><color=#E8C27A>NGUYÊN TÁC</color></size>\n" +
            "<i>Truyện Kiều</i> (Đoạn trường tân thanh)\n<size=22><color=#FFFFFFAA>Nguyễn Du (1766–1820) · các câu thơ trích dẫn thuộc phạm vi công cộng</color></size>\n\n\n" +
            "<size=24><color=#E8C27A>TÀI NGUYÊN & CÔNG CỤ</color></size>\n" +
            "Unity 6 · Universal Render Pipeline\n" +
            "Ink — inkle Ltd. <size=22><color=#FFFFFFAA>(giấy phép MIT)</color></size>\n" +
            "Texture & bầu trời HDRI — Poly Haven <size=22><color=#FFFFFFAA>(CC0)</color></size>\n" +
            "Rig & hoạt ảnh nhân vật — Adobe Mixamo\n" +
            "Giọng đọc — Microsoft Edge Neural TTS <size=22><color=#FFFFFFAA>(vi-VN HoaiMy, NamMinh)</color></size>\n" +
            "Nhạc nền & hiệu ứng âm thanh — tạo bằng mã (procedural)\n\n\n" +
            "<size=24><color=#E8C27A>CẢM ƠN</color></size>\n" +
            "Các thầy cô bộ môn, và những ai từng đọc Truyện Kiều mà thấy lòng mình chùng xuống.\n\n\n\n" +
            "<size=30><i>\"Có những chuyện chỉ khi ở trong hoàn cảnh đó mới hiểu được.\"</i></size>\n\n\n\n" +
            "<size=26><color=#E8C27A>Cảm ơn bạn đã chơi.</color></size>";

        private RectTransform _content;
        private TMP_Text _body;
        private float _shownAt;
        private Action _onClose;

        public bool IsOpen => gameObject.activeSelf;

        public static CreditsView Build(Transform canvas)
        {
            var root = StoryUIBuilder.Rect(canvas, "Credits", Vector2.zero, Vector2.one);
            StoryUIBuilder.Image(root, Color.black).raycastTarget = true;
            var view = root.gameObject.AddComponent<CreditsView>();
            var viewport = StoryUIBuilder.Rect(root, "Viewport", new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.94f));
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            view._content = StoryUIBuilder.Rect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
            view._content.pivot = new Vector2(0.5f, 1f);
            view._body = StoryUIBuilder.Text(view._content, "Body", Text, 30, Vector2.zero, Vector2.one, Color.white);
            view._body.alignment = TextAlignmentOptions.Top;
            view._body.lineSpacing = 8;
            view._body.richText = true;
            var hint = StoryUIBuilder.Text(root, "Hint", "Giữ Space: cuộn nhanh · Esc / Enter: đóng", 18, new Vector2(0.6f, 0.01f), new Vector2(0.985f, 0.05f), new Color(1, 1, 1, 0.4f));
            hint.alignment = TextAlignmentOptions.BottomRight;
            root.gameObject.SetActive(false);
            return view;
        }

        public void Open(Action onClose)
        {
            _onClose = onClose;
            gameObject.SetActive(true);
            _body.ForceMeshUpdate();
            float height = _body.preferredHeight + 40f;
            _content.sizeDelta = new Vector2(0f, height);
            // Start just below the viewport so the title scrolls into view.
            _content.anchoredPosition = new Vector2(0f, -((RectTransform)_content.parent).rect.height);
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
            bool fast = keyboard != null && keyboard.spaceKey.isPressed;
            float end = _content.sizeDelta.y - ((RectTransform)_content.parent).rect.height * 0.55f;
            if (_content.anchoredPosition.y < end)
                _content.anchoredPosition += new Vector2(0f, Time.unscaledDeltaTime * (fast ? 420f : 70f));
            if (keyboard == null || Time.unscaledTime - _shownAt < 0.6f) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { GamePause.ConsumeEscape(); Close(); }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Close();
        }
    }
}

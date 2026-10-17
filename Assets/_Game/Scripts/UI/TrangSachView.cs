using System.Collections;
using System.Text;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue;
using ThuyKieu.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Trang Sách overlay, built in code so every story scene (the hand-made Chapter 1 included) gets it without a
    /// prefab: the Định Mệnh ink bar on the left edge, a toast when a page is gained, an ink flash when fate crosses a
    /// threshold, and the book itself (B) listing the pages and how close the two souls have come.
    /// Reads the Ink variables only; the story writes them.
    /// </summary>
    public class TrangSachView : MonoBehaviour
    {
        private static readonly int[] Thresholds = { 30, 50, 70, 85 };
        private static readonly Color InkRed = new Color(0.62f, 0.08f, 0.06f, 0.95f);

        private DialogueManager _dialogue;
        private StoryAudio _audio;
        private RectTransform _fateFill;
        private TMP_Text _fateLabel, _toastTitle, _toastBody, _bookList, _bookFooter;
        private GameObject _toast, _book;
        private Image _flash;
        private int _lastFate = -1;
        private float _toastUntil, _nextArchive;

        public bool BookOpen => _book != null && _book.activeSelf;

        /// <summary>Creates the overlay for a scene's dialogue manager (once per scene).</summary>
        public static TrangSachView Attach(DialogueManager dialogue)
        {
            var existing = FindAnyObjectByType<TrangSachView>();
            if (existing != null) return existing;
            var view = new GameObject("TrangSach").AddComponent<TrangSachView>();
            view._dialogue = dialogue;
            view.Build();
            dialogue.InkTagsChanged += view.OnTags;
            return view;
        }

        private void OnDestroy() { if (_dialogue != null) _dialogue.InkTagsChanged -= OnTags; }

        private Ink.Runtime.Story Story => _dialogue != null ? _dialogue.InkStory : null;

        // ------------------------------------------------------------------ build

        private void Build()
        {
            if (StoryUIBuilder.Font == null)
            {
                var anyText = FindAnyObjectByType<TMP_Text>();
                if (anyText != null) StoryUIBuilder.Font = anyText.font;
            }
            _audio = FindAnyObjectByType<StoryAudio>();
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9; // the ink bar sits under the story canvas, so its fades and title cards cover it
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            Transform t = transform;
            // Toasts and the book go above the story canvas (trial screen included); the book hides while paused.
            var top = StoryUIBuilder.Rect(t, "Top", Vector2.zero, Vector2.one);
            var topCanvas = top.gameObject.AddComponent<Canvas>();
            topCanvas.overrideSorting = true;
            topCanvas.sortingOrder = 11;
            top.gameObject.AddComponent<GraphicRaycaster>();

            _flash = StoryUIBuilder.Image(StoryUIBuilder.Rect(t, "Flash", Vector2.zero, Vector2.one), new Color(InkRed.r, InkRed.g, InkRed.b, 0f));
            _flash.raycastTarget = false;

            // Định Mệnh: a thin ink bar on the left edge with the four thresholds marked.
            var track = StoryUIBuilder.Panel(t, "Fate", new Vector2(0.006f, 0.3f), new Vector2(0.013f, 0.7f), new Color(0, 0, 0, 0.35f), 6);
            _fateFill = StoryUIBuilder.Rect(track, "Fill", Vector2.zero, new Vector2(1, 0));
            StoryUIBuilder.Image(_fateFill, InkRed).raycastTarget = false;
            foreach (int mark in Thresholds)
                StoryUIBuilder.Image(StoryUIBuilder.Rect(track, "Mark" + mark, new Vector2(-0.4f, mark / 100f), new Vector2(1.4f, mark / 100f + 0.004f)), new Color(1f, 0.85f, 0.6f, 0.6f)).raycastTarget = false;
            _fateLabel = StoryUIBuilder.Text(t, "FateLabel", "ĐỊNH MỆNH\n<size=85%><color=#E8C27A>B</color>: Trang Sách</size>", 15, new Vector2(0.003f, 0.235f), new Vector2(0.1f, 0.295f), new Color(1f, 0.8f, 0.7f, 0.75f));

            // Toast: a page has fallen into Kiều's hand.
            var toast = StoryUIBuilder.Panel(top, "Toast", new Vector2(0.32f, 0.78f), new Vector2(0.68f, 0.93f), StoryUIBuilder.Paper, 14);
            _toast = toast.gameObject;
            _toastTitle = StoryUIBuilder.Text(toast, "Title", "", 18, new Vector2(0.04f, 0.66f), new Vector2(0.96f, 0.95f), new Color(0.55f, 0.2f, 0.08f));
            _toastTitle.alignment = TextAlignmentOptions.Center;
            _toastBody = StoryUIBuilder.Text(toast, "Body", "", 24, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.68f), new Color(0.18f, 0.1f, 0.05f));
            _toastBody.alignment = TextAlignmentOptions.Center;
            _toastBody.fontStyle = FontStyles.Italic;
            _toast.SetActive(false);

            // The book.
            var book = StoryUIBuilder.Panel(top, "Book", new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.92f), StoryUIBuilder.Paper, 18);
            _book = book.gameObject;
            var title = StoryUIBuilder.Text(book, "Title", "TRANG SÁCH", 34, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.98f), new Color(0.45f, 0.12f, 0.05f));
            title.alignment = TextAlignmentOptions.Center;
            _bookList = StoryUIBuilder.Text(book, "List", "", 19, new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.89f), new Color(0.18f, 0.1f, 0.05f));
            _bookList.alignment = TextAlignmentOptions.TopLeft;
            _bookFooter = StoryUIBuilder.Text(book, "Footer", "", 19, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.13f), new Color(0.3f, 0.15f, 0.08f));
            _bookFooter.alignment = TextAlignmentOptions.Center;
            _book.SetActive(false);
        }

        // ------------------------------------------------------------------ runtime

        private void Update()
        {
            var story = Story;
            if (story == null) return;

            int fate = PageCatalog.State(story, "dinh_menh");
            if (fate != _lastFate)
            {
                if (_lastFate >= 0) foreach (int mark in Thresholds) if (_lastFate < mark && fate >= mark) StartCoroutine(Flash());
                _lastFate = fate;
                _fateFill.anchorMax = new Vector2(1f, Mathf.Clamp01(fate / 100f));
            }

            if (Time.unscaledTime >= _nextArchive)
            {
                _nextArchive = Time.unscaledTime + 0.5f;
                foreach (var page in PageCatalog.All)
                    if (!page.Unusable && PageCatalog.State(story, page.Id) >= PageCatalog.Held) PageArchive.Remember(page.Id);
            }

            if (_toast.activeSelf && Time.unscaledTime > _toastUntil) _toast.SetActive(false);

            if (PauseMenuView.IsPaused) { if (BookOpen) _book.SetActive(false); return; }
            if (InputReader.BookPressedThisFrame)
            {
                if (BookOpen) _book.SetActive(false);
                else OpenBook();
            }
        }

        private void OnTags(string[] tags)
        {
            foreach (string raw in tags)
            {
                string tag = raw.Trim();
                bool half = tag.StartsWith("page_half:");
                if (!half && !tag.StartsWith("page:")) continue;
                if (!PageCatalog.TryGet(tag.Substring(tag.IndexOf(':') + 1).Trim(), out var page)) continue;
                ShowToast(half ? "TRANG RÁCH · mới nhớ được nửa câu" : page.Unusable ? "TRANG SÁCH · số phận đang kéo về" : "TRANG SÁCH · B để xem",
                    half ? page.Half : page.Verse);
                if (_audio != null) _audio.PlaySfx(page.Unusable ? "suspicion_up" : "clue");
            }
        }

        public void ShowToast(string title, string body)
        {
            _toastTitle.text = title;
            _toastBody.text = body;
            _toast.SetActive(true);
            _toastUntil = Time.unscaledTime + 4f;
        }

        private IEnumerator Flash()
        {
            if (_audio != null) _audio.PlaySfx("suspicion_up");
            ShowToast("ĐỊNH MỆNH PHẢN ĐÒN", "Câu chuyện đang kéo bạn về trang sách cũ.");
            for (float t = 0; t < 1.6f; t += Time.unscaledDeltaTime)
            {
                float a = t < 0.25f ? t / 0.25f : 1f - (t - 0.25f) / 1.35f;
                _flash.color = new Color(InkRed.r, InkRed.g, InkRed.b, 0.45f * Mathf.Clamp01(a));
                yield return null;
            }
            _flash.color = new Color(InkRed.r, InkRed.g, InkRed.b, 0f);
        }

        public void CloseBook() => _book.SetActive(false);

        public void OpenBook()
        {
            var story = Story;
            var b = new StringBuilder();
            int known = 0;
            foreach (var page in PageCatalog.All)
            {
                int state = PageCatalog.State(story, page.Id);
                if (state == PageCatalog.None) { b.Append("<color=#9A8A70>·  ? ? ?</color>\n\n"); continue; }
                known++;
                string verse = page.Verse.Replace("\n", " / ");
                if (state == PageCatalog.Torn) b.Append("<color=#7A6A50>·  ").Append(page.Half).Append("  <size=80%>(trang rách)</size></color>\n\n");
                else if (state == PageCatalog.Used) b.Append("<color=#8A7A60><s>·  ").Append(verse).Append("</s>  <size=80%>(đã xé)</size></color>\n\n");
                else
                {
                    b.Append("<color=#5A1E0E><b>·  ").Append(verse).Append("</b></color>\n");
                    b.Append("<size=80%><color=#8A5A2A>    ").Append(page.Hint).Append(page.Unusable ? "" : " · lật: +" + page.Cost + " Định Mệnh").Append("</color></size>\n\n");
                }
            }
            _bookList.text = b.ToString();
            int soul = PageCatalog.State(story, "dong_dieu");
            string hands = soul >= 7 ? "Hai bàn tay đã chạm vào nhau." : soul >= 4 ? "Hai bàn tay đang với tìm nhau." : "Hai bàn tay vẫn quay lưng lại với nhau.";
            _bookFooter.text = hands + "   ·   Định Mệnh " + PageCatalog.State(story, "dinh_menh") + "/100   ·   " + known + "/" + PageCatalog.All.Length + " trang\n<size=80%>B: gấp sách</size>";
            _book.SetActive(true);
            if (_audio != null) _audio.PlaySfx("paper", 0.6f);
        }
    }
}

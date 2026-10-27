using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>
    /// Creates the story canvas hierarchy (HUD, cinematic dialogue, journal, document reader,
    /// deduction board, chapter/fail cards). Views locate their parts by these names in Awake,
    /// so the generated object can be saved as a prefab and restyled in the Inspector.
    /// </summary>
    public static class StoryUIBuilder
    {
        public static readonly Color Ink = new Color(0.035f, 0.03f, 0.045f, 0.9f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.45f);
        public static readonly Color Paper = new Color(0.9f, 0.83f, 0.66f, 1f);
        public static readonly Color Vermilion = new Color(0.72f, 0.16f, 0.12f);
        private static TMP_FontAsset _font;
        /// <summary>Font used by the primitives below; Build sets it, runtime builders (GameShell) may too.</summary>
        public static TMP_FontAsset Font { get => _font; set => _font = value; }

        public static GameObject Build(TMP_FontAsset font)
        {
            _font = font;
            var root = new GameObject("StoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            Transform t = root.transform;

            // Cinematic bars
            var bars = Rect(t, "Letterbox", Vector2.zero, Vector2.one);
            Image(Rect(bars, "Top", new Vector2(0, 1), new Vector2(1, 1)), Color.black).rectTransform.pivot = new Vector2(0.5f, 1);
            Image(Rect(bars, "Bottom", new Vector2(0, 0), new Vector2(1, 0)), Color.black).rectTransform.pivot = new Vector2(0.5f, 0);

            BuildHud(t);
            BuildDialogue(t);
            BuildJournal(t);
            BuildDocument(t);
            BuildDeduction(t);
            BuildDossier(t);
            BuildTrial(t);
            BuildRecap(t);
            BuildCards(t);
            BuildPause(t);

            var fade = Image(Rect(t, "Fade", Vector2.zero, Vector2.one), Color.black);
            fade.gameObject.AddComponent<CanvasGroup>().alpha = 1f;
            fade.raycastTarget = false;

            root.AddComponent<StoryHUD>();
            root.AddComponent<StoryDialogueView>();
            root.AddComponent<JournalView>();
            root.AddComponent<DocumentView>();
            root.AddComponent<DeductionView>();
            root.AddComponent<DossierView>();
            root.AddComponent<TrialView>();
            root.AddComponent<RecapView>();
            root.AddComponent<PauseMenuView>();
            return root;
        }

        private static void BuildHud(Transform parent)
        {
            var hud = Rect(parent, "HUD", Vector2.zero, Vector2.one);
            // Header grows with the number of open options (StoryHUD sets its height), children use pixel offsets.
            // Width stops at 0.36 so it never runs under the objective meter (0.38–0.62).
            var header = Panel(hud, "Header", new Vector2(0.018f, 1f), new Vector2(0.36f, 1f), new Color(0, 0, 0, 0.42f), 14);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = new Vector2(0, -27 - 130);
            header.offsetMax = new Vector2(0, -27);
            var chapterLabel = Text(header, "ChapterLabel", "", 20, new Vector2(0, 1), new Vector2(1, 1), Gold);
            chapterLabel.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            Pixels(chapterLabel.rectTransform, -10, -42);
            var objective = Text(header, "Objective", "", 27, new Vector2(0, 1), new Vector2(1, 1), Color.white);
            objective.alignment = TextAlignmentOptions.TopLeft;
            Pixels(objective.rectTransform, -42, -112);
            var list = Text(header, "ExploreList", "", 20, new Vector2(0, 0), new Vector2(1, 1), new Color(0.86f, 0.86f, 0.82f));
            list.alignment = TextAlignmentOptions.TopLeft;
            list.rectTransform.offsetMin = new Vector2(16, 8);
            list.rectTransform.offsetMax = new Vector2(-16, -112);
            Text(hud, "Controls", "", 19, new Vector2(0.018f, 0.01f), new Vector2(0.7f, 0.05f), new Color(1, 1, 1, 0.55f));

            var prompt = Panel(hud, "Prompt", new Vector2(0.39f, 0.29f), new Vector2(0.61f, 0.345f), new Color(0, 0, 0, 0.7f), 14);
            Text(prompt, "Text", "", 24, Vector2.zero, Vector2.one, Color.white).alignment = TextAlignmentOptions.Center;

            var meter = Panel(hud, "Meter", new Vector2(0.38f, 0.905f), new Vector2(0.62f, 0.975f), new Color(0, 0, 0, 0.55f), 14);
            Text(meter, "Label", "", 20, new Vector2(0.04f, 0.5f), new Vector2(0.96f, 1f), Color.white).alignment = TextAlignmentOptions.Center;
            var track = Image(Rect(meter, "Track", new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.42f)), new Color(1, 1, 1, 0.15f));
            Image(Rect(track.transform, "Fill", Vector2.zero, new Vector2(0f, 1f)), Gold);

            var observe = Rect(hud, "Observe", new Vector2(0.4f, 0.42f), new Vector2(0.6f, 0.58f));
            var reticle = Text(observe, "Reticle", "[        ]", 54, new Vector2(0, 0.35f), new Vector2(1, 0.85f), new Color(1, 1, 1, 0.75f));
            reticle.alignment = TextAlignmentOptions.Center;
            Text(observe, "Label", "", 21, new Vector2(0, 0.12f), new Vector2(1, 0.35f), Gold).alignment = TextAlignmentOptions.Center;
            var observeTrack = Image(Rect(observe, "Track", new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.1f)), new Color(1, 1, 1, 0.2f));
            Image(Rect(observeTrack.transform, "Fill", Vector2.zero, new Vector2(0f, 1f)), Gold);

            var toast = Panel(hud, "Toast", new Vector2(0.7f, 0.83f), new Vector2(0.982f, 0.94f), new Color(0.05f, 0.04f, 0.03f, 0.88f), 14);
            Image(Rect(toast, "Accent", new Vector2(0, 0.12f), new Vector2(0.012f, 0.88f)), Gold);
            Text(toast, "Title", "", 18, new Vector2(0.05f, 0.56f), new Vector2(0.97f, 0.95f), Gold).fontStyle = FontStyles.Bold;
            Text(toast, "Body", "", 23, new Vector2(0.05f, 0.05f), new Vector2(0.97f, 0.58f), Color.white);

            // Secondary gauge under the action (stamina while running, visibility while sneaking).
            var gauge = Panel(hud, "Gauge", new Vector2(0.4f, 0.065f), new Vector2(0.6f, 0.11f), new Color(0, 0, 0, 0.5f), 14);
            Text(gauge, "Label", "", 17, new Vector2(0.03f, 0.45f), new Vector2(0.97f, 1f), new Color(1, 1, 1, 0.85f)).alignment = TextAlignmentOptions.Center;
            var gaugeTrack = Image(Rect(gauge, "Track", new Vector2(0.05f, 0.16f), new Vector2(0.95f, 0.38f)), new Color(1, 1, 1, 0.15f));
            Image(Rect(gaugeTrack.transform, "Fill", Vector2.zero, new Vector2(1f, 1f)), Gold);

            // Quick-time prompt: the key to press, what it does and how long is left.
            var qte = Panel(hud, "Qte", new Vector2(0.43f, 0.4f), new Vector2(0.57f, 0.6f), new Color(0.05f, 0.03f, 0.03f, 0.82f), 26);
            var key = Text(qte, "Key", "", 64, new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.95f), Gold);
            key.alignment = TextAlignmentOptions.Center; key.fontStyle = FontStyles.Bold;
            Text(qte, "Caption", "", 20, new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.42f), Color.white).alignment = TextAlignmentOptions.Center;
            var qteTrack = Image(Rect(qte, "Track", new Vector2(0.1f, 0.07f), new Vector2(0.9f, 0.13f)), new Color(1, 1, 1, 0.18f));
            Image(Rect(qteTrack.transform, "Fill", Vector2.zero, new Vector2(1f, 1f)), Vermilion);

            var bark = Rect(hud, "Bark", new Vector2(0.2f, 0.12f), new Vector2(0.8f, 0.2f));
            var barkText = Text(bark, "Text", "", 27, Vector2.zero, Vector2.one, Color.white);
            barkText.alignment = TextAlignmentOptions.Center;
            barkText.outlineWidth = 0.18f;
            barkText.outlineColor = new Color32(0, 0, 0, 220);
        }

        private static void BuildDialogue(Transform parent)
        {
            var dialogue = Rect(parent, "Dialogue", Vector2.zero, Vector2.one);
            var panel = Panel(dialogue, "Panel", new Vector2(0.12f, 0.035f), new Vector2(0.88f, 0.255f), Ink, 26);
            Image(Rect(panel, "Rule", new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.745f)), new Color(Gold.r, Gold.g, Gold.b, 0.35f));
            Text(panel, "Name", "", 27, new Vector2(0.03f, 0.75f), new Vector2(0.7f, 0.97f), Gold).fontStyle = FontStyles.Bold;
            var body = Text(panel, "Body", "", 30, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.72f), Color.white);
            body.alignment = TextAlignmentOptions.TopLeft;
            body.lineSpacing = 6;

            var inner = Text(dialogue, "Inner", "", 33, new Vector2(0.14f, 0.07f), new Vector2(0.86f, 0.27f), new Color(0.84f, 0.92f, 1f));
            inner.fontStyle = FontStyles.Italic;
            inner.alignment = TextAlignmentOptions.Center;
            inner.outlineWidth = 0.2f;
            inner.outlineColor = new Color32(0, 0, 0, 200);

            var spirit = Text(dialogue, "Spirit", "", 46, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.86f), Gold);
            spirit.alignment = TextAlignmentOptions.Center;
            spirit.fontStyle = FontStyles.Italic;
            spirit.characterSpacing = 4;
            spirit.outlineWidth = 0.12f;
            spirit.outlineColor = new Color32(80, 40, 0, 200);

            var verse = Panel(dialogue, "Verse", new Vector2(0.14f, 0.42f), new Vector2(0.86f, 0.6f), new Color(Paper.r, Paper.g, Paper.b, 0.93f), 26);
            var verseText = Text(verse, "Text", "", 36, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f), new Color(0.22f, 0.12f, 0.06f));
            verseText.alignment = TextAlignmentOptions.Center;
            verseText.fontStyle = FontStyles.Italic;

            Text(dialogue, "Hint", "Space / Click  »", 19, new Vector2(0.72f, 0.0f), new Vector2(0.88f, 0.034f), new Color(1, 1, 1, 0.5f)).alignment = TextAlignmentOptions.Right;

            var choices = Rect(dialogue, "Choices", new Vector2(0.52f, 0.28f), new Vector2(0.95f, 0.66f));
            var layout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10; layout.childAlignment = TextAnchor.LowerRight;
            layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var timer = Image(Rect(choices, "Timer", Vector2.zero, Vector2.one), new Color(1, 1, 1, 0.15f));
            var timerSize = timer.gameObject.AddComponent<LayoutElement>();
            timerSize.minHeight = 6; timerSize.preferredHeight = 6;
            Image(Rect(timer.transform, "Fill", Vector2.zero, Vector2.one), Vermilion);
            var template = Panel(choices, "Template", Vector2.zero, Vector2.one, new Color(0.06f, 0.05f, 0.07f, 0.92f), 14);
            template.gameObject.AddComponent<Button>();
            var size = template.gameObject.AddComponent<LayoutElement>();
            size.minHeight = 58; size.preferredHeight = 58;
            Image(Rect(template, "Accent", new Vector2(0, 0.15f), new Vector2(0.008f, 0.85f)), Gold);
            var label = Text(template, "Label", "", 25, new Vector2(0.03f, 0), new Vector2(0.98f, 1), Color.white);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            template.gameObject.SetActive(false);

        }

        private static void BuildJournal(Transform parent)
        {
            var journal = Image(Rect(parent, "Journal", Vector2.zero, Vector2.one), new Color(0, 0, 0, 0.65f)).transform;
            var panel = Panel(journal, "Panel", new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.9f), new Color(Paper.r, Paper.g, Paper.b, 0.97f), 26);
            Text(panel, "Title", "SỔ TAY MANH MỐI", 38, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), new Color(0.35f, 0.12f, 0.06f)).fontStyle = FontStyles.Bold;
            Image(Rect(panel, "Rule", new Vector2(0.05f, 0.875f), new Vector2(0.95f, 0.879f)), new Color(0.35f, 0.12f, 0.06f, 0.5f));
            var list = Text(panel, "List", "", 24, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.86f), new Color(0.15f, 0.1f, 0.06f));
            list.alignment = TextAlignmentOptions.TopLeft;
            list.lineSpacing = 4;
            Text(panel, "Hint", "J / Esc: đóng sổ tay", 19, new Vector2(0.05f, 0.015f), new Vector2(0.95f, 0.07f), new Color(0.3f, 0.2f, 0.1f, 0.8f)).alignment = TextAlignmentOptions.Right;
            journal.gameObject.SetActive(false);
        }

        private static void BuildDocument(Transform parent)
        {
            var document = Image(Rect(parent, "Document", Vector2.zero, Vector2.one), new Color(0, 0, 0, 0.72f)).transform;
            var paper = Panel(document, "Paper", new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.9f), Paper, 14);
            var title = Text(paper, "Title", "", 34, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.95f), new Color(0.3f, 0.1f, 0.05f));
            title.alignment = TextAlignmentOptions.Center; title.fontStyle = FontStyles.Bold;
            var body = Text(paper, "Body", "", 28, new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.82f), new Color(0.16f, 0.1f, 0.06f));
            body.alignment = TextAlignmentOptions.Top; body.lineSpacing = 10;
            var seal = Panel(paper, "Seal", new Vector2(0.7f, 0.04f), new Vector2(0.86f, 0.16f), Vermilion, 26);
            Text(seal, "Text", "ẤN", 30, Vector2.zero, Vector2.one, Paper).alignment = TextAlignmentOptions.Center;
            Text(document, "Hint", "E / Space: cất giấy", 21, new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.08f), new Color(1, 1, 1, 0.7f)).alignment = TextAlignmentOptions.Center;
            document.gameObject.SetActive(false);
        }

        private static void BuildDeduction(Transform parent)
        {
            var deduction = Image(Rect(parent, "Deduction", Vector2.zero, Vector2.one), new Color(0.02f, 0.01f, 0.02f, 0.82f)).transform;
            var panel = Panel(deduction, "Panel", new Vector2(0.17f, 0.12f), new Vector2(0.83f, 0.88f), new Color(0.07f, 0.05f, 0.06f, 0.96f), 26);
            Text(panel, "Title", "GHÉP MẢNH SỰ THẬT", 36, new Vector2(0.05f, 0.87f), new Vector2(0.75f, 0.97f), Gold).fontStyle = FontStyles.Bold;
            Text(panel, "Progress", "", 22, new Vector2(0.7f, 0.87f), new Vector2(0.95f, 0.97f), new Color(1, 1, 1, 0.6f)).alignment = TextAlignmentOptions.Right;
            var question = Text(panel, "Question", "", 31, new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.85f), Color.white);
            question.alignment = TextAlignmentOptions.TopLeft;
            var options = Rect(panel, "Options", new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.64f));
            var layout = options.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12; layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var template = Panel(options, "Template", Vector2.zero, Vector2.one, new Color(1, 1, 1, 0.06f), 14);
            template.gameObject.AddComponent<Button>();
            var size = template.gameObject.AddComponent<LayoutElement>();
            size.minHeight = 64; size.preferredHeight = 64;
            Text(template, "Label", "", 25, new Vector2(0.03f, 0), new Vector2(0.97f, 1), Color.white).alignment = TextAlignmentOptions.MidlineLeft;
            template.gameObject.SetActive(false);
            var verdict = Text(panel, "Verdict", "", 40, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.18f), Vermilion);
            verdict.alignment = TextAlignmentOptions.Center; verdict.fontStyle = FontStyles.Bold;
            deduction.gameObject.SetActive(false);
        }

        /// <summary>Kim Trọng's desk: two documents side by side and a question about where they contradict;
        /// also the final dossier with its four kinds of evidence.</summary>
        private static void BuildDossier(Transform parent)
        {
            var dossier = Image(Rect(parent, "Dossier", Vector2.zero, Vector2.one), new Color(0.02f, 0.01f, 0.02f, 0.85f)).transform;
            var panel = Panel(dossier, "Panel", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.94f), new Color(0.08f, 0.06f, 0.05f, 0.97f), 26);
            Text(panel, "Title", "", 34, new Vector2(0.04f, 0.9f), new Vector2(0.7f, 0.98f), Gold).fontStyle = FontStyles.Bold;
            Text(panel, "Progress", "", 22, new Vector2(0.7f, 0.9f), new Vector2(0.96f, 0.98f), new Color(1, 1, 1, 0.6f)).alignment = TextAlignmentOptions.Right;
            foreach (var (name, min, max) in new[] { ("Left", new Vector2(0.04f, 0.47f), new Vector2(0.49f, 0.89f)), ("Right", new Vector2(0.51f, 0.47f), new Vector2(0.96f, 0.89f)) })
            {
                var paper = Panel(panel, name, min, max, Paper, 14);
                var docTitle = Text(paper, "Title", "", 25, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.97f), new Color(0.35f, 0.1f, 0.05f));
                docTitle.alignment = TextAlignmentOptions.Center; docTitle.fontStyle = FontStyles.Bold;
                var docBody = Text(paper, "Body", "", 21, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.8f), new Color(0.16f, 0.1f, 0.06f));
                docBody.alignment = TextAlignmentOptions.TopLeft; docBody.lineSpacing = 4;
            }
            var question = Text(panel, "Question", "", 26, new Vector2(0.04f, 0.37f), new Vector2(0.96f, 0.46f), Color.white);
            question.alignment = TextAlignmentOptions.MidlineLeft;
            var options = Rect(panel, "Options", new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.36f));
            var layout = options.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8; layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var template = Panel(options, "Template", Vector2.zero, Vector2.one, new Color(1, 1, 1, 0.06f), 14);
            template.gameObject.AddComponent<Button>();
            var size = template.gameObject.AddComponent<LayoutElement>();
            size.minHeight = 50; size.preferredHeight = 50;
            Text(template, "Label", "", 22, new Vector2(0.02f, 0), new Vector2(0.98f, 1), Color.white).alignment = TextAlignmentOptions.MidlineLeft;
            template.gameObject.SetActive(false);
            var verdict = Text(panel, "Verdict", "", 30, new Vector2(0.04f, 0.015f), new Vector2(0.96f, 0.095f), Vermilion);
            verdict.alignment = TextAlignmentOptions.Center; verdict.fontStyle = FontStyles.Bold;
            // Summary mode: four evidence slots.
            var slots = Rect(panel, "Slots", new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.88f));
            for (int i = 0; i < 4; i++)
            {
                var slot = Panel(slots, "Slot" + i, new Vector2(0.01f + i * 0.25f, 0.05f), new Vector2(0.24f + i * 0.25f, 0.95f), new Color(1, 1, 1, 0.06f), 18);
                var slotTitle = Text(slot, "Title", "", 26, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f), Gold);
                slotTitle.alignment = TextAlignmentOptions.Center; slotTitle.fontStyle = FontStyles.Bold;
                var mark = Text(slot, "Mark", "", 90, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.82f), Color.white);
                mark.alignment = TextAlignmentOptions.Center;
                var slotBody = Text(slot, "Body", "", 20, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.55f), new Color(1, 1, 1, 0.85f));
                slotBody.alignment = TextAlignmentOptions.Top;
            }
            slots.gameObject.SetActive(false);
            dossier.gameObject.SetActive(false);
        }

        /// <summary>
        /// Chapter 7 courtroom: the witness's testimony on the left, actions or the evidence case on the right,
        /// the magistrate's trust along the top and the last thing said along the bottom. The 3D hall stays visible.
        /// </summary>
        private static void BuildTrial(Transform parent)
        {
            var trial = Rect(parent, "Trial", Vector2.zero, Vector2.one);
            var top = Panel(trial, "Top", new Vector2(0.025f, 0.875f), new Vector2(0.975f, 0.975f), new Color(0.05f, 0.03f, 0.03f, 0.9f), 18);
            var title = Text(top, "Title", "", 30, new Vector2(0.02f, 0.4f), new Vector2(0.55f, 0.98f), Gold);
            title.fontStyle = FontStyles.Bold;
            Text(top, "Subtitle", "", 19, new Vector2(0.02f, 0.02f), new Vector2(0.55f, 0.45f), new Color(1, 1, 1, 0.6f));
            Text(top, "TrustLabel", "Lòng tin của quan huyện", 19, new Vector2(0.58f, 0.52f), new Vector2(0.98f, 0.98f), new Color(1, 1, 1, 0.75f)).alignment = TextAlignmentOptions.MidlineRight;
            var track = Image(Rect(top, "Track", new Vector2(0.58f, 0.18f), new Vector2(0.98f, 0.42f)), new Color(1, 1, 1, 0.14f));
            Image(Rect(track.transform, "Fill", Vector2.zero, new Vector2(0.6f, 1f)), Gold);

            var left = Panel(trial, "Testimony", new Vector2(0.025f, 0.16f), new Vector2(0.52f, 0.855f), new Color(0.04f, 0.03f, 0.035f, 0.76f), 22);
            var header = Text(left, "Header", "LỜI KHAI", 24, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.98f), Vermilion);
            header.fontStyle = FontStyles.Bold;
            var rows = Rect(left, "Rows", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.89f));
            var layout = rows.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10; layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var row = Panel(rows, "Template", Vector2.zero, Vector2.one, new Color(1, 1, 1, 0.05f), 14);
            row.gameObject.AddComponent<Button>();
            var rowSize = row.gameObject.AddComponent<LayoutElement>();
            rowSize.minHeight = 96; rowSize.preferredHeight = 96;
            Image(Rect(row, "Accent", new Vector2(0, 0.12f), new Vector2(0.008f, 0.88f)), Gold);
            var rowText = Text(row, "Label", "", 23, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.96f), Color.white);
            rowText.alignment = TextAlignmentOptions.MidlineLeft;
            row.gameObject.SetActive(false);

            var right = Panel(trial, "Actions", new Vector2(0.54f, 0.16f), new Vector2(0.975f, 0.855f), new Color(0.04f, 0.03f, 0.035f, 0.72f), 22);
            var actionHeader = Text(right, "Header", "", 24, new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.98f), Gold);
            actionHeader.fontStyle = FontStyles.Bold;
            Text(right, "Help", "", 20, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.89f), new Color(1, 1, 1, 0.75f)).alignment = TextAlignmentOptions.TopLeft;
            foreach (var (name, label, min, max, color) in new[]
            {
                ("Press", "<color=#E8C27A>Q</color>   HỎI VẶN", new Vector2(0.05f, 0.4f), new Vector2(0.95f, 0.56f), new Color(1, 1, 1, 0.08f)),
                ("Present", "<color=#FFE0B0>E</color>   ĐƯA BẰNG CHỨNG", new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.36f), Vermilion)
            })
            {
                var button = Panel(right, name, min, max, color, 16);
                button.gameObject.AddComponent<Button>();
                Text(button, "Label", label, 28, Vector2.zero, Vector2.one, Color.white).alignment = TextAlignmentOptions.Center;
            }
            var cases = Rect(right, "Evidence", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.89f));
            var grid = cases.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(376, 94); grid.spacing = new Vector2(10, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2;
            var card = Panel(cases, "Template", Vector2.zero, Vector2.one, new Color(Paper.r, Paper.g, Paper.b, 0.95f), 12);
            card.gameObject.AddComponent<Button>();
            var cardTitle = Text(card, "Title", "", 21, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.96f), new Color(0.38f, 0.1f, 0.05f));
            cardTitle.fontStyle = FontStyles.Bold;
            var cardBody = Text(card, "Body", "", 15, new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.6f), new Color(0.18f, 0.11f, 0.06f));
            cardBody.alignment = TextAlignmentOptions.TopLeft;
            card.gameObject.SetActive(false);
            Text(right, "Back", "Backspace / chuột phải: cất hồ sơ", 18, new Vector2(0.55f, 0.002f), new Vector2(0.96f, 0.055f), new Color(1, 1, 1, 0.5f)).alignment = TextAlignmentOptions.Right;

            var bottom = Panel(trial, "Line", new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.145f), Ink, 22);
            Text(bottom, "Name", "", 22, new Vector2(0.02f, 0.6f), new Vector2(0.4f, 0.95f), Gold).fontStyle = FontStyles.Bold;
            var line = Text(bottom, "Text", "", 25, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.66f), Color.white);
            line.alignment = TextAlignmentOptions.TopLeft;

            var stamp = Text(trial, "Stamp", "", 110, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.62f), Vermilion);
            stamp.alignment = TextAlignmentOptions.Center; stamp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            stamp.outlineWidth = 0.22f; stamp.outlineColor = new Color32(30, 8, 4, 255);
            stamp.gameObject.SetActive(false);
            trial.gameObject.SetActive(false);
        }

        /// <summary>End of the game: the tone of the ending and what the player's choices were, chapter by chapter.</summary>
        private static void BuildRecap(Transform parent)
        {
            var recap = Image(Rect(parent, "Recap", Vector2.zero, Vector2.one), new Color(0.02f, 0.015f, 0.02f, 0.96f)).transform;
            var title = Text(recap, "Title", "HÀNH TRÌNH CỦA BẠN", 46, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.95f), Gold);
            title.alignment = TextAlignmentOptions.Center; title.fontStyle = FontStyles.Bold;
            var tone = Text(recap, "Tone", "", 28, new Vector2(0.1f, 0.77f), new Vector2(0.9f, 0.86f), Color.white);
            tone.alignment = TextAlignmentOptions.Center; tone.fontStyle = FontStyles.Italic;
            Image(Rect(recap, "Rule", new Vector2(0.3f, 0.765f), new Vector2(0.7f, 0.767f)), new Color(1f, 0.8f, 0.45f, 0.4f));
            foreach (var (name, min, max) in new[] { ("Left", new Vector2(0.07f, 0.13f), new Vector2(0.49f, 0.75f)), ("Right", new Vector2(0.51f, 0.13f), new Vector2(0.93f, 0.75f)) })
            {
                var column = Text(recap, name, "", 21, min, max, new Color(1, 1, 1, 0.88f));
                column.alignment = TextAlignmentOptions.TopLeft;
                column.lineSpacing = 6;
            }
            var hint = Text(recap, "Hint", "Enter / Space: tiếp tục", 21, new Vector2(0.3f, 0.04f), new Vector2(0.7f, 0.1f), new Color(1, 1, 1, 0.55f));
            hint.alignment = TextAlignmentOptions.Center;
            recap.gameObject.SetActive(false);
        }

        public const string ControlsHelp =
            "Di chuyển: <color=#E8C27A>WASD</color> · Chạy: <color=#E8C27A>Shift</color>\n" +
            "Khom người: <color=#E8C27A>Ctrl / C</color>\n" +
            "Xoay camera: <color=#E8C27A>Chuột</color>\n" +
            "Quan sát: <color=#E8C27A>Giữ chuột phải</color>\n" +
            "Tương tác: <color=#E8C27A>E</color>\n" +
            "Tiếp thoại: <color=#E8C27A>Space / Click</color>\n" +
            "Chọn trả lời: <color=#E8C27A>1–4</color>\n" +
            "Sổ tay: <color=#E8C27A>J</color> · Trang Sách: <color=#E8C27A>B</color>\n" +
            "Tạm dừng: <color=#E8C27A>Esc</color>";

        private static void BuildPause(Transform parent)
        {
            var pause = Image(Rect(parent, "Pause", Vector2.zero, Vector2.one), new Color(0.02f, 0.01f, 0.02f, 0.78f)).transform;
            var panel = Panel(pause, "Panel", new Vector2(0.36f, 0.2f), new Vector2(0.64f, 0.8f), new Color(0.07f, 0.05f, 0.06f, 0.97f), 26);
            var title = Text(panel, "Title", "TẠM DỪNG", 40, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.96f), Gold);
            title.alignment = TextAlignmentOptions.Center; title.fontStyle = FontStyles.Bold;
            for (int i = 0; i < PauseMenuView.ButtonNames.Length; i++)
            {
                var button = Panel(panel, PauseMenuView.ButtonNames[i], Vector2.zero, Vector2.one, i == 0 ? Vermilion : new Color(1, 1, 1, 0.07f), 14);
                button.gameObject.AddComponent<Button>();
                Text(button, "Label", PauseMenuView.ButtonLabels[i], 26, Vector2.zero, Vector2.one, Color.white).alignment = TextAlignmentOptions.Center;
            }
            PauseMenuView.LayoutButtons(panel);
            var controls = Panel(panel, "ControlsText", new Vector2(1.04f, 0.1f), new Vector2(1.9f, 0.9f), new Color(0.07f, 0.05f, 0.06f, 0.97f), 26);
            var text = Text(controls, "Text", ControlsHelp, 24, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f), Color.white);
            text.alignment = TextAlignmentOptions.TopLeft; text.lineSpacing = 14;
            pause.gameObject.SetActive(false);
        }

        private static void BuildCards(Transform parent)
        {
            var cards = Rect(parent, "Cards", Vector2.zero, Vector2.one);

            var chapter = Image(Rect(cards, "Chapter", Vector2.zero, Vector2.one), Color.black).transform;
            chapter.gameObject.AddComponent<CanvasGroup>();
            var number = Text(chapter, "Number", "", 30, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.64f), Gold);
            number.alignment = TextAlignmentOptions.Center; number.characterSpacing = 10;
            var title = Text(chapter, "Title", "", 66, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.56f), Color.white);
            title.alignment = TextAlignmentOptions.Center; title.fontStyle = FontStyles.Bold;
            Image(Rect(chapter, "Rule", new Vector2(0.44f, 0.405f), new Vector2(0.56f, 0.408f)), Gold);
            var subtitle = Text(chapter, "Line", "THÚY KIỀU: NGHỊCH MỆNH", 22, new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.39f), new Color(1, 1, 1, 0.55f));
            subtitle.alignment = TextAlignmentOptions.Center; subtitle.characterSpacing = 4;
            chapter.gameObject.SetActive(false);

            var tutorial = Panel(cards, "Tutorial", new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.72f), new Color(0.05f, 0.04f, 0.05f, 0.95f), 26);
            Text(tutorial, "Title", "HƯỚNG DẪN", 30, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.95f), Gold).fontStyle = FontStyles.Bold;
            var tutorialBody = Text(tutorial, "Body", "", 25, new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.8f), Color.white);
            tutorialBody.alignment = TextAlignmentOptions.TopLeft; tutorialBody.lineSpacing = 12;
            Text(tutorial, "Hint", "E / Space: đã hiểu", 20, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.14f), new Color(1, 1, 1, 0.6f)).alignment = TextAlignmentOptions.Right;
            tutorial.gameObject.SetActive(false);

            var fail = Image(Rect(cards, "Fail", Vector2.zero, Vector2.one), new Color(0.25f, 0.02f, 0.02f, 0.78f)).transform;
            var headline = Text(fail, "Headline", "", 58, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.62f), new Color(1f, 0.85f, 0.8f));
            headline.alignment = TextAlignmentOptions.Center; headline.fontStyle = FontStyles.Bold;
            var detail = Text(fail, "Detail", "", 27, new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.5f), new Color(1, 1, 1, 0.8f));
            detail.alignment = TextAlignmentOptions.Center;
            Text(fail, "Hint", "Quay lại điểm lưu gần nhất...", 21, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.36f), new Color(1, 1, 1, 0.5f)).alignment = TextAlignmentOptions.Center;
            fail.gameObject.SetActive(false);

            var complete = Image(Rect(cards, "Complete", Vector2.zero, Vector2.one), new Color(0, 0, 0, 0.82f)).transform;
            var completePanel = Panel(complete, "Panel", new Vector2(0.25f, 0.16f), new Vector2(0.75f, 0.84f), new Color(0.06f, 0.05f, 0.06f, 0.97f), 26);
            var completeTitle = Text(completePanel, "Title", "", 40, new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.95f), Gold);
            completeTitle.alignment = TextAlignmentOptions.Center; completeTitle.fontStyle = FontStyles.Bold;
            var lines = Text(completePanel, "Lines", "", 25, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.78f), Color.white);
            lines.alignment = TextAlignmentOptions.TopLeft; lines.lineSpacing = 10;
            var button = Panel(completePanel, "Continue", new Vector2(0.32f, 0.06f), new Vector2(0.68f, 0.18f), Vermilion, 14);
            button.gameObject.AddComponent<Button>();
            Text(button, "Label", "Tiếp tục  <size=18><color=#FFD8C8>(Enter)</color></size>", 28, Vector2.zero, Vector2.one, Color.white).alignment = TextAlignmentOptions.Center;
            complete.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ primitives

        public static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>Top-anchored child: a band between two pixel offsets from the parent top, with side padding.</summary>
        private static void Pixels(RectTransform rect, float top, float bottom)
        {
            rect.offsetMin = new Vector2(16, bottom);
            rect.offsetMax = new Vector2(-16, top);
        }

        public static Image Image(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color, int radius)
        {
            var rect = Rect(parent, name, min, max);
            Image(rect, color);
            rect.gameObject.AddComponent<RoundedImage>().SetRadius(radius);
            return rect;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string value, float size, Vector2 min, Vector2 max, Color color)
        {
            var rect = Rect(parent, name, min, max);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font != null) text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.margin = new Vector4(6, 2, 6, 2);
            return text;
        }

        /// <summary>Finds a named descendant component; logs instead of throwing so a restyled prefab degrades gracefully.</summary>
        public static T Find<T>(Transform root, string path) where T : Component
        {
            var child = root.Find(path);
            if (child == null) { Debug.LogWarning("[StoryUI] Missing " + path, root); return null; }
            return child.GetComponent<T>();
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThuyKieu.UI
{
    /// <summary>Menu button look: hovering selects it (mouse and keyboard share one highlight), the accent bar grows and the label turns gold.</summary>
    [RequireComponent(typeof(Button))]
    public class ShellButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        public RectTransform Accent;
        public TMP_Text Label;
        public Image Background;
        public Color LabelColor = Color.white;
        public Color HighlightColor = StoryUIBuilder.Gold;
        public Color BackgroundColor = new Color(1, 1, 1, 0f);
        public Color BackgroundHighlight = new Color(1, 1, 1, 0.08f);
        public float Indent = 14f;
        public System.Action Selected;

        private bool _selected;
        private float _t;
        private Vector2 _labelMin;


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GetComponent<Button>().interactable && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData) { _selected = true; Selected?.Invoke(); }
        public void OnDeselect(BaseEventData eventData) { _selected = false; }
        private void OnDisable() { _selected = false; _t = 0; Apply(); }

        private void Update()
        {
            float target = _selected ? 1f : 0f;
            if (Mathf.Approximately(_t, target)) return;
            _t = Mathf.MoveTowards(_t, target, Time.unscaledDeltaTime * 7f);
            Apply();
        }

        private void Apply()
        {
            float e = 1f - (1f - _t) * (1f - _t);
            bool interactable = GetComponent<Button>().interactable;
            if (Accent != null) Accent.localScale = new Vector3(1f, e, 1f);
            if (Label != null)
            {
                var color = Color.Lerp(LabelColor, HighlightColor, e);
                if (!interactable) color.a *= 0.4f;
                Label.color = color;
                Label.rectTransform.offsetMin = _labelMin + new Vector2(Indent * e, 0);
            }
            if (Background != null) Background.color = Color.Lerp(BackgroundColor, BackgroundHighlight, e);
        }

        /// <summary>Call after assigning the parts: remembers the label's resting position and paints the idle state.</summary>
        public void Setup()
        {
            if (Label != null) _labelMin = Label.rectTransform.offsetMin;
            Apply();
        }

        public void Refresh() => Apply();
    }
}

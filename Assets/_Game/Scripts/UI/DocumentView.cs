using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.UI
{
    /// <summary>Parchment reader for letters and contracts found in the world.</summary>
    public class DocumentView : MonoBehaviour
    {
        private GameObject _root, _seal;
        private TMP_Text _title, _body;
        private RectTransform _paper;

        public bool FastMode { get; set; }

        private void Awake()
        {
            _root = transform.Find("Document").gameObject;
            _paper = StoryUIBuilder.Find<RectTransform>(transform, "Document/Paper");
            _seal = transform.Find("Document/Paper/Seal").gameObject;
            _title = StoryUIBuilder.Find<TMP_Text>(transform, "Document/Paper/Title");
            _body = StoryUIBuilder.Find<TMP_Text>(transform, "Document/Paper/Body");
        }

        public IEnumerator Show(string title, string body, bool sealed_)
        {
            _title.text = title;
            _body.text = body;
            _seal.SetActive(sealed_);
            _root.SetActive(true);
            // Unfold: the sheet grows from a narrow strip.
            for (float t = 0; t < 1f && !FastMode; t += Time.unscaledDeltaTime * 3.2f)
            {
                _paper.localScale = new Vector3(1f, Mathf.SmoothStep(0.05f, 1f, t), 1f);
                yield return null;
            }
            _paper.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 1.0f);
            while (!FastMode && !Confirm()) yield return null;
            _root.SetActive(false);
        }

        private static bool Confirm()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                   || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        }
    }
}

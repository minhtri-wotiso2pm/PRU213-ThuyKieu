using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.UI
{
    /// <summary>End-of-game summary (built by StoryUIBuilder.BuildRecap): the ending's tone and two columns of choices.</summary>
    public class RecapView : MonoBehaviour
    {
        private GameObject _root;
        private TMP_Text _tone, _left, _right;

        /// <summary>QA autopilot continues on its own.</summary>
        public bool FastMode { get; set; }

        private void Awake()
        {
            var recap = transform.Find("Recap");
            if (recap == null) return;
            _root = recap.gameObject;
            _tone = StoryUIBuilder.Find<TMP_Text>(recap, "Tone");
            _left = StoryUIBuilder.Find<TMP_Text>(recap, "Left");
            _right = StoryUIBuilder.Find<TMP_Text>(recap, "Right");
        }

        public IEnumerator Show(string tone, string left, string right)
        {
            _tone.text = tone;
            _left.text = left;
            _right.text = right;
            _root.SetActive(true);
            yield return new WaitForSecondsRealtime(FastMode ? ThuyKieu.Core.Story.StoryCapture.Hold : 1.2f);
            while (!FastMode && !Confirm()) yield return null;
            _root.SetActive(false);
        }

        private static bool Confirm()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return !PauseMenuView.IsPaused && ((keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                                               || (mouse != null && mouse.leftButton.wasPressedThisFrame));
        }
    }
}

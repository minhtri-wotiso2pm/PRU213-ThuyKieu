using System.Collections;
using ThuyKieu.Core.Story;
using TMPro;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Writing that appears by itself on a page in the world (the epilogue's "Cảm ơn."), stroke by stroke.</summary>
    public class RevealSequence : StorySequence
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private string _sfx = "memory";

        public override IEnumerator Run(StoryDirector director)
        {
            if (_text == null) yield break;
            _text.gameObject.SetActive(true);
            _text.ForceMeshUpdate();
            int count = _text.textInfo.characterCount;
            var color = _text.color;
            if (director.Audio != null && !string.IsNullOrEmpty(_sfx)) director.Audio.PlaySfx(_sfx, 0.7f);
            float duration = director.Autopilot ? 0f : _duration;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                _text.maxVisibleCharacters = Mathf.CeilToInt(count * Mathf.Clamp01(k * 1.4f));
                color.a = Mathf.SmoothStep(0f, 1f, k);
                _text.color = color;
                yield return null;
            }
            _text.maxVisibleCharacters = count;
            color.a = 1f;
            _text.color = color;
            yield return new WaitForSeconds(director.Autopilot ? 0f : 1.2f);
        }
    }
}

using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Short physical beat (pushing a gate, searching a drawer): a gesture, a sound, a jolt.</summary>
    public class ActionBeatSequence : StorySequence
    {
        [SerializeField] private int _gesture = 6;
        [SerializeField] private string _sfx = "lock_rattle";
        [SerializeField] private float _shake = 0.15f;
        [SerializeField] private float _duration = 1.3f;
        [SerializeField] private Transform _animatedProp;
        [SerializeField] private Vector3 _propOffset = new Vector3(0, 0, 0.25f);

        public override IEnumerator Run(StoryDirector director)
        {
            var kieu = director.FindActor("Kieu");
            if (kieu != null) kieu.SetTalking(true, _gesture);
            if (director.Audio != null && !string.IsNullOrEmpty(_sfx)) director.Audio.PlaySfx(_sfx);
            if (director.Fx != null && _shake > 0f) director.Fx.Shake(_shake, 0.4f);
            Vector3 start = _animatedProp != null ? _animatedProp.localPosition : Vector3.zero;
            float duration = director.Autopilot ? 0f : _duration;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (_animatedProp != null) _animatedProp.localPosition = start + _propOffset * Mathf.Sin(Mathf.Clamp01(t / 0.5f) * Mathf.PI);
                yield return null;
            }
            if (_animatedProp != null) _animatedProp.localPosition = start;
            if (kieu != null) kieu.SetTalking(false, 0);
        }
    }
}

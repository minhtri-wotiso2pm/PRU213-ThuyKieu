using System.Collections;
using System.Linq;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Several characters walk to their marks at once (officials dragging the father away, a witness slipping out),
    /// optionally leaving the scene when they arrive. Waits until everyone stops or the time runs out.
    /// </summary>
    public class ProcessionSequence : StorySequence
    {
        [SerializeField] private StoryActor[] _actors;
        [SerializeField] private Transform[] _marks;
        [SerializeField] private float _speed = 1.6f;
        [SerializeField] private float _timeout = 6f;
        [SerializeField] private bool _leaveAtEnd;
        [SerializeField] private string _sfx;
        [SerializeField] private float _shake;

        public override IEnumerator Run(StoryDirector director)
        {
            if (director.Audio != null && !string.IsNullOrEmpty(_sfx)) director.Audio.PlaySfx(_sfx);
            if (director.Fx != null && _shake > 0f) director.Fx.Shake(_shake, 0.5f);
            for (int i = 0; i < _actors.Length && i < _marks.Length; i++)
                if (_actors[i] != null && _actors[i].isActiveAndEnabled) _actors[i].MoveTo(_marks[i].position, _speed);
            float limit = director.Autopilot ? 0.3f : _timeout;
            for (float t = 0; t < limit; t += Time.deltaTime)
            {
                if (t > 0.5f && !_actors.Any(a => a != null && a.isActiveAndEnabled && a.IsMoving)) break;
                yield return null;
            }
            for (int i = 0; i < _actors.Length && i < _marks.Length; i++)
            {
                if (_actors[i] == null) continue;
                if (_leaveAtEnd) _actors[i].gameObject.SetActive(false);
                else _actors[i].Place(_marks[i].position, _marks[i].rotation);
            }
        }
    }
}

using System.Collections;
using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// A guest or a girl of the house who is only there to show what the house is for: sits or stands where placed,
    /// faces someone, and now and then talks for a while, each on its own clock so no two move together. Has no eyes:
    /// nothing here notices the player.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class BackgroundPatron : MonoBehaviour
    {
        [SerializeField] private bool _sits;
        [SerializeField] private Vector3 _faces;
        [SerializeField] private Vector2 _quietSeconds = new Vector2(4f, 11f);
        [SerializeField] private Vector2 _talkSeconds = new Vector2(2f, 6f);

        private IEnumerator Start()
        {
            var actor = GetComponent<StoryActor>();
            yield return new WaitForSeconds(Random.Range(0.1f, 1.2f));
            if (_faces != Vector3.zero) actor.FaceInstant(_faces);
            if (_sits) actor.Sit(true);
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(Random.Range(_quietSeconds.x, _quietSeconds.y));
                actor.SetTalking(true, Random.Range(0, 3));
                yield return new WaitForSeconds(Random.Range(_talkSeconds.x, _talkSeconds.y));
                actor.SetTalking(false, 0);
            }
        }
    }
}

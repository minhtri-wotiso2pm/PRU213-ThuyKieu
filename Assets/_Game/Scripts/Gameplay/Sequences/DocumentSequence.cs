using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>Kiều picks up a paper; the parchment reader shows its contents.</summary>
    public class DocumentSequence : StorySequence
    {
        [SerializeField] private DocumentView _view;
        [SerializeField] private string _title;
        [SerializeField, TextArea(5, 12)] private string _body;
        [SerializeField] private bool _sealed;
        [Tooltip("World prop hidden once picked up (optional).")]
        [SerializeField] private GameObject _prop;

        public override IEnumerator Run(StoryDirector director)
        {
            var kieu = director.FindActor("Kieu");
            if (kieu != null) kieu.SetTalking(true, 6);
            if (director.Audio != null) director.Audio.PlaySfx("paper");
            yield return new WaitForSeconds(director.Autopilot ? 0f : 0.9f);
            if (kieu != null) kieu.SetTalking(false, 0);
            if (_prop != null) _prop.SetActive(false);
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            _view.FastMode = director.Autopilot;
            yield return _view.Show(_title, _body, _sealed);
            if (director.Audio != null) director.Audio.PlaySfx("paper", 0.5f);
        }
    }
}

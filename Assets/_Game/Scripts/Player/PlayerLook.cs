using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Player
{
    /// <summary>
    /// Two bodies for the one player: Kiều and the modern student who wakes up inside her. The classroom scenes show
    /// the student; switching moves the movement and story animator onto the visible body, so a final student model
    /// only has to replace the <see cref="_student"/> child.
    /// </summary>
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private GameObject _kieu;
        [SerializeField] private GameObject _student;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private StoryActor _actor;

        public bool IsStudent => _student != null && _student.activeSelf;

        public void ShowStudent(bool student)
        {
            if (_kieu == null || _student == null) return;
            _kieu.SetActive(!student);
            _student.SetActive(student);
            var animator = (student ? _student : _kieu).GetComponentInChildren<Animator>();
            if (_movement != null) _movement.SetAnimator(animator);
            if (_actor != null)
            {
                _actor.SetAnimator(animator);
                // The modern student talks plainly in class; the broad story gestures belong to Kiều's world.
                _actor.StillWhileTalking = student;
            }
        }
    }
}

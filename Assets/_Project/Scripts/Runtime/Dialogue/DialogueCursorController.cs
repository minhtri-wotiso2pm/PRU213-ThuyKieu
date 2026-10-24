using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Dialogue
{
    /// <summary>
    /// Frees the mouse cursor while a conversation is open so the player can click choices,
    /// and re-locks it afterwards. Kept as its own component so the dialogue core stays
    /// independent of the camera: delete this object and dialogue still works via keyboard.
    /// Lives on the persistent systems object, so the camera is looked up per conversation:
    /// every scene brings its own camera.
    /// </summary>
    public class DialogueCursorController : MonoBehaviour
    {
        private ThirdPersonCameraController cameraController;
        private DialogueManager manager;

        private void Start()
        {
            manager = DialogueManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.DialogueStarted += HandleStarted;
            manager.DialogueEnded += HandleEnded;
        }

        private void OnDestroy()
        {
            if (manager == null)
            {
                return;
            }

            manager.DialogueStarted -= HandleStarted;
            manager.DialogueEnded -= HandleEnded;
        }

        private void HandleStarted(DialogueData data)
        {
            ThirdPersonCameraController controller = ResolveCamera();
            if (controller != null)
            {
                controller.SetCursorLocked(false);
            }
        }

        private void HandleEnded()
        {
            ThirdPersonCameraController controller = ResolveCamera();
            if (controller != null)
            {
                controller.SetCursorLocked(true);
            }
        }

        private ThirdPersonCameraController ResolveCamera()
        {
            if (cameraController == null)
            {
                cameraController = FindAnyObjectByType<ThirdPersonCameraController>();
            }

            return cameraController;
        }
    }
}

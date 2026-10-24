using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ThuyKieu.Cutscene
{
    /// <summary>
    /// Opening cutscene: Kieu wakes up (first person, eyes opening, sitting up, looking around),
    /// then the camera cuts to third person and blends into the gameplay orbit camera.
    /// Owns only the cutscene: it disables player scripts while playing and hands them back at the end.
    /// Space skips.
    /// </summary>
    public class WakeUpCutscene : MonoBehaviour
    {
        [Header("Actors")]
        [SerializeField] private Transform player;
        [Tooltip("Kieu's model. Hidden during the first person part so the camera never sees inside it.")]
        [SerializeField] private GameObject playerVisual;
        [SerializeField] private Camera cutsceneCamera;
        [SerializeField] private ThirdPersonCameraController cameraController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerInteraction playerInteraction;

        [Header("Shots")]
        [Tooltip("Eye position lying on the pillow. Forward should look at the ceiling.")]
        [SerializeField] private Transform povLying;
        [Tooltip("Eye position sitting on the edge of the bed. Forward = where she looks after sitting up.")]
        [SerializeField] private Transform povSitting;
        [Tooltip("Where Kieu stands when the camera switches to third person.")]
        [SerializeField] private Transform standPoint;
        [Tooltip("First third person framing, before the camera blends into gameplay.")]
        [SerializeField] private Transform thirdPersonShot;

        [Header("Timing (seconds)")]
        [SerializeField] private bool playOnStart = true;
        [Tooltip("Play only the first time the scene loads in a session (not when coming back from outside).")]
        [SerializeField] private bool playOncePerSession = true;
        [SerializeField] private float closedEyesHold = 1.5f;
        [SerializeField] private float sitUpDuration = 2.6f;
        [SerializeField] private float lookAroundDuration = 4.5f;
        [SerializeField] private float thirdPersonHold = 2.8f;
        [SerializeField] private float blendToGameplay = 1.6f;

        [Header("Look")]
        [SerializeField] private float povFieldOfView = 68f;
        [Tooltip("Orbit yaw when control is handed back. Kieu turns to face it during the blend; pick one that keeps the camera inside the room.")]
        [SerializeField] private float gameplayYaw = 140f;
        [Tooltip("Gameplay orbit pitch used when control is handed back.")]
        [SerializeField] private float gameplayPitch = 15f;

        [Header("Text (from Doc/ThuyKieuProject_4_Chuong_v2.ink, chapter_1)")]
        [SerializeField] private CinematicTextUI text;
        [SerializeField] private string presenterLine = "PRU213  ·  TRÌNH BÀY";
        [SerializeField] private string gameTitle = "Thúy Kiều";
        [SerializeField] private string gameTagline = "Kiếm tiền có gì khó nhỉ?";
        [SerializeField] private string chapterLabel = "Chương I";
        [SerializeField] private string chapterTitle = "Bốn Trăm Lạng";
        [SerializeField] private string narratorName = "";
        [SerializeField] private Color narratorColor = new Color(0.86f, 0.72f, 0.45f);
        [TextArea] [SerializeField] private string narratorLine1 = "Mưa quất lên mái ngói Vương gia. Trong sân, dấu chân quan sai còn lẫn với bùn đất.";
        [TextArea] [SerializeField] private string narratorLine2 = "Vương Ông và Vương Quan vừa bị áp giải đi vì một vụ án mà cả nhà đều biết là oan.";
        [SerializeField] private string kieuName = "Thúy Kiều";
        [SerializeField] private Color kieuColor = new Color(0.93f, 0.55f, 0.6f);
        [TextArea] [SerializeField] private string kieuLine = "Cha và em còn đang chờ... Mình sẽ tìm cách.";
        [SerializeField] private string locationName = "Vương gia";
        [SerializeField] private string locationDetail = "Khuê phòng Thúy Kiều  ·  sau đêm quan sai đến";

        private static bool playedThisSession;

        /// <summary>The project disables domain reload on Play, so statics must be reset by hand.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            playedThisSession = false;
        }

        private RectTransform lidTop;
        private RectTransform lidBottom;
        private Image fader;
        private GameObject overlayRoot;
        private Volume blurVolume;
        private UniversalAdditionalCameraData cameraData;
        private bool hadPostProcessing;
        private float defaultFieldOfView;
        private Coroutine routine;

        public bool IsPlaying { get; private set; }

        private void Start()
        {
            if (playOnStart && !(playOncePerSession && playedThisSession))
            {
                Play();
            }
        }

        private void Update()
        {
            if (IsPlaying && InputReader.SkipPressedThisFrame)
            {
                StopCoroutine(routine);
                StartCoroutine(SkipRoutine());
            }
        }

        public void Play()
        {
            if (IsPlaying)
            {
                return;
            }

            playedThisSession = true;
            routine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            Begin();

            // 0. Opening titles over black (eyes still closed).
            SetLids(0f);
            SetBlur(1f);
            if (text != null)
            {
                yield return text.ShowGameTitle(presenterLine, gameTitle, gameTagline, 6f);
                yield return Hold(0.6f, LyingBreath);
                yield return text.ShowChapterCard(chapterLabel, chapterTitle, 4.2f);
            }

            // 1. Eyes closed, darkness, a blurred ceiling slowly breathing in and out.
            yield return Hold(closedEyesHold, LyingBreath);

            // 2. Heavy eyelids: two failed attempts, then fully open while the blur clears.
            Say(narratorName, narratorColor, narratorLine1, 2.6f, false);
            yield return AnimateLids(0f, 0.35f, 0.7f, 1f, 0.85f);
            yield return AnimateLids(0.35f, 0f, 0.25f, 0.85f, 0.85f);
            yield return Hold(0.35f, LyingBreath);
            yield return AnimateLids(0f, 0.7f, 0.9f, 0.85f, 0.55f);
            yield return AnimateLids(0.7f, 0.15f, 0.2f, 0.55f, 0.55f);
            yield return AnimateLids(0.15f, 1f, 1.3f, 0.55f, 0.25f);
            yield return Hold(0.6f, LyingBreath);

            // 3. Sit up: eye point swings from the pillow to the edge of the bed.
            Say(narratorName, narratorColor, narratorLine2, 2.4f, false);
            yield return SitUp();

            // 4. Look around the room, still first person.
            yield return LookAround();

            // 5. Cut to third person: blink to black, show Kieu standing next to the bed.
            yield return Fade(0f, 1f, 0.45f);
            EnterThirdPerson();
            if (text != null)
            {
                text.StartCoroutine(text.Letterbox(1f, 0.01f));
                text.StartCoroutine(text.ShowLocation(locationName, locationDetail, thirdPersonHold + 0.8f));
            }

            yield return Fade(1f, 0f, 0.6f);
            Say(kieuName, kieuColor, kieuLine, 1.6f, true);
            yield return PushIn(thirdPersonHold);

            // 6. Blend into the orbit camera, bars slide away, control comes back.
            if (text != null)
            {
                text.StartCoroutine(text.Letterbox(0f, blendToGameplay));
            }

            yield return BlendToGameplay();
            Finish();
        }

        private IEnumerator SkipRoutine()
        {
            IsPlaying = false;
            yield return Fade(fader.color.a, 1f, 0.25f);
            EnterThirdPerson();
            SnapToGameplay();
            yield return Fade(1f, 0f, 0.4f);
            Finish();
        }

        // ---------------------------------------------------------------- setup / teardown

        private void Begin()
        {
            IsPlaying = true;
            ResolveReferences();
            BuildOverlay();
            BuildBlurVolume();

            SetPlayerControl(false);
            if (playerVisual != null)
            {
                playerVisual.SetActive(false);
            }

            defaultFieldOfView = cutsceneCamera.fieldOfView;
            cutsceneCamera.fieldOfView = povFieldOfView;
            cutsceneCamera.transform.SetPositionAndRotation(povLying.position, povLying.rotation);
        }

        private void EnterThirdPerson()
        {
            SetLids(1f);
            SetBlur(0f);
            cutsceneCamera.fieldOfView = defaultFieldOfView;

            if (playerVisual != null)
            {
                playerVisual.SetActive(true);
            }

            Teleport(player, standPoint);
            cutsceneCamera.transform.SetPositionAndRotation(thirdPersonShot.position, thirdPersonShot.rotation);
        }

        private void Say(string speaker, Color color, string line, float holdAfter, bool innerThought)
        {
            if (text != null && !string.IsNullOrEmpty(line))
            {
                text.Say(speaker, color, line, holdAfter, innerThought);
            }
        }

        private void Finish()
        {
            IsPlaying = false;
            if (text != null)
            {
                text.HideAll();
            }

            if (cameraData != null)
            {
                cameraData.renderPostProcessing = hadPostProcessing;
            }

            if (blurVolume != null)
            {
                Destroy(blurVolume.profile);
                Destroy(blurVolume.gameObject);
            }

            if (overlayRoot != null)
            {
                Destroy(overlayRoot);
            }

            cameraController.SetOrientation(gameplayYaw, gameplayPitch);
            SetPlayerControl(true);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                player = tagged != null ? tagged.transform : null;
            }

            if (cutsceneCamera == null) cutsceneCamera = Camera.main;
            if (cameraController == null) cameraController = cutsceneCamera.GetComponent<ThirdPersonCameraController>();
            if (playerMovement == null && player != null) playerMovement = player.GetComponent<PlayerMovement>();
            if (playerInteraction == null && player != null) playerInteraction = player.GetComponent<PlayerInteraction>();
        }

        private void SetPlayerControl(bool enabled)
        {
            if (cameraController != null) cameraController.enabled = enabled;
            if (playerMovement != null) playerMovement.enabled = enabled;
            if (playerInteraction != null) playerInteraction.enabled = enabled;
        }

        /// <summary>CharacterController overrides transform writes, so disable it while teleporting.</summary>
        private static void Teleport(Transform target, Transform destination)
        {
            CharacterController controller = target.GetComponent<CharacterController>();
            bool hadController = controller != null && controller.enabled;
            if (hadController) controller.enabled = false;
            target.SetPositionAndRotation(destination.position, destination.rotation);
            if (hadController) controller.enabled = true;
        }

        // ---------------------------------------------------------------- shots

        private void LyingBreath(float time)
        {
            float breath = Mathf.Sin(time * 1.6f) * 0.012f;
            Quaternion drift = Quaternion.Euler(Mathf.Sin(time * 0.7f) * 1.2f, Mathf.Sin(time * 0.45f) * 1.5f, 0f);
            cutsceneCamera.transform.SetPositionAndRotation(
                povLying.position + povLying.up * breath,
                povLying.rotation * drift);
        }

        private IEnumerator SitUp()
        {
            Vector3 startPos = cutsceneCamera.transform.position;
            Quaternion startRot = cutsceneCamera.transform.rotation;

            for (float t = 0f; t < sitUpDuration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / sitUpDuration);
                // Arc: the head rises first, then swings forward over the bed edge.
                Vector3 pos = Vector3.Lerp(startPos, povSitting.position, k);
                pos += Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.12f;
                Quaternion rot = Quaternion.Slerp(startRot, povSitting.rotation, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / sitUpDuration * 1.15f)));
                // A slight tilt: she pushes up on one arm.
                rot *= Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI) * 6f);

                cutsceneCamera.transform.SetPositionAndRotation(pos, rot);
                SetBlur(Mathf.Lerp(0.25f, 0f, k));
                yield return null;
            }

            cutsceneCamera.transform.SetPositionAndRotation(povSitting.position, povSitting.rotation);
        }

        private IEnumerator LookAround()
        {
            // (time fraction, yaw offset, pitch offset): left toward the door, then right to the altar, back to centre.
            Vector3[] keys =
            {
                new Vector3(0.00f, 0f, 0f),
                new Vector3(0.28f, -40f, 4f),
                new Vector3(0.42f, -40f, 2f),
                new Vector3(0.70f, 32f, -3f),
                new Vector3(0.84f, 32f, -2f),
                new Vector3(1.00f, 0f, 0f),
            };

            for (float t = 0f; t < lookAroundDuration; t += Time.deltaTime)
            {
                Vector2 offset = SampleKeys(keys, t / lookAroundDuration);
                float bob = Mathf.Sin(t * 1.8f) * 0.008f;
                cutsceneCamera.transform.SetPositionAndRotation(
                    povSitting.position + Vector3.up * bob,
                    Quaternion.Euler(0f, offset.x, 0f) * povSitting.rotation * Quaternion.Euler(offset.y, 0f, 0f));
                yield return null;
            }
        }

        private IEnumerator PushIn(float duration)
        {
            Vector3 startPos = thirdPersonShot.position;
            Vector3 endPos = Vector3.Lerp(thirdPersonShot.position, player.position + Vector3.up * 1.3f, 0.18f);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                cutsceneCamera.transform.position = Vector3.Lerp(startPos, endPos, k);
                yield return null;
            }
        }

        private IEnumerator BlendToGameplay()
        {
            Vector3 startPos = cutsceneCamera.transform.position;
            Quaternion startRot = cutsceneCamera.transform.rotation;
            Pose target = cameraController.GetOrbitPose(gameplayYaw, gameplayPitch);
            Quaternion playerStart = player.rotation;
            Quaternion playerEnd = Quaternion.Euler(0f, gameplayYaw, 0f);

            for (float t = 0f; t < blendToGameplay; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / blendToGameplay);
                cutsceneCamera.transform.SetPositionAndRotation(
                    Vector3.Lerp(startPos, target.position, k),
                    Quaternion.Slerp(startRot, target.rotation, k));
                player.rotation = Quaternion.Slerp(playerStart, playerEnd, k);
                yield return null;
            }

            cutsceneCamera.transform.SetPositionAndRotation(target.position, target.rotation);
            player.rotation = playerEnd;
        }

        private void SnapToGameplay()
        {
            player.rotation = Quaternion.Euler(0f, gameplayYaw, 0f);
            Pose target = cameraController.GetOrbitPose(gameplayYaw, gameplayPitch);
            cutsceneCamera.transform.SetPositionAndRotation(target.position, target.rotation);
        }

        private static Vector2 SampleKeys(Vector3[] keys, float time)
        {
            for (int i = 1; i < keys.Length; i++)
            {
                if (time <= keys[i].x)
                {
                    float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(keys[i - 1].x, keys[i].x, time));
                    return new Vector2(Mathf.Lerp(keys[i - 1].y, keys[i].y, k), Mathf.Lerp(keys[i - 1].z, keys[i].z, k));
                }
            }

            Vector3 last = keys[keys.Length - 1];
            return new Vector2(last.y, last.z);
        }

        // ---------------------------------------------------------------- eyelids, fade, blur

        private IEnumerator Hold(float duration, System.Action<float> shot)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                shot(Time.time);
                yield return null;
            }
        }

        private IEnumerator AnimateLids(float from, float to, float duration, float blurFrom, float blurTo)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                SetLids(Mathf.Lerp(from, to, k));
                SetBlur(Mathf.Lerp(blurFrom, blurTo, k));
                LyingBreath(Time.time);
                yield return null;
            }

            SetLids(to);
            SetBlur(blurTo);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                SetFade(Mathf.Lerp(from, to, t / duration));
                yield return null;
            }

            SetFade(to);
        }

        /// <summary>0 = eyes closed, 1 = fully open. Lids overshoot past the screen edge so they fully clear.</summary>
        private void SetLids(float open)
        {
            float h = Mathf.Lerp(0.5f, -0.08f, open);
            lidTop.anchorMin = new Vector2(-0.1f, 1f - h);
            lidBottom.anchorMax = new Vector2(1.1f, h);
        }

        private void SetFade(float alpha)
        {
            fader.color = new Color(0f, 0f, 0f, alpha);
        }

        private void SetBlur(float weight)
        {
            if (blurVolume != null)
            {
                blurVolume.weight = weight;
            }
        }

        private void BuildOverlay()
        {
            overlayRoot = new GameObject("WakeUpCutscene_Overlay");
            overlayRoot.transform.SetParent(transform, false);
            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            lidTop = CreateBlack("LidTop", new Vector2(-0.1f, 0.5f), new Vector2(1.1f, 1.2f)).rectTransform;
            lidBottom = CreateBlack("LidBottom", new Vector2(-0.1f, -0.2f), new Vector2(1.1f, 0.5f)).rectTransform;
            fader = CreateBlack("Fader", Vector2.zero, Vector2.one);
            SetFade(0f);
        }

        private Image CreateBlack(string objectName, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(overlayRoot.transform, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Blurry, dark-edged "just woke up" look. Built at runtime so no profile asset is needed.</summary>
        private void BuildBlurVolume()
        {
            cameraData = cutsceneCamera.GetUniversalAdditionalCameraData();
            hadPostProcessing = cameraData.renderPostProcessing;
            cameraData.renderPostProcessing = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

            DepthOfField dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(0f);
            dof.gaussianEnd.Override(0.6f);
            dof.gaussianMaxRadius.Override(1.5f);
            dof.highQualitySampling.Override(true);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.55f);
            vignette.smoothness.Override(0.8f);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.6f);
            color.saturation.Override(-35f);

            GameObject go = new GameObject("WakeUpCutscene_Blur");
            go.transform.SetParent(transform, false);
            blurVolume = go.AddComponent<Volume>();
            blurVolume.isGlobal = true;
            blurVolume.priority = 100f;
            blurVolume.profile = profile;
        }
    }
}

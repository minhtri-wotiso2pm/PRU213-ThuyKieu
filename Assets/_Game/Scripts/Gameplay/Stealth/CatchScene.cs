using System;
using System.Collections;
using System.IO;
using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// The moment a watcher actually lays hands on Kiều, in first person, between a valid catch (<see cref="Watchman.Caught"/>:
    /// close enough, nothing between them, no wall or shut door on the way) and the chapter's own failure screen and
    /// checkpoint restore. Being seen starts a chase; only this ends one.
    ///
    /// Three ways it plays, chosen from where he really is: <see cref="Kind.Front"/> (he steps in, both hands take her by
    /// the collar, his face in the lantern light), <see cref="Kind.Rear"/> (a hand on her shoulder from behind, the view
    /// half turned towards him by the pull, never a 180° snap) and <see cref="Kind.FromHiding"/>, opened the way the place
    /// opens (<see cref="HideKind"/>): wardrobe doors pulled wide, a screen shoved aside, a curtain swept back, or low
    /// cover reached over; then she is dragged out. Two endings: <see cref="Ending.Drag"/>, she is hauled away (Chapter 3)
    /// and <see cref="Ending.Shove"/>, forced down (Chapter 4).
    ///
    /// The body moves through <see cref="GrabPose"/> on the watcher's own rig and animator (there is no grab clip). The
    /// force reads in order: the hands arrive, the fingers close on the cloth at the moment of contact, his shoulders haul
    /// back, and her view is yanked with them. Everything it takes over (camera, FOV, time scale, actors, watchers, audio,
    /// hiding-place props) is handed back before the chapter's failure runs, and also if the scene is torn down mid-catch.
    /// The screen is left black for the failure card.
    /// </summary>
    [DefaultExecutionOrder(2000)]
    public class CatchScene : MonoBehaviour
    {
        public enum Kind { Front, Rear, FromHiding }
        public enum Ending { Drag, Shove }

        [SerializeField] private ThirdPersonCameraController _cameraController;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private ScreenFx _fx;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private Watchman[] _watchers;
        [SerializeField] private Ending _ending = Ending.Shove;
        [SerializeField] private Color _faceLight = new Color(1f, 0.6f, 0.36f);
        [Tooltip("A side key on his face, set below the hat brim: enough to read eyes and mouth, never a flat wash.")]
        [SerializeField] private float _faceLightIntensity = 0.95f;

        private const int Walls = 1 << 8;
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        /// <summary>A catch is playing: no second watcher may start one, the chapter waits for it.</summary>
        public static bool Playing { get; private set; }
        /// <summary>A catch has just started (in the same frame): sounds that must not play over it can stop at once.</summary>
        public static event Action Started;
        public Kind LastKind { get; private set; }
        public HideKind LastHide { get; private set; }
        /// <summary>The slowest frame while the last catch played (seconds): the first catch must not stutter.</summary>
        public float LastWorstFrame { get; private set; }
        /// <summary>How many catches have played since the game started (QA: two watchers at once must make one).</summary>
        public static int Count { get; private set; }
        /// <summary>QA: when set (with -autopilotCatchClips), the next catch is recorded frame by frame under this name.</summary>
        public static string ClipName { get; set; }

        // Play mode keeps statics between sessions in the Editor (domain reload is off): start every session clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Playing = false; Count = 0; ClipName = null; }

        private Camera _camera;
        private Light _light;
        private string _captureFolder;
        private int _captureFrame;

        // What the playing catch has taken over, so Cleanup can hand it all back from anywhere.
        private bool _owns;
        private Watchman _catcher;
        private GrabPose _pose;
        private StoryActor _actor;
        private Animator _animator;
        private NavMeshAgent _agent;
        private bool _agentWas;
        private float _fov, _timeScale;
        private Transform _prop, _propParent, _hinge, _carried;
        private Vector3 _propPosition, _carriedAt;
        private Quaternion _propRotation;

        private void Awake()
        {
            _camera = _cameraController != null ? _cameraController.GetComponent<Camera>() : Camera.main;
            Prewarm();
        }

        /// <summary>
        /// Everything a catch needs is made before the first one, so the first one does not stall: the face light and the
        /// pose layer on every watcher (disabled); the sounds are played once, silent, in Start.
        /// </summary>
        private void Prewarm()
        {
            var go = new GameObject("CatchFaceLight");
            go.transform.SetParent(transform, false);
            _light = go.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = _faceLight;
            _light.range = 1.8f;
            _light.intensity = 0f;
            _light.shadows = LightShadows.None;
            _light.enabled = false;
            if (_watchers != null)
                foreach (var watcher in _watchers)
                    if (watcher != null && watcher.Actor != null && watcher.Actor.GetComponent<GrabPose>() == null)
                        watcher.Actor.gameObject.AddComponent<GrabPose>().enabled = false;
        }

        // Sounds are warmed once the audio system has set itself up (its Awake runs in an order of its own).
        private void Start()
        {
            if (_audio != null)
                foreach (string id in new[] { "cloth", "blow", "breath_in", "whoosh", "stumble", "sting_low", "door_fast", "slide_fast", "step_wood", "paper", "step" })
                    _audio.PlaySfx(id, 0f);
        }

        // Torn down mid-catch (scene change, quitting): give everything back; the chapter's failure is not run.
        private void OnDisable() { if (_owns) Cleanup(true); }

        /// <summary>Stops a playing catch where it is and hands everything back, without the failure (teardown, QA).</summary>
        public void Abort()
        {
            if (!_owns) return;
            StopAllCoroutines();
            Cleanup(true);
        }

        /// <summary>
        /// Starts the catch if it can be shown honestly: he is within reach (a catch from across the yard, which a test may
        /// force, is not dressed up as a grab). Whether the catch is valid at all is the watcher's call (sight, reach, no
        /// wall or shut door on the way) and is not judged again here. Returns false when it does not play; the
        /// caller then fails the chapter directly, unless another catch is already playing.
        /// </summary>
        public bool TryPlay(Watchman catcher, string speakerId, string line, Action done)
        {
            if (Playing || catcher == null || catcher.Actor == null || _camera == null || _player == null) return false;
            var pose = catcher.Actor.GetComponent<GrabPose>();
            if (pose == null) pose = catcher.Actor.gameObject.AddComponent<GrabPose>();
            if (!pose.Ready) return false;
            // From her body, not the camera: right after a teleport (checkpoint, story) the camera may still be catching up.
            Vector3 flat = _player.transform.position - catcher.transform.position;
            flat.y = 0f;
            var hide = HideSpot.Current;
            float reach = hide != null ? 2.2f : 2.0f;
            if (flat.magnitude > reach)
            {
                StoryAutopilot.Note($"CATCH not staged: {catcher.name} {flat.magnitude:F1} m away (plain failure) · {catcher.State} at {catcher.transform.position} · her at {_player.transform.position} · hidden in {(hide != null ? hide.name : "nothing")}");
                return false;
            }
            StartCoroutine(Run(catcher, pose, hide, speakerId, line, done));
            return true;
        }

        private IEnumerator Run(Watchman catcher, GrabPose pose, HideSpot hide, string speakerId, string line, Action done)
        {
            Playing = true;
            Started?.Invoke();
            _owns = true;
            Count++;
            LastWorstFrame = 0f;
            _catcher = catcher;
            _pose = pose;
            _actor = catcher.Actor;
            _animator = _actor.GetComponentInChildren<Animator>();
            _agent = _actor.GetComponent<NavMeshAgent>();
            _agentWas = _agent != null && _agent.enabled;
            // Real time for the scene, whatever pace a QA run keeps.
            _timeScale = Time.timeScale;
            Time.timeScale = 1f;
            var cam = _camera.transform;
            _fov = _camera.fieldOfView;

            // 0 s: the world stops for her. Every watcher holds still, his walking and steering are handed to this scene.
            if (_watchers != null) foreach (var watcher in _watchers) if (watcher != null) watcher.Pause(true);
            _player.ControlLocked = true;
            _cameraController.enabled = false;
            _actor.Stop();
            if (_agent != null) _agent.enabled = false;
            _actor.enabled = false;
            if (_audio != null) { _audio.Duck(0.65f); _audio.DuckAmbience(0.75f); }

            Vector3 eye = cam.position;
            Vector3 toHer = eye - _actor.transform.position;
            toHer.y = 0f;
            float distance = toHer.magnitude;
            toHer = distance > 0.01f ? toHer / distance : _actor.transform.forward;
            Vector3 viewFlat = cam.forward;
            viewFlat.y = 0f;
            viewFlat.Normalize();
            float angle = Vector3.Angle(viewFlat, -toHer);
            var kind = hide != null ? Kind.FromHiding : angle > 100f ? Kind.Rear : Kind.Front;
            var hideKind = hide != null ? hide.Kind : HideKind.Auto;
            LastKind = kind;
            LastHide = hideKind;
            bool low = hideKind == HideKind.Low;
            float floor = _player.transform.position.y;
            float eyeAbove = eye.y - floor;
            pose.LeanDegrees = 18f + Mathf.Clamp01((1.5f - eyeAbove) / 0.6f) * 26f + (low ? 12f : 0f);
            pose.enabled = true;
            pose.Look = pose.Lean = pose.Right = pose.Left = pose.Grip = 0f;

            // Where his body ends up: close enough to hold her (0.6 m), never through a wall or cover.
            Vector3 start = _actor.transform.position;
            Vector3 stand = start;
            if (kind != Kind.FromHiding)
            {
                float step = Mathf.Max(0f, distance - 0.62f);
                if (Physics.SphereCast(start + Vector3.up * 1.0f, 0.25f, toHer, out var hit, step, Walls, QueryTriggerInteraction.Ignore))
                    step = Mathf.Max(0f, hit.distance - 0.05f);
                stand = start + toHer * step;
            }
            var facing = Quaternion.LookRotation(toHer, Vector3.up);
            var head = _animator != null && _animator.isHuman ? _animator.GetBoneTransform(HumanBodyBones.Head) : null;
            Vector3 Face() => head != null ? head.position : _actor.transform.position + Vector3.up * 1.6f;

            // The hiding place, if any: what moves when he finds her, and how to put it back.
            _prop = hideKind == HideKind.Wardrobe || hideKind == HideKind.Screen ? hide.ShakeVisual : null;
            bool doors = _prop != null && hideKind == HideKind.Wardrobe;
            _propParent = _prop != null ? _prop.parent : null;
            _propPosition = _prop != null ? _prop.localPosition : Vector3.zero;
            _propRotation = _prop != null ? _prop.localRotation : Quaternion.identity;
            if (doors)
            {
                _hinge = new GameObject("CatchHinge").transform;
                _hinge.SetParent(_propParent, false);
                _hinge.localPosition = new Vector3(-0.54f, 0f, 0.36f);
                _hinge.localRotation = Quaternion.identity;
                _prop.SetParent(_hinge, true);
            }
            Vector3 across = Vector3.Cross(Vector3.up, toHer).normalized; // his right

            // Light on his face from her side. His own lantern (carried at his side) goes down to his left hip while his
            // hands are on her, out of the middle of the picture.
            _light.enabled = true;
            _carried = null;
            foreach (Transform child in _actor.transform)
                if (child.GetComponentInChildren<Light>(true) != null) { _carried = child; _carriedAt = child.localPosition; break; }

            float reachAt = kind == Kind.FromHiding ? 0.62f : 0.15f;
            float contactAt = kind == Kind.FromHiding ? 0.86f : 0.4f;
            float pullEnd = contactAt + 0.68f;
            float end = pullEnd + 0.62f;
            float side = Vector3.Dot(Vector3.Cross(Vector3.up, viewFlat), -toHer) >= 0f ? 1f : -1f;
            Vector3 camStart = cam.position;
            Quaternion camStartRotation = cam.rotation;
            bool contact = false, gasp = false, barked = false, shoved = false, revealed = false, stepped = false;
            string clip = ClipName;
            ClipName = null;
            string clipFolder = clip != null ? ClipFolder(clip) : null;
            if (clipFolder != null) { Time.captureFramerate = 30; _captureFrame = 0; _captureFolder = clipFolder; }

            for (float t = 0f; t < end; t += Time.deltaTime)
            {
                if (_actor == null) break; // he was destroyed under us: hand back what is left and fail as usual
                LastWorstFrame = Mathf.Max(LastWorstFrame, Time.unscaledDeltaTime);
                float lunge = Smooth(Mathf.InverseLerp(reachAt, contactAt, t));
                // The haul starts hard (most of it in the first third) and eases: that is what reads as force.
                float pullK = Mathf.InverseLerp(contactAt, pullEnd, t);
                float pull = 1f - (1f - pullK) * (1f - pullK) * (1f - pullK);
                float fall = Smooth(Mathf.InverseLerp(pullEnd, end, t));

                // His body: steps in (the walk cycle carries the feet), leans, and takes a short step back with her.
                Vector3 back = -toHer * 0.16f * pull;
                _actor.transform.SetPositionAndRotation(Vector3.Lerp(start, stand, lunge) + back,
                    Quaternion.Slerp(_actor.transform.rotation, facing, 1f - Mathf.Exp(-14f * Time.deltaTime)));
                if (_animator != null) _animator.SetFloat(SpeedId, kind == Kind.FromHiding ? 0f : (t > reachAt && t < contactAt ? 0.7f : 0f), 0.06f, Time.deltaTime);
                if (!stepped && t >= reachAt + 0.05f && kind != Kind.FromHiding)
                {
                    stepped = true;
                    if (_audio != null) _audio.PlayAt("step_wood", _actor.transform.position, 0.9f, 8f, 0.9f);
                }

                // The hiding place opens the way it opens. Wardrobe: the doors swing on their hinge (his left hand pulls
                // them). Screen: shoved aside and turned. Curtain: his left hand sweeps it back. Low cover: he leans over.
                if (kind == Kind.FromHiding)
                {
                    float open = Smooth(Mathf.InverseLerp(0.22f, 0.6f, t));
                    if (!revealed && t >= 0.22f)
                    {
                        revealed = true;
                        string sound = doors ? "door_fast" : _prop != null ? "slide_fast" : !string.IsNullOrEmpty(hide.SoundId) && hide.SoundId != "door" ? hide.SoundId : "cloth";
                        if (_audio != null) { _audio.PlayAt(sound, hide.transform.position + Vector3.up, 1f, 10f); _audio.PlayAt("cloth", hide.transform.position + Vector3.up, 0.7f, 8f); }
                        if (_fx != null) _fx.Shake(0.25f, 0.2f);
                    }
                    if (doors) _hinge.localRotation = Quaternion.Euler(0f, -55f * open, 0f);
                    else if (_prop != null)
                    {
                        _prop.localPosition = _propPosition + _propParent.InverseTransformVector(across * 0.35f * open);
                        _prop.localRotation = Quaternion.Euler(0f, 30f * open, 0f) * _propRotation;
                    }
                    if (t < contactAt)
                    {
                        if (doors) { pose.LeftTarget = _prop.position + _prop.up * 1.0f + _prop.right * 0.3f; pose.Left = Smooth(Mathf.InverseLerp(0.05f, 0.25f, t)); }
                        else if (hideKind == HideKind.Curtain)
                        {
                            // The sweep: from his left across her, at chest height, where the curtain hangs.
                            Vector3 plane = hide.transform.position + Vector3.up * 0.4f - toHer * 0.1f;
                            pose.LeftTarget = plane - across * Mathf.Lerp(0.45f, -0.35f, open);
                            pose.Left = Smooth(Mathf.InverseLerp(0.05f, 0.25f, t));
                        }
                    }
                }

                // Hands: each to the edge of her collar on its own side (his right hand to his right; the arms never cross).
                // The fingers close on the cloth in the instant of contact, a little past a plain grip.
                Vector3 eyeNow = cam.position;
                Vector3 collar = eyeNow + Vector3.down * 0.26f + toHer * 0.02f;
                bool sweeping = kind == Kind.FromHiding && t < contactAt && (doors || hideKind == HideKind.Curtain);
                pose.RightTarget = collar + across * 0.16f;
                if (!sweeping) pose.LeftTarget = collar - across * 0.16f;
                pose.Right = lunge;
                if (!sweeping) pose.Left = kind == Kind.Rear ? 0.35f * pull : Smooth(Mathf.InverseLerp(reachAt + 0.08f, contactAt + 0.06f, t));
                pose.Grip = Smooth(Mathf.InverseLerp(contactAt - 0.08f, contactAt + 0.04f, t)) * 1.12f;
                // Leaning in to reach, then straightening as he hauls: the shoulders and back do the pulling.
                float reachLean = Mathf.Max(lunge, kind == Kind.FromHiding ? Smooth(Mathf.InverseLerp(0.3f, 0.7f, t)) : 0f);
                pose.Lean = reachLean * (1f - 0.45f * pull) * (1f - 0.3f * fall);
                pose.Look = Smooth(Mathf.InverseLerp(0f, 0.25f, t));
                pose.LookTarget = eyeNow;

                // Her view. Front: the eyes go to his face. Rear: the pull turns her two thirds of the way round, over a
                // third of a second. From hiding: dragged out of the opening towards him (up and out from low cover).
                Vector3 faceNow = Face();
                Vector3 camPosition = camStart;
                Quaternion want = Quaternion.LookRotation(faceNow - eyeNow, Vector3.up);
                Quaternion look = kind == Kind.Rear
                    ? Quaternion.Slerp(camStartRotation, want, 0.68f * Smooth(Mathf.InverseLerp(contactAt - 0.05f, contactAt + 0.3f, t)) + 0.3f * pull)
                    : Quaternion.Slerp(camStartRotation, want, Smooth(Mathf.InverseLerp(0.02f, kind == Kind.FromHiding ? 0.55f : 0.32f, t)));
                if (t >= contactAt)
                {
                    // Pulled to him: towards his face, no closer than 0.42 m, never into a wall; a jolt on the grab itself.
                    Vector3 toFace = faceNow - camStart;
                    Vector3 dir = toFace.normalized;
                    float want2 = Mathf.Max(0f, toFace.magnitude - 0.42f) * (kind == Kind.FromHiding ? 1f : 0.55f);
                    float jolt = 0.05f * Mathf.Exp(-(t - contactAt) * 22f);
                    camPosition = camStart + dir * SafeMove(camStart, dir, want2 * pull + jolt);
                    if (low) camPosition += Vector3.up * SafeMove(camPosition, Vector3.up, 0.3f) * pull;
                }
                // The end: hauled away along his turn (Chapter 3) or forced down, off balance (Chapter 4).
                if (_ending == Ending.Drag)
                {
                    Vector3 away = Quaternion.AngleAxis(70f * side, Vector3.up) * -toHer;
                    camPosition += away * SafeMove(camPosition, away, 0.45f) * fall;
                    look = Quaternion.AngleAxis(38f * side * fall, Vector3.up) * look * Quaternion.Euler(18f * fall, 0f, -10f * side * fall);
                }
                else
                {
                    camPosition += Vector3.down * SafeMove(camPosition, Vector3.down, 0.38f) * fall;
                    look = look * Quaternion.Euler(34f * fall, -16f * side * fall, 14f * side * fall);
                }
                cam.SetPositionAndRotation(camPosition, look);
                _camera.fieldOfView = Mathf.Lerp(_fov, _fov - 6f, pull) + 4f * fall;

                // A side key on his face: from her side, off to one side and a little below the hat brim, so the brim and
                // the far cheek keep their shadow.
                Vector3 toEye = (eyeNow - faceNow).normalized;
                _light.transform.position = faceNow + toEye * 0.4f + across * 0.42f + Vector3.down * 0.06f;
                _light.intensity = _faceLightIntensity * Smooth(Mathf.InverseLerp(reachAt, contactAt, t)) * (1f - fall);

                // Sound on the frames that cause it.
                if (!contact && t >= contactAt)
                {
                    contact = true;
                    if (_audio != null)
                    {
                        _audio.PlaySfx("cloth", 1f);
                        _audio.PlaySfx("blow", 0.75f);
                        _audio.PlaySfx("sting_low", 0.55f);
                    }
                    if (_fx != null) { _fx.Shake(0.6f, 0.3f); _fx.Flash(0.1f); }
                }
                if (!gasp && t >= contactAt + 0.1f) { gasp = true; if (_audio != null) _audio.PlaySfx("breath_in", 0.9f); }
                if (!barked && t >= contactAt + 0.1f && _hud != null && !string.IsNullOrEmpty(line)) { barked = true; _hud.ShowBark(speakerId, line, 1.8f); }
                if (!shoved && t >= pullEnd)
                {
                    shoved = true;
                    if (_audio != null) { _audio.PlaySfx("whoosh", 0.7f); _audio.PlaySfx(_ending == Ending.Shove ? "stumble" : "step_wood", 0.8f); }
                    if (_fx != null) _fx.Shake(0.4f, 0.3f);
                }
                if (_carried != null) _carried.localPosition = Vector3.Lerp(_carriedAt, new Vector3(-0.32f, 0.72f, -0.12f), Smooth(Mathf.InverseLerp(0f, reachAt + 0.1f, t)));
                if (_hud != null && t >= pullEnd + 0.32f) _hud.FadeAlpha = Smooth(Mathf.InverseLerp(pullEnd + 0.32f, end, t));

                yield return null;
            }
            if (_hud != null) _hud.FadeAlpha = 1f;
            if (clipFolder != null) yield return null;

            string by = _catcher != null ? _catcher.name : "?";
            Cleanup(false);
            StoryAutopilot.Note($"CATCH {kind}{(kind == Kind.FromHiding ? " " + hideKind : "")} by {by} · {(_ending == Ending.Drag ? "dragged" : "shoved")} · worst frame {LastWorstFrame * 1000f:F0} ms" + (clip != null ? $" · case {clip}" : "") + (clipFolder != null ? $" ({_captureFrame} frames recorded)" : ""));
            done?.Invoke();
        }

        /// <summary>
        /// Hands back everything the catch took over (camera, FOV, time, actors, watchers, audio, props). Safe to call twice.
        /// After a normal catch the controls stay locked and the screen dark for the failure that follows; torn down early,
        /// both are released.
        /// </summary>
        private void Cleanup(bool interrupted)
        {
            if (!_owns) return;
            _owns = false;
            _captureFolder = null;
            Time.captureFramerate = 0;
            if (_light != null) { _light.intensity = 0f; _light.enabled = false; }
            if (_carried != null) _carried.localPosition = _carriedAt;
            Time.timeScale = _timeScale;
            if (_pose != null) { _pose.Look = _pose.Lean = _pose.Right = _pose.Left = _pose.Grip = 0f; _pose.enabled = false; }
            if (_animator != null) _animator.SetFloat(SpeedId, 0f);
            if (_prop != null)
            {
                if (_hinge != null) { _prop.SetParent(_propParent, true); Destroy(_hinge.gameObject); }
                _prop.localPosition = _propPosition;
                _prop.localRotation = _propRotation;
            }
            if (_actor != null)
            {
                _actor.enabled = true;
                if (_agent != null && _agentWas) { _agent.enabled = true; if (_agent.isOnNavMesh) _agent.Warp(_actor.transform.position); }
            }
            if (_watchers != null) foreach (var watcher in _watchers) if (watcher != null) watcher.Pause(false);
            if (_camera != null) _camera.fieldOfView = _fov;
            if (_cameraController != null) _cameraController.enabled = true;
            if (interrupted && _player != null) _player.ControlLocked = false;
            if (interrupted && _hud != null) _hud.FadeAlpha = 0f;
            if (_audio != null) { _audio.Duck(0f); _audio.DuckAmbience(0f); }
            _catcher = null; _pose = null; _actor = null; _animator = null; _agent = null;
            _prop = _propParent = _hinge = _carried = null;
            Playing = false;
        }

        /// <summary>How far the eye can move from <paramref name="from"/> along <paramref name="direction"/> (up to
        /// <paramref name="want"/>) before it would touch a wall.</summary>
        private static float SafeMove(Vector3 from, Vector3 direction, float want)
        {
            if (want <= 0f) return 0f;
            return Physics.SphereCast(from, 0.12f, direction, out var hit, want, Walls, QueryTriggerInteraction.Ignore) ? Mathf.Max(0f, hit.distance - 0.02f) : want;
        }

        private static float Smooth(float x) => x * x * (3f - 2f * x);

        private static string ClipFolder(string clip)
        {
            var args = System.Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-autopilotCatchClips");
            if (index < 0 || index + 1 >= args.Length) return null;
            string folder = Path.Combine(args[index + 1], clip);
            Directory.CreateDirectory(folder);
            return folder;
        }

        // QA recording: after every pose and the camera have been set this frame (WaitForEndOfFrame never comes in batch
        // mode, so the frame is rendered here instead).
        private void LateUpdate()
        {
            if (_captureFolder == null) return;
            StoryCapture.ShotFile(Path.Combine(_captureFolder, $"f{_captureFrame++:000}.png"), 640, 360);
        }
    }
}

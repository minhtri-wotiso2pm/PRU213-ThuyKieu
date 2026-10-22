using System.Collections;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Prologue — Trang sách. The afternoon classroom (the modern student at her desk, the class leaving after the bell),
    /// then, through the light of the book, Kiều's room on the morning of the arrest, the Vương courtyard where the
    /// officials take the father and brother, and the rain that falls that afternoon (Chapter 1 begins in it).
    /// </summary>
    public class PrologueWorld : MonoBehaviour, IChapterSummary
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private PlayerLook _look;

        [Header("Classroom")]
        [SerializeField] private Transform _seat;
        [SerializeField] private StoryActor _ngan;
        [SerializeField] private StoryActor _teacher;
        [SerializeField] private StoryActor[] _classmates;
        [SerializeField] private Transform _classDoor;
        [SerializeField] private GameObject _bookGlow;
        [Tooltip("Kiều studying her new hands (Animations/ThuyKieu/TK-LookAtHands).")]
        [SerializeField] private AnimationClip _lookAtHands;

        [Header("Vương house")]
        [SerializeField] private Transform _wake, _courtyard;
        [SerializeField] private StoryActor _father, _brother, _mother;
        [SerializeField] private Transform _fatherSpot, _brotherSpot, _motherSpot, _motherRainSpot;
        [SerializeField] private StoryActor[] _officials;
        [SerializeField] private Transform[] _officialSpots;
        [SerializeField] private Transform _gate;
        [SerializeField] private Light _sun;
        [SerializeField] private ParticleSystem _rain;
        [SerializeField] private GameObject _rainProps;

        private const string Controls = "WASD: đi · E: tương tác · Chuột phải: quan sát · Space: tiếp lời thoại";
        private const string LuteFreePlay = "dan_tu_do";
        private const string NewBodyBeat = "than_xac";

        private bool _seated = true, _classLeft, _nganLeft;
        private Coroutine _interactionGesture, _handling;
        private float _lastPluck = -10f;

        private void Awake()
        {
            var lute = LuteSequence.Create(gameObject, LuteFreePlay);
            lute.ControlsAfter = Controls;
            lute.Plucked += _ => _lastPluck = Time.time;
            _director.RegisterSequence(lute);
            _director.RegisterSequence(WorldSequence.Create(gameObject, NewBodyBeat, NewBody));
        }

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.ExploreChanged += OnExplore;
            _director.TagReceived += OnTag;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.ExploreChanged -= OnExplore;
            _director.TagReceived -= OnTag;
        }

        private IEnumerator Start()
        {
            LightExposure.Enable(false);
            ClearFatherFallSpace();
            if (_navMesh != null) _navMesh.BuildNavMesh();
            // The Vương family and officials are hand-blocked for this scene. Letting their agents sample the shared
            // NavMesh can snap them onto the low table or back against the wall before their authored sequences run.
            foreach (var actor in FindObjectsByType<StoryActor>())
            {
                bool staged = actor == _father || actor == _brother || actor == _mother
                    || System.Array.IndexOf(_officials, actor) >= 0;
                if (staged) actor.DisableNavigation();
                else actor.EnableNavigation();
            }
            _director.Player.AllowCrouch = false;
            RefreshTutorials();
            // The hồ cầm prop is built at an adult's size; Kiều is 1.3 m tall with short arms and could not hold it
            // the way it is played, so it is sized to her (absolute, so a re-run never compounds).
            var hoCam = GameObject.Find("HoCam");
            Transform lute = hoCam != null ? hoCam.transform.Find("Lute") : null;
            if (lute != null) lute.localScale = Vector3.one * LuteScale;
            _director.Hud.SetControls(Controls);
            _look.ShowStudent(true);
            foreach (var official in _officials) official.gameObject.SetActive(false);
            if (_rain != null) _rain.Stop();
            if (_rainProps != null) _rainProps.SetActive(false);
            if (_bookGlow != null) _bookGlow.SetActive(false);
            if (_director.Audio != null) _director.Audio.SetAmbience("classroom");
            // Everyone at their desks for the lesson: the animator needs one frame before the sit trigger takes.
            yield return null;
            _director.Player.Teleport(_seat.position, _seat.rotation);
            var playerActor = _director.Player.GetComponent<StoryActor>();
            playerActor.SetFacingLocked(true);
            playerActor.Sit(true);
            _ngan.SetFacingLocked(true);
            _ngan.Sit(true);
            foreach (var classmate in _classmates) classmate.Sit(true);
        }

        private void RefreshTutorials()
        {
            foreach (var tutorial in FindObjectsByType<TutorialSequence>(FindObjectsInactive.Include))
            {
                if (tutorial.Id == "tutorial_classroom")
                    tutorial.Configure(
                        "<color=#E8C27A>Đi lại:</color> WASD · <color=#E8C27A>xoay camera:</color> chuột\n" +
                        "<color=#E8C27A>Tương tác:</color> đến gần thứ có dấu sáng vàng rồi bấm <color=#E8C27A>E</color>\n" +
                        "<color=#E8C27A>Lời thoại:</color> Space / click để đọc tiếp, 1–4 để chọn câu trả lời\n\n" +
                        "Cất quyển Truyện Kiều vào cặp. Ngân và những thứ quanh lớp là tùy chọn.",
                        "WASD: đi · E: tương tác · Space: tiếp lời thoại · Esc: tạm dừng");
                else if (tutorial.Id == "tutorial_room")
                    tutorial.Configure(
                        "<color=#E8C27A>Quan sát:</color> giữ <color=#E8C27A>chuột phải</color> để nhìn kỹ một thứ.\n" +
                        "<color=#E8C27A>Sổ tay:</color> bấm <color=#E8C27A>J</color> để xem lại những gì đã biết.\n\n" +
                        "Hãy soi gương trước. Khuôn mặt trong đó sẽ cho biết mình đang ở đâu.",
                        "WASD: đi · E: xem · Chuột phải: quan sát · J: sổ tay");
            }
        }

        /// <summary>The collapse clip swings Vương Ông sideways farther than his capsule; move only the nearby veranda
        /// planter out of that swept volume so his body does not pass through its trunk.</summary>
        private void ClearFatherFallSpace()
        {
            foreach (var transform in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!transform.name.StartsWith("ChauMai") || Vector3.Distance(transform.position, _father.transform.position) > 1.7f) continue;
                transform.position += Vector3.right * 1.35f;
            }
        }

        private void OnExplore(bool exploring)
        {
            if (!exploring) return;
            if (_seated) StartCoroutine(StandUp());
            // The deskmate goes home once she has said goodbye.
            if (!_nganLeft && _director.Story.state.VisitCountAtPathString("pl_ngan") > 0) { _nganLeft = true; Leave(_ngan, _classDoor); }
        }

        /// <summary>After the bell: the student gets up from her desk, the class and the teacher file out.</summary>
        private IEnumerator StandUp()
        {
            _seated = false;
            var player = _director.Player;
            player.ControlLocked = true;
            player.GetComponent<StoryActor>().SetFacingLocked(false);
            player.GetComponent<StoryActor>().Sit(false);
            _ngan.Sit(false);
            if (!_classLeft)
            {
                _classLeft = true;
                for (int i = 0; i < _classmates.Length; i++) StartCoroutine(LeaveLater(_classmates[i], 0.6f + i * 0.45f));
                StartCoroutine(LeaveLater(_teacher, 1.2f));
            }
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 1.6f);
            if (_director.InExplore) player.ControlLocked = false;
        }

        private IEnumerator LeaveLater(StoryActor actor, float delay)
        {
            actor.Sit(false);
            yield return new WaitForSeconds(_director.Autopilot ? 0f : delay + 1.3f);
            Leave(actor, _classDoor);
        }

        private void Leave(StoryActor actor, Transform door)
        {
            if (actor == null || !actor.isActiveAndEnabled) return;
            actor.SetFacingLocked(false);
            actor.Sit(false);
            // Only actors that can actually navigate (the teacher) walk out; rigged-but-seated actors with no
            // agent (Ngan, the classmates) still slip away off-camera, exactly as they did when unrigged.
            StartCoroutine(actor.HasRig && actor.Agent != null ? WalkOut(actor, door) : SlipAway(actor));
        }

        /// <summary>A posed (unrigged) classmate cannot walk, so it is gone the first moment the camera looks elsewhere.</summary>
        private IEnumerator SlipAway(StoryActor actor)
        {
            var renderers = actor.GetComponentsInChildren<Renderer>();
            for (float t = 0; t < 30f && actor.isActiveAndEnabled; t += Time.deltaTime)
            {
                bool seen = false;
                foreach (var r in renderers) seen |= r.isVisible;
                if (!seen || _director.Autopilot) break;
                yield return null;
            }
            actor.gameObject.SetActive(false);
        }

        private IEnumerator WalkOut(StoryActor actor, Transform door)
        {
            actor.MoveTo(door.position, 1.3f);
            for (float t = 0; t < 12f && actor.isActiveAndEnabled; t += Time.deltaTime)
            {
                if (t > 1f && !actor.IsMoving) break;
                yield return null;
            }
            actor.gameObject.SetActive(false);
        }

        private void OnTag(string key, string value)
        {
            if (key == "gameplay" && value == "book_glow" && _bookGlow != null) _bookGlow.SetActive(true);
            // Room interactions: Kiều touches the unfamiliar face in the mirror, takes up the hồ cầm and plays it,
            // lifts Kim Trọng's letter to read; the lute and the letter are seen through her eyes.
            if (key == "camera" && value == "Mirror") Handle(TouchFace());
            if (key == "camera" && value == "Dan")
            {
                // PlayLute drives both arms itself. Layering the generic pickup clip underneath twists the áo dài
                // sleeves and makes the forearm snap through the strings.
                Handle(PlayLute());
            }
            if (key == "camera" && value == "Desk")
            {
                // ReadLetter also owns both hands; keep the relaxed base pose underneath its IK.
                Handle(ReadLetter());
            }
            // The gong at the gate: the officials burst into the courtyard.
            if (key == "sfx" && value == "gong" && !_officials[0].gameObject.activeSelf) StartCoroutine(OfficialsArrive());
            if (key == "speaker" && _officials[0].gameObject.activeSelf && _director.CurrentSequence == null) StartCoroutine(Address(value));
            if (key == "gameplay" && value == "arrest") StartCoroutine(WatchArrest());
            if (key == "hands") Hands(value);
        }

        /// <summary>
        /// The confrontation is between the official and Vương Ông, not with the player: each faces the other while
        /// either speaks, and Kiều looks at whoever has the floor (at the official when she speaks or thinks).
        /// </summary>
        private IEnumerator Address(string speaker)
        {
            // RouteTags turns everyone towards the speaker (and the speaker towards the player) after TagReceived.
            yield return null;
            var kieu = _director.Player.GetComponent<StoryActor>();
            StoryActor official = _officials[0];
            switch (speaker)
            {
                case "QuanSai":
                    // Before Kiều intervenes he confronts Vương Ông. Once she has chosen an answer, he is
                    // replying to her and must not keep speaking over her shoulder at her father.
                    StoryActor addressee = _director.GetInt("pl_hanh_dong") > 0 ? kieu : _father;
                    foreach (var guard in _officials) guard.TurnForLine(addressee.transform.position);
                    _father.TurnForLine(official.transform.position);
                    kieu.TurnForLine(official.transform.position);
                    break;
                case "VuongOng":
                    foreach (var guard in _officials) guard.TurnForLine(_father.transform.position);
                    _father.TurnForLine(official.transform.position);
                    kieu.TurnForLine(_father.transform.position);
                    break;
                case "Kieu":
                    foreach (var guard in _officials) guard.TurnForLine(kieu.transform.position);
                    _father.TurnForLine(official.transform.position);
                    kieu.TurnForLine(official.transform.position);
                    break;
                case "NoiTam":
                    kieu.TurnForLine(official.transform.position);
                    break;
            }
        }

        /// <summary>
        /// While the father and brother are marched out, the shot stays at Kiều's eyes as she follows them to the gate;
        /// it must not be replaced by the following dialogue's Gate shot while the escort is still moving.
        /// </summary>
        private IEnumerator WatchArrest()
        {
            for (float t = 0f; _director.CurrentSequence != "arrest" && t < 30f; t += Time.deltaTime) yield return null;
            if (_director.CurrentSequence != "arrest") yield break;
            var camera = _director.CameraController;
            bool locked = camera.ControlLocked;
            var shot = new GameObject("ArrestShot").transform;
            camera.ControlLocked = true;
            var kieu = _director.Player.GetComponent<StoryActor>();
            Vector3 localEye = kieu.transform.InverseTransformPoint(kieu.HeadPosition);
            Vector3 look = _father.transform.position + Vector3.up;
            shot.position = kieu.transform.TransformPoint(localEye) + kieu.transform.forward * 0.12f;
            shot.rotation = Quaternion.LookRotation(look - shot.position);
            camera.CutTo(shot);
            while (_director.CurrentSequence == "arrest")
            {
                Vector3 father = _father.transform.position;
                look = Vector3.Lerp(look, father + Vector3.up, 1f - Mathf.Exp(-3f * Time.deltaTime));
                shot.position = kieu.transform.TransformPoint(localEye) + kieu.transform.forward * 0.12f;
                shot.rotation = Quaternion.LookRotation(look - shot.position);
                // DialogueCameraRig receives camera:Gate on Vương Quan's next line. Reassert Kiều's POV until the
                // escort is actually over, otherwise that tag drags the viewer away from her mid-shot.
                camera.SetDialogueCue(shot);
                kieu.FaceTowards(father);
                _mother.FaceTowards(father);
                _mother.SetTalking(true, StoryDirector.Gesture("crying"));
                yield return null;
            }
            kieu.FaceTowards(null);
            _mother.FaceTowards(null);
            camera.SetDialogueCue(null);
            camera.ControlLocked = locked;
            Destroy(shot.gameObject);
        }

        /// <summary>
        /// The teacher's question: "up" puts most of the class's hands up; "pick" (he calls on her) lowers theirs
        /// and raises hers; "down" lowers hers as she answers.
        /// </summary>
        private void Hands(string beat)
        {
            for (int i = 0; i < _classmates.Length; i++) _classmates[i].RaiseHand(beat == "up" && i % 3 != 1);
            _director.Player.GetComponent<StoryActor>().RaiseHand(beat == "pick");
        }

        private IEnumerator OfficialsArrive()
        {
            for (int i = 0; i < _officials.Length; i++)
            {
                _officials[i].gameObject.SetActive(true);
                _officials[i].Place(_gate.position + _gate.right * (i - 0.5f) * 0.9f, _gate.rotation);
                StartCoroutine(MarchOfficial(_officials[i], _officialSpots[i], 2.6f));
            }
            yield return null;
        }

        /// <summary>
        /// The courtyard NavMesh also contains raised furniture surfaces. The entrance is therefore staged directly
        /// along the clear centre aisle, with locomotion driven explicitly, so an official cannot climb onto the meal
        /// table while delivering his line.
        /// </summary>
        private static IEnumerator MarchOfficial(StoryActor actor, Transform destination, float speed)
        {
            var agent = actor.Agent;
            if (agent != null) agent.enabled = false;
            actor.SetScriptedSpeed(speed);
            Vector3 target = destination.position;
            while ((actor.transform.position - target).sqrMagnitude > 0.0025f)
            {
                Vector3 direction = target - actor.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation,
                        Quaternion.LookRotation(direction), 540f * Time.deltaTime);
                    actor.transform.position = Vector3.MoveTowards(actor.transform.position, target, speed * Time.deltaTime);
                }
                yield return null;
            }
            actor.transform.SetPositionAndRotation(target, destination.rotation);
            actor.SetScriptedSpeed(null);
            actor.EnableNavigation();
        }

        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "LopHoc":
                    // The opening classroom must never inherit Kiều's historical body from a previous Play session.
                    _look.ShowStudent(true);
                    break;
                case "PhongKieu":
                    // Continuing at pl_phong can skip gameplay:into_room. Restore body and location together;
                    // switching only the model is what left Kiều standing inside the classroom.
                    if (_look.IsStudent) BecomeKieu();
                    if (Vector3.Distance(_director.Player.transform.position, _wake.position) > 20f)
                    {
                        _director.Player.Teleport(_wake.position, _wake.rotation);
                        _director.CameraController.SnapBehindTarget(_wake.eulerAngles.y);
                    }
                    break;
                case "VuongGia_San":
                    if (_look.IsStudent) BecomeKieu();
                    StartCoroutine(ToCourtyard());
                    break;
                case "VuongGia_Mua":
                    if (_look.IsStudent) BecomeKieu();
                    StartCoroutine(Rain());
                    break;
            }
        }

        /// <summary>The light of the book: white, then Kiều's room in the morning, as Kiều (the "into_room" beat).</summary>
        public IEnumerator IntoKieu()
        {
            _director.Hud.SetFadeColor(new Color(1f, 0.97f, 0.9f));
            yield return _director.Hud.Fade(1f, _director.Autopilot ? 0f : 0.6f);
            if (_bookGlow != null) _bookGlow.SetActive(false);
            BecomeKieu();
            _director.Player.Teleport(_wake.position, _wake.rotation);

            // The awakening is seen through Kiều's eyes, not from behind her body.
            var actor = _director.Player.GetComponent<StoryActor>();
            actor.SetTalking(true, 5); // TK-Look Around
            yield return null; // Let the new humanoid rig settle before reading its head position.
            var animator = _director.Player.GetComponentInChildren<Animator>();
            Transform head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            var viewObject = new GameObject("WakePOV_Runtime");
            Transform view = viewObject.transform;
            Vector3 forward = _wake.forward;
            view.position = (head != null ? head.position : _wake.position + Vector3.up * 1.5f) + forward * 0.12f;
            view.rotation = Quaternion.LookRotation(forward, Vector3.up);
            bool cameraWasLocked = _director.CameraController.ControlLocked;
            _director.CameraController.ControlLocked = true;
            _director.CameraController.CutTo(view);
            var gesture = _director.Player.gameObject.AddComponent<WakeFirstPersonGesture>();
            gesture.Initialize(animator, view);
            if (_director.Fx != null) _director.Fx.SetWakeBlur(1f);

            yield return _director.Hud.Card("MỞ ĐẦU · II", "PHÒNG KIỀU", true, 2.2f);
            _director.Hud.SetFadeColor(Color.black);
            yield return _director.Hud.Fade(1f, 0f);
            yield return _director.Hud.Fade(0f, _director.Autopilot ? 0f : 0.75f);

            // Dazed eyes clear while the raised hand lowers and the gaze checks both sides of the room.
            Quaternion startView = view.rotation;
            float lookDuration = _director.Autopilot ? 0f : 2.8f;
            for (float t = 0f; t < lookDuration; t += Time.deltaTime)
            {
                float progress = t / Mathf.Max(lookDuration, 0.01f);
                float yaw = Mathf.Sin(progress * Mathf.PI * 2f - Mathf.PI * 0.5f) * 17f;
                float pitch = Mathf.Sin(progress * Mathf.PI) * 2.5f;
                view.rotation = startView * Quaternion.Euler(pitch, yaw, 0f);
                gesture.Weight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 4f))
                    * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.58f) / 0.36f)));
                if (_director.Fx != null) _director.Fx.SetWakeBlur(1f - Mathf.SmoothStep(0f, 1f, progress));
                yield return null;
            }
            gesture.Weight = 0f;
            actor.SetTalking(false, 0);
            if (_director.Fx != null) _director.Fx.SetWakeBlur(0f);
            _director.CameraController.SetDialogueCue(null);
            _director.CameraController.ControlLocked = cameraWasLocked;
            Destroy(gesture);
            Destroy(viewObject);
            _director.CameraController.SnapBehindTarget(_wake.eulerAngles.y);
        }

        /// <summary>
        /// Her first moments in Kiều's body, played rather than told. In a close shot she raises hands that are not hers
        /// and turns them over, studying them (the authored TK-LookAtHands clip); a moment later she hears her own voice
        /// and it is a stranger's. Then the view returns behind her and she is free to move.
        /// </summary>
        private IEnumerator NewBody(StoryDirector director)
        {
            bool fast = director.Autopilot;
            var hud = director.Hud;
            var player = director.Player;
            var actor = player.GetComponent<StoryActor>();
            var animator = player.GetComponentInChildren<Animator>();
            Transform body = player.transform;
            Transform head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            Transform leftHand = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.LeftHand) : null;
            Transform rightHand = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.RightHand) : null;
            var camera = director.CameraController;
            bool cameraLocked = camera.ControlLocked;
            // The clip bows her head to her hands; no glance left over from a conversation may turn it away.
            actor.FaceTowards(null);

            var view = new GameObject("NewBodyPOV").transform;
            Vector3 eyePosition = head != null ? head.position : body.position + Vector3.up * 1.5f;
            // A close three-quarter shot from her front right: her face and her hands in the one frame.
            Vector3 lens = eyePosition - Vector3.up * 0.1f + body.forward * 0.62f + body.right * 0.5f;
            view.SetPositionAndRotation(lens, Quaternion.LookRotation(eyePosition + body.forward * 0.2f - Vector3.up * 0.15f - lens));
            camera.ControlLocked = true;
            camera.CutTo(view);
            if (director.Audio != null) director.Audio.PlaySfx("breath_startled", 0.75f);
            if (director.Fx != null) director.Fx.Shake(0.08f, 0.35f);

            // The clip plays over the story controller through a short-lived graph, blended in and out of her idle.
            PlayableGraph graph = default;
            AnimationMixerPlayable mixer = default;
            float length = _lookAtHands != null ? _lookAtHands.length : 10f;
            // What she sees, what she blurts out loud, then whose voice that was: each line waits for the one before
            // it to be heard out, and she holds her hands up until the last has been said.
            var lines = new[]
            {
                Say("NoiTam", "Ngón tay thon, trắng như ngà... Đây không phải tay mình."),
                Say("Kieu", "Tay... tay của ai thế này?"),
                Say("NoiTam", "Giọng ai mà trong như tiếng đàn thế này? Là giọng... của nàng.")
            };
            var starts = new float[lines.Length];
            var spoken = new float[lines.Length];
            float cue = 2.6f;
            for (int i = 0; i < lines.Length; i++)
            {
                starts[i] = cue;
                spoken[i] = SpokenLength(lines[i].Item1, lines[i].Item2);
                cue += spoken[i] + 0.35f;
            }
            float total = Mathf.Max(length, cue + 0.4f);
            var handsClip = default(AnimationClipPlayable);
            if (_lookAtHands != null && animator != null && animator.runtimeAnimatorController != null)
            {
                graph = PlayableGraph.Create("NewBodyHands");
                graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                mixer = AnimationMixerPlayable.Create(graph, 2);
                graph.Connect(AnimatorControllerPlayable.Create(graph, animator.runtimeAnimatorController), 0, mixer, 0);
                handsClip = AnimationClipPlayable.Create(graph, _lookAtHands);
                graph.Connect(handsClip, 0, mixer, 1);
                AnimationPlayableOutput.Create(graph, "Hands", animator).SetSourcePlayable(mixer);
                mixer.SetInputWeight(0, 1f);
                graph.Play();
            }
            for (float t = 0f; !fast && t < total; t += Time.deltaTime)
            {
                if (graph.IsValid())
                {
                    // Past the clip's last held pose, keep her on it until the lines are done.
                    if (t > length - 0.7f) handsClip.SetTime(length - 0.7f);
                    float weight = Mathf.SmoothStep(0f, 1f, Mathf.Min(t / 0.4f, (total - t) / 0.6f));
                    mixer.SetInputWeight(0, 1f - weight);
                    mixer.SetInputWeight(1, weight);
                }
                // The shot keeps both her face and her hands in frame as they come up.
                if (leftHand != null && rightHand != null)
                {
                    Vector3 aim = Vector3.Lerp((leftHand.position + rightHand.position) * 0.5f, eyePosition, 0.45f);
                    view.rotation = Quaternion.Slerp(view.rotation, Quaternion.LookRotation(aim - view.position), 1f - Mathf.Exp(-4f * Time.deltaTime));
                }
                for (int i = 0; i < lines.Length; i++)
                    if (t < starts[i] && t + Time.deltaTime >= starts[i])
                        hud.ShowBark(lines[i].Item1, lines[i].Item2, spoken[i] + 0.3f);
                yield return null;
            }
            if (graph.IsValid()) graph.Destroy();
            camera.SetDialogueCue(null);
            camera.ControlLocked = cameraLocked;
            camera.SnapBehindTarget(body.eulerAngles.y);
            Destroy(view.gameObject);
            hud.SetControls(Controls);
            actor.SetTalking(false, 0);
        }

        /// <summary>A bark to say later; written as a call so the voice manifest exporter finds its line.</summary>
        private static (string, string) Say(string speakerId, string text) => (speakerId, text);

        /// <summary>How long a bark takes to say: its voice clip's length, or a reading pace when it has none.</summary>
        private static float SpokenLength(string speakerId, string text)
        {
            var clip = Resources.Load<AudioClip>(Dialogue.VoiceLine.ResourceFolder + "/" + Dialogue.VoiceLine.Key(speakerId, text));
            return clip != null ? clip.length : Mathf.Clamp(text.Length * 0.06f, 1.5f, 5f);
        }

        private void PlayInteractionGesture(int gesture, float duration)
        {
            if (_interactionGesture != null) StopCoroutine(_interactionGesture);
            _interactionGesture = StartCoroutine(InteractionGesture(gesture, duration));
        }

        private IEnumerator InteractionGesture(int gesture, float duration)
        {
            // RouteTags finishes after TagReceived and may clear the player's dialogue state, so start on the next frame.
            yield return null;
            var actor = _director.Player.GetComponent<StoryActor>();
            float hold = _director.Autopilot ? 0.05f : duration;
            for (float t = 0f; t < hold; t += Time.deltaTime)
            {
                actor.SetTalking(true, gesture);
                yield return null;
            }
            actor.SetTalking(false, 0);
            _interactionGesture = null;
        }

        private void Handle(IEnumerator interaction)
        {
            if (_handling != null) StopCoroutine(_handling);
            _handling = StartCoroutine(interaction);
        }

        private HandReachPose AddPose()
        {
            var player = _director.Player;
            var pose = player.GetComponent<HandReachPose>();
            if (pose == null) pose = player.gameObject.AddComponent<HandReachPose>();
            pose.Initialize(player.GetComponentInChildren<Animator>());
            pose.Weight = 0f;
            pose.Lean = 0f;
            pose.HandAim = 1f;
            pose.AfterPose = null;
            pose.SetTargets(null, null, null);
            pose.SetHandAim(null, null, null, null);
            pose.LeftCurl = pose.RightCurl = 0f;
            pose.SetElbowHints(null, null);
            return pose;
        }

        /// <summary>Looking at the stranger in the glass, her fingertips drift along the cheek until the hub returns.</summary>
        private IEnumerator TouchFace()
        {
            var actor = _director.Player.GetComponent<StoryActor>();
            var pose = AddPose();
            var focus = GameObject.Find("Focus_Mirror");
            Transform body = actor.transform;
            if (_director.Audio != null) _director.Audio.PlaySfx("mirror_reveal", 0.65f);
            if (_director.Fx != null) _director.Fx.Flash(0.08f);
            for (float t = 0f; !_director.InExplore; t += Time.deltaTime)
            {
                // The wrist sits under the jaw so the fingers rest on the cheek.
                float stroke = Mathf.Sin(t * 1.3f) * 0.02f;
                Vector3 wrist = actor.HeadPosition - Vector3.up * (0.14f + stroke) + body.forward * 0.045f + body.right * 0.06f;
                pose.SetTargets(null, wrist, focus != null ? focus.transform.position : (Vector3?)null);
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / 0.9f);
                yield return null;
            }
            yield return LetGo(pose);
        }

        /// <summary>
        /// She steps up to the hồ cầm, closes her hands on its neck, lifts it from the wall, the view drops to her own
        /// hands and the "Bạc mệnh" motif sounds once; then the player plays it freely (LuteSequence), her plucking
        /// hand following each note; back in the hub she leans it against the wall again.
        /// </summary>
        private IEnumerator PlayLute()
        {
            var hoCam = GameObject.Find("HoCam");
            Transform lute = hoCam != null ? hoCam.transform.Find("Lute") : null;
            if (lute == null) yield break;
            var player = _director.Player;
            var actor = player.GetComponent<StoryActor>();
            Transform body = actor.transform;
            // Eye height before the picking-up clip bends her down.
            Vector3 eye = body.InverseTransformPoint(actor.HeadPosition);
            // The stand point leaves the lute beyond her arms; the shot has just cut, so she is simply placed closer.
            Vector3 toLute = Vector3.ProjectOnPlane(lute.position - body.position, Vector3.up);
            if (toLute.magnitude > LuteReach) player.Teleport(body.position + toLute - toLute.normalized * LuteReach, body.rotation);
            // That close, the authored shot from behind her shoulder sees only her back: watch the pickup in profile.
            var profile = new GameObject("LutePickupShot");
            profile.transform.position = body.position - body.forward * 0.4f - body.right * 1.3f + Vector3.up * 1.05f;
            profile.transform.LookAt(body.position + body.forward * 0.25f + Vector3.up * 0.7f);
            _director.CameraController.CutTo(profile.transform);

            var pose = AddPose();
            Pose rest = new Pose(lute.position, lute.rotation);
            float reach = _director.Autopilot ? 0.01f : 0.7f;
            for (float t = 0f; t < reach; t += Time.deltaTime)
            {
                pose.SetTargets(lute.TransformPoint(LuteWallGrip), lute.TransformPoint(LuteNeckGrip), lute.TransformPoint(0f, 0.4f, 0f));
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / reach);
                pose.Lean = LuteBend;
                yield return null;
            }
            pose.Weight = 1f;

            // From here the lute moves only in her hand: its neck is kept in the posed left hand every frame.
            Quaternion wallFacing = body.rotation;
            Pose held = LuteHeld(body, eye);
            float lift = 0f;
            pose.AfterPose = () => HoldLute(lute, pose.LeftHand, rest, held, lift);
            yield return CarryLute(pose, lute, body, eye, rest, wallFacing, 0f, 1f, (k, h) => { lift = k; held = h; });
            // Full-strength IK pulls this wide-sleeved rig's shoulders into an unnatural T-pose. A partial reach keeps
            // the fingertips at the strings while allowing both sleeves to fall from relaxed shoulders.
            pose.Weight = LutePlayingPoseWeight;

            Vector3 center = OnLute(held, new Vector3(0f, 0.3f, 0f));
            // Framed between the soundboard and the fretting hand so both hands are in view.
            // A little higher than the letter's view, clear of the hair falling past her face.
            var view = PointOfView("LutePOV", body, eye + new Vector3(0f, 0.04f, 0f), OnLute(held, new Vector3(0f, 0.45f, 0f)));
            _director.CameraController.CutTo(view.transform);
            if (_director.Audio != null) _director.Audio.PlaySfx("dan");
            // Seen from her eyes the wide sleeve hides the plucking hand, so after a look down at the lute the shot
            // moves to the open side of the room. Keep it wide and north of the curtain so neither sleeve fills the lens.
            var hands = new GameObject("LuteHandsShot");
            Vector3 strings = OnLute(held, LutePluck(0f));
            hands.transform.position = strings + body.forward * 0.92f - body.right * 0.48f + Vector3.up * 0.3f;
            hands.transform.LookAt(OnLute(held, new Vector3(0f, 0.5f, 0f)));
            Vector3 fret = OnLute(held, LuteFretGrip);
            float firstPerson = _director.Autopilot ? 0f : 1.6f;
            Transform shot = view.transform;
            for (float t = 0f; !_director.InExplore; t += Time.deltaTime)
            {
                if (t >= firstPerson && view != null) { _director.CameraController.CutTo(hands.transform); shot = hands.transform; Destroy(view); }
                // Free play closes the dialogue, which drops the shot: keep it on her hands while she plays.
                if (_director.CurrentSequence == LuteFreePlay) _director.CameraController.SetDialogueCue(shot);
                // Each note the player plucks flicks the fingers across the strings, then the hand settles back beside them.
                float pluck = (Time.time - _lastPluck) / 0.25f;
                float swing = pluck < 1f ? Mathf.Sin(pluck * Mathf.PI) : 0f;
                pose.SetTargets(fret, lute.TransformPoint(LutePluck(swing)), center);
                AimLuteHands(pose, lute);
                yield return null;
            }
            if (view != null) Destroy(view);
            Destroy(hands);

            // Back in the hub: the camera stays on her, in profile, while she turns and leans the lute on the wall.
            var camera = _director.CameraController;
            bool cameraLocked = camera.ControlLocked;
            bool locked = player.ControlLocked;
            player.ControlLocked = true;
            camera.ControlLocked = true;
            camera.CutTo(profile.transform);
            yield return CarryLute(pose, lute, body, eye, rest, wallFacing, 1f, 0f, (k, h) => { lift = k; held = h; });
            pose.AfterPose = null;
            lute.SetPositionAndRotation(rest.position, rest.rotation);
            yield return LetGo(pose);
            camera.SetDialogueCue(null);
            camera.ControlLocked = cameraLocked;
            camera.SnapBehindTarget(body.eulerAngles.y);
            Destroy(profile);
            player.ControlLocked = locked && !_director.InExplore;
        }

        /// <summary>
        /// Moves the left hand (and the lute in it) between the wall and the playing hold. The wall is too close to
        /// play facing it, so she turns to face the room as she lifts it, and back to the wall to lean it there again.
        /// </summary>
        private IEnumerator CarryLute(HandReachPose pose, Transform lute, Transform body, Vector3 eye, Pose rest, Quaternion wallFacing,
            float from, float to, System.Action<float, Pose> setLift)
        {
            Vector3 restFret = OnLute(rest, LuteWallGrip);
            float duration = _director.Autopilot ? 0.01f : 1.5f;
            for (float t = 0f; t <= duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(from, to, Mathf.Clamp01(t / duration));
                _director.Player.Teleport(body.position, Quaternion.AngleAxis(LuteTurn * k, Vector3.up) * wallFacing);
                Pose held = LuteHeld(body, eye);
                Vector3 heldFret = OnLute(held, LuteFretGrip);
                setLift(k, held);
                // She straightens up as the lute comes off the wall (and bends again to put it back).
                pose.Lean = LuteBend * (1f - k);
                // The right hand slides from the neck down to the strings as the lute comes round, and the hands
                // turn into the playing hold: the left wrapped round the neck, the right over the strings.
                pose.SetTargets(Vector3.Lerp(restFret, heldFret, k),
                    lute.TransformPoint(Vector3.Lerp(LuteNeckGrip, LutePluck(0f), k)), lute.TransformPoint(0f, 0.4f, 0f));
                pose.HandAim = k;
                AimLuteHands(pose, lute, k);
                yield return null;
            }
            _director.Player.Teleport(body.position, Quaternion.AngleAxis(LuteTurn * to, Vector3.up) * wallFacing);
            setLift(to, LuteHeld(body, eye));
        }

        /// <summary>
        /// The left hand wraps the neck from behind, palm towards it, fingers curled over the fingerboard; the right
        /// hand lies over the strings, palm towards the soundboard, curled fingers reaching up and across to pluck.
        /// </summary>
        private static void AimLuteHands(HandReachPose pose, Transform lute, float weight = 1f)
        {
            Quaternion turn = lute.rotation;
            pose.SetHandAim(turn * new Vector3(0.55f, 0.35f, 0.75f), turn * new Vector3(0.75f, 0f, 0.65f),
                turn * LutePluckFingers, turn * Vector3.back);
            pose.LeftCurl = 32f * weight;
            pose.RightCurl = 12f * weight;
            // The right forearm comes over the rim in front of the soundboard instead of through the body, the left
            // elbow hangs out to her left below the neck.
            pose.SetElbowHints(lute.TransformPoint(-0.45f, 0.4f, -0.1f), lute.TransformPoint(0.45f, 0.45f, 0.35f));
        }

        /// <summary>
        /// Turns the lute towards the playing hold with its neck in her hand. Any gap her reach left at the wall
        /// closes over the first part of the lift, so the lute slides into the hand instead of jumping.
        /// </summary>
        private static void HoldLute(Transform lute, Transform hand, Pose rest, Pose held, float lift)
        {
            Quaternion rotation = Quaternion.Slerp(rest.rotation, held.rotation, lift);
            Vector3 grip = rotation * (Vector3.Lerp(LuteWallGrip, LuteFretGrip, lift) * LuteScale);
            Vector3 atWall = OnLute(rest, LuteWallGrip) - grip;
            lute.SetPositionAndRotation(Vector3.Lerp(atWall, hand.position - grip, Mathf.Clamp01(lift * 3f)), rotation);
        }

        /// <summary>
        /// She steps up to the writing desk, bends over it and lays both hands on Kim Trọng's letter; it
        /// leaves the desk only in her hands, she reads it, and back in the hub she lays it down again in view.
        /// </summary>
        private IEnumerator ReadLetter()
        {
            var letterObject = GameObject.Find("KieuRoom/Letter");
            if (letterObject == null) yield break;
            Transform letter = letterObject.transform;
            var text = GameObject.Find("KieuRoom/LetterText");
            var player = _director.Player;
            var actor = player.GetComponent<StoryActor>();
            Transform body = actor.transform;
            Vector3 eye = body.InverseTransformPoint(actor.HeadPosition);
            // The stand point is a step back from the desk; the shot has just cut, so she is simply placed at its edge.
            Vector3 toLetter = Vector3.ProjectOnPlane(letter.position - body.position, Vector3.up);
            if (toLetter.magnitude > LetterReach) player.Teleport(body.position + toLetter - toLetter.normalized * LetterReach, body.rotation);
            var carried = new Carry(letter, text != null ? text.transform : null);
            var pose = AddPose();
            // Hands come down onto the paper as she bends over the desk.
            float reach = _director.Autopilot ? 0.01f : 0.8f;
            for (float t = 0f; t < reach; t += Time.deltaTime)
            {
                pose.SetTargets(LetterHand(carried.Rest.position, body, -1f, 0f), LetterHand(carried.Rest.position, body, 1f, 0f), letter.position);
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / reach);
                pose.Lean = LetterBend * pose.Weight;
                yield return null;
            }
            pose.Weight = 1f;
            // Lifted square to her at arm's length, the far edge tipped up towards her eyes.
            Vector3 across = Vector3.ProjectOnPlane(letter.right, Vector3.up);
            float squareUp = Vector3.SignedAngle(across, body.right, Vector3.up);
            if (Mathf.Abs(squareUp) > 90f) squareUp -= 180f * Mathf.Sign(squareUp);
            Pose held = new Pose(body.TransformPoint(0f, eye.y - 0.34f, 0.4f),
                Quaternion.AngleAxis(-58f, body.right) * Quaternion.AngleAxis(squareUp, Vector3.up) * letter.rotation);
            // From here the paper moves only in her hands: it is kept between them every frame.
            float lift = 0f;
            pose.AfterPose = () => HoldLetter(carried, pose, body, held, lift);
            yield return CarryLetter(pose, carried, body, held, 0f, 1f, k => lift = k);

            var view = PointOfView("LetterPOV", body, eye, letter.position);
            _director.CameraController.CutTo(view.transform);
            while (!_director.InExplore)
            {
                pose.SetTargets(LetterHand(held.position, body, -1f, 1f), LetterHand(held.position, body, 1f, 1f), letter.position);
                yield return null;
            }
            Destroy(view);

            // Back in the hub: the desk shot stays while she lays the letter down again.
            var camera = _director.CameraController;
            bool cameraLocked = camera.ControlLocked;
            bool locked = player.ControlLocked;
            player.ControlLocked = true;
            camera.ControlLocked = true;
            var deskShot = GameObject.Find("CAM_Desk");
            if (deskShot != null) camera.CutTo(deskShot.transform);
            yield return CarryLetter(pose, carried, body, held, 1f, 0f, k => lift = k);
            pose.AfterPose = null;
            carried.Place(carried.Rest);
            yield return LetGo(pose);
            camera.SetDialogueCue(null);
            camera.ControlLocked = cameraLocked;
            camera.SnapBehindTarget(body.eulerAngles.y);
            player.ControlLocked = locked && !_director.InExplore;
        }

        private IEnumerator CarryLetter(HandReachPose pose, Carry carried, Transform body, Pose held, float from, float to, System.Action<float> setLift)
        {
            float duration = _director.Autopilot ? 0.01f : 0.9f;
            for (float t = 0f; t <= duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(from, to, Mathf.Clamp01(t / duration));
                setLift(k);
                Pose at = LetterAt(carried, held, k);
                pose.SetTargets(LetterHand(at.position, body, -1f, k), LetterHand(at.position, body, 1f, k), at.position);
                // She straightens up as the letter comes off the desk (and bends again to lay it down).
                pose.Lean = LetterBend * (1f - k);
                yield return null;
            }
            setLift(to);
        }

        private static Pose LetterAt(Carry carried, Pose held, float lift) =>
            new Pose(Vector3.Lerp(carried.Rest.position, held.position, lift), Quaternion.Slerp(carried.Rest.rotation, held.rotation, lift));

        /// <summary>
        /// Places the letter between her posed hands. Any gap her reach left on the desk closes over the first part
        /// of the lift, so the paper slides into her fingers instead of jumping.
        /// </summary>
        private static void HoldLetter(Carry carried, HandReachPose pose, Transform body, Pose held, float lift)
        {
            Pose at = LetterAt(carried, held, lift);
            Vector3 grips = (LetterHand(at.position, body, -1f, lift) + LetterHand(at.position, body, 1f, lift)) * 0.5f;
            Vector3 hands = (pose.LeftHand.position + pose.RightHand.position) * 0.5f;
            at.position += (hands - grips) * Mathf.Clamp01(lift * 3f);
            carried.Place(at);
        }

        /// <summary>Hands go back to the animation once the prop is where it belongs.</summary>
        private IEnumerator LetGo(HandReachPose pose)
        {
            float duration = _director.Autopilot ? 0.01f : 0.4f;
            float startWeight = pose.Weight;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                pose.Weight = startWeight * (1f - Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            Destroy(pose);
            _handling = null;
        }

        /// <summary>Eye level just in front of the face, looking down at what she holds.</summary>
        private static GameObject PointOfView(string name, Transform body, Vector3 localEye, Vector3 subject)
        {
            var view = new GameObject(name);
            Vector3 eye = body.TransformPoint(localEye) + body.forward * 0.12f;
            view.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(subject - eye, Vector3.up));
            return view;
        }

        /// <summary>
        /// Held upright as a pear-bodied lute (tì bà) is: soundboard facing out and tipped up to her eyes, body against
        /// her waist, neck leaning towards her left shoulder; not slung across her like a guitar.
        /// </summary>
        private static Pose LuteHeld(Transform body, Vector3 localEye)
        {
            Quaternion rotation = Quaternion.LookRotation(body.forward * 0.9f + Vector3.up * 0.42f,
                Vector3.up * 0.94f - body.right * 0.34f);
            // Measured from the eyes and high on her body: her arms are short, and the plucking hand must reach the strings.
            Vector3 center = body.TransformPoint(0.07f, localEye.y - 0.4f, 0.24f);
            // The lute's pivot is the bottom of its body, below the soundboard centre.
            return new Pose(center - rotation * new Vector3(0f, 0.3f * LuteScale, 0f), rotation);
        }

        /// <summary>A lute-local point (unscaled, as the prop is built) for the lute at the given pose.</summary>
        private static Vector3 OnLute(Pose lute, Vector3 local) => lute.position + lute.rotation * (local * LuteScale);

        // Lute-local wrist positions (+Y up the neck, +Z out of the soundboard, +X to her right when held): the left
        // hand's grip on the neck at the wall and when playing (wrapped round from behind its left edge), the right
        // hand's hold on the neck while lifting, and the plucking hand beside the strings over the lower body.
        private static readonly Vector3 LuteWallGrip = new Vector3(0f, 0.8f, -0.03f);
        private static readonly Vector3 LuteFretGrip = new Vector3(-0.07f, 0.62f, -0.05f);
        private static readonly Vector3 LuteNeckGrip = new Vector3(0f, 0.62f, -0.03f);
        // The right wrist sits below and to her right of the strings so its curled fingertips land on them between the
        // bridge and the sound hole; a pluck flicks the fingers out across the strings.
        private static readonly Vector3 LutePluckFingers = new Vector3(-0.85f, 0.35f, -0.15f);
        private static Vector3 LutePluck(float swing) => new Vector3(0.08f + swing * 0.03f, 0.27f, 0.15f + swing * 0.03f);
        // How far from the lute's foot she stands to reach its neck.
        private const float LuteReach = 0.2f;
        // The prop's size in her hands (see Start).
        private const float LuteScale = 0.75f;
        // How far from the letter she stands at the desk's edge, and how far she bends over it.
        private const float LetterReach = 0.33f;
        private const float LetterBend = 25f;
        // Forward bend at the waist while her hands are at the lute on the wall.
        private const float LuteBend = 28f;
        private const float LutePlayingPoseWeight = 0.8f;
        // She turns from the wall to the room to play.
        private const float LuteTurn = 180f;

        /// <summary>
        /// Wrist at a lower corner of the letter: resting on the paper on the desk (held 0), under the near edge
        /// when lifted (held 1) so the thumbs hold it without covering the text.
        /// </summary>
        private static Vector3 LetterHand(Vector3 letter, Transform body, float side, float held) =>
            letter + body.right * (side * 0.13f)
            + Vector3.Lerp(Vector3.up * 0.02f, -Vector3.up * 0.09f - body.forward * 0.05f, held);

        /// <summary>Moves props rigidly with the first one, between where they rest and where she holds them.</summary>
        private sealed class Carry
        {
            private readonly Transform[] _items;
            private readonly Pose[] _rest;

            public Carry(params Transform[] items)
            {
                _items = System.Array.FindAll(items, item => item != null);
                _rest = new Pose[_items.Length];
                for (int i = 0; i < _items.Length; i++) _rest[i] = new Pose(_items[i].position, _items[i].rotation);
            }

            /// <summary>Where the first prop lay when it was picked up.</summary>
            public Pose Rest => _rest[0];

            /// <summary>Puts the first prop at <paramref name="pivot"/> and the others where they lie relative to it.</summary>
            public void Place(Pose pivot)
            {
                Quaternion delta = pivot.rotation * Quaternion.Inverse(_rest[0].rotation);
                for (int i = 0; i < _items.Length; i++)
                    _items[i].SetPositionAndRotation(pivot.position + delta * (_rest[i].position - _rest[0].position), delta * _rest[i].rotation);
            }
        }

        /// <summary>Kiều's body and the Vương household's morning; she is no longer seated at the school desk.</summary>
        private void BecomeKieu()
        {
            _seated = false;
            _look.ShowStudent(false);
            _director.Player.GetComponent<StoryActor>().SetFacingLocked(false);
            _father.Place(_fatherSpot.position, _fatherSpot.rotation);
            _brother.Place(_brotherSpot.position, _brotherSpot.rotation);
            _mother.Place(_motherSpot.position, _motherSpot.rotation);
            if (_director.Audio != null) _director.Audio.SetAmbience("house");
            _director.Hud.SetChapterLabel("Mở đầu · Phòng Kiều");
        }

        /// <summary>
        /// "Ra sân": she walks out through her door (seen from behind) before the cut, then comes the last steps
        /// across the yard to her place in front of the family, instead of vanishing from the room.
        /// </summary>
        private IEnumerator ToCourtyard()
        {
            var player = _director.Player;
            var camera = _director.CameraController;
            var door = GameObject.Find("DoorTarget");
            // The first line has already cut to the courtyard shot: follow her to the door instead.
            yield return null;
            var anchor = GameObject.Find("CAM_Wide_Courtyard");
            if (door != null)
            {
                camera.SetDialogueCue(null);
                camera.SnapBehindTarget(player.transform.eulerAngles.y);
                // The doorway faces the courtyard (+X); she leaves through it.
                yield return WalkPlayer(door.transform.position + Vector3.right * 0.9f, 1.6f, 2.5f);
            }
            yield return _director.Hud.Fade(1f, _director.Autopilot ? 0f : 0.35f);
            Vector3 entry = _courtyard.position - Vector3.right * 1.6f;
            player.Teleport(entry, Quaternion.LookRotation(_courtyard.position - entry));
            if (anchor != null) camera.CutTo(anchor.transform);
            if (_director.Audio != null) _director.Audio.SetAmbience("courtyard");
            StartCoroutine(_director.Hud.Fade(0f, _director.Autopilot ? 0f : 0.6f));
            yield return WalkPlayer(_courtyard.position, 1.4f, 2f);
            player.Teleport(_courtyard.position, _courtyard.rotation);
        }

        /// <summary>Walks the player (with the walk animation) to a point, giving up after <paramref name="limit"/> seconds.</summary>
        private IEnumerator WalkPlayer(Vector3 target, float speed, float limit)
        {
            var player = _director.Player;
            player.ScriptedSpeed = speed;
            for (float t = 0f; t < limit && !_director.Autopilot; t += Time.deltaTime)
            {
                Vector3 to = Vector3.ProjectOnPlane(target - player.transform.position, Vector3.up);
                if (to.magnitude < 0.12f) break;
                player.ScriptedMove = to.normalized;
                yield return null;
            }
            player.ScriptedMove = null;
        }

        /// <summary>That afternoon: rain on the empty courtyard, the mother on the steps.</summary>
        private IEnumerator Rain()
        {
            // Fast dialogue input can reach the rain tag while the escort is still crossing the yard. Do not remove
            // Vương Ông and Vương Quan until the guards have actually taken them through the gate.
            while (_director.CurrentSequence == "arrest") yield return null;
            yield return _director.Hud.Fade(1f, 0.6f);
            foreach (var official in _officials) official.gameObject.SetActive(false);
            _father.gameObject.SetActive(false);
            _brother.gameObject.SetActive(false);
            _mother.Place(_motherRainSpot.position, _motherRainSpot.rotation);
            // Grey-teal afternoon rain, as in the painted panels: soft and readable rather than blue.
            if (_sun != null) { _sun.intensity = 0.5f; _sun.color = new Color(0.7f, 0.74f, 0.78f); }
            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.5f);
            RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.62f);
            RenderSettings.fogDensity = 0.03f;
            if (_rain != null) _rain.Play();
            if (_rainProps != null) _rainProps.SetActive(true);
            if (_director.Fx != null) _director.Fx.SetMood("tension");
            if (_director.Audio != null) _director.Audio.SetAmbience("rain");
            _director.Player.Teleport(_courtyard.position, _courtyard.rotation);
            _director.CameraController.SnapBehindTarget(_courtyard.eulerAngles.y);
            yield return _director.Hud.Fade(0f, 0.9f);
        }

        public System.Collections.Generic.IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int opinion = (int)v["pl_che_kieu"];
            yield return "Bạn từng nghĩ về Kiều: " + (opinion == 1 ? "\"Dễ mà, mình sẽ làm tốt hơn.\"" : opinion == 2 ? "\"Phải có cách khác.\"" : "\"Kiều cam chịu quá.\"");
            yield return "Căn phòng của Kiều: đã xem " + (int)v["pl_vat"] + "/3 kỷ vật.";
            int act = (int)v["pl_hanh_dong"];
            yield return "Khi quan sai đến: " + (act == 1 ? "bạn lao ra chắn trước mặt cha." : act == 2 ? "bạn đòi xem đơn, hỏi tội danh." : "bạn đứng yên, ghi nhớ từng gương mặt.");
            yield return "<i>Chương 1 — Gia biến: đêm ấy, Mã Giám Sinh đến gõ cửa.</i>";
        }
    }
}

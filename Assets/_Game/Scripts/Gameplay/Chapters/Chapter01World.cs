using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Sequences;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters
{
    /// <summary>
    /// Chapter 1 — the rainy night at the Vương house. Stages the family and Mã Giám Sinh for each Ink scene,
    /// brings Mã Giám Sinh in after one purposeful investigation, and runs the risky silver-inspection beat.
    /// </summary>
    public class Chapter01World : MonoBehaviour, IChapterSummary
    {
        private const string Negotiation = "VuongGia_DamPhan";

        [SerializeField] private StoryDirector _director;
        [SerializeField] private StoryActor _mother;
        [SerializeField] private StoryActor _ma;
        [SerializeField] private StoryActor _servant;
        [SerializeField] private ObserveMode _observe;
        [Tooltip("The bag of silver on the contract table, shown once Mã Giám Sinh has put it down.")]
        [SerializeField] private GameObject _silver;
        [Tooltip("The written contract and its seal, laid on the table when he brings it out.")]
        [SerializeField] private GameObject _contract;
        [SerializeField] private Transform _kieuMeet, _motherMeet, _maMeet, _servantPorch;
        [SerializeField] private Transform _motherTable, _maTable, _tableFocus;
        [SerializeField] private Transform _kieuDoor, _motherDoor, _maGate, _servantGate;

        private string _scene = string.Empty;
        private float _glanceTimer = 6f, _glanceHold, _barkCooldown, _suspicionCarry;
        private bool _knocked, _caughtObserving, _safeCueShown;
        private ObservableTarget _silverObservation;
        private StoryActor _kieu;
        private Coroutine _action, _arrival, _motherRoutine;
        private bool _motherSeated;
        private Transform _cabinet, _cabinetReplacement, _leftDoor, _rightDoor;
        private Transform _gateLeftHinge, _gateRightHinge;
        private Renderer[] _cabinetRenderers;
        private Pose _contractRest;
        private Vector3 _contractCentreOffset;

        public bool Glancing => _glanceHold > 0f;

        private void OnEnable()
        {
            _director.SceneChanged += OnScene;
            _director.CheckpointRestored += OnRestored;
            _director.TagReceived += OnTag;
        }

        private void OnDisable()
        {
            _director.SceneChanged -= OnScene;
            _director.CheckpointRestored -= OnRestored;
            _director.TagReceived -= OnTag;
        }

        private void Start()
        {
            _kieu = _director.Player.GetComponent<StoryActor>();
            _director.Player.AllowCrouch = false;
            _director.Hud.SetControls("WASD: đi · Shift: chạy · Chuột phải: quan sát · E: tương tác · J: sổ tay");
            // Mã Giám Sinh only arrives later; a resumed save replays its scene tag and stages him again.
            ArrangeInterior();
            PrepareGate();
            PrepareCabinet();
            ClearCabinetDoorway();
            DressSilverBag();
            PrepareSilverObservation();
            RefreshSuspicionTutorial();
            CacheContractPose();
            PlaceTableLamp();
            PlaceHairpin();
            // Mã Giám Sinh is all polite composure: on neutral lines he nods along instead of throwing his arms wide.
            _ma.CalmGesture = StoryDirector.Gesture("satisfied");
            _kieu.FitVisualHeight(1.72f);
            // Everyone else is sized against Kiều by the head bone: whole-model bounds count hats and hairpins and made
            // Mã Giám Sinh tower over her.
            float kieu = _kieu.HeadHeight;
            _mother.FitHeadHeight(kieu);
            _ma.FitHeadHeight(kieu * 1.07f);
            _servant.FitHeadHeight(kieu * 1.05f);
            if (_scene.Length == 0) { SetGuests(false); _contract.SetActive(false); }
        }

        private void OnTag(string key, string value)
        {
            if (key == "camera" && value == "OTS_Kieu_Cabinet") Begin(OpenCabinet());
            if (key == "camera" && value == "OTS_Kieu_Altar") Begin(LightIncense());
            if (key == "camera" && value == "MCU_MeKieu" && _scene == "VuongGia_DemMua") Begin(SitWithMother());
            if (key == "camera" && value == "Hug_MeKieu") Begin(Embrace());
            if (key == "gameplay" && value == "document_khe") Begin(InspectContract());
        }

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (!_knocked && _director.IsExploreAvailable("c1_cong"))
            {
                _knocked = true;
                if (_director.Audio != null) _director.Audio.PlaySfx("knock");
                Bark("NoiTam", "Có tiếng gõ cổng. Giữa đêm mưa thế này... là hắn.");
                MotherDo(MotherRises());
            }
            if (_scene == Negotiation && _director.IsExploreAvailable("c1_tui_bac")) UpdateSuspicion();
        }

        // ------------------------------------------------------------------ suspicion

        private void UpdateSuspicion()
        {
            Vector3 player = _director.Player.transform.position;
            float distance = Flat(player - _ma.transform.position);
            if (_glanceHold > 0f)
            {
                _glanceHold -= Time.deltaTime;
                _ma.FaceTowards(player);
            }
            else
            {
                _ma.FaceTowards(_tableFocus.position);
                if ((_glanceTimer -= Time.deltaTime) <= 0f)
                {
                    _glanceTimer = Random.Range(6f, 10f);
                    _glanceHold = 2f;
                }
            }

            bool watched = Glancing || Vector3.Dot(_ma.transform.forward, (player - _ma.transform.position).normalized) > 0.6f;
            bool inspectingSilver = _observe != null && _observe.IsObserving && _observe.Focus == _silverObservation;
            float rate = -4f;
            if (inspectingSilver && watched && distance < 11f)
            {
                rate = _director.GetInt("c1_nghi_ngo") >= 95 ? 0f : 26f;
                _observe.CancelFocus(_silverObservation);
                if (!_caughtObserving)
                {
                    _caughtObserving = true;
                    _safeCueShown = false;
                    Bark("MaGiamSinh", "Cô nương đang nhìn trộm bạc của ta sao?");
                }
            }
            else if (inspectingSilver)
            {
                rate = 0f;
                if (!_safeCueShown)
                {
                    _safeCueShown = true;
                    Bark("NoiTam", "Hắn đang quay đi. Nhìn dấu trên nén bạc, nhanh lên.");
                }
            }
            else if (!watched)
            {
                _caughtObserving = false;
                _safeCueShown = false;
            }

            int value = _director.GetInt("c1_nghi_ngo");
            _suspicionCarry += rate * Time.deltaTime;
            int step = (int)_suspicionCarry;
            if (step != 0)
            {
                _suspicionCarry -= step;
                value = Mathf.Clamp(value + step, 0, 100);
                _director.SetInt("c1_nghi_ngo", value);
            }
            float normalized = value / 100f;
            _director.Hud.SetMeter(!watched ? "Mã đang quay đi — có thể quan sát bạc" :
                    value >= 60 ? "MÃ GIÁM SINH ĐANG NGỜ VỰC" : "Nghi ngờ của Mã Giám Sinh",
                normalized, Color.Lerp(new Color(1f, 0.85f, 0.4f), new Color(0.9f, 0.2f, 0.15f), normalized));
        }

        private void OnRestored()
        {
            _director.SetInt("c1_nghi_ngo", Mathf.Min(_director.GetInt("c1_nghi_ngo"), 45));
            _suspicionCarry = 0f;
            _glanceHold = 0f;
            _glanceTimer = 6f;
            _caughtObserving = false;
            _safeCueShown = false;
        }

        private void PrepareSilverObservation()
        {
            if (_silver == null) return;
            _silverObservation = _silver.GetComponent<ObservableTarget>();
            if (_silverObservation == null) _silverObservation = _silver.AddComponent<ObservableTarget>();
            _silverObservation.Configure(_director, "c1_tui_bac", "Dấu trên nén bạc", _silver.transform, 2.2f, 8f);
            if (_observe != null) _observe.RefreshTargets();
        }

        private static readonly string SuspicionTutorial =
            "<color=#E8C27A>Nhìn dấu trên túi bạc</color> khi Mã Giám Sinh quay sang phía Mẹ.\n\n" +
            "Giữ <color=#E8C27A>chuột phải</color> và hướng camera vào túi bạc. Khi hắn nhìn lại, hãy buông ra; bị bắt gặp chỉ làm hắn nghi ngờ hơn, câu chuyện vẫn tiếp tục.\n\n" +
            "Bạn cũng có thể bỏ qua túi bạc và nhấn <color=#E8C27A>E</color> để đối chất ngay.";

        private void RefreshSuspicionTutorial()
        {
            foreach (var tutorial in FindObjectsByType<TutorialSequence>(FindObjectsInactive.Include))
                if (tutorial.Id == "tutorial_suspicion")
                    tutorial.Configure(SuspicionTutorial, "WASD: đi · Chuột phải: quan sát bạc · E: đối chất · J: sổ tay");
        }

        // ------------------------------------------------------------------ staging

        private void OnScene(string scene)
        {
            _scene = scene;
            if (scene != "VuongGia_BanKyKhe") _contract.SetActive(false);
            switch (scene)
            {
                case "VuongGia_DemMua": SetGuests(false); MotherDo(MotherWeeps()); break;
                case "VuongGia_MaDen":
                    if (_arrival != null) StopCoroutine(_arrival);
                    _arrival = StartCoroutine(GuestArrival());
                    break;
                case Negotiation:
                    StartCoroutine(StageNegotiation());
                    break;
                case "VuongGia_BanKyKhe": Begin(PresentContract()); break;
                case "VuongGia_RaDi": StartCoroutine(Stage(() =>
                {
                    SetGuests(true);
                    _silver.SetActive(false);
                    Teleport(_kieuDoor);
                    Place(_mother, _motherDoor);
                    // Mã Giám Sinh waits outside on the lane, clear of the open gate leaves (the gate marks stand where
                    // the leaves swing), so neither he nor Kiều passes through them.
                    SetGateOpen(1f);
                    _ma.Place(new Vector3(0.9f, 0f, -15.4f), Quaternion.LookRotation(Vector3.forward));
                    _servant.Place(new Vector3(-1.1f, 0f, -16f), Quaternion.LookRotation(Vector3.forward));
                }, 1.1f)); break;
            }
        }

        private IEnumerator StageNegotiation()
        {
            while (_arrival != null) yield return null;
            SetGuests(true);
            _silver.SetActive(true);
            _mother.Sit(false);
            _kieu.Sit(false);
            Place(_mother, _motherTable);
            Place(_ma, _maTable);
            Place(_servant, _servantPorch);
        }

        private void SetGuests(bool present)
        {
            _ma.gameObject.SetActive(present);
            _servant.gameObject.SetActive(present);
            _silver.SetActive(present && _scene == Negotiation);
        }

        private void Teleport(Transform point)
        {
            _director.Player.Teleport(point.position, point.rotation);
            _director.CameraController.SnapBehindTarget(point.eulerAngles.y);
        }

        private static void Place(StoryActor actor, Transform point) => actor.Place(point.position, point.rotation);

        private IEnumerator Stage(System.Action move, float fade)
        {
            yield return _director.Hud.Fade(1f, fade * 0.5f);
            move();
            yield return null; // let the moved rigs pose before framing them
            // The line's shot was taken before the people moved: frame them where they now stand.
            if (_director.Shots != null) _director.Shots.Reshoot();
            yield return new WaitForSeconds(_director.Autopilot ? 0f : fade * 0.3f);
            yield return _director.Hud.Fade(0f, fade);
        }

        // ------------------------------------------------------------------ physical story actions

        private void Begin(IEnumerator routine)
        {
            if (_action != null) StopCoroutine(_action);
            foreach (var pose in _director.Player.GetComponents<HandReachPose>()) Destroy(pose);
            _action = StartCoroutine(routine);
        }

        /// <summary>Kiều opens the gate in view; the visitors cross the threshold on foot before the cut indoors.</summary>
        private IEnumerator GuestArrival()
        {
            SetGateOpen(0f);
            SetGuests(true);
            _silver.SetActive(false);
            _contract.SetActive(false);
            _kieu.Sit(false);
            if (_motherSeated) { if (_motherRoutine != null) StopCoroutine(_motherRoutine); ReleaseMother(); }

            // Through the middle of the gateway: the gate marks stand on the posts (x ±1.9), right where the leaves
            // swing open, so walking in along them passed through both. He leads slightly right, the servant follows.
            Vector3 maOutside = new Vector3(0.35f, 0f, _maGate.position.z - 2.2f);
            Vector3 servantOutside = new Vector3(-0.3f, 0f, _maGate.position.z - 3.3f);
            Vector3 maInside = new Vector3(0.6f, 0f, _maGate.position.z + 2.1f);
            Vector3 servantInside = new Vector3(-0.45f, 0f, _maGate.position.z + 1.0f);
            _ma.DisableNavigation();
            _servant.DisableNavigation();
            _ma.Place(maOutside, Quaternion.LookRotation(Vector3.forward));
            _servant.Place(servantOutside, Quaternion.LookRotation(Vector3.forward));

            if (_director.Audio != null)
            {
                _director.Audio.PlaySfx("knock", 1f);
                yield return new WaitForSeconds(_director.Autopilot ? 0f : 0.34f);
                _director.Audio.PlaySfx("knock", 0.9f);
            }

            var camera = _director.CameraController;
            bool cameraLocked = camera.ControlLocked;
            var shot = new GameObject("GateArrivalShot").transform;
            shot.SetPositionAndRotation(new Vector3(3.8f, 1.9f, -9.7f),
                Quaternion.LookRotation(new Vector3(0f, 1.35f, -13.0f) - new Vector3(3.8f, 1.9f, -9.7f)));
            camera.ControlLocked = true;
            camera.CutTo(shot);

            var pose = AddPose(_kieu);
            Vector3 latch = new Vector3(0.25f, 1.18f, -11.72f);
            float reach = _director.Autopilot ? 0.01f : 0.65f;
            for (float t = 0f; t < reach; t += Time.deltaTime)
            {
                pose.SetTargets(null, latch, latch);
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / reach);
                camera.SetDialogueCue(shot);
                yield return null;
            }

            float openDuration = _director.Autopilot ? 0.01f : 1.05f;
            for (float t = 0f; t < openDuration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / openDuration);
                SetGateOpen(k);
                pose.Weight = 1f - k;
                camera.SetDialogueCue(shot);
                yield return null;
            }
            SetGateOpen(1f);
            Destroy(pose);

            // Kiều steps aside out of the gateway to let them in, turned towards them.
            Vector3 from = _director.Player.transform.position;
            Vector3 aside = new Vector3(-1.15f, from.y, from.z + 0.9f);
            float step = _director.Autopilot ? 0.01f : 0.55f;
            for (float t = 0f; t < step; t += Time.deltaTime)
            {
                Vector3 at = Vector3.Lerp(from, aside, Mathf.SmoothStep(0f, 1f, t / step));
                _director.Player.Teleport(at, Quaternion.LookRotation(Vector3.ProjectOnPlane(maOutside - at, Vector3.up)));
                camera.SetDialogueCue(shot);
                yield return null;
            }

            float duration = _director.Autopilot ? 0.01f : 2.8f;
            _ma.SetScriptedSpeed(1.5f);
            _servant.SetScriptedSpeed(1.5f);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                MoveActor(_ma, Vector3.Lerp(maOutside, maInside, k), Vector3.forward);
                MoveActor(_servant, Vector3.Lerp(servantOutside, servantInside, k), Vector3.forward);
                camera.SetDialogueCue(shot);
                yield return null;
            }
            _ma.SetScriptedSpeed(null);
            _servant.SetScriptedSpeed(null);

            yield return _director.Hud.Fade(1f, _director.Autopilot ? 0f : 0.28f);
            Teleport(_kieuMeet);
            Place(_mother, _motherMeet);
            Place(_ma, _maMeet);
            Place(_servant, _servantPorch);
            _ma.EnableNavigation();
            _servant.EnableNavigation();
            _ma.FaceTowards(_kieuMeet.position);
            camera.SetDialogueCue(null);
            camera.ControlLocked = cameraLocked;
            Destroy(shot.gameObject);
            yield return _director.Hud.Fade(0f, _director.Autopilot ? 0f : 0.45f);
            _arrival = null;
        }

        private static void MoveActor(StoryActor actor, Vector3 position, Vector3 direction)
        {
            actor.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        }

        private void MotherDo(IEnumerator routine)
        {
            if (_motherRoutine != null) StopCoroutine(_motherRoutine);
            _motherRoutine = StartCoroutine(routine);
        }

        /// <summary>
        /// The night opens on the mother weeping in her chair at the table, her face in her hands (as in the painted panel).
        /// She stays there while Kiều looks round the house and only rises at the knock.
        /// </summary>
        private IEnumerator MotherWeeps()
        {
            if (!TableSeats(out Vector3 motherSeat, out Quaternion motherFacing, out _, out _)) yield break;
            if (!_motherSeated)
            {
                // The night opens with Kiều standing at her mother's side, not out by the gate she is speaking across. She
                // stands on the mother's own side of the table, in arm's reach: across it she was out of range and the
                // table blocked talking to her.
                Vector3 side = motherSeat + new Vector3(0.25f, 0f, -0.9f);
                _director.Player.Teleport(side, Quaternion.LookRotation(Vector3.ProjectOnPlane(motherSeat - side, Vector3.up)));
                _mother.DisableNavigation();
                _mother.Place(motherSeat, motherFacing);
                yield return null; // the animator needs a frame before the sit trigger takes
                _mother.Sit(true);
                _motherSeated = true;
            }
            _mother.StillWhileSeated = true;
            // Let the sit settle before layering the crying pose over it.
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 0.8f);
            var pose = AddPose(_mother);
            Transform body = _mother.transform;
            // HandReachPose targets the wrists, not the palms. Keep the wrists below and outside the cheeks so the
            // palms cover her face without driving the forearms and finger roots through it. Follow the animated head
            // smoothly: pinning this point once left the hands floating while the seated idle continued to breathe.
            Vector3 head = _mother.HeadPosition;
            Vector3 face = head - Vector3.up * 0.13f + body.forward * 0.11f;
            // Head bowed into her hands.
            Vector3 gaze = head + body.forward * 0.4f - Vector3.up * 0.55f;
            // Her face is towards +body.forward (checked against a render from each side), so the palms aim back along
            // −forward to cover it, fingers up. Elbows hang low and close to her sides: the forearms fold up under the
            // chin instead of spreading into a T.
            pose.SetHandAim(Vector3.up, -body.forward, Vector3.up, -body.forward);
            pose.SetElbowHints(head - body.right * 0.14f - Vector3.up * 0.6f + body.forward * 0.05f,
                head + body.right * 0.14f - Vector3.up * 0.6f + body.forward * 0.05f);
            pose.LeftCurl = 8f;
            pose.RightCurl = 8f;
            for (float t = 0f; _motherSeated; t += Time.deltaTime)
            {
                // The shoulders shake a little with each sob.
                float sob = Mathf.Sin(t * 5.5f) * 0.012f * (0.5f + 0.5f * Mathf.Sin(t * 0.7f));
                Vector3 desiredFace = _mother.HeadPosition - Vector3.up * 0.13f + body.forward * 0.11f;
                face = Vector3.Lerp(face, desiredFace, 1f - Mathf.Exp(-12f * Time.deltaTime));
                Vector3 hands = face - Vector3.up * sob;
                pose.SetTargets(hands - body.right * 0.05f, hands + body.right * 0.05f, gaze);
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / 1.2f);
                yield return null;
            }
        }

        /// <summary>At the knock she lowers her hands, gets up and steps back from the chair, clear of the table.</summary>
        private IEnumerator MotherRises()
        {
            if (!_motherSeated) yield break;
            _motherSeated = false;
            _mother.StillWhileSeated = false;
            foreach (var pose in _mother.GetComponents<HandReachPose>()) Destroy(pose);
            _mother.Sit(false);
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 0.9f);
            Vector3 from = _mother.transform.position;
            Vector3 to = from - _mother.transform.forward * 0.65f;
            Quaternion facing = _mother.transform.rotation;
            float duration = _director.Autopilot ? 0.01f : 0.7f;
            _mother.SetScriptedSpeed(0.9f);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                _mother.Place(Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)), facing);
                yield return null;
            }
            _mother.SetScriptedSpeed(null);
            _mother.EnableNavigation();
            _motherRoutine = null;
        }

        private void ReleaseMother()
        {
            _motherSeated = false;
            _mother.StillWhileSeated = false;
            foreach (var pose in _mother.GetComponents<HandReachPose>()) Destroy(pose);
            _mother.Sit(false);
            _mother.EnableNavigation();
        }

        /// <summary>Kiều sits down beside her weeping mother; her mother stays as she is and Kiều gets up again afterwards.</summary>
        private IEnumerator SitWithMother()
        {
            if (!TableSeats(out _, out _, out Vector3 kieuSeat, out Quaternion kieuFacing)) yield break;
            if (!_motherSeated) MotherDo(MotherWeeps());
            _director.Player.Teleport(kieuSeat, kieuFacing);
            _kieu.Sit(true);
            yield return new WaitForSeconds(_director.Autopilot ? 0f : 0.35f);
            while (!_director.InExplore && _scene == "VuongGia_DemMua") yield return null;
            // She stands up where she sat: the stool is clear of the table. Moving her before the stand-up played slid
            // her off the stool still seated. The Sit To Stand clip carries her hips 35 cm forward and snaps them back
            // when it ends, so the root counters that drift and she rises straight up (the long skirt hides her feet).
            Transform hips = _kieu.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Hips);
            Vector3 anchor = hips.position;
            _kieu.Sit(false);
            float settle = 0.4f;
            for (float t = 0f; (_kieu.IsSitting || settle > 0f) && t < 3f; t += Time.deltaTime)
            {
                if (!_kieu.IsSitting) settle -= Time.deltaTime;
                Vector3 drift = Vector3.ProjectOnPlane(hips.position - anchor, Vector3.up);
                if (drift.sqrMagnitude > 0.0001f)
                    _director.Player.Teleport(_director.Player.transform.position - drift, _director.Player.transform.rotation);
                yield return null;
            }
            _action = null;
        }

        private static readonly Vector3 EmbraceSpot = new Vector3(-0.7f, 0f, -4.4f);

        /// <summary>
        /// "Ôm Mẹ lần cuối": the two stand close and hold each other, seen side-on, until the farewell is over.
        /// </summary>
        private IEnumerator Embrace()
        {
            // The door under the porch is too cramped to film: they meet in the open yard below the steps, Kiều on the
            // gate side (she is leaving), the house behind her mother.
            Transform mother = _mother.transform;
            // Head heights while both still stand naturally (bones lag a frame behind a teleport).
            float motherHead = _mother.HeadPosition.y - mother.position.y;
            float kieuHead = _kieu.HeadPosition.y - _kieu.transform.position.y;
            Vector3 toKieu = Vector3.back;
            Vector3 side = Vector3.Cross(Vector3.up, toKieu);
            Vector3 motherAt = EmbraceSpot, kieuAt = EmbraceSpot + toKieu * 0.36f;
            _mother.Place(motherAt, Quaternion.LookRotation(toKieu));
            _director.Player.Teleport(kieuAt, Quaternion.LookRotation(-toKieu));
            var kieuPose = AddPose(_kieu);
            var motherPose = AddPose(_mother);
            // Fixed targets (recomputing them from the bending bodies each frame made the hands wander): the mother's arms
            // round her daughter's shoulders, the daughter's round her mother's back, elbows out to the sides.
            Vector3 kieuShoulders = kieuAt + Vector3.up * (kieuHead - 0.3f) + toKieu * 0.09f;
            Vector3 motherBack = motherAt + Vector3.up * (motherHead - 0.55f) - toKieu * 0.09f;
            motherPose.SetTargets(kieuShoulders - side * 0.11f, kieuShoulders + side * 0.11f, kieuAt + Vector3.up * kieuHead);
            motherPose.SetElbowHints(motherAt + Vector3.up * (motherHead - 0.45f) - side * 0.4f, motherAt + Vector3.up * (motherHead - 0.45f) + side * 0.4f);
            kieuPose.SetTargets(motherBack + side * 0.12f, motherBack - side * 0.12f, motherAt + Vector3.up * motherHead);
            kieuPose.SetElbowHints(kieuAt + Vector3.up * (kieuHead - 0.55f) + side * 0.38f, kieuAt + Vector3.up * (kieuHead - 0.55f) - side * 0.38f);
            var camera = _director.CameraController;
            bool cameraLocked = camera.ControlLocked;
            camera.ControlLocked = true;
            var shot = new GameObject("EmbraceShot").transform;
            Vector3 focus = (motherAt + kieuAt) * 0.5f + Vector3.up * 1.3f;
            shot.position = EmbraceViewpoint(focus, side, toKieu);
            shot.rotation = Quaternion.LookRotation(focus - shot.position);
            camera.CutTo(shot);
            for (float t = 0f; !_director.InExplore; t += Time.deltaTime)
            {
                kieuPose.Weight = motherPose.Weight = Mathf.SmoothStep(0f, 1f, t / 0.8f);
                camera.SetDialogueCue(shot);
                yield return null;
            }
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                kieuPose.Weight = motherPose.Weight = 1f - t / 0.5f;
                yield return null;
            }
            Destroy(kieuPose);
            Destroy(motherPose);
            camera.SetDialogueCue(null);
            camera.ControlLocked = cameraLocked;
            Destroy(shot.gameObject);
            _action = null;
        }

        /// <summary>
        /// A two-shot of the embrace from whichever side has a clear line to them (the porch has pillars and walls close by):
        /// side-on first, then over each one's shoulder, pulling in closer when the farther spots are blocked.
        /// </summary>
        private static Vector3 EmbraceViewpoint(Vector3 focus, Vector3 side, Vector3 toKieu)
        {
            Vector3[] directions =
            {
                side, -side, (side - toKieu).normalized, (-side - toKieu).normalized, (side + toKieu).normalized, (-side + toKieu).normalized
            };
            foreach (float distance in new[] { 2f, 1.4f, 1f })
                foreach (var direction in directions)
                {
                    Vector3 candidate = focus + direction * distance + Vector3.up * 0.15f;
                    if (!Physics.Linecast(focus, candidate, out _, ~0, QueryTriggerInteraction.Ignore)
                        && !Physics.CheckSphere(candidate, 0.2f, ~0, QueryTriggerInteraction.Ignore))
                        return candidate;
                }
            return focus + side * 1f + Vector3.up * 0.15f;
        }

        private static bool TableSeats(out Vector3 motherSeat, out Quaternion motherFacing, out Vector3 kieuSeat, out Quaternion kieuFacing)
        {
            Transform table = GameObject.Find("ChairsAndContractTable")?.transform;
            motherSeat = kieuSeat = default;
            motherFacing = kieuFacing = Quaternion.identity;
            if (table == null) return false;
            motherSeat = table.TransformPoint(new Vector3(-0.77f, 0f, 0f));
            // Kiều's stool stands further out than her mother's chair: at 0.77 her hips came down on the gap between the
            // table edge and the stool and she sank through it.
            kieuSeat = table.TransformPoint(new Vector3(1.08f, 0.05f, 0f));
            Vector3 centre = table.position;
            motherFacing = Quaternion.LookRotation(Vector3.ProjectOnPlane(centre - motherSeat, Vector3.up));
            kieuFacing = Quaternion.LookRotation(Vector3.ProjectOnPlane(centre - kieuSeat, Vector3.up));
            return true;
        }

        private IEnumerator OpenCabinet()
        {
            if (_cabinet == null || _cabinetReplacement == null) yield break;
            foreach (var renderer in _cabinetRenderers) if (renderer != null) renderer.enabled = false;
            _cabinetReplacement.gameObject.SetActive(true);
            var pose = AddPose(_kieu);
            Vector3 handle = _cabinet.TransformPoint(new Vector3(0.08f, 1.0f, 0.31f));
            float duration = _director.Autopilot ? 0.01f : 0.8f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                pose.SetTargets(null, handle, handle);
                pose.Weight = Mathf.SmoothStep(0f, 1f, t / duration);
                yield return null;
            }
            if (_director.Audio != null) _director.Audio.PlaySfx("creak");
            duration = _director.Autopilot ? 0.01f : 0.9f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                _leftDoor.localRotation = Quaternion.Euler(0f, -112f * k, 0f);
                _rightDoor.localRotation = Quaternion.Euler(0f, 112f * k, 0f);
                pose.Weight = 1f - Mathf.SmoothStep(0f, 1f, k);
                yield return null;
            }
            Destroy(pose);
            _action = null;
        }

        private IEnumerator LightIncense()
        {
            Transform altar = GameObject.Find("FamilyAltar")?.transform;
            if (altar == null) yield break;

            Vector3 stand = altar.position + Vector3.back * 0.78f + Vector3.up * 0.05f;
            Vector3 facing = altar.position - stand;
            facing.y = 0f;
            _director.Player.Teleport(stand, Quaternion.LookRotation(facing));
            yield return null;

            var pose = AddPose(_kieu);
            GameObject incense = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            incense.name = "LitIncense_Runtime";
            Destroy(incense.GetComponent<Collider>());
            incense.transform.localScale = new Vector3(0.008f, 0.14f, 0.008f);
            incense.GetComponent<Renderer>().material.color = new Color(0.42f, 0.12f, 0.06f);
            Vector3 start = _kieu.HeadPosition - Vector3.up * 0.42f + _kieu.transform.forward * 0.28f;
            // The imported altar already contains its incense bowl. Its centre is slightly
            // behind the altar origin, so place the stick into that bowl instead of in front of it.
            Vector3 planted = altar.TransformPoint(new Vector3(0f, 1.05f, 0.12f));
            float duration = _director.Autopilot ? 0.01f : 1.2f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                Vector3 at = Vector3.Lerp(start, planted, k);
                incense.transform.position = at;
                pose.SetTargets(_kieu.transform.TransformPoint(-0.07f, 1.05f, 0.28f), at, planted);
                pose.Weight = Mathf.SmoothStep(0f, 1f, Mathf.Min(k * 2f, 1f));
                yield return null;
            }
            incense.transform.position = planted;
            CreateIncenseSmoke(incense.transform);
            for (float t = 0f; t < (_director.Autopilot ? 0.01f : 0.45f); t += Time.deltaTime)
            {
                pose.Weight = 1f - Mathf.SmoothStep(0f, 1f, t / 0.45f);
                yield return null;
            }
            Destroy(pose);
            _action = null;
        }

        private IEnumerator PresentContract()
        {
            _contract.SetActive(true);
            var pose = AddPose(_ma);
            Vector3 tableCentre = _contractRest.position + _contractCentreOffset;
            Vector3 heldCentre = _ma.HeadPosition - Vector3.up * 0.45f + _ma.transform.forward * 0.32f;
            float duration = _director.Autopilot ? 0.01f : 1.05f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                Vector3 centre = Vector3.Lerp(heldCentre, tableCentre, k);
                _contract.transform.position = centre - _contractCentreOffset;
                pose.SetTargets(null, centre, centre);
                pose.Weight = Mathf.Sin(k * Mathf.PI);
                yield return null;
            }
            _contract.transform.SetPositionAndRotation(_contractRest.position, _contractRest.rotation);
            Destroy(pose);
            _action = null;
        }

        private IEnumerator InspectContract()
        {
            if (!_contract.activeSelf) _contract.SetActive(true);
            for (float t = 0f; _director.CurrentSequence != "document_khe" && t < 30f; t += Time.deltaTime) yield return null;
            if (_director.CurrentSequence != "document_khe") yield break;
            var pose = AddPose(_kieu);
            Vector3 tableCentre = _contractRest.position + _contractCentreOffset;
            Vector3 heldCentre = _kieu.HeadPosition - Vector3.up * 0.38f + _kieu.transform.forward * 0.34f;
            float duration = _director.Autopilot ? 0.01f : 0.75f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                Vector3 centre = Vector3.Lerp(tableCentre, heldCentre, k);
                _contract.transform.position = centre - _contractCentreOffset;
                pose.SetTargets(centre - _kieu.transform.right * 0.13f, centre + _kieu.transform.right * 0.13f, centre);
                pose.Weight = k;
                yield return null;
            }
            while (_director.CurrentSequence == "document_khe")
            {
                heldCentre = _kieu.HeadPosition - Vector3.up * 0.38f + _kieu.transform.forward * 0.34f;
                _contract.transform.position = heldCentre - _contractCentreOffset;
                pose.SetTargets(heldCentre - _kieu.transform.right * 0.13f, heldCentre + _kieu.transform.right * 0.13f, heldCentre);
                pose.Weight = 1f;
                yield return null;
            }
            duration = _director.Autopilot ? 0.01f : 0.65f;
            Vector3 from = _contract.transform.position;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                _contract.transform.position = Vector3.Lerp(from, _contractRest.position, k);
                pose.Weight = 1f - k;
                yield return null;
            }
            _contract.transform.SetPositionAndRotation(_contractRest.position, _contractRest.rotation);
            Destroy(pose);
            _action = null;
        }

        private HandReachPose AddPose(StoryActor actor)
        {
            foreach (var old in actor.GetComponents<HandReachPose>()) Destroy(old);
            var pose = actor.gameObject.AddComponent<HandReachPose>();
            pose.Initialize(actor.GetComponentInChildren<Animator>());
            return pose;
        }

        private void PrepareCabinet()
        {
            _cabinet = GameObject.Find("ScholarCabinet")?.transform;
            if (_cabinet == null) return;
            _cabinetRenderers = _cabinet.GetComponentsInChildren<Renderer>(true);
            Material material = _cabinetRenderers.Length > 0 ? _cabinetRenderers[0].sharedMaterial : null;
            _cabinetReplacement = new GameObject("OpeningCabinet").transform;
            _cabinetReplacement.SetParent(_cabinet, false);
            Box(_cabinetReplacement, "Back", new Vector3(0f, 0.91f, -0.23f), new Vector3(1.04f, 1.82f, 0.08f), material);
            Box(_cabinetReplacement, "LeftSide", new Vector3(-0.5f, 0.91f, 0f), new Vector3(0.08f, 1.82f, 0.52f), material);
            Box(_cabinetReplacement, "RightSide", new Vector3(0.5f, 0.91f, 0f), new Vector3(0.08f, 1.82f, 0.52f), material);
            Box(_cabinetReplacement, "Top", new Vector3(0f, 1.78f, 0f), new Vector3(1.04f, 0.08f, 0.52f), material);
            Box(_cabinetReplacement, "Bottom", new Vector3(0f, 0.05f, 0f), new Vector3(1.04f, 0.1f, 0.52f), material);
            Box(_cabinetReplacement, "Shelf", new Vector3(0f, 0.82f, 0f), new Vector3(0.96f, 0.06f, 0.48f), material);
            _leftDoor = new GameObject("LeftDoorHinge").transform;
            _leftDoor.SetParent(_cabinetReplacement, false);
            _leftDoor.localPosition = new Vector3(-0.5f, 0.92f, 0.29f);
            Box(_leftDoor, "LeftDoor", new Vector3(0.25f, 0f, 0f), new Vector3(0.49f, 1.68f, 0.07f), material);
            _rightDoor = new GameObject("RightDoorHinge").transform;
            _rightDoor.SetParent(_cabinetReplacement, false);
            _rightDoor.localPosition = new Vector3(0.5f, 0.92f, 0.29f);
            Box(_rightDoor, "RightDoor", new Vector3(-0.25f, 0f, 0f), new Vector3(0.49f, 1.68f, 0.07f), material);
            Box(_cabinetReplacement, "KeepsakeBox", new Vector3(-0.2f, 0.9f, 0.02f), new Vector3(0.28f, 0.16f, 0.22f), material);
            _cabinetReplacement.gameObject.SetActive(false);
        }

        private void ArrangeInterior()
        {
            Transform table = GameObject.Find("ChairsAndContractTable")?.transform;
            if (table != null) table.localScale = Vector3.one * 1.15f;

            Transform altar = GameObject.Find("FamilyAltar")?.transform;
            if (altar != null)
            {
                altar.SetPositionAndRotation(new Vector3(0f, 0f, 5.92f), Quaternion.identity);
                altar.localScale = Vector3.one * 1.35f;
                MovePoint("AltarTarget", altar.position + Vector3.back * 0.78f);
                MovePoint("AltarTargetStand", altar.position + Vector3.back * 0.78f + Vector3.up * 0.05f);
                MovePoint("Focus_Altar", altar.position + Vector3.up * 1.22f);
                Transform lamp = GameObject.Find("AltarOilLamp")?.transform;
                if (lamp != null) lamp.position = altar.position + new Vector3(0.41f, 1.22f, -0.04f);
            }

            GameObject screen = GameObject.Find("DecorativeScreen");
            if (screen != null) screen.SetActive(false);
            GameObject bed = GameObject.Find("Bed");
            if (bed != null) bed.SetActive(false);

            GameObject cabinet = GameObject.Find("ScholarCabinet");
            if (cabinet != null && GameObject.Find("SideCabinet") == null)
            {
                GameObject sideCabinet = Instantiate(cabinet, cabinet.transform.parent);
                sideCabinet.name = "SideCabinet";
                sideCabinet.transform.SetPositionAndRotation(new Vector3(-4.75f, 0f, 5.72f), Quaternion.Euler(0f, 180f, 0f));
            }

            MovePoint("MotherTable", new Vector3(-1.35f, 0f, 2.75f));
            // The servant waited a step from the open right door leaf, which then covered every shot of him, and faced
            // the house wall. He waits further along the porch, turned to the door Kiều comes out of.
            GameObject porch = GameObject.Find("ServantPorch");
            if (porch != null)
            {
                var spot = new Vector3(3.5f, 0f, -3.4f);
                porch.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(new Vector3(0f, 0f, -3f) - spot));
                // Kiều asks him face to face, a step in front of him (her old mark faced where he used to stand).
                GameObject ask = GameObject.Find("ServantAskStand");
                if (ask != null)
                {
                    Vector3 stand = spot + porch.transform.forward * 1.25f + Vector3.up * 0.05f;
                    ask.transform.SetPositionAndRotation(stand, Quaternion.LookRotation(-porch.transform.forward));
                }
            }
            MovePoint("MaTable", new Vector3(1.35f, 0f, 2.75f));
            EnsureShot("CAM_OTS_Kieu_Cabinet", new Vector3(3.15f, 1.55f, 4.15f), new Vector3(4.8f, 1.02f, 5.58f));
            EnsureShot("CAM_OTS_Kieu_Altar", new Vector3(-2.15f, 1.62f, 4.28f), new Vector3(0f, 1.18f, 5.95f));
            EnsureShot("CAM_MCU_MeKieu", new Vector3(1.55f, 1.52f, 4.05f), new Vector3(-0.88f, 1.12f, 3.08f));
        }

        private static void MovePoint(string name, Vector3 position)
        {
            GameObject point = GameObject.Find(name);
            if (point != null) point.transform.position = position;
        }

        private void EnsureShot(string name, Vector3 position, Vector3 look)
        {
            GameObject shot = GameObject.Find(name);
            if (shot == null)
            {
                shot = new GameObject(name);
                shot.transform.SetParent(transform, false);
            }
            shot.transform.SetPositionAndRotation(position, Quaternion.LookRotation(look - position));
        }

        private void PrepareGate()
        {
            Transform leftLeaf = GameObject.Find("GateDoor-1")?.transform;
            Transform rightLeaf = GameObject.Find("GateDoor1")?.transform;
            if (leftLeaf == null || rightLeaf == null) return;

            Transform parent = leftLeaf.parent;
            _gateLeftHinge = GameObject.Find("StoryGateLeftHinge")?.transform;
            if (_gateLeftHinge == null)
            {
                _gateLeftHinge = new GameObject("StoryGateLeftHinge").transform;
                _gateLeftHinge.SetParent(parent, true);
                _gateLeftHinge.position = new Vector3(-1.72f, 0f, -13.2f);
                leftLeaf.SetParent(_gateLeftHinge, true);
            }

            _gateRightHinge = GameObject.Find("StoryGateRightHinge")?.transform;
            if (_gateRightHinge == null)
            {
                _gateRightHinge = new GameObject("StoryGateRightHinge").transform;
                _gateRightHinge.SetParent(parent, true);
                _gateRightHinge.position = new Vector3(1.72f, 0f, -13.2f);
                rightLeaf.SetParent(_gateRightHinge, true);
            }
            SetGateOpen(0f);
        }

        private void SetGateOpen(float amount)
        {
            if (_gateLeftHinge != null) _gateLeftHinge.localRotation = Quaternion.Euler(0f, -100f * amount, 0f);
            if (_gateRightHinge != null) _gateRightHinge.localRotation = Quaternion.Euler(0f, 100f * amount, 0f);
        }

        private void ClearCabinetDoorway()
        {
            if (_cabinet == null) return;
            foreach (Transform item in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!item.name.StartsWith("Jar")) continue;
                Vector3 offset = item.position - _cabinet.position;
                offset.y = 0f;
                if (offset.sqrMagnitude < 1.6f) item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The bag of silver the story describes: a tied linen pouch lying on the table with its mouth open, new ingots
        /// (nén bạc) showing in it and a short row beside it, each with the red Phúc Ký stamp. Replaces the blockout ball.
        /// </summary>
        private void DressSilverBag()
        {
            if (_silver == null || _silver.transform.Find("PouchDressed") != null) return;
            Transform old = _silver.transform.Find("Pouch");
            if (old == null) return;
            Vector3 spot = old.position;
            Transform model = _silver.transform.Find("PouchModel");
            foreach (Transform child in _silver.transform) child.gameObject.SetActive(child == model);
            // The generated hemp pouch (placed by the painted-look pass) stands on the real table top.
            if (model != null)
            {
                model.position = new Vector3(spot.x, TableTop(spot), spot.z);
                return;
            }

            var linen = Lit(new Color(0.42f, 0.33f, 0.25f), 0.08f);
            var shadow = Lit(new Color(0.06f, 0.05f, 0.04f), 0.05f);
            var cord = Lit(new Color(0.58f, 0.08f, 0.05f), 0.3f);
            var silver = Lit(new Color(0.86f, 0.86f, 0.88f), 0.85f, 0.9f);
            var stamp = Lit(new Color(0.62f, 0.05f, 0.03f), 0.25f);

            var bag = new GameObject("PouchDressed").transform;
            bag.SetParent(_silver.transform, false);
            bag.SetPositionAndRotation(new Vector3(spot.x, TableTop(spot), spot.z), Quaternion.Euler(0f, 20f, 0f));
            // A heavy cloth sack sitting upright: lopsided body, gathered neck tied with red cord, mouth folded open on top.
            Piece(bag, PrimitiveType.Sphere, new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(0.17f, 0.14f, 0.16f), linen);
            Piece(bag, PrimitiveType.Sphere, new Vector3(0.025f, 0.05f, 0.02f), Vector3.zero, new Vector3(0.15f, 0.1f, 0.14f), linen);
            Piece(bag, PrimitiveType.Cylinder, new Vector3(0f, 0.145f, 0f), Vector3.zero, new Vector3(0.11f, 0.02f, 0.11f), linen);
            Piece(bag, PrimitiveType.Cylinder, new Vector3(0f, 0.138f, 0f), Vector3.zero, new Vector3(0.118f, 0.006f, 0.118f), cord);
            Piece(bag, PrimitiveType.Cylinder, new Vector3(0f, 0.165f, 0f), Vector3.zero, new Vector3(0.15f, 0.006f, 0.15f), linen);
            Piece(bag, PrimitiveType.Cylinder, new Vector3(0f, 0.167f, 0f), Vector3.zero, new Vector3(0.12f, 0.004f, 0.12f), shadow);
            Piece(bag, PrimitiveType.Cube, new Vector3(0.07f, 0.11f, 0.03f), new Vector3(0f, 0f, -55f), new Vector3(0.006f, 0.08f, 0.01f), cord);
            // The new ingots packed inside, their stamped tops showing in the open mouth.
            Ingot(bag, new Vector3(-0.026f, 0.172f, -0.012f), new Vector3(8f, 15f, 0f), silver, stamp);
            Ingot(bag, new Vector3(0.028f, 0.175f, 0.008f), new Vector3(-6f, -12f, 5f), silver, stamp);
            Ingot(bag, new Vector3(0f, 0.168f, 0.035f), new Vector3(10f, 80f, -4f), silver, stamp);
        }

        /// <summary>A boat-shaped silver ingot with raised ends and a red stamp in its hollow.</summary>
        private static void Ingot(Transform parent, Vector3 position, Vector3 euler, Material silver, Material stamp)
        {
            var ingot = new GameObject("Ingot").transform;
            ingot.SetParent(parent, false);
            ingot.localPosition = position;
            ingot.localRotation = Quaternion.Euler(euler);
            ingot.localScale = Vector3.one * 0.8f;
            Piece(ingot, PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(0.07f, 0.02f, 0.038f), silver);
            Piece(ingot, PrimitiveType.Sphere, new Vector3(-0.034f, 0.012f, 0f), Vector3.zero, new Vector3(0.03f, 0.026f, 0.042f), silver);
            Piece(ingot, PrimitiveType.Sphere, new Vector3(0.034f, 0.012f, 0f), Vector3.zero, new Vector3(0.03f, 0.026f, 0.042f), silver);
            Piece(ingot, PrimitiveType.Cube, new Vector3(0f, 0.0105f, 0f), Vector3.zero, new Vector3(0.018f, 0.002f, 0.018f), stamp);
        }

        /// <summary>
        /// The contract: an aged paper sheet written in vertical columns with a vermilion seal and a thumbprint, laid flat
        /// on the table top. The blockout ink strips are hidden.
        /// </summary>
        private void DressContract()
        {
            Transform sheet = null;
            foreach (Transform child in _contract.transform)
            {
                if (child.name.StartsWith("ContractInkLine") || child.name == "ContractSeal") child.gameObject.SetActive(false);
                else if (child.TryGetComponent(out Renderer _)) sheet = child;
            }
            if (sheet == null) return;
            var art = Resources.Load<Texture2D>("Props/ToKhe");
            // The sheet takes the painted page's proportions, so its writing is not stretched.
            if (art != null)
            {
                Vector3 scale = sheet.localScale;
                scale.z = scale.x * art.height / Mathf.Max(1f, art.width);
                sheet.localScale = scale;
            }
            Vector3 size = sheet.lossyScale;
            var paper = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            // Painted art (a flat top-down scan, long side across) wins over the procedural sheet when it is present. It goes on
            // a quad just above the sheet, whose UVs run upright along the long side (the cube's top face is turned 90°).
            paper.SetTexture("_BaseMap", art != null ? null : ContractTexture(size));
            paper.SetColor("_BaseColor", art != null ? new Color(0.86f, 0.78f, 0.6f) : Color.white);
            if (art != null)
            {
                var face = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                face.name = "ToKheArt";
                Destroy(face.GetComponent<Collider>());
                face.SetParent(sheet, false);
                face.localPosition = new Vector3(0f, 0.51f, 0f);
                face.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var artMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                artMaterial.SetTexture("_BaseMap", art);
                artMaterial.SetFloat("_Smoothness", 0.1f);
                face.GetComponent<Renderer>().sharedMaterial = artMaterial;
            }
            paper.SetFloat("_Smoothness", 0.1f);
            sheet.GetComponent<Renderer>().sharedMaterial = paper;
            // Lie on the table actually there (the table was rescaled after the sheet was placed).
            var bounds = sheet.GetComponent<Renderer>().bounds;
            float lift = TableTop(bounds.center) + 0.001f - bounds.min.y;
            _contract.transform.position += Vector3.up * lift;
        }

        /// <summary>
        /// Procedural sheet: warm paper with darkened edges, columns of small brush characters read right to left, and the
        /// signature column at the left closed by a vermilion seal and a thumbprint below the writing. The page is drawn
        /// upright, then laid on the cube's top face, whose texture V runs along the sheet's long (X) side.
        /// </summary>
        private static Texture2D ContractTexture(Vector3 size)
        {
            int across = 1024, down = Mathf.RoundToInt(across * Mathf.Clamp(size.z / Mathf.Max(0.01f, size.x), 0.4f, 1.6f));
            var random = new System.Random(1404);
            var page = new Color[across * down];
            for (int y = 0; y < down; y++)
                for (int x = 0; x < across; x++)
                {
                    float edge = Mathf.Min(Mathf.Min(x, across - 1 - x) / (float)across, Mathf.Min(y, down - 1 - y) / (float)down);
                    float stain = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.07f + Mathf.PerlinNoise(x * 0.06f + 7f, y * 0.06f) * 0.025f;
                    float shade = 0.92f - stain - Mathf.Clamp01(0.05f - edge) * 2.4f;
                    page[y * across + x] = new Color(0.94f * shade, 0.86f * shade, 0.68f * shade);
                }
            void Blot(float cx, float cy, float rx, float ry, Color color, float alpha)
            {
                for (int y = Mathf.FloorToInt(cy - ry); y <= Mathf.CeilToInt(cy + ry); y++)
                    for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++)
                    {
                        if (x < 0 || y < 0 || x >= across || y >= down) continue;
                        float dx = (x - cx) / Mathf.Max(1f, rx), dy = (y - cy) / Mathf.Max(1f, ry);
                        float d = dx * dx + dy * dy;
                        if (d > 1f) continue;
                        int i = y * across + x;
                        page[i] = Color.Lerp(page[i], color, alpha * Mathf.Clamp01((1f - d) * 3f));
                    }
            }
            void Stroke(float x0, float y0, float x1, float y1, float width, Color color)
            {
                int steps = Mathf.CeilToInt(Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1)) / Mathf.Max(1f, width * 0.5f));
                for (int k = 0; k <= steps; k++)
                {
                    float t = k / (float)Mathf.Max(1, steps);
                    // Brush pressure: full in the middle, lifting at the ends.
                    float press = 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI);
                    Blot(Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), width * press, width * press, color, 0.92f);
                }
            }
            void Glyph(float cx, float cy, float size, Color color)
            {
                float h = size * 0.5f, w = size / 15f;
                // Strokes snap to a 3 × 3 grid inside the square, so each mark reads as a character rather than a splash.
                float G(int i) => (i - 1) * h * 0.62f;
                for (int k = 0, strokes = 4 + random.Next(4); k < strokes; k++)
                {
                    int a = random.Next(3), b = random.Next(3);
                    switch (random.Next(5))
                    {
                        case 0: case 1: Stroke(cx - h * 0.8f, cy + G(a), cx + h * 0.8f, cy + G(a) + h * 0.06f, w, color); break;
                        case 2: Stroke(cx + G(a), cy - h * 0.85f, cx + G(a), cy + h * 0.85f, w, color); break;
                        case 3: Stroke(cx + G(a), cy + G(b), cx + G(a) - h * 0.55f, cy + G(b) - h * 0.55f, w, color); break;
                        default: Blot(cx + G(a), cy + G(b), w * 1.3f, w * 1.3f, color, 0.9f); break;
                    }
                }
            }

            var ink = new Color(0.09f, 0.065f, 0.05f);
            int margin = across / 14, columns = 11;
            float step = (across - margin * 2f) / columns, glyph = step * 0.6f, pitch = glyph * 1.22f;
            for (int c = 0; c < columns; c++)
            {
                float cx = across - margin - step * (c + 0.5f);
                bool title = c == 0, signature = c >= columns - 2;
                float letter = title ? glyph * 1.25f : glyph;
                int count = title ? 4 : signature ? 3 : Mathf.FloorToInt((down - margin * 2f) / pitch) - random.Next(2);
                for (int g = 0; g < count; g++)
                    Glyph(cx, down - margin - letter * 0.6f - g * (title ? letter * 1.3f : pitch), letter, ink);
            }
            // Seal and thumbprint under the signature columns, clear of the writing.
            var vermilion = new Color(0.74f, 0.11f, 0.07f);
            float sealSize = step * 1.3f, sealX = margin + step * 0.35f, sealY = margin;
            for (float y = 0; y < sealSize; y++)
                for (float x = 0; x < sealSize; x++)
                {
                    bool border = x < 5 || y < 5 || x > sealSize - 6 || y > sealSize - 6;
                    bool groove = Mathf.PerlinNoise(x * 0.18f, y * 0.18f) > 0.55f;
                    if (border || (!groove && random.NextDouble() < 0.92))
                    {
                        int i = (int)(sealY + y) * across + (int)(sealX + x);
                        page[i] = Color.Lerp(page[i], vermilion, 0.82f);
                    }
                }
            Vector2 print = new Vector2(margin + step * 1.85f + sealSize * 0.5f, margin + sealSize * 0.55f);
            for (int ring = 0; ring < 9; ring++)
            {
                float r = 6f + ring * 3.2f;
                for (float a = 0f; a < Mathf.PI * 2f; a += 0.05f)
                    Blot(print.x + Mathf.Cos(a) * r * 0.75f, print.y + Mathf.Sin(a) * r, 1.3f, 1.3f, vermilion, 0.55f);
            }

            // Lay the upright page onto the face: texture (u, v) = (page down, page across).
            var texture = new Texture2D(down, across, TextureFormat.RGB24, true) { name = "ContractSheet", anisoLevel = 8 };
            var pixels = new Color[down * across];
            for (int y = 0; y < down; y++)
                for (int x = 0; x < across; x++)
                    pixels[x * down + y] = page[y * across + x];
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Height of the table top under a point. The table set has no colliders of its own, so its meshes get temporary ones.
        /// </summary>
        private static float TableTop(Vector3 point)
        {
            Transform table = GameObject.Find("ChairsAndContractTable")?.transform;
            var added = new List<Collider>();
            if (table != null)
                foreach (var filter in table.GetComponentsInChildren<MeshFilter>())
                    if (!filter.TryGetComponent(out Collider _)) added.Add(filter.gameObject.AddComponent<MeshCollider>());
            Physics.SyncTransforms();
            float top = point.y;
            float best = float.MaxValue;
            foreach (var hit in Physics.RaycastAll(new Vector3(point.x, point.y + 1.5f, point.z), Vector3.down, 2.5f, ~0, QueryTriggerInteraction.Ignore))
                // The lowest surface above the floor rails is the top itself; cups and jars stand higher.
                if (hit.point.y > 0.5f && hit.point.y < best && (table == null || hit.transform.IsChildOf(table))) best = hit.point.y;
            foreach (var collider in added) DestroyImmediate(collider);
            return best < float.MaxValue ? best : top;
        }

        /// <summary>
        /// The contract lamp stood right by Kiều's seat and filled her close-ups at the table: it goes to the middle of the
        /// back edge (the altar side), out of the line between the seats and the cameras, still lighting the contract.
        /// </summary>
        private static void PlaceTableLamp()
        {
            var lamp = GameObject.Find("ContractOilLamp");
            Transform table = GameObject.Find("ChairsAndContractTable")?.transform;
            if (lamp == null || table == null) return;
            Vector3 back = table.position + table.forward * 0.42f;
            lamp.transform.position = new Vector3(back.x, lamp.transform.position.y, back.z);
            SetOnTable(lamp);
        }

        /// <summary>Stands a prop on the table top under it (props were placed before the table was rescaled).</summary>
        private static void SetOnTable(GameObject prop)
        {
            if (prop == null) return;
            var renderers = prop.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float top = TableTop(bounds.center);
            if (Mathf.Abs(top - bounds.center.y) < 1f) prop.transform.position += Vector3.up * (top - bounds.min.y);
        }

        /// <summary>
        /// Secret: the gold hairpin Kim Trọng once found in the garden and gave back lies dropped at the foot of the
        /// side cabinet. Nothing points to it but a faint glint; picking it up during the night at home earns his trust.
        /// </summary>
        private void PlaceHairpin()
        {
            if (_director.GetBool("c1_thoa")) return;
            Transform cabinet = GameObject.Find("SideCabinet")?.transform;
            if (cabinet == null) return;
            var hairpin = new GameObject("Secret_ThoaVang").transform;
            hairpin.position = cabinet.position + new Vector3(0.45f, 0.012f, -0.7f);
            hairpin.rotation = Quaternion.Euler(0f, 35f, 0f);
            hairpin.localScale = Vector3.one * 1.6f;
            var gold = Lit(new Color(0.95f, 0.72f, 0.3f), 0.85f, 1f);
            Piece(hairpin, PrimitiveType.Cylinder, Vector3.zero, new Vector3(90f, 0f, 0f), new Vector3(0.007f, 0.085f, 0.007f), gold);
            Piece(hairpin, PrimitiveType.Sphere, new Vector3(0f, 0.008f, 0.09f), Vector3.zero, Vector3.one * 0.028f, gold);
            Piece(hairpin, PrimitiveType.Sphere, new Vector3(0f, 0.018f, 0.09f), Vector3.zero, Vector3.one * 0.014f,
                Lit(new Color(0.35f, 0.7f, 0.5f), 0.9f));
            var glint = new GameObject("Glint").AddComponent<Light>();
            glint.transform.SetParent(hairpin, false);
            glint.transform.localPosition = new Vector3(0f, 0.12f, 0.04f);
            glint.type = LightType.Point;
            glint.color = new Color(1f, 0.82f, 0.45f);
            glint.range = 0.7f;
            glint.shadows = LightShadows.None;
            var trigger = hairpin.gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.2f;
            trigger.center = new Vector3(0f, 0.1f, 0.04f);
            hairpin.gameObject.AddComponent<SecretPickup>().Configure("Nhặt vật gì lấp lánh",
                () => _scene == "VuongGia_DemMua" && _director.InExplore && !_director.GetBool("c1_thoa"),
                () => FindHairpin(hairpin.position));
            StartCoroutine(Twinkle(glint));
        }

        private static IEnumerator Twinkle(Light glint)
        {
            while (glint != null)
            {
                // Mostly dark, now and then a brief catch of lamplight on the gold.
                float t = Time.time % 3.2f;
                glint.intensity = t < 0.5f ? Mathf.Sin(t / 0.5f * Mathf.PI) * 0.9f : 0.08f;
                yield return null;
            }
        }

        private void FindHairpin(Vector3 position)
        {
            _kieu.FaceInstant(position);
            _director.SetBool("c1_thoa", true);
            _director.SetInt("kt_tin_tuong", _director.GetInt("kt_tin_tuong") + 1);
            if (_director.Audio != null) _director.Audio.PlaySfx("memory", 0.6f);
            _director.Hud.ShowToast("BÍ MẬT · CHIẾC THOA VÀNG", "Thoa của Kiều, Kim Trọng nhặt được ở vườn rồi trả lại. Kim Trọng tin tưởng +1");
            _barkCooldown = 0f;
            Bark("KieuThat", "Thoa ấy... chàng Kim trả ta qua bức tường hoa. Giữ giùm ta, đừng để ai thấy.");
        }

        private static Material Lit(Color color, float smoothness, float metallic = 0f)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void Piece(Transform parent, PrimitiveType type, Vector3 position, Vector3 euler, Vector3 scale, Material material)
        {
            var piece = GameObject.CreatePrimitive(type).transform;
            piece.SetParent(parent, false);
            piece.localPosition = position;
            piece.localRotation = Quaternion.Euler(euler);
            piece.localScale = scale;
            piece.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(piece.GetComponent<Collider>());
        }

        private void CacheContractPose()
        {
            DressContract();
            _contractRest = new Pose(_contract.transform.position, _contract.transform.rotation);
            var renderers = _contract.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            _contractCentreOffset = bounds.center - _contract.transform.position;
        }

        private static Transform Box(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            box.name = name;
            box.SetParent(parent, false);
            box.localPosition = localPosition;
            box.localScale = localScale;
            if (material != null) box.GetComponent<Renderer>().sharedMaterial = material;
            box.GetComponent<Collider>().enabled = false;
            return box;
        }

        private static void CreateIncenseSmoke(Transform incense)
        {
            var smokeObject = new GameObject("IncenseSmoke");
            smokeObject.transform.SetParent(incense, false);
            smokeObject.transform.localPosition = Vector3.up * 0.52f;
            smokeObject.transform.localScale = Vector3.one;
            var smoke = smokeObject.AddComponent<ParticleSystem>();
            var main = smoke.main;
            main.startLifetime = 2.6f;
            main.startSpeed = 0.1f;
            main.startSize = 0.035f;
            main.startColor = new Color(0.75f, 0.72f, 0.68f, 0.42f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = smoke.emission;
            emission.rateOverTime = 5f;
            var shape = smoke.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.006f;
            var renderer = smoke.GetComponent<ParticleSystemRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.color = new Color(0.7f, 0.68f, 0.65f, 0.35f);
            renderer.sharedMaterial = material;
            smoke.Play();
        }

        // ------------------------------------------------------------------ helpers

        private void Bark(string speaker, string text)
        {
            if (_barkCooldown > 0f) return;
            _barkCooldown = 4f;
            _director.Hud.ShowBark(speaker, text);
        }

        private static float Flat(Vector3 v) { v.y = 0; return v.magnitude; }

        public IEnumerable<string> SummaryLines(Ink.Runtime.Story story)
        {
            var v = story.variablesState;
            int clues = (int)v["c1_manh_moi"];
            int suspicion = Mathf.Clamp((int)v["c1_nghi_ngo"], 0, 100);
            yield return "Manh mối cốt lõi: " + clues + " / 3";
            yield return "Tỉnh táo: " + (int)v["tinh_tao"];
            yield return (bool)v["doc_ky_khe"] ? "Tờ khế: bạn đã đọc kỹ từng chữ trước khi điểm chỉ." : "Tờ khế: bạn đã ký vội để cứu cha.";
            if ((bool)v["c1_thoa"]) yield return "Bí mật: chiếc thoa vàng của Kim Trọng nằm trong tay áo bạn.";
            if ((bool)v["c1_bac_dau"]) yield return "Bạc dấu Phúc Ký — ai đó đã đưa tiền cho hắn đi \"hỏi vợ\".";
            yield return "Nghi ngờ của Mã Giám Sinh: " + suspicion + "%" + (suspicion >= 60 ? " — hắn sẽ để mắt tới bạn trên đường đi" : " — hắn tin bạn là cô gái ngoan");
        }
    }
}

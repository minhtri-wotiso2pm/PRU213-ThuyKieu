using System;
using ThuyKieu.Core.Story;
using ThuyKieu.Player;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Stealth
{
    public enum WatchState { Patrol, Suspicious, Investigate, Chase, Search, Return, Scripted }

    /// <summary>
    /// A person keeping watch in the first-person chapters: patrol → suspicious → investigate → chase → search → back to the
    /// round, with no knowledge he has not earned.
    /// <list type="bullet">
    /// <item>Sight follows <see cref="SightModel"/> and always needs a clear line from his eyes to her head or chest.</item>
    /// <item>Noises (<see cref="Noise"/>) can make him look or come over to check, never chase on their own.</item>
    /// <item>He knows a hiding place only if he saw her go into it. Otherwise he searches around where he last saw her.</item>
    /// <item>He catches her only within arm's reach, in sight, on the same side of any wall.</item>
    /// </list>
    /// Someone without <c>_canChase</c> (Tú Bà) shouts for <c>_ally</c> instead of running.
    /// </summary>
    [RequireComponent(typeof(StoryActor))]
    public class Watchman : MonoBehaviour
    {
        public const float SuspiciousAt = 0.3f, InvestigateAt = 0.65f, NoiseCap = 0.64f;

        [Header("Round")]
        [SerializeField] private Transform[] _waypoints;
        [Tooltip("Seconds spent at each waypoint (falls back to Pause).")]
        [SerializeField] private float[] _pauses;
        [SerializeField] private float _pause = 2f;
        [SerializeField] private bool _drunk;
        [SerializeField] private float _patrolSpeed = 1.1f;
        [SerializeField] private float _investigateSpeed = 1.7f;
        [SerializeField] private float _chaseSpeed = 3.3f;

        [Header("Senses")]
        [SerializeField] private Transform _eyes;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _range = 9f;
        [SerializeField] private float _halfAngle = 40f;
        [SerializeField] private LayerMask _walls = 1 << 8;
        [Tooltip("Daytime: only someone in a forbidden place is worth watching.")]
        [SerializeField] private bool _onlyInRestrictedZones;
        [Tooltip("Night: hearing follows what he is doing (see Attention), and soft sounds must come three times to bring him over.")]
        [SerializeField] private bool _carefulListener;
        [Tooltip("Side vision: half-angle in degrees and reach in metres (0 = the shared defaults of SightModel).")]
        [SerializeField] private float _peripheralHalfAngle, _peripheralRange;
        [Tooltip("Within this distance someone in plain view beside him is as plain as in front (0 = only in front; see SightModel).")]
        [SerializeField] private float _nearRange;
        [SerializeField] private float _nearFloor;
        [Tooltip("Someone bumping into him from behind is felt: he turns round to look.")]
        [SerializeField] private bool _touchSense;
        [Tooltip("After walking to check where a thrown pebble landed and finding nothing, he looks about this many spots there before going back (0 = he goes straight back).")]
        [SerializeField] private int _lureChecks;

        [Header("Role")]
        [SerializeField] private bool _canChase = true;
        [SerializeField] private Watchman _ally;
        [Tooltip("When he gives chase he calls the ally to where he saw her (the ally must find her himself).")]
        [SerializeField] private bool _callAllyOnChase;
        [SerializeField] private float _searchSeconds = 20f;
        [Tooltip("Seconds out of sight before a chase becomes a search (once he is at the last place he knew).")]
        [SerializeField] private float _lostSeconds = 3f;
        [SerializeField] private float _alertSeconds = 45f;
        [SerializeField] private float _catchDistance = 1.3f;

        [Header("Voice")]
        [SerializeField] private string _speakerId = "NguoiCanh";
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private string[] _suspiciousLines;
        [SerializeField] private string[] _investigateLines;
        [SerializeField] private string[] _chaseLines;
        [SerializeField] private string[] _searchLines;
        [SerializeField] private string[] _giveUpLines;

        private StoryActor _actor;
        private NavMeshAgent _agent;
        private int _index;
        private float _wait, _scan, _stateTime, _lookTime, _lastSeen = -99f, _alert, _barkCooldown, _repath, _softNoiseAt = -99f, _alarmCooldown;
        private Vector3 _target, _searchCentre, _searchPoint;
        private int _searchLeft, _softCount;
        private bool _checking, _paused, _entered, _lure;
        private float _touchCooldown, _searchLimit = 20f, _searchReach = 5f, _noticedAt = -99f;
        private readonly System.Collections.Generic.List<float> _lures = new System.Collections.Generic.List<float>();
        private float _stuck;
        private Action _scriptArrived;
        private NavMeshPath _path;
        private NavMeshQueryFilter Filter => new NavMeshQueryFilter { agentTypeID = _agent != null ? _agent.agentTypeID : 0, areaMask = NavMesh.AllAreas };

        public WatchState State { get; private set; } = WatchState.Patrol;
        public float Awareness { get; private set; }
        public bool SeesPlayer { get; private set; }
        public Vector3 LastKnown { get; private set; }
        /// <summary>Senses on. Off while asleep or part of a scene the story controls.</summary>
        public bool Perceiving { get; set; } = true;
        /// <summary>Scales how fast he grows sure (the chapter's wariness, a drunk night).</summary>
        public float Sensitivity { get; set; } = 1f;
        public HideSpot KnownHide { get; private set; }
        public bool CanChase => _canChase;
        public Transform Eyes => _eyes != null ? _eyes : transform;
        public StoryActor Actor => _actor;
        public int WaypointIndex => _index;
        /// <summary>The stops of his current round.</summary>
        public Transform[] Waypoints => _waypoints;
        public bool IsAlert => _alert > 0f;
        /// <summary>The other watcher he calls (Tú Bà's guard; at night after the alarm, his fellow guard).</summary>
        public Watchman Ally { get => _ally; set => _ally = value; }
        /// <summary>A door in his way is being opened (<see cref="DoorUser"/>): his senses work, his walking waits.</summary>
        public bool HeldAtDoor { get; set; }
        /// <summary>Frozen by the chapter (dialogue, a set piece).</summary>
        public bool Paused => _paused;
        /// <summary>Calls <see cref="Ally"/> to where he saw her when he gives chase (Chapter 4 after the alarm).</summary>
        public bool CallsAlly { get => _callAllyOnChase; set => _callAllyOnChase = value; }
        public int LureChecks { get => _lureChecks; set => _lureChecks = value; }
        public bool TouchSense { get => _touchSense; set => _touchSense = value; }
        public float HalfAngle => _halfAngle;
        public float Range => _range;
        public float PeripheralHalfAngle => _peripheralHalfAngle;
        public float PeripheralRange => _peripheralRange;
        public float NearRange => _nearRange;
        public float NearFloor => _nearFloor;

        /// <summary>
        /// Sets how he sees (QA: the sight probe measures the old shared values and this chapter's own on the same rig, the
        /// same spot, the same light). Zeros mean the shared defaults of <see cref="SightModel"/>.
        /// </summary>
        public void SetSight(float halfAngle, float peripheralHalf, float peripheralRange, float nearRange, float nearFloor, bool touch)
        {
            _halfAngle = halfAngle;
            _peripheralHalfAngle = peripheralHalf;
            _peripheralRange = peripheralRange;
            _nearRange = nearRange;
            _nearFloor = nearFloor;
            _touchSense = touch;
        }
        /// <summary>Thrown pebbles he has walked off to check in the last <see cref="LureWindow"/> seconds.</summary>
        public int RecentLures { get { _lures.RemoveAll(t => Time.time - t > LureWindow); return _lures.Count; } }
        /// <summary>After this many lures within <see cref="LureWindow"/> seconds he stops leaving his post for a pebble: he only looks that way.</summary>
        public const int LureLimit = 3;
        public const float LureWindow = 300f;

        /// <summary>Where the last sound he heard came from (not where she is now).</summary>
        public Vector3 LastHeard { get; private set; }
        /// <summary>Where he is walking to check (investigate) or searching around.</summary>
        public Vector3 Target => State == WatchState.Search ? _searchCentre : _target;

        public event Action<Watchman> Caught;
        public event Action<Watchman, Noise.Event> Heard;
        public event Action<Watchman, WatchState> StateChanged;

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _agent = GetComponent<NavMeshAgent>();
            _path = new NavMeshPath();
        }

        private void OnEnable()
        {
            Noise.Emitted += OnNoise;
            HideSpot.Entered += OnHideEntered;
            HideSpot.Exited += OnHideExited;
        }

        private void OnDisable()
        {
            Noise.Emitted -= OnNoise;
            HideSpot.Entered -= OnHideEntered;
            HideSpot.Exited -= OnHideExited;
        }

        // She came out of the place he saw her go into: it is empty now, he no longer knows where she is.
        private void OnHideExited(HideSpot spot)
        {
            if (spot != KnownHide) return;
            KnownHide = null;
            _checking = false;
        }

        // ------------------------------------------------------------------ control from the chapter

        /// <summary>Freezes him where he stands (dialogue, the chapter's set pieces) without losing what he was doing.</summary>
        public void Pause(bool paused)
        {
            if (paused == _paused) { if (paused) _actor.Stop(); else Resume(); return; }
            _paused = paused;
            if (paused) { _pausedAt = Time.time; _actor.Stop(); return; }
            // What he knew keeps its age: a conversation does not make the last sighting older.
            float held = Time.time - _pausedAt;
            _lastSeen += held;
            _softNoiseAt += held;
            Resume();
        }

        private float _pausedAt;

        /// <summary>Puts him back on his round at a waypoint, calm: checkpoints and chapter starts.</summary>
        public void ResetAt(int waypoint, float pause = 1.5f)
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            _index = Mathf.Clamp(waypoint, 0, _waypoints.Length - 1);
            _actor.Place(_waypoints[_index].position, _waypoints[_index].rotation);
            Forget();
            _wait = pause;
        }

        /// <summary>Clears what he has noticed (after a checkpoint restore) without moving him.</summary>
        public void Forget()
        {
            Awareness = 0f;
            SeesPlayer = false;
            KnownHide = null;
            _checking = false;
            _scriptArrived = null;
            _alert = 0f;
            _alarmCooldown = 0f;
            _softNoiseAt = -99f;
            _softCount = 0;
            _lure = false;
            _lures.Clear();
            // What he saw before the checkpoint is no longer "just now".
            _lastSeen = _noticedAt = -99f;
            _touchCooldown = 0f;
            var hands = GetComponent<DoorUser>();
            if (hands != null) hands.Cancel();
            HeldAtDoor = false;
            SetState(WatchState.Patrol, "forgotten (checkpoint, reset)");
        }

        /// <summary>Swaps his round (a guard sent to search the west wing after the alarm).</summary>
        public void SetRoute(Transform[] waypoints, float[] pauses)
        {
            _waypoints = waypoints;
            _pauses = pauses;
            _index = 0;
        }

        /// <summary>
        /// A new round, joined on foot from where he is (no jump): he walks to its nearest stop unless he is busy with
        /// something more pressing (a chase, a check, a search, a story walk), after which he joins it anyway.
        /// </summary>
        public void Rejoin(Transform[] waypoints, float[] pauses)
        {
            SetRoute(waypoints, pauses);
            _wait = 0f;
            if (State == WatchState.Patrol || State == WatchState.Suspicious || State == WatchState.Return) SetState(WatchState.Return, "new round");
        }

        /// <summary>Becomes wary for a while: notices faster and has just been told something is wrong.</summary>
        public void MakeAlert(float seconds) { _alert = Mathf.Max(_alert, seconds); }

        /// <summary>
        /// Walks somewhere for the story (a door to check). Seeing her still overrides the walk, and a chase in progress is
        /// not interrupted (returns false). <paramref name="arrived"/> is called only once he is really there; if he cannot
        /// get there he gives up the walk and goes back to his round without it.
        /// </summary>
        public bool Script(Vector3 destination, float speed, Action arrived)
        {
            if (State == WatchState.Chase) { if (Tracing) Trace(State, State, "story walk refused: chasing"); return false; }
            destination = SafeZone.Porch(destination);
            var hands = GetComponent<DoorUser>();
            if (hands != null) hands.Cancel();
            _target = destination;
            _scriptArrived = arrived;
            SetState(WatchState.Scripted, "story walk");
            _actor.MoveTo(destination, speed);
            return true;
        }

        public void EndScript(bool resumeRound = true)
        {
            _scriptArrived = null;
            if (State == WatchState.Scripted) SetState(resumeRound ? WatchState.Return : WatchState.Patrol, "story walk ended");
        }

        /// <summary>Someone shouted for him: he hurries to the spot and looks, ready to chase what he finds (not mid-chase).</summary>
        public void Alarm(Vector3 point)
        {
            if (State == WatchState.Chase) return;
            Awareness = Mathf.Max(Awareness, InvestigateAt);
            point = SafeZone.Porch(point);
            LastKnown = point;
            Investigate(point, 4f);
        }

        /// <summary>Still checking, but somewhere else now (a newer sound): no new bark, no restart of the state.</summary>
        private void Retarget(Vector3 point)
        {
            if (Tracing) Trace(State, State, "newer sound, going there instead");
            _target = point;
            _lookTime = 4f;
            _scan = 0f;
            _actor.MoveTo(point, _investigateSpeed);
        }

        public void Investigate(Vector3 point, float lookSeconds)
        {
            // Kiều's room is hers: whatever brought him, he looks from its doorway.
            point = SafeZone.Porch(point);
            _lure = false;
            _target = point;
            _lookTime = lookSeconds;
            SetState(WatchState.Investigate, "going to check");
            _actor.MoveTo(point, _investigateSpeed);
            Bark(_investigateLines);
        }

        // ------------------------------------------------------------------ senses

        /// <summary>Whether a watcher who sees her at this moment, this aware, knows the place she slips into.</summary>
        public static bool WitnessesHiding(bool seesNow, float awareness) => seesNow && awareness >= SuspiciousAt;

        private void OnHideEntered(HideSpot spot)
        {
            if (Perceiving && WitnessesHiding(SeesPlayer, Awareness)) KnownHide = spot;
        }

        /// <summary>
        /// How far sounds carry for him now (night listeners only): keyed up while checking or searching, half his mind on
        /// the errand while the story walks him somewhere, ordinary on his round or in a chase.
        /// </summary>
        public float Attention
        {
            get
            {
                if (!_carefulListener) return 1f;
                switch (State)
                {
                    case WatchState.Suspicious: return 1.15f;
                    case WatchState.Investigate:
                    case WatchState.Search: return 1.25f;
                    case WatchState.Scripted: return 0.85f;
                    default: return 1f;
                }
            }
        }

        private void OnNoise(Noise.Event noise)
        {
            if (!Perceiving || _paused || !isActiveAndEnabled) return;
            if (!Noise.Hears(noise, Eyes.position, _walls, Attention)) return;
            LastHeard = noise.Position;
            Heard?.Invoke(this, noise);
            // In a chase he goes by what he sees and by her own sounds; a pebble landing elsewhere does not draw him off.
            if (State == WatchState.Chase) { if (noise.Kind != NoiseKind.Thrown) LastKnown = SafeZone.Porch(noise.Position); return; }
            bool thrown = noise.Kind == NoiseKind.Thrown;
            // Lured too often lately: he only turns his head to a pebble now.
            if (thrown && _lureChecks > 0 && RecentLures >= LureLimit && State != WatchState.Investigate && State != WatchState.Search)
            {
                _target = noise.Position;
                if (State == WatchState.Patrol || State == WatchState.Return) { SetState(WatchState.Suspicious, "another pebble, not going this time"); Bark(_suspiciousLines); }
                return;
            }
            // Night listeners need a third soft sound within the window before they come over; one or two only turn the head.
            if (Time.time - _softNoiseAt >= 4f) _softCount = 0;
            bool repeated = Time.time - _softNoiseAt < 4f && (!_carefulListener || _softCount >= 2);
            if (noise.Radius >= Noise.Loud || repeated)
            {
                if (State == WatchState.Investigate && _carefulListener)
                {
                    // A newer sound he really heard is where to look now (the place he was going is stale), wherever it
                    // is. Close to where he is already going, it only keeps him looking there longer.
                    if (Vector3.Distance(noise.Position, _target) > 2f) Retarget(noise.Position);
                    else _lookTime = Mathf.Max(_lookTime, 4f);
                }
                else if (State != WatchState.Investigate || Vector3.Distance(noise.Position, transform.position) < Vector3.Distance(_target, transform.position))
                {
                    Awareness = Mathf.Clamp(Awareness, SuspiciousAt, NoiseCap);
                    Investigate(noise.Position, thrown && _lureChecks > 0 ? 5f : 4f);
                    if (thrown && _lureChecks > 0) { _lure = true; _lures.Add(Time.time); }
                }
                return;
            }
            _softNoiseAt = Time.time;
            _softCount++;
            Awareness = Mathf.Min(NoiseCap, Awareness + 0.12f);
            if (State == WatchState.Patrol || State == WatchState.Return) { _target = noise.Position; SetState(WatchState.Suspicious, "soft sound"); Bark(_suspiciousLines); }
        }

        private void UpdateSight()
        {
            SeesPlayer = false;
            if (!Perceiving || _player == null || _player.IsHidden || SafeZone.PlayerSafe) return;
            if (_onlyInRestrictedZones && !RestrictedZone.PlayerInside) return;
            Vector3 eye = Eyes.position;
            Vector3 feet = _player.transform.position;
            Vector3 head = feet + Vector3.up * (_player.IsCrouching ? 0.95f : 1.5f);
            Vector3 chest = feet + Vector3.up * (_player.IsCrouching ? 0.6f : 1.05f);
            Vector3 to = head - eye;
            Vector3 flat = new Vector3(to.x, 0f, to.z);
            Vector3 forward = Eyes.forward; forward.y = 0f;
            float light = LightExposure.Enabled ? LightExposure.At(chest) : 0.6f;
            float rate = SightModel.Rate(new SightModel.Input
            {
                Distance = to.magnitude,
                Angle = Vector3.Angle(forward, flat),
                HalfAngle = _halfAngle,
                Range = _range,
                Light = light,
                Crouched = _player.IsCrouching,
                Speed = _player.CurrentSpeed,
                Running = _player.IsRunning,
                PeripheralHalf = _peripheralHalfAngle,
                PeripheralReach = _peripheralRange,
                NearRange = _nearRange,
                NearFloor = _nearFloor,
                FlatDistance = flat.magnitude
            });
            if (rate <= 0f) return;
            bool clear = !Physics.Linecast(eye, head, _walls, QueryTriggerInteraction.Ignore)
                         || !Physics.Linecast(eye, chest, _walls, QueryTriggerInteraction.Ignore);
            if (!clear) return;
            SeesPlayer = true;
            LastKnown = feet;
            _lastSeen = Time.time;
            Awareness = Mathf.Min(1f, Awareness + rate * Sensitivity * (_alert > 0f ? 1.3f : 1f) * Time.deltaTime);
            if (Awareness >= SuspiciousAt) _noticedAt = Time.time;
        }

        // ------------------------------------------------------------------ behaviour

        private void Update()
        {
            if (_barkCooldown > 0f) _barkCooldown -= Time.deltaTime;
            if (_paused) return;
            UpdateSight();
            if (_touchSense) Touch();
            if (!SeesPlayer && State != WatchState.Chase) Awareness = Mathf.Max(0f, Awareness - 0.2f * Time.deltaTime);
            // A door in his way is being opened: what he sees still counts, his walking and deciding wait for the door.
            if (HeldAtDoor) return;
            // Her room is not his to walk into, whatever sent him there: he stops at its door and looks from outside.
            if (_actor.IsMoving && SafeZone.Contains(_actor.Requested)) _actor.MoveTo(SafeZone.Porch(_actor.Requested), _agent != null ? _agent.speed : _investigateSpeed);
            if (_alert > 0f) _alert -= Time.deltaTime;
            _stateTime += Time.deltaTime;
            // Stuck: told to walk, not getting anywhere (another body in a doorway, a goal he cannot reach).
            _stuck = _actor.IsMoving && _actor.Velocity < 0.05f ? _stuck + Time.deltaTime : 0f;

            if (_alarmCooldown > 0f) _alarmCooldown -= Time.deltaTime;
            if (Awareness >= 1f && State != WatchState.Chase)
            {
                if (_canChase)
                {
                    SetState(WatchState.Chase, "sure it is her");
                    Bark(_chaseLines);
                    // The other one is told where she was seen: he comes to look there (and must find her himself).
                    if (_callAllyOnChase && _ally != null && _ally.isActiveAndEnabled && _ally.Perceiving) _ally.Alarm(LastKnown);
                }
                else if (_alarmCooldown <= 0f) RaiseAlarm();
            }
            else if (Awareness >= InvestigateAt && (_canChase || _alarmCooldown <= 0f)
                     && (State == WatchState.Patrol || State == WatchState.Suspicious || State == WatchState.Return || State == WatchState.Scripted))
                Investigate(LastKnown, 3f);
            else if (Awareness >= SuspiciousAt && (State == WatchState.Patrol || State == WatchState.Return) && SeesPlayer)
            { _target = LastKnown; SetState(WatchState.Suspicious, "a glimpse"); Bark(_suspiciousLines); }

            switch (State)
            {
                case WatchState.Patrol: Patrol(); break;
                case WatchState.Suspicious: Suspicious(); break;
                case WatchState.Investigate: InvestigateStep(); break;
                case WatchState.Chase: Chase(); break;
                case WatchState.Search: Search(); break;
                case WatchState.Return: ReturnStep(); break;
                case WatchState.Scripted: ScriptedStep(); break;
            }
        }

        /// <summary>Stuck for longer than a moment: give up on that walk.</summary>
        private bool Stuck => _stuck > 1.5f;

        /// <summary>He got where he was walking, or as near as he can get (stopped, stuck, or at the end of a partial path).</summary>
        private bool Stopped => !_actor.PathPending && (!_actor.IsMoving || Stuck);

        private void ScriptedStep()
        {
            if (_stateTime < 0.3f || !Stopped) return;
            // Told he has arrived only when he is there; a walk that cannot get there is dropped, not reported as done.
            if (Flat2(_actor.Destination - transform.position) <= 1.2f && Flat2(_target - transform.position) <= 1.5f)
            {
                var arrived = _scriptArrived;
                _scriptArrived = null;
                arrived?.Invoke();
            }
            else
            {
                _scriptArrived = null;
                SetState(WatchState.Return, $"story walk cannot get there ({_actor.PathStatus})");
            }
        }

        private void Patrol()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                // Look about at the stop: slowly to his post's facing and either side of it.
                _scan += Time.deltaTime;
                Quaternion facing = _waypoints[_index].rotation * Quaternion.Euler(0f, Mathf.Sin(_scan * 0.9f) * 50f, 0f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 70f * Time.deltaTime);
                if (_wait <= 0f) MoveToWaypoint((_index + 1) % _waypoints.Length);
                return;
            }
            if (!_actor.IsMoving || Stuck)
            {
                _scan = 0f;
                float pause = _pauses != null && _index < _pauses.Length && _pauses[_index] > 0f ? _pauses[_index] : _pause;
                _wait = pause + (_drunk ? UnityEngine.Random.Range(0.3f, 1.8f) : 0f);
            }
        }

        private void MoveToWaypoint(int index)
        {
            _index = index;
            float speed = _patrolSpeed * (_drunk ? UnityEngine.Random.Range(0.88f, 1.05f) : 1f);
            if (!_actor.MoveTo(_waypoints[_index].position, speed)) _wait = 1f;
        }

        private void Suspicious()
        {
            _actor.Stop();
            Face(_target, 160f);
            if (_stateTime > 2.6f && Awareness < SuspiciousAt * 0.5f) { SetState(WatchState.Return, "nothing more"); Bark(_giveUpLines); }
        }

        private void InvestigateStep()
        {
            if (TryCheckKnownHide()) return;
            if (!Stopped) return;
            _lookTime -= Time.deltaTime;
            _scan += Time.deltaTime;
            transform.Rotate(0f, Mathf.Sin(_scan * 2f) * 80f * Time.deltaTime, 0f);
            if (_lookTime > 0f) return;
            // He came because he saw something: look around properly. A noise alone gets a shrug; a pebble (someone threw
            // it) a short look round where it fell first.
            // (Sent by a pebble, only a sighting he really took in counts, not a shape at the edge of his eye.)
            if (Time.time - (_lure ? _noticedAt : _lastSeen) < 12f) BeginSearch(LastKnown, "nothing here, she was seen");
            else if (_lure && _lureChecks > 0) BeginSearch(_target, "a pebble, nobody about: looking round", _lureChecks, 3f);
            else { SetState(WatchState.Return, "checked, nothing there"); Bark(_giveUpLines); }
        }

        private void Chase()
        {
            if (SafeZone.PlayerSafe)
            {
                // She got back into her own room: the chase ends; he looks about outside its door for a while.
                BeginSearch(SafeZone.Porch(LastKnown), "she got into her room");
                return;
            }
            if (_player.IsHidden)
            {
                if (TryCheckKnownHide()) return;
                // She vanished somewhere he did not see: search where she was last seen.
                BeginSearch(LastKnown, "she vanished");
                return;
            }
            if ((_repath -= Time.deltaTime) <= 0f)
            {
                _repath = 0.2f;
                _actor.MoveTo(SeesPlayer ? _player.transform.position : LastKnown, _chaseSpeed);
            }
            if (SeesPlayer && CanCatch()) { _actor.Stop(); Caught?.Invoke(this); return; }
            float lost = Time.time - _lastSeen;
            // Lost her: once at the last place he knew (or as near as he can get), or at most a few seconds later, he
            // searches. He never runs on at a goal he cannot reach.
            if (lost > _lostSeconds && (Stopped || _actor.PathStatus != NavMeshPathStatus.PathComplete))
                BeginSearch(LastKnown, "lost sight of her");
            else if (lost > _lostSeconds + 5f)
                BeginSearch(LastKnown, "lost sight of her a while ago");
            else if (SeesPlayer && Stopped && _actor.PathStatus != NavMeshPathStatus.PathComplete)
                Face(_player.transform.position, 240f); // in sight but out of reach: he stands and watches her
        }

        private void BeginSearch(Vector3 centre, string why, int spots = 3, float reach = 5f)
        {
            // A long look round takes as long as it needs (a pebble's spots), never less than an ordinary search.
            _searchLimit = Mathf.Max(_searchSeconds, spots * 7f);
            _searchReach = reach;
            // Search where he can walk: the last place he knew if he can get there, else as near to it as the floor allows.
            _searchCentre = ReachableNear(SafeZone.Porch(centre));
            _searchLeft = spots;
            _lookTime = 0f;
            Awareness = Mathf.Min(Awareness, NoiseCap);
            SetState(WatchState.Search, why);
            Bark(_searchLines);
            NextSearchPoint();
        }

        /// <summary>The point, if he can walk there; otherwise the nearest place on his way that he can reach.</summary>
        private Vector3 ReachableNear(Vector3 point)
        {
            if (!NavMesh.SamplePosition(point, out NavMeshHit onFloor, 1.5f, Filter)) return transform.position;
            if (!NavMesh.CalculatePath(transform.position, onFloor.position, Filter, _path) || _path.corners.Length == 0) return transform.position;
            return _path.status == NavMeshPathStatus.PathComplete ? onFloor.position : _path.corners[_path.corners.Length - 1];
        }

        /// <summary>
        /// The next place to look: 1.5–5 m from where he lost her (a pebble: within 3 m of where it fell, each looked at longer), that he can walk to by a short way (no detour round a
        /// wall to a spot that only looked close), else the centre itself.
        /// </summary>
        private void NextSearchPoint()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(Mathf.Min(1.5f, _searchReach * 0.5f), _searchReach);
                Vector3 probe = _searchCentre + new Vector3(offset.x, 0f, offset.y);
                if (!NavMesh.SamplePosition(probe, out NavMeshHit hit, 1.5f, Filter)) continue;
                if (SafeZone.Contains(hit.position)) continue;
                if (!NavMesh.CalculatePath(_searchCentre, hit.position, Filter, _path) || _path.status != NavMeshPathStatus.PathComplete) continue;
                if (PathLength(_path) > Flat2(hit.position - _searchCentre) * 1.6f + 1.5f) continue;
                if (!_actor.MoveTo(hit.position, _investigateSpeed)) continue;
                _searchPoint = hit.position;
                _lookTime = _searchReach < 4f ? 3.5f : 2f;
                return;
            }
            _searchPoint = _searchCentre;
            _actor.MoveTo(_searchCentre, _investigateSpeed);
            _lookTime = 2f;
        }

        private static float PathLength(NavMeshPath path)
        {
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return length;
        }

        private void Search()
        {
            if (TryCheckKnownHide()) return;
            if (_stateTime > _searchLimit) { SetState(WatchState.Return, "search time up"); Bark(_giveUpLines); return; }
            // A spot he cannot get to: on to the next one (it does not count as looked at).
            if (Stuck) { NextSearchPoint(); _stuck = 0f; return; }
            if (!Stopped) return;
            _lookTime -= Time.deltaTime;
            _scan += Time.deltaTime;
            transform.Rotate(0f, Mathf.Sin(_scan * 2.4f) * 90f * Time.deltaTime, 0f);
            if (_lookTime > 0f) return;
            if (--_searchLeft <= 0) { SetState(WatchState.Return, "searched every spot"); Bark(_giveUpLines); return; }
            NextSearchPoint();
        }

        private void ReturnStep()
        {
            if (_waypoints == null || _waypoints.Length == 0) { SetState(WatchState.Patrol, "no round"); return; }
            // Once, on the first frame of the state, however long that frame was.
            if (!_entered) { _entered = true; MoveToWaypoint(NearestWaypoint()); return; }
            if (!Stopped) return;
            _alert = _alertSeconds;
            _wait = 1f;
            SetState(WatchState.Patrol, Flat2(_waypoints[_index].position - transform.position) <= 1.5f ? "back on his round" : "back on his round, as near as he could get");
        }

        /// <summary>
        /// He saw her slip into this place: walk to it, yank it open and find her (with a warning rattle first). Only from
        /// beside it: if he cannot get there, he forgets the place and searches as near to it as he can.
        /// </summary>
        private bool TryCheckKnownHide()
        {
            if (KnownHide == null || HideSpot.Current != KnownHide) return false;
            // A hiding place in her own room is not his to search.
            if (SafeZone.Contains(KnownHide.ExitPosition)) { KnownHide = null; _checking = false; return false; }
            Vector3 front = KnownHide.ExitPosition;
            if (!_checking)
            {
                _checking = true;
                _lookTime = 1.5f;
                _actor.MoveTo(front, State == WatchState.Chase ? _chaseSpeed : _investigateSpeed);
                return true;
            }
            if (!Stopped) return true;
            // Beside it: the walk there was whole and ended within reach (the floor near a screen or a wardrobe may stop
            // him a little short of where she stood). A walk cut short by a shut door or a wall is not.
            if (_actor.PathStatus != NavMeshPathStatus.PathComplete || Flat2(front - transform.position) > 1.8f)
            {
                var place = KnownHide;
                KnownHide = null;
                _checking = false;
                BeginSearch(front, $"cannot get to {place.name} ({_actor.PathStatus})");
                return true;
            }
            _actor.Stop();
            Face(KnownHide.transform.position, 240f);
            if (_lookTime == 1.5f) { KnownHide.Rattle(1.5f); Bark(_chaseLines); }
            _lookTime -= Time.deltaTime;
            if (_lookTime <= 0f) { _checking = false; Caught?.Invoke(this); }
            return true;
        }

        private static float Flat2(Vector3 v) { v.y = 0f; return v.magnitude; }

        private bool CanCatch()
        {
            if (SafeZone.PlayerSafe) return false;
            Vector3 offset = _player.transform.position - transform.position;
            offset.y = 0f;
            if (offset.magnitude > _catchDistance) return false;
            Vector3 chest = transform.position + Vector3.up * 1.1f;
            Vector3 hers = _player.transform.position + Vector3.up * 0.9f;
            if (Physics.Linecast(chest, hers, _walls, QueryTriggerInteraction.Ignore)) return false;
            // Same side of every wall: the walk to her must be about as short as the reach.
            if (!NavMesh.CalculatePath(transform.position, _player.transform.position, Filter, _path) || _path.status != NavMeshPathStatus.PathComplete) return false;
            float length = 0f;
            for (int i = 1; i < _path.corners.Length; i++) length += Vector3.Distance(_path.corners[i - 1], _path.corners[i]);
            return length <= 2f;
        }

        /// <summary>Someone who does not run (Tú Bà) stands, points and shouts; the ally comes to where she was seen.</summary>
        private void RaiseAlarm()
        {
            _alarmCooldown = 4f;
            _target = LastKnown;
            SetState(WatchState.Suspicious, "raised the alarm");
            Bark(_chaseLines);
            if (_ally != null) _ally.Alarm(LastKnown);
        }

        /// <summary>The stop of his round he can walk to soonest (by the walk, not as the crow flies through a wall).</summary>
        private int NearestWaypoint()
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                float distance = NavMesh.CalculatePath(transform.position, _waypoints[i].position, Filter, _path) && _path.status == NavMeshPathStatus.PathComplete
                    ? PathLength(_path)
                    : 1000f + Vector3.Distance(transform.position, _waypoints[i].position);
                if (distance < bestDistance) { bestDistance = distance; best = i; }
            }
            return best;
        }

        private void Resume()
        {
            switch (State)
            {
                case WatchState.Patrol: if (_wait <= 0f && _waypoints != null && _waypoints.Length > 0) _actor.MoveTo(_waypoints[_index].position, _patrolSpeed); break;
                case WatchState.Investigate: case WatchState.Scripted: _actor.MoveTo(_target, _investigateSpeed); break;
                case WatchState.Return: _entered = false; break;
                // Back on his way to the spot he was going to search (a pause there must not count as looking at it).
                case WatchState.Search: if (Flat2(_searchPoint - transform.position) > 0.6f) _actor.MoveTo(_searchPoint, _investigateSpeed); break;
                case WatchState.Chase: _repath = 0f; break;
            }
        }

        /// <summary>
        /// She bumped into him (bodies touching, nothing between them): he feels it and turns round to look, as anyone would.
        /// Not sight, and not through a wall: only real contact in the same space.
        /// </summary>
        private void Touch()
        {
            if (_touchCooldown > 0f) { _touchCooldown -= Time.deltaTime; return; }
            if (!Perceiving || _player == null || _player.IsHidden || SafeZone.PlayerSafe || State == WatchState.Chase) return;
            Vector3 offset = _player.transform.position - transform.position;
            if (Mathf.Abs(offset.y) > 1f) return;
            offset.y = 0f;
            if (offset.magnitude > TouchReach) return;
            if (Physics.Linecast(transform.position + Vector3.up * 1.1f, _player.transform.position + Vector3.up * 0.9f, _walls, QueryTriggerInteraction.Ignore)) return;
            _touchCooldown = 2f;
            LastKnown = _player.transform.position;
            // A little over the threshold: the same frame's fading of what he has not seen must not take it back under.
            Awareness = Mathf.Max(Awareness, InvestigateAt + 0.05f);
            if (Tracing) Trace(State, State, "bumped into from behind");
            Investigate(LastKnown, 3f);
        }

        /// <summary>
        /// Two bodies touching: his capsule's radius and hers (with its skin), and a hand's breadth, since her body slides
        /// round his as soon as they meet (0.68 m when neither can be read).
        /// </summary>
        public float TouchReach
        {
            get
            {
                if (_touchReach > 0f) return _touchReach;
                var mine = GetComponent<CapsuleCollider>();
                var hers = _player != null ? _player.GetComponent<CharacterController>() : null;
                _touchReach = mine != null && hers != null ? mine.radius + hers.radius + hers.skinWidth + 0.12f : 0.68f;
                return _touchReach;
            }
        }

        private float _touchReach;

        private void Face(Vector3 point, float speed)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), speed * Time.deltaTime);
        }

        /// <summary>QA: "-traceWatch" notes every change of state with its reason and what he knew (never every frame).</summary>
        public static bool Tracing => _tracing ?? (_tracing = Array.IndexOf(System.Environment.GetCommandLineArgs(), "-traceWatch") >= 0).Value;
        private static bool? _tracing;

        private void Trace(WatchState from, WatchState to, string why)
        {
            string Age(float at) => at < -90f ? "never" : (Time.time - at).ToString("F1") + " s ago";
            StoryAutopilot.Note($"WATCH {name} {from}→{to} ({why ?? "?"}) · at {Flat(transform.position)} · seen {Age(_lastSeen)} at {Flat(LastKnown)} · heard at {Flat(LastHeard)} · target {Flat(_target)} · " +
                                $"path {_actor.PathStatus}{(_actor.PathPending ? " pending" : "")} {_actor.RemainingDistance:F1} m left · {_actor.Velocity:F1} m/s · awareness {Awareness:F2}");
        }

        private static string Flat(Vector3 v) => $"({v.x:F1}, {v.z:F1})";

        private void SetState(WatchState state, string why = null)
        {
            if (Tracing && _actor != null) Trace(State, state, why);
            if (state != WatchState.Investigate && state != WatchState.Chase && state != WatchState.Search) _checking = false;
            State = state;
            _stateTime = 0f;
            _entered = false;
            _stuck = 0f;
            _scan = 0f;
            StateChanged?.Invoke(this, state);
        }

        private void Bark(string[] lines)
        {
            if (_hud == null || lines == null || lines.Length == 0 || _barkCooldown > 0f || _player == null) return;
            if (Vector3.Distance(_player.transform.position, transform.position) > 16f) return;
            _barkCooldown = 3.5f;
            _hud.ShowBark(_speakerId, lines[UnityEngine.Random.Range(0, lines.Length)], 2.6f);
        }
    }
}

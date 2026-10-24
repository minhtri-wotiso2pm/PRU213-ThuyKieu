using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.House;
using ThuyKieu.Player;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Stealth
{
    /// <summary>
    /// A watcher's hands on the house's doors. A shut door carves the NavMesh, so a walk through it comes back partial and
    /// ends at the leaf. When that leaf is a door he may open (<see cref="HouseDoor.NpcMayOpen"/>: unlocked, no story gate,
    /// not Kiều's room, not the gates or the back door), he stops at it, turns to it, pushes it open (a quick shove in a
    /// chase), waits for the floor to clear and walks on to where he was going. A locked door stays a wall: he ends his walk
    /// at it as before.
    /// <list type="bullet">
    /// <item>On his round (patrol, going back to it) he pulls a door he opened himself to behind him once he is through, unless
    /// someone (Kiều, another watcher) stands in its swing; checking or chasing, he leaves it open.</item>
    /// <item>One hand per door (<see cref="HouseDoor.User"/>): a second watcher waits for the first instead of fighting it.</item>
    /// <item>The door sounds for the player like any door, but emits no <see cref="Noise"/>: neither he nor anyone else goes
    /// to check a door a watcher opened.</item>
    /// </list>
    /// While he works a door, <see cref="Watchman.HeldAtDoor"/> holds his walking and deciding; his eyes and ears go on.
    /// </summary>
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(Watchman))]
    public class DoorUser : MonoBehaviour
    {
        private enum Step { None, Approach, Opening, Settling, Through, Closing }

        [Tooltip("How close to the doorway's middle he stops to work the door (metres, on the floor).")]
        [SerializeField] private float _reach = 1.7f;
        [SerializeField] private float _openSeconds = 1.1f;
        [SerializeField] private float _chaseOpenSeconds = 0.4f;
        [Tooltip("After the leaf stops, the NavMesh needs a moment to clear where it stood (carving waits for a still obstacle).")]
        [SerializeField] private float _settleSeconds = 0.65f;
        [Tooltip("Nobody this close to the doorway when he pulls it to.")]
        [SerializeField] private float _clearance = 1.4f;

        private Watchman _watch;
        private StoryActor _actor;
        private NavMeshAgent _agent;
        private Step _step;
        private HouseDoor _door, _toClose;
        private Vector3 _goal, _expected, _lookedGoal;
        private float _speed, _timer, _startSide, _refusedAt = -99f, _lookedAt = -99f;
        private NavMeshPath _path;
        private HouseDoor _refused;

        /// <summary>The door he is working or about to pull to (QA).</summary>
        public HouseDoor Door => _door != null ? _door : _toClose;
        /// <summary>Doors opened and shut by his hand since the scene started (QA).</summary>
        public int Opened { get; private set; }
        public int Closed { get; private set; }
        /// <summary>A door he would have used but may not (locked, story-gated, someone else's hand on it) (QA).</summary>
        public HouseDoor LastRefused => _refused;

        private void Awake()
        {
            _watch = GetComponent<Watchman>();
            _actor = GetComponent<StoryActor>();
            _agent = GetComponent<NavMeshAgent>();
            _path = new NavMeshPath();
        }

        private void OnDisable() { Cancel(); }

        /// <summary>Lets go of any door and any wait (a checkpoint, a story walk that places him).</summary>
        public void Cancel()
        {
            if (_door != null) _door.Release(this);
            if (_toClose != null) _toClose.Release(this);
            _door = _toClose = null;
            _step = Step.None;
            if (_watch != null) _watch.HeldAtDoor = false;
        }

        private void Update()
        {
            if (_watch.Paused || !_watch.isActiveAndEnabled) return;
            switch (_step)
            {
                case Step.None: LookForDoor(); break;
                case Step.Approach: Approach(); break;
                case Step.Opening: Opening(); break;
                case Step.Settling: Settling(); break;
                case Step.Through: Through(); break;
                case Step.Closing: Closing(); break;
            }
        }

        // ------------------------------------------------------------------ a shut door in his way

        private void LookForDoor()
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || _agent.pathPending || !_agent.hasPath) return;
            if (_agent.pathStatus != NavMeshPathStatus.PathPartial) return;
            Vector3 goal = _actor.Requested;
            Vector3 end = _agent.pathEndPosition;
            Vector3 here = transform.position;
            foreach (var door in HouseDoor.All)
            {
                if (door == null || door.IsOpen) continue;
                Vector3 centre = door.Centre;
                if (Flat(centre - here) > _reach || Flat(centre - end) > 1.5f) continue;
                // The door must stand between him and where he is going (not a shut door beside a dead end).
                if (Side(door, here) * Side(door, goal) >= 0f) continue;
                if (!door.NpcMayOpen || (door.User != null && door.User != this))
                {
                    // Not his to open, or another one is working it: a locked door is a wall; a door in use, a moment's wait.
                    if (door.NpcMayOpen && door.User is DoorUser) { Hold(door, goal); _step = Step.Opening; _timer = 4f; return; }
                    if (_refused != door || Time.time - _refusedAt > 5f)
                    {
                        _refused = door;
                        _refusedAt = Time.time;
                        if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} leaves {door.name} shut ({(door.NpcCanUse ? door.Lock != DoorLock.None ? door.Lock.ToString() : "story gate" : "not his to open")})");
                    }
                    continue;
                }
                Open(door, goal);
                return;
            }
            // The walk ends short of the goal and no shut door is at hand: the way may still be through one further on (a
            // partial path stops at the nearest floor it can reach, which can be in the next room, across a wall from the
            // goal). He walks to the near side of the best such door first, then works it.
            if ((goal - _lookedGoal).sqrMagnitude < 0.04f && Time.time - _lookedAt < 1.5f) return;
            _lookedGoal = goal;
            _lookedAt = Time.time;
            var best = DoorOnTheWay(here, goal, end, out Vector3 near);
            if (best == null) return;
            _door = best;
            _goal = goal;
            _speed = _agent.speed;
            _actor.MoveTo(near, _speed);
            _expected = _actor.Requested;
            _watch.HeldAtDoor = true;
            _step = Step.Approach;
            _timer = 20f;
            if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} heads for {best.name} ({_watch.State}): the way to ({goal.x:F1}, {goal.z:F1}) is through it");
        }

        /// <summary>
        /// The shut door, his to open, that stands between him and the goal and gets him nearest to it, if any: its near side
        /// reachable by a whole walk, its far side nearer the goal than where his walk now ends (or that end is across a wall from it). The shortest such way wins.
        /// </summary>
        private HouseDoor DoorOnTheWay(Vector3 here, Vector3 goal, Vector3 end, out Vector3 near)
        {
            near = here;
            HouseDoor best = null;
            float bestCost = float.MaxValue;
            var filter = new NavMeshQueryFilter { agentTypeID = _agent.agentTypeID, areaMask = NavMesh.AllAreas };
            foreach (var door in HouseDoor.All)
            {
                if (door == null || door.IsOpen) continue;
                Vector3 centre = door.Centre;
                if (Flat(centre - goal) > 9f || Side(door, here) * Side(door, goal) >= 0f) continue;
                Vector3 normal = door.transform.right * Mathf.Sign(Side(door, here));
                Vector3 far = centre - normal * 0.9f;
                // Worth it if it gets him nearer, or where his walk ends now is across a wall from the goal (the next room).
                if (Flat(goal - far) > Flat(goal - end) - 0.2f && !Physics.Linecast(end + Vector3.up, goal + Vector3.up, 1 << 8, QueryTriggerInteraction.Ignore)) continue;
                if (!NavMesh.SamplePosition(centre + normal * 0.9f, out NavMeshHit hit, 0.6f, filter)) continue;
                if (!NavMesh.CalculatePath(here, hit.position, filter, _path) || _path.status != NavMeshPathStatus.PathComplete) continue;
                if (!door.NpcMayOpen)
                {
                    if (_refused != door || Time.time - _refusedAt > 5f)
                    {
                        _refused = door;
                        _refusedAt = Time.time;
                        if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} leaves {door.name} shut ({(door.NpcCanUse ? door.Lock != DoorLock.None ? door.Lock.ToString() : "story gate" : "not his to open")})");
                    }
                    continue;
                }
                float length = 0f;
                for (int i = 1; i < _path.corners.Length; i++) length += Vector3.Distance(_path.corners[i - 1], _path.corners[i]);
                float cost = length + Flat(goal - far);
                if (cost < bestCost) { bestCost = cost; best = door; near = hit.position; }
            }
            return best;
        }

        /// <summary>Walking to the near side of the door on his way.</summary>
        private void Approach()
        {
            if (Redirected) { LetGo(); return; }
            _timer -= Time.deltaTime;
            Vector3 here = transform.position;
            bool there = Flat(_door.Centre - here) <= _reach || !_actor.IsMoving && !_actor.PathPending;
            if (!there) { if (_timer <= 0f) Release(); return; }
            var door = _door;
            _door = null;
            if (door.IsOpen) { _watch.HeldAtDoor = false; _step = Step.None; _actor.MoveTo(_goal, _speed); return; }
            if (!door.NpcMayOpen || (door.User != null && !(door.User is DoorUser))) { Release(); return; }
            if (door.User != null && door.User != this) { Hold(door, _goal); _step = Step.Opening; _timer = 4f; return; }
            Open(door, _goal);
        }

        private void Open(HouseDoor door, Vector3 goal)
        {
            Hold(door, goal);
            bool chasing = _watch.State == WatchState.Chase;
            if (!door.OpenBy(this, chasing ? _chaseOpenSeconds : _openSeconds, chasing)) { Release(); return; }
            Opened++;
            _step = Step.Opening;
            _timer = 4f;
            if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} opens {door.name} ({_watch.State}) on the way to ({goal.x:F1}, {goal.z:F1})");
        }

        private void Hold(HouseDoor door, Vector3 goal)
        {
            _door = door;
            _goal = goal;
            if (_step != Step.Approach) _speed = _agent.speed;
            _startSide = Side(door, transform.position);
            _expected = _actor.Requested;
            _actor.Stop();
            _watch.HeldAtDoor = true;
        }

        /// <summary>Something sent him elsewhere while he stood at the door (a sound, a sighting, a story walk).</summary>
        private bool Redirected => (_actor.Requested - _expected).sqrMagnitude > 0.04f;

        private void LetGo()
        {
            if (_door != null) _door.Release(this);
            _door = null;
            _step = Step.None;
            _watch.HeldAtDoor = false;
        }

        private void Opening()
        {
            if (Redirected) { LetGo(); return; }
            Face(_door.Centre);
            _timer -= Time.deltaTime;
            // Another watcher's hand on it: wait for him to finish, then go through his open door (or work it ourselves).
            if (_door.User != null && _door.User != this)
            {
                if (_door.IsOpen && !_door.Moving) { _step = Step.Settling; _timer = _settleSeconds; }
                else if (_timer <= 0f) Release();
                return;
            }
            if (_door.User == null && !_door.IsOpen)
            {
                if (!_door.OpenBy(this, _openSeconds)) { Release(); return; }
                Opened++;
            }
            if (_door.Openness >= 0.95f && !_door.Moving) { _step = Step.Settling; _timer = _settleSeconds; }
            else if (_timer <= 0f) Release();
        }

        private void Settling()
        {
            if (Redirected) { LetGo(); return; }
            Face(_door.Centre);
            if ((_timer -= Time.deltaTime) > 0f) return;
            // On with the walk he was on: through the open doorway now.
            _watch.HeldAtDoor = false;
            _actor.MoveTo(_goal, _speed);
            _expected = _actor.Requested;
            bool routine = _watch.State == WatchState.Patrol || _watch.State == WatchState.Return;
            if (routine && _door.User == this) { _toClose = _door; _step = Step.Through; _timer = 12f; }
            else { _door.Release(this); _step = Step.None; }
            _door = null;
        }

        private void Release()
        {
            LetGo();
            _actor.MoveTo(_goal, _speed);
        }

        // ------------------------------------------------------------------ pulling it to behind him

        private void Through()
        {
            _timer -= Time.deltaTime;
            bool routine = _watch.State == WatchState.Patrol || _watch.State == WatchState.Return;
            // Something more pressing came up, or he never got through: leave it open.
            if (!routine || _timer <= 0f || _toClose.User != this) { Forget(); return; }
            Vector3 here = transform.position;
            if (Side(_toClose, here) * _startSide >= 0f || Flat(_toClose.Centre - here) < 1.6f) return;
            if (SomeoneInTheWay(_toClose)) return;
            if (!_toClose.CloseBy(this)) { Forget(); return; }
            Closed++;
            _step = Step.Closing;
            _timer = 3f;
            if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} pulls {_toClose.name} to behind him");
        }

        private void Closing()
        {
            _timer -= Time.deltaTime;
            // Someone stepped into its swing: open it again rather than shut it on them.
            if (_toClose.Moving && SomeoneInTheWay(_toClose))
            {
                _toClose.Release(this);
                if (_toClose.OpenBy(this, 0.5f)) { if (Watchman.Tracing) StoryAutopilot.Note($"DOOR {name} stops pulling {_toClose.name}: someone in the doorway"); }
                Forget();
                return;
            }
            if (!_toClose.Moving || _timer <= 0f) Forget();
        }

        private void Forget()
        {
            if (_toClose != null) _toClose.Release(this);
            _toClose = null;
            _step = Step.None;
        }

        /// <summary>Kiều or anyone with a NavMesh body near the doorway (the leaf would hit them).</summary>
        private bool SomeoneInTheWay(HouseDoor door)
        {
            Vector3 centre = door.Centre;
            var player = FindPlayer();
            if (player != null && Flat(player.transform.position - centre) < _clearance) return true;
            foreach (var agent in FindObjectsByType<NavMeshAgent>())
                if (agent != _agent && agent.isActiveAndEnabled && Flat(agent.transform.position - centre) < _clearance) return true;
            return false;
        }

        private static PlayerMovement _player;
        private static PlayerMovement FindPlayer() => _player != null ? _player : (_player = FindAnyObjectByType<PlayerMovement>());

        private void Face(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 240f * Time.deltaTime);
        }

        /// <summary>Which side of the doorway a point is on (signed distance through the leaf).</summary>
        private static float Side(HouseDoor door, Vector3 point) => Vector3.Dot(point - door.Centre, door.transform.right);

        private static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
    }
}

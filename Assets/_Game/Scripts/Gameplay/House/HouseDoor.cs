using System;
using System.Collections.Generic;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.House
{
    public enum DoorKind { Hinged, Sliding }

    /// <summary>What holds a door shut. A latch outside or a bolt inside can only be worked from one side.</summary>
    public enum DoorLock { None, Locked, LatchedOutside, Bolted }

    /// <summary>
    /// A door of the house that is really connected: a solid leaf that swings or slides, carves the NavMesh while shut, and
    /// sounds like what the player does to it.
    /// <list type="bullet">
    /// <item>Tap E: snap it open or shut (loud, 6–8 m).</item>
    /// <item>Hold E: ease it (1.5 m creaks).</item>
    /// <item>Locks are worked by holding E with the right thing in hand (key ring, hairpin) from the right side; letting go
    /// of a half-lifted latch drops it with a clack.</item>
    /// <item>A <see cref="Gate"/> message blocks an unlocked door for story reasons and says why. A physical lock always
    /// speaks first, so "locked" and "not yet" are never confused.</item>
    /// </list>
    /// </summary>
    public class HouseDoor : MonoBehaviour, IHoldInteractable, IInteractableDisplayName, IInteractionAvailability
    {
        public struct Snapshot { public float Openness; public DoorLock Lock; }

        private const float TapSeconds = 0.25f;

        [SerializeField] private DoorKind _kind = DoorKind.Hinged;
        [SerializeField] private Transform _leaf;
        [Tooltip("Hinged: degrees around the hinge when fully open (sign picks the direction).")]
        [SerializeField] private float _openAngle = 100f;
        [Tooltip("Sliding: local offset of the panel when fully open.")]
        [SerializeField] private Vector3 _slide = new Vector3(1.2f, 0f, 0f);
        [SerializeField] private DoorLock _lock = DoorLock.None;
        [Tooltip("Locked doors open with this item (the key ring).")]
        [SerializeField] private string _keyItem = "chia";
        [Tooltip("A latch on the outside can be lifted from inside with this (a hairpin through the gap).")]
        [SerializeField] private string _toolItem = "tram";
        [Tooltip("A point on the side where the bolt sits, or the inside of a room latched from outside.")]
        [SerializeField] private Transform _lockSide;
        [SerializeField] private string _name = "cửa";
        [SerializeField] private float _workSeconds = 1.6f;
        [SerializeField] private float _openSpeed = 0.55f;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private KieuInventory _inventory;
        [SerializeField] private bool _playerCanUse = true;
        [Tooltip("Watchers may open and shut it by hand when their round, a check or a search goes through it (never through a lock " +
                 "or a story gate). Off for Kiều's own room, the gates and the back door.")]
        [SerializeField] private bool _npcCanUse = true;

        private float _openness, _target, _work, _pressAt = -1f, _creak, _fastFrom, _fastTo, _fastTime = -1f;
        private bool _working, _closing;
        private Quaternion _leafRotation;
        private Vector3 _leafPosition;
        private Vector3 _centre;

        /// <summary>Every door in the loaded scenes (watchers look here for the one in their way).</summary>
        public static readonly List<HouseDoor> All = new List<HouseDoor>();

        /// <summary>Returns why an unlocked door must stay shut for now (null = free to open).</summary>
        public Func<string> Gate;
        public event Action<HouseDoor> Changed;
        /// <summary>The player tried a door that would not give: rattled a lock, or was turned back by the gate.</summary>
        public event Action<HouseDoor, string> Refused;

        public float Openness => _openness;
        public bool IsOpen => _openness > 0.35f;
        public DoorLock Lock => _lock;
        public string Name => _name;
        public bool PlayerCanUse { get => _playerCanUse; set => _playerCanUse = value; }
        public bool IsAvailable => _playerCanUse && _fastTime < 0f;
        public bool NpcCanUse { get => _npcCanUse; set => _npcCanUse = value; }
        public DoorKind Kind => _kind;
        /// <summary>The middle of the doorway at floor level (where the shut leaf stands).</summary>
        public Vector3 Centre => _centre;
        /// <summary>The leaf is moving (thrown, slammed, or worked by someone else).</summary>
        public bool Moving => _fastTime >= 0f;
        /// <summary>Who is working it by hand now (a watcher): another one waits rather than fight over it.</summary>
        public object User { get; private set; }
        /// <summary>The last move was a watcher's hand, not Kiều's (story handlers that only mean Kiều can ignore it).</summary>
        public bool MovedByNpc { get; private set; }

        private void Awake()
        {
            if (_leaf == null) _leaf = transform;
            _leafRotation = _leaf.localRotation;
            _leafPosition = _leaf.localPosition;
            // The doorway's middle, from the shut leaf's own solid (scenes are saved with every leaf shut).
            var solid = _leaf.GetComponentInChildren<Collider>();
            _centre = solid != null ? solid.bounds.center : _leaf.position;
            _centre.y = transform.position.y;
            Apply();
        }

        private void OnEnable() { All.Add(this); }

        private void OnDisable() { All.Remove(this); User = null; }

        // ------------------------------------------------------------------ interaction

        public string DisplayName
        {
            get
            {
                string reason = BlockReason(out bool workable);
                if (reason != null) return workable ? reason : "<color=#C9A27A>" + reason + "</color>";
                return IsOpen ? "Đóng " + _name + "  <size=80%><color=#A89A86>(nhấn E: đóng sập, vang xa · giữ E: khép khẽ)</color></size>"
                              : "Mở " + _name + "  <size=80%><color=#A89A86>(nhấn E: đẩy mạnh, vang xa · giữ E: mở khẽ)</color></size>";
            }
        }

        public void Interact() { Tick(true, false); }

        public float Tick(bool pressed, bool held)
        {
            if (pressed) MovedByNpc = false;
            string reason = BlockReason(out bool workable);
            if (reason != null && !workable)
            {
                if (pressed) Refuse(reason);
                return -1f;
            }
            if (_lock != DoorLock.None) return WorkLock(pressed, held);

            if (pressed) { _pressAt = Time.time; _closing = IsOpen; }
            if (_pressAt < 0f) return -1f;
            float heldFor = Time.time - _pressAt;
            if (held && heldFor >= TapSeconds)
            {
                // Easing it: a hand on the door, a little at a time.
                _target = _closing ? 0f : 1f;
                _openness = Mathf.MoveTowards(_openness, _target, _openSpeed * Time.deltaTime);
                Apply();
                if ((_creak -= Time.deltaTime) <= 0f)
                {
                    _creak = 0.7f;
                    Sound(_kind == DoorKind.Sliding ? "slide_soft" : "door_creak_soft", 0.35f);
                    Noise.Emit(transform.position, EaseRadius, NoiseKind.Door);
                }
                if (Mathf.Approximately(_openness, _target)) { _pressAt = -1f; Changed?.Invoke(this); }
                return _closing ? 1f - _openness : _openness;
            }
            if (!held)
            {
                if (heldFor < TapSeconds) Snap(_closing ? 0f : 1f);
                else Changed?.Invoke(this);
                _pressAt = -1f;
            }
            return -1f;
        }

        public void Lost()
        {
            if (_working && _lock == DoorLock.LatchedOutside && _work > 0.25f) DropLatch();
            _working = false;
            _work = 0f;
            if (_pressAt >= 0f) { _pressAt = -1f; Changed?.Invoke(this); }
        }

        /// <summary>Message when the door cannot simply be opened; <paramref name="workable"/> when holding E will work it.</summary>
        private string BlockReason(out bool workable)
        {
            workable = false;
            bool onLockSide = OnLockSide();
            switch (_lock)
            {
                case DoorLock.Locked:
                    if (Has(_keyItem)) { workable = true; return "Giữ E: tra chìa mở khoá " + _name; }
                    return Capital(_name) + " khoá móc — cần chìa";
                case DoorLock.LatchedOutside:
                    if (!onLockSide) { workable = true; return "Giữ E: nhấc then " + _name; }
                    if (Has(_toolItem)) { workable = true; return "Giữ E: luồn trâm qua khe, nâng then"; }
                    return Capital(_name) + " cài then từ bên ngoài";
                case DoorLock.Bolted:
                    if (onLockSide) { workable = true; return "Giữ E: kéo then " + _name + " thật chậm"; }
                    return Capital(_name) + " cài then phía bên kia";
            }
            if (!IsOpen && Gate != null)
            {
                string gate = Gate();
                if (!string.IsNullOrEmpty(gate)) return gate;
            }
            return null;
        }

        private float WorkLock(bool pressed, bool held)
        {
            if (pressed) { _working = true; _work = 0f; _pressAt = Time.time; }
            if (!_working) return -1f;
            float seconds = _lock == DoorLock.LatchedOutside && OnLockSide() ? _workSeconds * 1.8f : _workSeconds;
            if (held)
            {
                _work += Time.deltaTime / seconds;
                if ((_creak -= Time.deltaTime) <= 0f)
                {
                    _creak = 0.55f;
                    Sound(_lock == DoorLock.Locked ? "key_turn" : _lock == DoorLock.Bolted ? "bolt_slow" : "latch_lift", 0.3f);
                    Noise.Emit(transform.position, 1.5f, NoiseKind.Lock);
                }
                if (_work >= 1f)
                {
                    var was = _lock;
                    _lock = DoorLock.None;
                    _working = false;
                    _work = 0f;
                    Sound(was == DoorLock.Locked ? "lock_open" : "latch_lift", 0.6f);
                    // Turning a key in an old padlock carries further than lifting a latch.
                    Noise.Emit(transform.position, was == DoorLock.Locked ? 5f : 2f, NoiseKind.Lock);
                    Changed?.Invoke(this);
                    return 1f;
                }
                return _work;
            }
            // Let go: a quick yank of the bolt slides it with a bang; a half-lifted latch drops back.
            if (_lock == DoorLock.Bolted && Time.time - _pressAt < TapSeconds)
            {
                _lock = DoorLock.None;
                Sound("bolt_fast", 0.9f);
                Noise.Emit(transform.position, 7f, NoiseKind.Lock);
                Changed?.Invoke(this);
            }
            else if (_lock == DoorLock.LatchedOutside && _work > 0.25f) DropLatch();
            _working = false;
            _work = 0f;
            return -1f;
        }

        private void DropLatch()
        {
            Sound("latch_drop", 0.9f);
            Noise.Emit(transform.position, 6f, NoiseKind.Lock);
        }

        private void Refuse(string reason)
        {
            Sound("lock_rattle", 0.5f);
            Noise.Emit(transform.position, 3f, NoiseKind.Door);
            Refused?.Invoke(this, reason);
        }

        private void Snap(float to)
        {
            MovedByNpc = false;
            _fastFrom = _openness;
            _fastTo = to;
            _fastTime = 0f;
            bool slam = to < _openness;
            // Flung open: the latch and the swing are heard from the start. Slammed shut: the slam is heard when the leaf
            // hits the frame, at the end of the swing. Either way the house hears the bang once, at that moment. A door
            // barely ajar snapped shut makes less of it.
            float travel = Mathf.Abs(to - _openness);
            string sound = _kind == DoorKind.Sliding ? (slam ? "slide_slam" : "slide_fast") : slam ? "door_slam" : "door_fast";
            float volume = Mathf.Lerp(0.5f, 0.9f, travel);
            if (slam) { _bangSound = sound; _bangVolume = volume; }
            else { _bangSound = null; Sound(sound, volume); }
            _bang = (slam ? SlamRadius : FlingRadius) * Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(travel / 0.6f));
        }

        private string _bangSound;
        private float _bangVolume;

        /// <summary>How far a door flung open (or slammed shut) is heard; easing it with E held is heard at <see cref="EaseRadius"/>.</summary>
        public const float FlingRadius = 8f, SlamRadius = 6f, EaseRadius = 1.5f;

        private float _bang;

        // ------------------------------------------------------------------ story control

        /// <summary>Moves the door for the story (someone else opens or shuts it); <paramref name="seconds"/> 0 = at once.</summary>
        public void SetOpen(float openness, float seconds = 0f, string sound = null)
        {
            MovedByNpc = false;
            // A slam is heard when the leaf hits, at the end of the move; any other sound with the move itself.
            if (!string.IsNullOrEmpty(sound) && seconds > 0f && sound.Contains("slam")) Move(openness, seconds, null, 0f, sound, 0.9f);
            else Move(openness, seconds, sound, 0.9f, null, 0f);
        }

        // ------------------------------------------------------------------ watchers' hands

        /// <summary>A watcher may open it now: his to use, not locked, not held shut by the story.</summary>
        public bool NpcMayOpen
        {
            get
            {
                if (!_npcCanUse || _lock != DoorLock.None) return false;
                if (Gate != null && !string.IsNullOrEmpty(Gate())) return false;
                return true;
            }
        }

        /// <summary>
        /// A watcher opens it by hand: a steady push over <paramref name="seconds"/> (a quick shove in a chase), heard by the
        /// player like any door, never a noise that he or anyone else takes for Kiều. False when it is not his to open
        /// (locked, a story gate, a door watchers leave alone), when Kiều has her hand on it, or someone else is working it.
        /// </summary>
        public bool OpenBy(object who, float seconds = 1.1f, bool hurried = false)
        {
            if (!NpcMayOpen || (User != null && User != who) || _pressAt >= 0f || _working) return false;
            User = who;
            if (_openness >= 0.98f && !Moving) return true;
            string sound = _kind == DoorKind.Sliding ? "slide_fast" : hurried ? "door_fast" : "door_creak_soft";
            Move(1f, seconds, sound, hurried ? 0.8f : _kind == DoorKind.Sliding ? 0.35f : 0.6f, null, 0f);
            MovedByNpc = true;
            return true;
        }

        /// <summary>A watcher pulls it to behind him: an easy close, the leaf meeting the frame softly at the end. Same rules as <see cref="OpenBy"/>.</summary>
        public bool CloseBy(object who, float seconds = 1.3f)
        {
            if (!_npcCanUse || (User != null && User != who) || _pressAt >= 0f || _working) return false;
            User = who;
            Move(0f, seconds, _kind == DoorKind.Sliding ? null : "door_creak_soft", 0.45f, _kind == DoorKind.Sliding ? "slide_fast" : "latch_drop", 0.4f);
            MovedByNpc = true;
            return true;
        }

        /// <summary>He is done with it: someone else may work it now.</summary>
        public void Release(object who) { if (User == who) User = null; }

        private void Move(float openness, float seconds, string startSound, float startVolume, string endSound, float endVolume)
        {
            // Someone else's hand: heard by the player, never a noise the watchers take for Kiều (no watcher hears his own door).
            _bang = 0f;
            _bangSound = endSound;
            _bangVolume = endVolume;
            if (!string.IsNullOrEmpty(startSound)) Sound(startSound, startVolume);
            if (seconds <= 0f) { _bangSound = null; _openness = Mathf.Clamp01(openness); _fastTime = -1f; Apply(); Changed?.Invoke(this); return; }
            _fastFrom = _openness;
            _fastTo = Mathf.Clamp01(openness);
            _fastTime = 0f;
            _fastDuration = seconds;
        }

        public void SetLock(DoorLock state) { _lock = state; _working = false; _work = 0f; }

        public Snapshot Save() => new Snapshot { Openness = _fastTime >= 0f ? _fastTo : _openness, Lock = _lock };

        public void Load(Snapshot snapshot)
        {
            User = null;
            MovedByNpc = false;
            _lock = snapshot.Lock;
            _openness = snapshot.Openness;
            _fastTime = -1f;
            _pressAt = -1f;
            _working = false;
            _bang = 0f;
            _bangSound = null;
            Apply();
        }

        private float _fastDuration = 0.3f;

        private void Update()
        {
            if (_fastTime < 0f) return;
            _fastTime += Time.deltaTime;
            float k = Mathf.Clamp01(_fastTime / _fastDuration);
            _openness = Mathf.Lerp(_fastFrom, _fastTo, 1f - (1f - k) * (1f - k));
            Apply();
            if (k < 1f) return;
            _fastTime = -1f;
            _fastDuration = 0.3f;
            if (_bangSound != null) { Sound(_bangSound, _bangVolume); _bangSound = null; }
            if (_bang > 0f) { Noise.Emit(transform.position, _bang, NoiseKind.Door); _bang = 0f; }
            Changed?.Invoke(this);
        }

        private void Apply()
        {
            if (_leaf == null) return;
            if (_kind == DoorKind.Hinged) _leaf.localRotation = _leafRotation * Quaternion.Euler(0f, _openAngle * _openness, 0f);
            else _leaf.localPosition = _leafPosition + _slide * _openness;
        }

        // ------------------------------------------------------------------ helpers

        private bool OnLockSide()
        {
            if (_lockSide == null || _inventory == null) return true;
            Vector3 toSide = _lockSide.position - transform.position;
            Vector3 toPlayer = _inventory.transform.position - transform.position;
            toSide.y = toPlayer.y = 0f;
            return Vector3.Dot(toSide, toPlayer) > 0f;
        }

        private bool Has(string item) => _inventory != null && _inventory.Has(item);

        private void Sound(string id, float volume)
        {
            if (_audio != null) _audio.PlayAt(id, transform.position + Vector3.up * 1.1f, volume, 20f);
        }

        private static string Capital(string text) => string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text.Substring(1);
    }
}

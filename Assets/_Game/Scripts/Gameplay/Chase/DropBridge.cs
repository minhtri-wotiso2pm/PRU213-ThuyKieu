using System;
using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Interaction;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using Unity.AI.Navigation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// The plank bridge over the stream ravine before the temple path. Its far end is tied to one old stake, rotted at the
    /// foot. Once over, Kiều can kick the stake out (E, standing beside it on the far bank): the ropes go, the far end drops
    /// and the deck swings down against the near bank, hanging into the ravine. Nobody can cross after that, so the pursuer
    /// is stopped by the ravine itself (his link across is gone and the deck no longer spans it), not by being switched off.
    /// </summary>
    public class DropBridge : MonoBehaviour, IInteractable, IInteractableDisplayName, IInteractionAvailability, IChaseReset
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private StoryAudio _audio;
        [SerializeField] private ScreenFx _fx;
        [Tooltip("Hinge at the near end; the deck and its rails hang under it.")]
        [SerializeField] private Transform _deck;
        [SerializeField] private Vector3 _deckDownEuler = new Vector3(70f, 0f, 0f);
        [Tooltip("The far stake; it tips into the ravine when kicked.")]
        [SerializeField] private Transform _stake;
        [SerializeField] private Vector3 _stakeDownEuler = new Vector3(-60f, 0f, 0f);
        [Tooltip("Where she has to stand to reach the stake: the far bank, beside it.")]
        [SerializeField] private BoxCollider _farBank;
        [SerializeField] private NavMeshLink _crossing;
        [SerializeField] private string _label = "Đạp đổ cọc cầu";

        private Quaternion _deckUp, _stakeUp;
        private Coroutine _falling;

        public bool Armed { get; set; }
        public bool Dropped { get; private set; }
        public float DroppedAt { get; private set; } = -1f;
        /// <summary>Other conditions (Cô Lam must be across first when she runs with Kiều).</summary>
        public Func<bool> Ready { get; set; }
        public NavMeshLink Crossing => _crossing;
        public Transform Stake => _stake;
        public event Action Fell;

        public string DisplayName => _label;

        public bool IsAvailable => Armed && !Dropped && _player != null && !_player.ControlLocked && OnFarBank(_player.transform.position)
                                   && (Ready == null || Ready());

        public bool OnFarBank(Vector3 feet) => ChaseCourse.Inside(_farBank, feet + Vector3.up * 0.5f);

        private void Awake()
        {
            _deckUp = _deck.localRotation;
            _stakeUp = _stake.localRotation;
            var walk = _deck.Find("Walk");
            _walk = walk != null ? walk : _deck;
        }

        private Transform _walk;
        private float _strainIn;

        /// <summary>
        /// The rope rails take her weight as she runs over the planks: a strain now and then from the rail beside her (the
        /// planks themselves are her own steps, by surface). Silent off the deck, after the drop and outside the run.
        /// </summary>
        private void Update()
        {
            if (!Armed || Dropped || _player == null || _audio == null || Time.deltaTime <= 0f) return;
            Vector3 local = _walk.InverseTransformPoint(_player.transform.position);
            bool onDeck = Mathf.Abs(local.x) < 0.55f && local.z > -0.5f && local.z < 0.5f && Mathf.Abs(_player.transform.position.y - _walk.position.y) < 0.6f;
            if (!onDeck) { _strainIn = 0.25f; return; }
            if ((_strainIn -= Time.deltaTime) > 0f) return;
            _strainIn = UnityEngine.Random.Range(1.1f, 1.6f);
            Vector3 side = _walk.right * (UnityEngine.Random.value < 0.5f ? -0.72f : 0.72f);
            _audio.PlayAt("rope_strain", _player.transform.position + side + Vector3.up * 0.9f, 0.55f, 14f, UnityEngine.Random.Range(0.95f, 1.05f));
        }

        public void ResetForChase()
        {
            if (_falling != null) StopCoroutine(_falling);
            _falling = null;
            Dropped = false;
            DroppedAt = -1f;
            _deck.localRotation = _deckUp;
            _stake.localRotation = _stakeUp;
            if (_crossing != null) _crossing.enabled = true;
        }

        public void Interact() { if (IsAvailable) Drop(); }

        public void Drop()
        {
            if (Dropped) return;
            Dropped = true;
            DroppedAt = Time.time;
            if (_crossing != null) _crossing.enabled = false;
            StoryAutopilot.Note("bridge dropped");
            _falling = StartCoroutine(Fall());
            Fell?.Invoke();
        }

        private IEnumerator Fall()
        {
            // Each once per drop, from where it happens: her heel on the stake, the rope torn loose, the deck swinging down
            // on its ropes, the deck meeting the near bank (it never reaches the water far below).
            if (_audio != null) _audio.PlayAt("stake_kick", _stake.position + Vector3.up * 0.6f, 1f, 26f);
            if (_fx != null) _fx.Shake(0.25f, 0.25f);
            Quaternion stakeDown = _stakeUp * Quaternion.Euler(_stakeDownEuler);
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _stake.localRotation = Quaternion.Slerp(_stakeUp, stakeDown, t / 0.35f);
                yield return null;
            }
            _stake.localRotation = stakeDown;
            if (_audio != null)
            {
                _audio.PlayAt("rope_snap", _stake.position + Vector3.up, 0.9f, 30f);
                _audio.PlayAt("deck_swing", _deck.position, 0.8f, 36f);
            }
            Quaternion deckDown = _deckUp * Quaternion.Euler(_deckDownEuler);
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = t / 0.8f;
                _deck.localRotation = Quaternion.Slerp(_deckUp, deckDown, k * k);
                yield return null;
            }
            _deck.localRotation = deckDown;
            if (_audio != null) _audio.PlayAt("deck_hit", _deck.position + Vector3.down * 2.5f, 1f, 40f);
            if (_fx != null) _fx.Shake(0.35f, 0.4f);
            _falling = null;
        }
    }
}

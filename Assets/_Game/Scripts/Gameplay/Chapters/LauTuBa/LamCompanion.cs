using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using ThuyKieu.Player;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chapters.LauTuBa
{
    /// <summary>
    /// Chị Lam on the way out, without a follower AI that could blunder into a guard: she waits at hiding points and only
    /// moves on to the next when Kiều has gone ahead to it and the stretch between is safe, meaning no guard chasing or
    /// searching within 10 m of it and none with a clear line to it.
    /// <list type="bullet">
    /// <item>If someone comes into view while she is moving, she ducks back to the last point and he only comes to look.</item>
    /// <item>A stretch that takes too long (stuck) ends with her moved to the next point while no one, the camera included, sees.</item>
    /// <item>Once Kiều waits in the kitchen, Lam joins her within 40 s at most, so the escape can always finish.</item>
    /// </list>
    /// </summary>
    public class LamCompanion : MonoBehaviour
    {
        [SerializeField] private StoryActor _lam;
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Transform[] _nodes;
        [SerializeField] private Watchman[] _watchers;
        [SerializeField] private StoryHUD _hud;
        [SerializeField] private float _reach = 2.6f;
        [SerializeField] private float _speed = 1.25f;
        [SerializeField] private LayerMask _walls = 1 << 8;

        private int _node;
        private bool _moving, _following;
        private float _segmentTime, _waitTime, _whisper;

        public bool Following => _following;
        public int Node => _node;
        public bool Arrived => _following && !_moving && _nodes != null && _node == _nodes.Length - 1;

        public void Begin()
        {
            _following = true;
            _node = 0;
            _moving = false;
            _waitTime = 0f;
            _lam.Place(_nodes[0].position, _nodes[0].rotation);
        }

        /// <summary>Checkpoints: back at a node, waiting.</summary>
        public void Restore(bool following, int node)
        {
            _following = following;
            _moving = false;
            _waitTime = 0f;
            if (!following || _nodes == null || _nodes.Length == 0) return;
            _node = Mathf.Clamp(node, 0, _nodes.Length - 1);
            _lam.Place(_nodes[_node].position, _nodes[_node].rotation);
        }

        private void Update()
        {
            if (!_following || _nodes == null || _nodes.Length == 0) return;
            if (_whisper > 0f) _whisper -= Time.deltaTime;
            if (_moving) { Move(); return; }
            if (_node >= _nodes.Length - 1) return;
            _waitTime += Time.deltaTime;
            int next = _node + 1;
            bool ahead = PlayerReached(next);
            if (!ahead) return;
            if (SegmentSafe(_nodes[_node].position, _nodes[next].position))
            {
                _moving = true;
                _segmentTime = 0f;
                _lam.MoveTo(_nodes[next].position, _speed);
                return;
            }
            if (_whisper <= 0f && Vector3.Distance(_player.transform.position, _lam.transform.position) < 9f)
            {
                _whisper = 7f;
                _hud.ShowBark("CoLam", "(thì thầm) Chưa được… có người.", 2.2f);
            }
            // The last stretch never strands the player: once Kiều is in the kitchen, Lam slips over unseen.
            if (next == _nodes.Length - 1 && _waitTime > 40f && Unseen(_nodes[next].position)) Warp(next);
        }

        private void Move()
        {
            _segmentTime += Time.deltaTime;
            int next = _node + 1;
            var watcher = SpottingWatcher(_lam.transform.position);
            if (watcher != null)
            {
                // Seen moving: back to cover; he only comes to look where she was.
                watcher.Investigate(_lam.transform.position, 3f);
                _moving = false;
                _lam.MoveTo(_nodes[_node].position, _speed * 1.3f);
                _waitTime = 0f;
                return;
            }
            if (!_lam.IsMoving && Vector3.Distance(_lam.transform.position, _nodes[next].position) < 0.6f) { Arrive(next); return; }
            if (_segmentTime > 15f && Unseen(_nodes[next].position) && Unseen(_lam.transform.position)) Warp(next);
        }

        private void Arrive(int node)
        {
            _node = node;
            _moving = false;
            _waitTime = 0f;
            _lam.Place(_nodes[node].position, _nodes[node].rotation);
        }

        private void Warp(int node)
        {
            _lam.Place(_nodes[node].position, _nodes[node].rotation);
            Arrive(node);
        }

        private bool PlayerReached(int node)
        {
            Vector3 player = _player.transform.position;
            for (int i = node; i < _nodes.Length; i++)
                if (Flat(player - _nodes[i].position) < _reach) return true;
            return false;
        }

        /// <summary>Safe to cross from <paramref name="a"/> to <paramref name="b"/>: nobody hunting nearby, nobody looking at it.</summary>
        public bool SegmentSafe(Vector3 a, Vector3 b)
        {
            if (_watchers == null) return true;
            foreach (var watcher in _watchers)
            {
                if (watcher == null || !watcher.isActiveAndEnabled || !watcher.Perceiving) continue;
                bool hunting = watcher.State == WatchState.Chase || watcher.State == WatchState.Search;
                for (int i = 0; i <= 4; i++)
                {
                    Vector3 point = Vector3.Lerp(a, b, i / 4f) + Vector3.up * 1.1f;
                    float distance = Vector3.Distance(watcher.Eyes.position, point);
                    if (hunting && distance < 10f) return false;
                    if (distance < 10f && !Physics.Linecast(watcher.Eyes.position, point, _walls, QueryTriggerInteraction.Ignore)) return false;
                }
            }
            return true;
        }

        private Watchman SpottingWatcher(Vector3 position)
        {
            if (_watchers == null) return null;
            Vector3 point = position + Vector3.up * 1.1f;
            foreach (var watcher in _watchers)
            {
                if (watcher == null || !watcher.isActiveAndEnabled || !watcher.Perceiving) continue;
                Vector3 to = point - watcher.Eyes.position;
                if (to.magnitude > 9f) continue;
                Vector3 forward = watcher.Eyes.forward; forward.y = 0f; to.y = 0f;
                if (Vector3.Angle(forward, to) > 45f) continue;
                if (!Physics.Linecast(watcher.Eyes.position, point, _walls, QueryTriggerInteraction.Ignore)) return watcher;
            }
            return null;
        }

        private bool Unseen(Vector3 position)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                Vector3 viewport = camera.WorldToViewportPoint(position + Vector3.up);
                bool onScreen = viewport.z > 0f && viewport.x > -0.1f && viewport.x < 1.1f && viewport.y > -0.1f && viewport.y < 1.1f;
                if (onScreen && !Physics.Linecast(camera.transform.position, position + Vector3.up, _walls, QueryTriggerInteraction.Ignore)) return false;
            }
            return SpottingWatcher(position) == null;
        }

        private static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
    }
}

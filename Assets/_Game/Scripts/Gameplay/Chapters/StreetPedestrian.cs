using ThuyKieu.Core.Story;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Gameplay.Chapters
{
    [RequireComponent(typeof(StoryActor))]
    public class StreetPedestrian : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private Vector3[] _route;
        [SerializeField] private float _speed = 1.1f;
        [SerializeField] private int _startIndex;
        private StoryActor _actor;
        private NavMeshPath _path;
        private int _index;
        private float _wait;
        private bool _exploring, _hasDestination;

        private void Awake()
        {
            _actor = GetComponent<StoryActor>();
            _path = new NavMeshPath();
            _index = _startIndex;
            _wait = 0.5f + _startIndex * 0.4f;
        }

        private void OnEnable()
        {
            if (_director == null) return;
            _director.ExploreChanged += OnExplore;
            _exploring = _director.InExplore;
        }

        private void OnDisable()
        {
            if (_director != null) _director.ExploreChanged -= OnExplore;
            if (_actor != null) _actor.Stop();
        }

        private void OnExplore(bool exploring)
        {
            _exploring = exploring;
            if (exploring) return;
            _actor.Stop();
            _hasDestination = false;
        }

        private void Update()
        {
            if (!_exploring || _route == null || _route.Length < 2) return;
            if (_actor.Agent == null || !_actor.Agent.enabled || !_actor.Agent.isOnNavMesh)
            {
                _actor.EnableNavigation();
                return;
            }
            if (_wait > 0f) { _wait -= Time.deltaTime; return; }
            if (_actor.IsMoving) return;
            if (_hasDestination)
            {
                _hasDestination = false;
                _wait = Random.Range(1.5f, 4f);
                return;
            }
            for (int attempt = 0; attempt < _route.Length; attempt++)
            {
                _index = (_index + 1) % _route.Length;
                if (!NavMesh.SamplePosition(_route[_index], out var hit, 2f, _actor.Agent.areaMask)) continue;
                if (Vector3.Distance(transform.position, hit.position) < 0.8f) continue;
                if (!NavMesh.CalculatePath(transform.position, hit.position, _actor.Agent.areaMask, _path) || _path.status != NavMeshPathStatus.PathComplete) continue;
                _hasDestination = _actor.MoveTo(hit.position, _speed);
                return;
            }
            _wait = 1f;
        }
    }
}

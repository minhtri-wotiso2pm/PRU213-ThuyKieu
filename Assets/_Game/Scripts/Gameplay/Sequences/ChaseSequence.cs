using System.Collections;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Free-running chase through the back alley: pursuers on the NavMesh close in with a rubber band (never hopeless,
    /// never safe), sprinting costs stamina, and carts or baskets knocked over behind the player slow them down.
    /// The alley forks into a short dark shortcut and a longer, crowded night market. Writes "c5_duong" and "c5_xo_do".
    /// </summary>
    public class ChaseSequence : StorySequence
    {
        [SerializeField] private Transform _start;
        [SerializeField] private StoryActor[] _pursuers;
        [SerializeField] private Transform[] _pursuerStarts;
        [SerializeField] private ChaseObstacle[] _obstacles;
        [SerializeField] private BoxCollider _finish, _shortcut, _market;
        [Tooltip("Crowded stretch where pursuers have to push through people.")]
        [SerializeField] private BoxCollider _crowd;
        [SerializeField] private Stamina _stamina;
        [SerializeField] private float _catchDistance = 1.1f;
        [SerializeField] private DogPursuer _dog;
        [SerializeField, Range(0.5f, 1f)] private float _pursuitSpeedMultiplier = 0.85f;
        [Tooltip("QA route: waypoints the autopilot follows.")]
        [SerializeField] private Transform[] _autopilotRoute;

        private static readonly string[] Shouts = { "Đứng lại!", "Bắt lấy con ranh!", "Nó chạy đằng kia!", "Chặn đầu nó!" };

        public override IEnumerator Run(StoryDirector director)
        {
            var player = director.Player;
            var hud = director.Hud;
            var companion = FindAnyObjectByType<Follower>();
            bool thorough = director.Autopilot && StoryAutopilot.Args.Contains("-autopilotThorough");
            float load = (director.GetInt("c4_tien") == 2 ? 1.35f : 1f) + (companion != null && companion.isActiveAndEnabled ? 0.15f : 0f);
            _stamina.Load = load;
            hud.SetControls("WASD: chạy · Shift: chạy nước rút (tốn sức) · E: xô đổ xe hàng, thúng mủng");
            _stamina.enabled = true;
            if (director.Audio != null) { director.Audio.PlayMusic("chase"); director.Audio.SetAmbience("market_night"); }

            int route = 0;
            bool forced = false;
            yield return Begin(director, companion);
            float repath = 0f, shout = 2f, autoTime = 0.8f, nearest = float.MaxValue;
            int autoIndex = 0;
            while (!_finish.bounds.Contains(player.transform.position + Vector3.up * 0.5f))
            {
                if (director.Autopilot) AutopilotStep(player, ref autoIndex, ref autoTime);
                Vector3 position = player.transform.position + Vector3.up * 0.5f;
                if (_shortcut != null && _shortcut.bounds.Contains(position)) route = 1;
                if (_market != null && _market.bounds.Contains(position)) route = 2;

                float closest = float.MaxValue;
                foreach (var pursuer in _pursuers) closest = Mathf.Min(closest, Flat(pursuer.transform.position, player.transform.position));
                if (_dog != null) closest = Mathf.Min(closest, Flat(_dog.transform.position, player.transform.position));
                if ((repath -= Time.deltaTime) <= 0f)
                {
                    repath = 0.15f;
                    foreach (var pursuer in _pursuers) Pursue(pursuer, player.transform.position);
                    if (_dog != null)
                    {
                        var leader = _pursuers.Length > 0 ? _pursuers[0].transform : null;
                        Vector3 target = leader != null ? leader.position + leader.right * 0.8f : player.transform.position;
                        float correction = Mathf.Clamp(Flat(_dog.transform.position, target) * 0.5f, 0f, 0.6f);
                        _dog.Pursue(target, PursuitSpeed(leader != null ? leader.position : _dog.transform.position, player.transform.position) + correction);
                    }
                }
                if ((shout -= Time.deltaTime) <= 0f)
                {
                    shout = Random.Range(3.5f, 6f);
                    hud.ShowBark("GiaNhan", Shouts[Random.Range(0, Shouts.Length)], 1.6f);
                }

                hud.SetMeter(closest < 5f ? "CHÚNG ĐANG ĐUỔI SÁT!" : "Chúng đang đuổi theo — chạy!", 1f - Mathf.Clamp01((closest - 1f) / 14f), new Color(0.9f, 0.25f, 0.2f));
                hud.SetGauge(_stamina.Exhausted ? "Hết hơi! Đi bộ lấy lại sức" : "Thể lực (Shift: chạy nước rút)", _stamina.Value,
                    _stamina.Exhausted ? new Color(0.9f, 0.3f, 0.25f) : new Color(0.55f, 0.85f, 0.6f));

                nearest = Mathf.Min(nearest, closest);
                bool caught = closest < _catchDistance;
                if (thorough && !forced && autoTime < 0.5f) { forced = true; caught = true; }
                if (caught)
                {
                    StoryAutopilot.Note("CAUGHT in chase_alley → retry");
                    director.SetInt("c5_bi_bat", director.GetInt("c5_bi_bat") + 1);
                    director.SetFreeControl(false);
                    foreach (var pursuer in _pursuers) pursuer.Stop();
                    if (_dog != null) _dog.Stop();
                    if (director.Audio != null) director.Audio.PlaySfx("fail");
                    yield return hud.FailCard("Bị bắt lại!", "Để dành sức chạy nước rút (Shift) cho đoạn chúng sát nút. Bấm E khi chạy ngang xe hàng, thúng mủng để xô đổ chặn đường.", director.Autopilot ? 0f : 2.2f);
                    route = 0;
                    autoIndex = 0;
                    autoTime = 0.8f;
                    yield return Begin(director, companion);
                }
                yield return null;
            }

            director.SetFreeControl(false);
            foreach (var pursuer in _pursuers) pursuer.Stop();
            if (_dog != null) _dog.Stop();
            foreach (var obstacle in _obstacles) obstacle.Armed = false;
            _stamina.enabled = false;
            StoryAutopilot.Note("chase_alley nearest pursuer " + nearest.ToString("0.0") + " m");
            director.SetInt("c5_duong", route == 0 ? 1 : route);
            director.SetInt("c5_xo_do", _obstacles.Count(o => o.Toppled));
        }

        /// <summary>Places everyone at the start: the player out of the gate, the pursuers a few strides behind.</summary>
        private IEnumerator Begin(StoryDirector director, Follower companion)
        {
            var player = director.Player;
            player.Teleport(_start.position, _start.rotation);
            director.CameraController.SnapBehindTarget(_start.eulerAngles.y);
            if (companion != null) companion.Snap();
            for (int i = 0; i < _pursuers.Length; i++)
            {
                _pursuers[i].gameObject.SetActive(true);
                _pursuers[i].EnableNavigation();
                _pursuers[i].Place(_pursuerStarts[i].position, _pursuerStarts[i].rotation);
            }
            foreach (var obstacle in _obstacles) { obstacle.ResetObstacle(); obstacle.Armed = true; }
            if (_dog != null && _pursuerStarts.Length > 0)
            {
                var leader = _pursuerStarts[0];
                _dog.StartAt(leader.position + leader.right * 0.8f, leader.rotation);
            }
            _stamina.Refill();
            yield return director.Hud.Fade(0f, 0.4f);
            director.SetFreeControl(true);
        }

        /// <summary>Rubber band: fast when far behind, a little slower than a sprint when close; slowed by wreckage and crowds.</summary>
        private void Pursue(StoryActor pursuer, Vector3 target)
        {
            pursuer.MoveTo(target, PursuitSpeed(pursuer.transform.position, target));
        }

        private float PursuitSpeed(Vector3 position, Vector3 target)
        {
            float distance = Flat(position, target);
            float speed = (distance > 14f ? 6.2f : distance < 5f ? 4.3f : 4.8f) * _pursuitSpeedMultiplier;
            foreach (var obstacle in _obstacles)
                if (obstacle.Toppled && Flat(obstacle.SlowCenter, position) < obstacle.SlowRadius) speed *= 0.35f;
            if (_crowd != null && _crowd.bounds.Contains(position + Vector3.up * 0.5f)) speed *= 0.8f;
            return speed;
        }

        private static float Flat(Vector3 a, Vector3 b) { a.y = 0; b.y = 0; return Vector3.Distance(a, b); }

        // QA: hop along the route a few metres at a time, knocking over whatever is in reach.
        private void AutopilotStep(ThuyKieu.Player.PlayerMovement player, ref int index, ref float time)
        {
            if (_autopilotRoute == null || _autopilotRoute.Length == 0) return;
            time -= Time.deltaTime;
            if (time > 0f) return;
            time = 0.25f;
            index = Mathf.Min(index + 1, _autopilotRoute.Length - 1);
            var point = _autopilotRoute[index];
            player.Teleport(point.position, point.rotation);
            foreach (var obstacle in _obstacles)
                if (obstacle.IsAvailable && Flat(obstacle.transform.position, point.position) < 3f && Random.value < 0.6f) obstacle.Topple();
        }
    }
}

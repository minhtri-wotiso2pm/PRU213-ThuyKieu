using System.Collections;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Stealth listening: crouch inside the listening zone, out of sight, until the meter fills.
    /// Speakers glance around now and then; a guard may patrol. Being spotted restarts from the start point.
    /// </summary>
    public class EavesdropSequence : StorySequence
    {
        [SerializeField] private BoxCollider _zone;
        [SerializeField] private Transform _start;
        [SerializeField] private VisionSensor[] _sensors;
        [SerializeField] private GuardPatrol _guard;
        [SerializeField] private StoryActor[] _glancers;
        [SerializeField] private Transform _glanceTarget;
        [SerializeField] private float _seconds = 7f;
        [SerializeField] private string _objective = "Lẻn đến gần và nghe lén";
        [SerializeField] private string _caughtDetail = "Bọn họ đã thấy bạn.";
        [SerializeField] private string _musicAfter;

        public float Progress { get; private set; }

        public override IEnumerator Run(StoryDirector director)
        {
            var player = director.Player;
            var hud = director.Hud;
            string previousObjective = hud.Objective;
            hud.SetObjective(_objective);
            player.AllowCrouch = true;
            if (director.Audio != null) { director.Audio.PlayMusic("stealth"); director.Audio.Duck(0.6f); }
            SetSensors(true);
            if (_guard != null) _guard.Patrolling = true;
            director.SetFreeControl(true);
            Progress = 0f;
            float glanceTimer = 4f, glanceHold = 0f;
            StoryActor glancer = null;

            while (Progress < 1f)
            {
                if (director.Autopilot) AutopilotStep(player);
                bool inside = _zone.bounds.Contains(player.transform.position + Vector3.up * 0.5f);
                if (inside && player.IsCrouching) Progress += Time.deltaTime / _seconds;
                else Progress = Mathf.Max(0f, Progress - Time.deltaTime * 0.04f);

                // Speakers look around every few seconds, which is when an exposed listener gets caught.
                if (_glancers != null && _glancers.Length > 0 && _glanceTarget != null)
                {
                    if (glanceHold > 0f)
                    {
                        glanceHold -= Time.deltaTime;
                        if (glanceHold <= 0f && glancer != null) FacePartners();
                    }
                    else if ((glanceTimer -= Time.deltaTime) <= 0f)
                    {
                        glancer = _glancers[Random.Range(0, _glancers.Length)];
                        glancer.FaceTowards(_glanceTarget.position);
                        glanceHold = 1.9f;
                        glanceTimer = Random.Range(4.5f, 7.5f);
                    }
                }

                float awareness = 0f;
                foreach (var sensor in _sensors) if (sensor != null) awareness = Mathf.Max(awareness, sensor.Awareness);
                if (awareness > 0.04f) hud.SetMeter("CÓ NGƯỜI ĐANG NHÌN VỀ PHÍA NÀY!", awareness, new Color(0.9f, 0.25f, 0.2f));
                else hud.SetMeter(inside ? (player.IsCrouching ? "Đang nghe lén..." : "Khom người (Ctrl / C) để nghe rõ") : "Lại gần vách để nghe", Progress, new Color(1f, 0.8f, 0.45f));

                if (awareness >= 1f)
                {
                    yield return Caught(director);
                    Progress = 0f;
                }
                yield return null;
            }

            director.SetFreeControl(false);
            player.ForceCrouch = false;
            hud.SetObjective(previousObjective, false);
            SetSensors(false);
            FacePartners();
            if (director.Audio != null)
            {
                director.Audio.Duck(1f);
                if (!string.IsNullOrEmpty(_musicAfter)) director.Audio.PlayMusic(_musicAfter);
            }
        }

        private IEnumerator Caught(StoryDirector director)
        {
            director.SetFreeControl(false);
            if (director.Audio != null) director.Audio.PlaySfx("detect");
            if (director.Fx != null) director.Fx.Shake(0.3f, 0.4f);
            yield return director.Hud.FailCard("Bị phát hiện!", _caughtDetail, director.Autopilot ? 0f : 2f);
            director.Player.Teleport(_start.position, _start.rotation);
            director.CameraController.SnapBehindTarget(_start.eulerAngles.y);
            RestrictedZone.ResetPresence();
            foreach (var sensor in _sensors) if (sensor != null) sensor.ResetAwareness();
            if (_guard != null) _guard.ResetPatrol();
            FacePartners();
            yield return director.Hud.Fade(0f, 0.5f);
            director.SetFreeControl(true);
        }

        private void SetSensors(bool active)
        {
            foreach (var sensor in _sensors)
                if (sensor != null) { sensor.Active = active; sensor.ResetAwareness(); }
        }

        private void FacePartners()
        {
            if (_glancers == null || _glancers.Length < 2) return;
            _glancers[0].FaceTowards(_glancers[1].transform.position);
            _glancers[1].FaceTowards(_glancers[0].transform.position);
        }

        // QA: walk straight into the zone's centre and crouch.
        private void AutopilotStep(ThuyKieu.Player.PlayerMovement player)
        {
            Vector3 centre = _zone.bounds.center;
            centre.y = player.transform.position.y;
            if (Vector3.Distance(player.transform.position, centre) > 0.3f)
                player.Teleport(centre, player.transform.rotation);
            player.ForceCrouch = true;
        }
    }
}

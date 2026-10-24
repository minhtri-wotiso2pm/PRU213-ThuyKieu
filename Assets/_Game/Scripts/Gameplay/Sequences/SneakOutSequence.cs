using System.Collections;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Gameplay.Stealth;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 5 opening: cross the back yard past the lantern-carrying guard and the chained dog, then ease the alley gate
    /// open by holding E — push too long in one go and the hinge squeals. Writes how quietly it went to "c5_lang_le".
    /// </summary>
    public class SneakOutSequence : StorySequence
    {
        [SerializeField] private Transform _start;
        [SerializeField] private GuardPatrol _guard;
        [SerializeField] private DogSentry _dog;
        [SerializeField] private BoxCollider _gateZone;
        [SerializeField] private Transform _gateLeaf;
        [SerializeField] private float _openSeconds = 2.8f;
        [SerializeField] private string _objective = "Lẻn qua sân sau, mở cửa ngõ thật khẽ";

        public override IEnumerator Run(StoryDirector director)
        {
            var player = director.Player;
            var hud = director.Hud;
            var companion = FindAnyObjectByType<Follower>();
            bool thorough = director.Autopilot && StoryAutopilot.Args.Contains("-autopilotThorough");
            hud.SetObjective(_objective);
            player.AllowCrouch = true;
            if (director.Audio != null) director.Audio.PlayMusic("stealth");
            _guard.Patrolling = true;
            _guard.Vision.Active = true;
            _dog.Active = true;
            _dog.ResetCount();
            Quaternion closed = _gateLeaf.localRotation;
            director.SetFreeControl(true);

            float open = 0f, creak = 0f, autoHold = 0f;
            int caught = 0, creaks = 0;
            bool forced = false;
            while (open < 1f)
            {
                if (director.Autopilot) AutopilotStep(player, ref autoHold);
                bool inside = _gateZone.bounds.Contains(player.transform.position + Vector3.up * 0.5f);
                bool holding = inside && !player.IsHidden && (director.Autopilot ? autoHold > 0f : InputReader.IsInteractHeld);
                if (holding)
                {
                    open += Time.deltaTime / _openSeconds;
                    creak += Time.deltaTime * (player.IsCrouching ? 0.55f : 0.8f);
                    if (creak >= 1f)
                    {
                        creak = 0f;
                        creaks++;
                        if (director.Audio != null) director.Audio.PlaySfx("creak");
                        _guard.Investigate(_gateZone.transform.position, 5f);
                        _guard.Vision.Alert(0.45f);
                        hud.ShowBark("NguoiCanh", "Tiếng gì kẽo kẹt ngoài cửa ngõ thế?", 2.5f);
                    }
                }
                else creak = Mathf.Max(0f, creak - Time.deltaTime * 0.9f);
                _gateLeaf.localRotation = closed * Quaternion.Euler(0f, open * 75f, 0f);

                float awareness = _guard.Vision.Awareness;
                if (awareness > 0.04f) hud.SetMeter(awareness > 0.6f ? "NGƯỜI CANH ĐANG TIẾN LẠI!" : "Người canh nghi ngờ...", awareness, new Color(0.9f, 0.25f, 0.2f));
                else hud.SetMeter(inside ? "Giữ E đẩy cửa thật chậm — buông ra trước khi cửa kêu" : "Lẻn đến cửa ngõ (đi trong bóng tối, khom người)", open, new Color(1f, 0.8f, 0.45f));
                if (inside) hud.SetGauge("Tiếng bản lề", creak, new Color(0.95f, 0.35f, 0.25f));
                else if (_dog.Unease > 0.1f) hud.SetGauge("Con chó đang nghe ngóng...", _dog.Unease, new Color(0.95f, 0.6f, 0.25f));

                bool spotted = _guard.Vision.Spotted;
                // QA: exercise the caught → retry path once.
                if (thorough && !forced && open > 0.2f) { forced = true; spotted = true; }
                if (spotted)
                {
                    caught++;
                    StoryAutopilot.Note("CAUGHT in sneak_out → retry");
                    director.SetInt("c5_bi_bat", director.GetInt("c5_bi_bat") + 1);
                    director.SetFreeControl(false);
                    HideSpot.ExitAll();
                    if (director.Audio != null) director.Audio.PlaySfx("detect");
                    yield return hud.FailCard("Bị phát hiện!", "\"Đứa nào lảng vảng ngoài sân sau?\" — Tránh vùng đèn lồng, khom người qua chỗ con chó, và đẩy cửa từng chút một.", director.Autopilot ? 0f : 2f);
                    player.Teleport(_start.position, _start.rotation);
                    director.CameraController.SnapBehindTarget(_start.eulerAngles.y);
                    _guard.ResetPatrol();
                    _guard.Patrolling = true;
                    _dog.ResetDog();
                    if (companion != null) companion.Snap();
                    open = 0f; creak = 0f;
                    _gateLeaf.localRotation = closed;
                    yield return hud.Fade(0f, 0.5f);
                    director.SetFreeControl(true);
                }
                yield return null;
            }

            director.SetFreeControl(false);
            player.ForceCrouch = false;
            _dog.Active = false;
            director.SetInt("c5_lang_le", Mathf.Clamp(3 - caught * 2 - _dog.Barks - creaks, 0, 3));
        }

        // QA: walk to the gate crouched and push in short bursts.
        private void AutopilotStep(ThuyKieu.Player.PlayerMovement player, ref float hold)
        {
            Vector3 gate = _gateZone.bounds.center;
            gate.y = player.transform.position.y;
            if (Vector3.Distance(player.transform.position, gate) > 0.3f) player.Teleport(gate, player.transform.rotation);
            player.ForceCrouch = true;
            hold -= Time.deltaTime;
            if (hold < -0.8f) hold = 0.9f;
        }
    }
}

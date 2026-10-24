using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Cinematic run down through the bamboo: Kiều runs a fixed line with a tracking camera while the pursuers close in.
    /// Quick-time prompts (jump a root, duck a branch, scramble up a bank, pull a fallen companion up) keep her ahead;
    /// misses let them gain. Writes "c5_qte" (0–3) and "c5_co_lam_thoat".
    /// </summary>
    public class ForestRunSequence : StorySequence
    {
        private struct Beat
        {
            public float At;
            public Key Key;
            public string Label, Caption;
            public bool Mash, Companion;
        }

        [SerializeField] private Transform[] _rail;
        [SerializeField] private StoryActor[] _pursuers;
        [SerializeField] private Transform _camera;
        [SerializeField] private float _speed = 5.2f;
        [SerializeField] private float _startGap = 7f;
        private const float ShoutedCloser = 2.5f;
        [SerializeField] private float _window = 1.5f;
        [SerializeField] private float _mashWindow = 2.4f;
        [SerializeField] private int _mashCount = 7;

        private static readonly Key[] Keys = { Key.Space, Key.A, Key.D, Key.E };
        private float[] _lengths;
        private float _total;

        public override IEnumerator Run(StoryDirector director)
        {
            var player = director.Player;
            var hud = director.Hud;
            var companion = FindAnyObjectByType<Follower>();
            bool withCompanion = companion != null && companion.isActiveAndEnabled && director.GetBool("c4_ru_co_lam");
            bool thorough = director.Autopilot && StoryAutopilot.Args.Contains("-autopilotThorough");
            Measure();
            var beats = new List<Beat>
            {
                new Beat { At = 0.16f, Key = Key.Space, Label = "SPACE", Caption = "Nhảy qua rễ tre!" },
                new Beat { At = 0.34f, Key = Key.A, Label = "A", Caption = "Né cành tre — sang trái!" },
                new Beat { At = 0.58f, Key = Key.D, Label = "D", Caption = "Né sang phải!" },
                new Beat { At = 0.8f, Key = Key.E, Label = "E", Caption = "Bấm liên tục — trèo lên bờ dốc!", Mash = true }
            };
            if (withCompanion) beats.Insert(2, new Beat { At = 0.46f, Key = Key.E, Label = "E", Caption = "Chị Lam ngã! Bấm liên tục — kéo chị dậy!", Mash = true, Companion = true });

            director.SetFreeControl(false);
            hud.SetControls("Bấm đúng phím hiện giữa màn hình (Space · A · D · E) trước khi vạch đỏ cạn");
            Restart(player, companion, withCompanion);
            AimCamera(player, 0f);
            director.CameraController.CutTo(_camera);
            if (director.Audio != null) { director.Audio.PlayMusic("chase"); director.Audio.SetAmbience("forest_rain"); }
            float baseScale = Time.timeScale;

            int successes = 0, next = 0;
            bool companionSaved = false, forced = false;
            // Sở Khanh shouted for the pursuers at the edge of the bamboo: they start closer.
            float startGap = director.GetInt("c5_so_khanh") == 3 ? _startGap - ShoutedCloser : _startGap;
            float distance = 0f, gap = startGap, stumble = 0f;
            while (distance < _total - 0.5f)
            {
                distance = Mathf.Max(distance, Project(player.transform.position));
                player.ScriptedSpeed = stumble > 0f ? 2.6f : _speed;
                if (stumble > 0f) stumble -= Time.deltaTime;
                player.ScriptedMove = PointAt(distance + 2f) - player.transform.position;
                AimCamera(player, distance);
                for (int i = 0; i < _pursuers.Length; i++)
                {
                    Vector3 target = PointAt(distance - gap - i * 1.3f);
                    float error = Vector3.Distance(_pursuers[i].transform.position, target);
                    _pursuers[i].MoveTo(target, _speed + Mathf.Clamp(error, 0f, 4f));
                }
                if (Random.value < Time.deltaTime * 0.4f && director.Fx != null) director.Fx.Shake(0.05f, 0.3f);

                if (next < beats.Count && distance >= beats[next].At * _total)
                {
                    var beat = beats[next++];
                    Time.timeScale = baseScale * 0.55f;
                    bool success = false;
                    yield return Prompt(director, beat, result => success = result);
                    Time.timeScale = baseScale;
                    if (thorough && !forced) { forced = true; success = false; gap = 0f; }
                    if (success)
                    {
                        if (beat.Companion) companionSaved = true; else successes++;
                        gap = Mathf.Min(gap + 0.8f, 9f);
                        if (director.Audio != null) director.Audio.PlaySfx("qte_ok");
                    }
                    else
                    {
                        if (beat.Companion) LoseCompanion(companion, hud);
                        else { gap -= 2.6f; stumble = 0.7f; }
                        if (director.Audio != null) { director.Audio.PlaySfx("qte_fail"); director.Audio.PlaySfx("stumble"); }
                        if (director.Fx != null) director.Fx.Shake(0.25f, 0.4f);
                    }
                }

                if (gap < 1.6f)
                {
                    StoryAutopilot.Note("CAUGHT in chase_forest → retry");
                    director.SetInt("c5_bi_bat", director.GetInt("c5_bi_bat") + 1);
                    player.ScriptedMove = null;
                    yield return hud.FailCard("Bị đuổi kịp!", "Bấm đúng phím hiện trên màn hình trước khi vạch đỏ cạn. Bấm sai phím cũng là vấp ngã.", director.Autopilot ? 0f : 2f);
                    successes = 0; next = 0; companionSaved = false; distance = 0f; gap = startGap; stumble = 0f;
                    Restart(player, companion, withCompanion);
                    yield return hud.Fade(0f, 0.4f);
                }
                yield return null;
            }

            player.ScriptedMove = null;
            hud.HideQte();
            director.CameraController.SetDialogueCue(null);
            foreach (var pursuer in _pursuers) { pursuer.Stop(); pursuer.gameObject.SetActive(false); }
            director.SetInt("c5_qte", Mathf.Clamp(successes - 1, 0, 3));
            director.SetBool("c5_co_lam_thoat", withCompanion && companionSaved);
        }

        private void Restart(ThuyKieu.Player.PlayerMovement player, Follower companion, bool withCompanion)
        {
            player.Teleport(_rail[0].position, _rail[0].rotation);
            if (withCompanion) { companion.gameObject.SetActive(true); companion.Following = true; companion.Snap(); }
            for (int i = 0; i < _pursuers.Length; i++)
            {
                _pursuers[i].gameObject.SetActive(true);
                _pursuers[i].EnableNavigation();
                _pursuers[i].Place(PointAt(-_startGap - i * 1.3f), _rail[0].rotation);
            }
        }

        private static void LoseCompanion(Follower companion, ThuyKieu.UI.StoryHUD hud)
        {
            if (companion == null) return;
            companion.Following = false;
            companion.GetComponent<StoryActor>().Stop();
            hud.ShowBark("CoLam", "Kiều! Đừng quay lại — chạy đi!", 2.5f);
        }

        private IEnumerator Prompt(StoryDirector director, Beat beat, System.Action<bool> done)
        {
            var hud = director.Hud;
            float window = beat.Mash ? _mashWindow : _window;
            int presses = 0;
            float autoAt = Random.Range(0.25f, 0.6f);
            bool autoSucceed = Random.value < 0.85f;
            for (float t = 0f; t < window; t += Time.unscaledDeltaTime)
            {
                hud.ShowQte(beat.Label, beat.Caption + (beat.Mash ? "  (" + presses + "/" + _mashCount + ")" : string.Empty), 1f - t / window,
                    beat.Mash ? new Color(1f, 0.6f, 0.35f) : new Color(1f, 0.82f, 0.45f));
                if (director.Autopilot)
                {
                    if (t >= autoAt && autoSucceed) { hud.HideQte(); done(true); yield break; }
                }
                else if (!ThuyKieu.UI.PauseMenuView.IsPaused)
                {
                    if (InputReader.KeyPressedThisFrame(beat.Key))
                    {
                        if (!beat.Mash || ++presses >= _mashCount) { hud.HideQte(); done(true); yield break; }
                    }
                    else if (Keys.Any(k => k != beat.Key && InputReader.KeyPressedThisFrame(k))) { hud.HideQte(); done(false); yield break; }
                }
                yield return null;
            }
            hud.HideQte();
            done(false);
        }

        /// <summary>
        /// Leading shot: the camera runs ahead of Kiều, low and off to one side, looking back past her at the
        /// torches of the pursuers, so the player sees how close they are.
        /// </summary>
        private void AimCamera(ThuyKieu.Player.PlayerMovement player, float distance)
        {
            Vector3 ahead = PointAt(distance + 3f), behind = PointAt(distance - 3.5f);
            Vector3 forward = (ahead - behind).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            Vector3 kieu = player.transform.position + Vector3.up * 1.25f;
            Vector3 chasers = PointAt(distance - 4f) + Vector3.up * 1.2f;
            _camera.position = PointAt(distance + 4.2f) + Vector3.up * 1.55f + side * 1.5f;
            _camera.rotation = Quaternion.LookRotation(Vector3.Lerp(kieu, chasers, 0.3f) - _camera.position);
        }

        // ------------------------------------------------------------------ rail

        private void Measure()
        {
            _lengths = new float[_rail.Length];
            _total = 0f;
            for (int i = 1; i < _rail.Length; i++) { _total += Vector3.Distance(_rail[i - 1].position, _rail[i].position); _lengths[i] = _total; }
        }

        private Vector3 PointAt(float distance)
        {
            if (distance <= 0f) return _rail[0].position + (_rail[0].position - _rail[1].position).normalized * -distance;
            for (int i = 1; i < _rail.Length; i++)
                if (distance <= _lengths[i])
                    return Vector3.Lerp(_rail[i - 1].position, _rail[i].position, (distance - _lengths[i - 1]) / (_lengths[i] - _lengths[i - 1]));
            return _rail[_rail.Length - 1].position;
        }

        private float Project(Vector3 point)
        {
            float best = 0f, bestDistance = float.MaxValue;
            for (int i = 1; i < _rail.Length; i++)
            {
                Vector3 a = _rail[i - 1].position, b = _rail[i].position;
                float k = Mathf.Clamp01(Vector3.Dot(point - a, b - a) / (b - a).sqrMagnitude);
                float d = Vector3.Distance(point, Vector3.Lerp(a, b, k));
                if (d < bestDistance) { bestDistance = d; best = _lengths[i - 1] + k * (_lengths[i] - _lengths[i - 1]); }
            }
            return best;
        }
    }
}

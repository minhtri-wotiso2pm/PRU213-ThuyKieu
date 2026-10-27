using System.Collections;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// A lục bát exchange ("# gameplay:lucbat_&lt;id&gt;"): pick the rhyme word for position six of the eight-word line,
    /// then the tone of the line for this listener. Score 0–3 (right word, right tone, and a point for both) goes to the
    /// couplet's Ink variable; the story reacts. When the two souls are close (đồng điệu ≥ 6) Kiều Thật greys out one wrong
    /// word; in the courtroom the "Thiện căn" page, if held, is turned to light the whole answer.
    /// Registered at runtime by the director, so no scene needs to author it.
    /// </summary>
    public class LucBatSequence : StorySequence
    {
        public static LucBatSequence Create(GameObject host, string id)
        {
            var sequence = host.AddComponent<LucBatSequence>();
            sequence.AssignId(id);
            return sequence;
        }

        public override IEnumerator Run(StoryDirector director)
        {
            var couplet = LucBatCatalog.Get(Id);
            if (couplet == null) yield break;
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            var view = LucBatView.Create();
            view.SetTitle(couplet.Title.ToUpper());
            view.SetLuc(couplet.Luc);
            view.SetBat(string.Format(couplet.BatPattern, "<color=#B23A1E>[ ? ]</color>"));

            bool verse = couplet.Id == "lucbat_ch7" && director.GetInt("ts_chu_tam") == PageCatalog.Held;
            if (verse)
            {
                director.SetInt("ts_chu_tam", PageCatalog.Used);
                view.SetFeedback("Trang \"Thiện căn ở tại lòng ta\" sáng lên trong tay áo: chữ tâm chỉ đường.");
                if (director.Audio != null) director.Audio.PlaySfx("clue");
            }
            bool helped = !verse && director.GetInt("dong_dieu") >= 6;
            double deadline = couplet.TimeLimit > 0f ? ThuyKieu.Core.GamePause.UnpausedTime + couplet.TimeLimit : -1;

            // Step 1: the rhyme word.
            var dimTiles = couplet.Tiles.Select((_, i) => verse && i != couplet.Correct).ToArray();
            if (helped)
            {
                int wrong = Enumerable.Range(0, couplet.Tiles.Length).First(i => i != couplet.Correct);
                dimTiles[wrong] = true;
                view.SetFeedback("Kiều Thật khẽ nói: \"Đừng chọn chữ " + couplet.Tiles[wrong] + ".\"");
            }
            view.ShowOptions(couplet.Tiles.Select(t => "<b>" + t + "</b>").ToArray(), dimTiles);
            yield return Capture(director, "1_tiles");
            int tile = -1;
            yield return Pick(director, view, couplet.Tiles.Length, dimTiles, deadline, couplet.TimeLimit, Autopick(couplet.Correct, couplet.Tiles.Length, dimTiles), i => tile = i);
            bool tileOk = tile == couplet.Correct;
            string word = couplet.Tiles[couplet.Correct];
            view.SetFeedback(tileOk ? "Đúng vần, đúng thanh." : (tile < 0 ? "Hết giờ." : couplet.Why[tile]) + " Chữ đúng là \"" + word + "\".");
            if (director.Audio != null) director.Audio.PlaySfx(tileOk ? "clue" : "wrong");
            view.SetBat(string.Format(couplet.BatPattern, "<color=#B23A1E>" + word + "</color>"));
            yield return Hold(director, 1.2f);

            // Step 2: the tone of the whole line (the prologue's verse has only the original).
            bool toneOk = true;
            int toneIndex = 0;
            if (couplet.Tones.Length > 1)
            {
                var dimTones = couplet.Tones.Select((_, i) => verse && i != couplet.Preferred).ToArray();
                view.ShowOptions(couplet.Tones.Select(t => "<color=#E8C27A>" + t.Label + "</color> — " + t.Line).ToArray(), dimTones);
                view.SetFeedback("Giọng nào hợp với người đang nghe?");
                yield return Capture(director, "2_tones");
                int tone = -1;
                yield return Pick(director, view, couplet.Tones.Length, dimTones, deadline, couplet.TimeLimit, Autopick(couplet.Preferred, couplet.Tones.Length, dimTones), i => tone = i);
                toneIndex = Mathf.Max(0, tone);
                toneOk = couplet.Preferred < 0 ? tone >= 0 : tone == couplet.Preferred;
            }

            int score = couplet.Tones.Length > 1 ? (tileOk ? 1 : 0) + (toneOk ? 1 : 0) + (tileOk && toneOk ? 1 : 0) : (tileOk ? 3 : 0);
            director.SetInt(couplet.Variable, score);
            StoryAutopilot.Note("LUCBAT " + couplet.Id + " tile " + (tileOk ? "ok" : "wrong") + " · tone " + (toneOk ? "ok" : "off") + " · score " + score);

            view.ShowOptions(new string[0], null);
            view.SetTimer(-1f);
            view.SetLuc(couplet.Luc);
            view.SetBat(couplet.Tones[toneIndex].Line);
            view.SetFeedback(score >= 3 ? "Đúng luật, đúng lòng người nghe." : score == 2 ? "Câu thơ hay, nhưng chưa thật chạm tới người nghe." : "Câu thơ vấp váp.");
            ThuyKieu.Dialogue.VoicePlayer.PlayBark("Kieu", couplet.Luc + ", " + couplet.Tones[toneIndex].Line);
            yield return Hold(director, 2.4f);
            view.Close();
        }

        /// <summary>Waits for a key (1–n), a click, the autopilot's answer or the deadline (-1 = none).</summary>
        private static IEnumerator Pick(StoryDirector director, LucBatView view, int count, bool[] dimmed, double deadline, float limit, int autopick, System.Action<int> done)
        {
            if (director.Autopilot)
            {
                yield return new WaitForSecondsRealtime(0.2f);
                done(autopick);
                yield break;
            }
            while (true)
            {
                yield return null;
                if (PauseMenuView.IsPaused) continue;
                if (deadline > 0f)
                {
                    float left = (float)(deadline - ThuyKieu.Core.GamePause.UnpausedTime);
                    view.SetTimer(left / limit);
                    if (left <= 0f) { done(-1); yield break; }
                }
                int choice = view.Clicked;
                var keyboard = Keyboard.current;
                if (keyboard != null)
                    for (int i = 0; i < count && i < 9; i++)
                        if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame) choice = i;
                view.Clicked = -1;
                if (choice >= 0 && choice < count && !dimmed[choice]) { done(choice); yield break; }
            }
        }

        /// <summary>QA answer: right for the Nghịch Mệnh run, wrong for the Định Mệnh run, otherwise right most of the time.</summary>
        private static int Autopick(int correct, int count, bool[] dimmed)
        {
            string target = StoryAutopilot.EndingTarget;
            int right = correct < 0 ? 0 : correct;
            int wrong = Enumerable.Range(0, count).FirstOrDefault(i => i != right && !dimmed[i]);
            if (target == "nghich_menh") return right;
            if (target == "dinh_menh") return count > 1 ? wrong : right;
            return Random.value < 0.7f || count == 1 ? right : wrong;
        }

        /// <summary>QA stills of the exchange while the autopilot plays it.</summary>
        private IEnumerator Capture(StoryDirector director, string step)
        {
            if (!director.Autopilot || StoryCapture.Directory == null) yield break;
            yield return new WaitForSecondsRealtime(0.8f);
            StoryCapture.Shot("LB_" + Id + "_" + step);
        }

        private static IEnumerator Hold(StoryDirector director, float seconds)
        {
            if (director.Autopilot) { yield return new WaitForSecondsRealtime(0.1f); yield break; }
            double started = ThuyKieu.Core.GamePause.UnpausedTime;
            while (true)
            {
                if (PauseMenuView.IsPaused) { yield return null; continue; }
                float t = (float)(ThuyKieu.Core.GamePause.UnpausedTime - started);
                if (t >= seconds) break;
                var keyboard = Keyboard.current;
                if (t > 0.4f && keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) break;
                yield return null;
            }
        }
    }
}

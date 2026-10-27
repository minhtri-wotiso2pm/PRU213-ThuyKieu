using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Dialogue;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 7 cross-examination, one round per instance (Tú Bà, Mã Giám Sinh, Tú Bà again).
    /// The witness gives four statements. The player presses a statement to hear more (a press can make the witness
    /// correct a detail, opening a new contradiction) or presents evidence from the Chapter 6 dossier against it.
    /// The right evidence breaks the statement and raises the magistrate's trust; the wrong one costs trust. The round
    /// ends when its key statement falls; every other broken statement adds to the score that sets the ending's tone.
    /// Mistakes earn hints (an inner-voice nudge, then a marked statement and the card to use). At zero trust Kim Trọng
    /// speaks up once (if the plan was made at the gate); after that the round is lost — never replayed: the magistrate
    /// reads the papers himself, the story goes on, and the round scores nothing.
    /// Writes c7_r&lt;round&gt;, c7_diem, c7_long_tin and c7_thua_vong.
    /// </summary>
    public class TrialSequence : StorySequence
    {
        private struct Line { public string Speaker, Text; }

        private sealed class Proof
        {
            public string Evidence;
            public int Points;
            public Line[] Lines;
        }

        private sealed class Statement
        {
            public Line Claim;
            public Line Revision;
            public Line[] Press;
            public Proof[] Proofs = new Proof[0];
            public bool Key;
            /// <summary>The proofs only fit once the witness has corrected the claim under pressing.</summary>
            public bool NeedsRevision;
            public bool Revised, Disproved, Hinted;
            public Line Current => Revised ? Revision : Claim;
        }

        private sealed class Evidence { public string Id, Title, Body; }

        [SerializeField] private TrialView _view;
        [SerializeField, Range(1, 3)] private int _round = 1;

        private const float StartTrust = 0.6f, RecoverTrust = 0.4f, LostTrust = 0.1f, WrongCost = 0.2f, ProofGain = 0.15f, BribedCost = 0.15f;
        private const int ForesightCost = 10, LostRoundFate = 10;
        private static bool _lifelineUsed;

        private StoryDirector _director;
        private float _trust;

        private static Line Say(string speaker, string text) => new Line { Speaker = speaker, Text = text };

        public override IEnumerator Run(StoryDirector director)
        {
            _director = director;
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            if (director.Audio != null) director.Audio.PlayMusic("trial");
            if (_round == 1) _lifelineUsed = false;
            // A lost round leaves the magistrate cold, but the next witness still gets a fair hearing.
            _trust = _round == 1 ? StartTrust : Mathf.Max(RecoverTrust, director.GetInt("c7_long_tin") / 100f);
            // Định Mệnh ≥ 85: the magistrate has taken Tú Bà's gift. Seeing the bribe at the gate cancels it.
            if (_round == 1 && director.GetInt("dinh_menh") >= 85 && !director.GetBool("c7_biet_hoi_lo")) _trust -= BribedCost;
            bool thorough = director.Autopilot && StoryAutopilot.Args.Contains("-autopilotThorough");
            bool forcedLoss = false;

            var statements = Build();
            var evidence = Available();
            string witness = statements[0].Claim.Speaker;
            _view.Open("ĐỐI CHẤT · VÒNG " + _round + " / 3", "Lời khai của " + SpeakerCatalog.Get(witness).DisplayName + " · bằng chứng trong tay: " + evidence.Count);
            _view.SetTrust(_trust);
            ShowHelp();
            int score = 0, selected = 0, mistakes = 0;
            bool lost = false;
            Refresh(statements, selected);
            Shot("CU_" + witness);
            yield return Speak(statements[0].Claim, 0.6f);

            while (!statements.Any(s => s.Key && s.Disproved))
            {
                // Pick an action: (statement, press) or (statement, evidence index).
                int action = -1, card = -1;
                if (director.Autopilot)
                {
                    yield return new WaitForSecondsRealtime(0.15f);
                    // Thorough QA walks both hints in round 1 and a lost round in round 2.
                    string target = StoryAutopilot.EndingTarget;
                    if (target == "dinh_menh" && !forcedLoss) { forcedLoss = true; _trust = 0.01f; selected = 0; card = WrongCard(statements[0], evidence); action = 2; }
                    else if (target == "nghich_menh" && _round == 1 && Foresight(statements) < 0 && HeldPage() != null) { action = 3; }
                    else if (thorough && _round == 1 && mistakes < 2) { selected = 0; card = WrongCard(statements[0], evidence); action = 2; }
                    else if (thorough && _round == 2 && !forcedLoss) { forcedLoss = true; _trust = 0.01f; selected = 0; card = WrongCard(statements[0], evidence); action = 2; }
                    else AutopilotChoice(statements, evidence, ref selected, out action, out card);
                }
                else
                {
                    yield return WaitForChoice(statements, evidence, selected, (s, a, c) => { selected = s; action = a; card = c; });
                }
                Refresh(statements, selected);
                var statement = statements[selected];
                if (action == 1) { yield return PressStatement(statement); Refresh(statements, selected); continue; }
                if (action == 3) { yield return TurnPage(statements); Refresh(statements, selected); ShowHelp(); continue; }

                var proof = card >= 0 && (!statement.NeedsRevision || statement.Revised)
                    ? statement.Proofs.FirstOrDefault(p => p.Evidence == evidence[card].Id) : null;
                if (proof != null)
                {
                    statement.Disproved = true;
                    score += proof.Points;
                    _trust = Mathf.Min(1f, _trust + ProofGain * proof.Points);
                    _view.SetTrust(_trust);
                    if (director.Audio != null) director.Audio.PlaySfx("objection");
                    if (director.Fx != null) director.Fx.Shake(0.25f, 0.35f);
                    StoryAutopilot.Note("TRIAL r" + _round + " proved statement " + (selected + 1) + " with " + proof.Evidence + (statement.Key ? " (key)" : ""));
                    yield return _view.Stamp(statement.Key ? "BÁC BỎ!" : "MÂU THUẪN!", statement.Key ? StoryUIBuilder.Vermilion : StoryUIBuilder.Gold, director.Autopilot ? 0.1f : 1.1f);
                    foreach (var line in proof.Lines) yield return Speak(line, 0.4f);
                    Refresh(statements, selected);
                    continue;
                }

                // Wrong evidence (or evidence against a statement that is still true).
                _trust -= WrongCost;
                mistakes++;
                _view.SetTrust(_trust);
                if (director.Audio != null) director.Audio.PlaySfx("wrong");
                if (director.Fx != null) director.Fx.Shake(0.15f, 0.3f);
                StoryAutopilot.Note("TRIAL r" + _round + " wrong evidence on statement " + (selected + 1));
                Shot("CU_QuanHuyen");
                yield return Speak(Wrong(), 0.5f);
                if (_trust > 0.001f)
                {
                    yield return Hint(statements, evidence, mistakes);
                    Refresh(statements, selected);
                    continue;
                }

                if (!_lifelineUsed && director.GetBool("c7_ke_hoach_kt") && !thorough && StoryAutopilot.EndingTarget != "dinh_menh")
                {
                    _lifelineUsed = true;
                    _trust = 0.35f;
                    _view.SetTrust(_trust);
                    Shot("CU_KimTrong");
                    yield return Speak(Say("KimTrong", "Bẩm quan, xin cho học trò nói một lời. Nàng sợ quá nên rối, không phải nói càn. Xin quan cho nàng xét lại lời khai một lần nữa."), 0.6f);
                    Shot("CU_QuanHuyen");
                    yield return Speak(Say("QuanHuyen", "Nể mặt nhà họ Kim. Lần này thôi đấy."), 0.6f);
                    yield return Hint(statements, evidence, Mathf.Max(mistakes, 2));
                    Refresh(statements, selected);
                    continue;
                }
                lost = true;
                break;
            }

            if (lost)
            {
                // Out of patience: the magistrate reads the papers himself. The truth still comes out, but not from Kiều.
                StoryAutopilot.Note("TRIAL r" + _round + " lost the magistrate → round lost, story goes on");
                if (director.Audio != null) director.Audio.PlaySfx("fail");
                foreach (var s in statements) if (s.Key) s.Disproved = true;
                Refresh(statements, selected);
                yield return _view.Stamp("QUAN TỰ XÉT", StoryUIBuilder.Vermilion, director.Autopilot ? 0.1f : 1.4f);
                foreach (var line in Takeover()) yield return Speak(line, 0.4f);
                score = 0;
                _trust = LostTrust;
                _view.SetTrust(_trust);
                director.SetInt("c7_thua_vong", director.GetInt("c7_thua_vong") + 1);
                director.SetInt("dinh_menh", Mathf.Min(100, director.GetInt("dinh_menh") + LostRoundFate));
            }

            director.SetInt("c7_r" + _round, score);
            director.SetInt("c7_diem", director.GetInt("c7_r1") + director.GetInt("c7_r2") + director.GetInt("c7_r3"));
            director.SetInt("c7_long_tin", Mathf.Clamp(Mathf.RoundToInt(_trust * 100f), 1, 100));
            StoryAutopilot.Note("TRIAL r" + _round + (lost ? " lost" : " done") + " · score " + score + " · trust " + Mathf.RoundToInt(_trust * 100f));
            yield return new WaitForSecondsRealtime(director.Autopilot ? 0f : 0.8f);
            _view.Close();
        }

        // ------------------------------------------------------------------ hints

        /// <summary>
        /// After a mistake: the first nudges towards the statement that ends the round; from the second on, that
        /// statement is marked and the card that breaks it is named — by Kiều Thật when the two souls are close,
        /// by Kim Trọng when the plan was made at the gate, otherwise by the inner voice.
        /// </summary>
        private IEnumerator Hint(List<Statement> statements, List<Evidence> evidence, int mistakes)
        {
            int target = statements.FindIndex(s => s.Key && !s.Disproved);
            if (target < 0) yield break;
            int card = Card(statements[target], evidence);
            string n = (target + 1).ToString();
            if (mistakes <= 1 || card < 0)
            {
                yield return Speak(Say("NoiTam", "Khoan. Đừng đánh vào chỗ râu ria. Câu thứ " + n + " mới là điều họ cần giấu nhất."), 0.4f);
                yield break;
            }

            statements[target].Hinted = true;
            string title = evidence[card].Title;
            if (_director.GetInt("dong_dieu") >= 5)
                yield return Speak(Say("KieuThat", "Câu thứ " + n + ". Ta còn nhớ tờ \"" + title + "\" — nó đang nằm trong tay ngươi."), 0.4f);
            else if (_director.GetBool("c7_ke_hoach_kt"))
                yield return Speak(Say("KimTrong", "(khẽ) Câu thứ " + n + ". Đưa \"" + title + "\" ra, nàng."), 0.4f);
            else
                yield return Speak(Say("NoiTam", "Bình tĩnh. Câu thứ " + n + " — \"" + title + "\" là đủ để bẻ nó."), 0.4f);
        }

        /// <summary>What the magistrate says when he stops listening and reads the key paper himself.</summary>
        private Line[] Takeover()
        {
            switch (_round)
            {
                case 1:
                    return new[]
                    {
                        Say("QuanHuyen", "Đủ rồi! Đưa hết giấy tờ lên đây, bản quan tự xem."),
                        Say("QuanHuyen", "Giao kèo... \"mỗi người giao đến, trả bốn trăm năm mươi lạng\". Mai mối kiểu gì thế này?"),
                        Say("NoiTam", "Quan tự tìm ra, không phải nhờ mình. Ông ta chẳng còn tin lời mình nữa.")
                    };
                case 2:
                    return new[]
                    {
                        Say("QuanHuyen", "Im cả! Bản quan đọc giấy còn nhanh hơn nghe các ngươi cãi."),
                        Say("QuanHuyen", "Giao kèo lập từ mùng chín tháng Giêng. Mã Giám Sinh, thế mà ngươi bảo chưa từng gặp mụ?"),
                        Say("NoiTam", "Lại là quan tự đọc ra. Mình chỉ đứng đó như một cái bóng.")
                    };
                default:
                    return new[]
                    {
                        Say("QuanHuyen", "Đủ! Món nợ này bản quan tự tính."),
                        Say("QuanHuyen", "Bốn trăm năm mươi lạng, đúng bằng giá trên giao kèo. Đây là nợ, hay là giá mua người?"),
                        Say("NoiTam", "Sự thật vẫn lộ ra. Nhưng không phải từ miệng mình.")
                    };
            }
        }

        // ------------------------------------------------------------------ input

        private delegate void Chosen(int statement, int action, int card);

        /// <summary>1–4 / arrows pick a statement; Q presses; E opens the evidence case, where 1–9 or a click presents.</summary>
        private IEnumerator WaitForChoice(List<Statement> statements, List<Evidence> evidence, int selected, Chosen done)
        {
            bool caseOpen = false;
            _view.ClickedRow = _view.ClickedCard = -1;
            _view.PressClicked = _view.PresentClicked = false;
            ShowRight(statements, evidence, selected, caseOpen);
            while (true)
            {
                yield return null;
                if (PauseMenuView.IsPaused) continue;
                var keyboard = Keyboard.current;
                int row = _view.ClickedRow;
                _view.ClickedRow = -1;
                if (!caseOpen && keyboard != null)
                {
                    for (int i = 0; i < statements.Count; i++) if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame) row = i;
                    if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) row = (selected + statements.Count - 1) % statements.Count;
                    if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) row = (selected + 1) % statements.Count;
                }
                if (row >= 0 && row != selected)
                {
                    selected = row;
                    caseOpen = false;
                    Refresh(statements, selected);
                    ShowRight(statements, evidence, selected, caseOpen);
                    Shot("CU_" + statements[selected].Current.Speaker);
                    VoicePlayer.PlayBark(statements[selected].Current.Speaker, statements[selected].Current.Text);
                    _view.ShowLine(statements[selected].Current.Speaker, statements[selected].Current.Text);
                    continue;
                }
                if (statements[selected].Disproved) continue;
                bool press = _view.PressClicked || (!caseOpen && keyboard != null && keyboard.qKey.wasPressedThisFrame);
                bool present = _view.PresentClicked || (!caseOpen && keyboard != null && keyboard.eKey.wasPressedThisFrame);
                _view.PressClicked = _view.PresentClicked = false;
                if (press) { done(selected, 1, -1); yield break; }
                if (!caseOpen && keyboard != null && keyboard.tKey.wasPressedThisFrame && HeldPage() != null) { done(selected, 3, -1); yield break; }
                if (present && !caseOpen) { caseOpen = true; ShowRight(statements, evidence, selected, caseOpen); continue; }
                if (caseOpen)
                {
                    int card = _view.ClickedCard;
                    _view.ClickedCard = -1;
                    if (keyboard != null)
                    {
                        for (int i = 0; i < evidence.Count && i < 9; i++) if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame) card = i;
                        // Esc stays the pause key; Backspace or the right mouse button closes the case.
                        var mouse = Mouse.current;
                        if (keyboard.backspaceKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame || (mouse != null && mouse.rightButton.wasPressedThisFrame))
                        {
                            caseOpen = false;
                            ShowRight(statements, evidence, selected, caseOpen);
                            continue;
                        }
                    }
                    if (card >= 0) { done(selected, 2, card); yield break; }
                }
            }
        }

        private void ShowRight(List<Statement> statements, List<Evidence> evidence, int selected, bool caseOpen)
        {
            if (caseOpen) _view.ShowEvidence(evidence.Select(e => new TrialView.Card { Title = e.Title, Body = e.Body }).ToList());
            else _view.ShowActions(selected, statements[selected].Disproved);
        }

        private void Refresh(List<Statement> statements, int selected)
        {
            _view.ShowRows(statements.Select(s => new TrialView.Row { Speaker = s.Current.Speaker, Text = s.Current.Text, Disproved = s.Disproved, Revised = s.Revised, Hinted = s.Hinted }).ToList(), selected);
        }

        private IEnumerator PressStatement(Statement statement)
        {
            if (_director.Audio != null) _director.Audio.PlaySfx("ui", 0.6f);
            foreach (var line in statement.Press) yield return Speak(line, 0.35f);
            if (statement.Revision.Text != null && !statement.Revised)
            {
                statement.Revised = true;
                if (_director.Audio != null) _director.Audio.PlaySfx("clue");
                yield return Speak(Say("NoiTam", "Mụ vừa sửa lời. Câu khai đã đổi — xem lại xem giờ nó trái với thứ gì."), 0.3f);
            }
        }

        /// <summary>Shows and speaks a line, waiting for the voice (or a reading pause) and a key to skip.</summary>
        private IEnumerator Speak(Line line, float minimum)
        {
            var actor = _director.FindActor(line.Speaker);
            if (actor != null) actor.SetTalking(true, line.Speaker == "TuBa" ? 2 : line.Speaker == "Kieu" ? 4 : 0);
            // Inner voices have no body to cut to.
            if (line.Speaker != "NoiTam" && line.Speaker != "KieuThat") Shot(line.Speaker == "Kieu" ? "CU_Kieu" : "CU_" + line.Speaker);
            _view.ShowLine(line.Speaker, line.Text);
            VoicePlayer.PlayBark(line.Speaker, line.Text);
            if (_director.Autopilot) { yield return new WaitForSecondsRealtime(0.05f); if (actor != null) actor.SetTalking(false, 0); yield break; }
            float wait = Mathf.Clamp(line.Text.Length * 0.045f, 1.4f, 7f);
            for (float t = 0; t < wait; t += Time.unscaledDeltaTime)
            {
                if (t > minimum && Skip()) break;
                yield return null;
            }
            if (actor != null) actor.SetTalking(false, 0);
        }

        private static bool Skip()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return !PauseMenuView.IsPaused && ((keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                                               || (mouse != null && mouse.rightButton.wasPressedThisFrame));
        }

        private void Shot(string id) { if (_director.Shots != null) _director.Shots.Shot(id); }

        private static Line Wrong()
        {
            switch (Random.Range(0, 3))
            {
                case 0: return Say("QuanHuyen", "Đưa thứ ấy ra lúc này là có ý gì? Nó chẳng liên quan gì đến câu khai ấy!");
                case 1: return Say("QuanHuyen", "Hồ đồ! Công đường không phải chỗ để đem giấy tờ ra bày cho đẹp.");
                default: return Say("QuanHuyen", "Bản quan chẳng thấy thứ đó chứng minh được điều gì cả. Còn làm mất thời gian nữa thì đừng trách.");
            }
        }

        // ------------------------------------------------------------------ Trang Sách: Tiên tri

        /// <summary>Pages with a courtroom use of their own (the witnesses' stand, the closing verse) are never spent here.</summary>
        private static readonly string[] Reserved = { "ts_be_dau", "ts_chu_tam" };

        /// <summary>The first page the player can still turn (in catalog order), or null.</summary>
        private PageCatalog.Page HeldPage() =>
            PageCatalog.All.FirstOrDefault(p => !p.Unusable && !Reserved.Contains(p.Id) && _director.GetInt(p.Id) == PageCatalog.Held);

        /// <summary>Index of the statement already lit by a turned page, or -1.</summary>
        private static int Foresight(List<Statement> statements) => statements.FindIndex(s => s.Hinted);

        private void ShowHelp()
        {
            int held = PageCatalog.All.Count(p => !p.Unusable && !Reserved.Contains(p.Id) && _director.GetInt(p.Id) == PageCatalog.Held);
            _view.ExtraHelp = held > 0 ? "<color=#E8C27A>T</color> lật một trang Tiên tri: chỉ ra câu khai then chốt (còn " + held + " trang · +" + ForesightCost + " Định Mệnh)" : string.Empty;
        }

        /// <summary>Turns a held page: the verse lights the statement this round turns on.</summary>
        private IEnumerator TurnPage(List<Statement> statements)
        {
            var page = HeldPage();
            int target = statements.FindIndex(s => s.Key && !s.Disproved);
            if (page == null || target < 0) yield break;
            _director.SetInt(page.Id, PageCatalog.Used);
            _director.SetInt("dinh_menh", Mathf.Min(100, _director.GetInt("dinh_menh") + ForesightCost));
            statements[target].Hinted = true;
            if (_director.Audio != null) _director.Audio.PlaySfx("clue");
            StoryAutopilot.Note("TRIAL r" + _round + " turned page " + page.Id + " (Tiên tri)");
            yield return Speak(Say("TrangSach", page.Verse.Replace('\n', ' ')), 0.4f);
            yield return Speak(Say("NoiTam", "Trang sách đã nói trước rồi. Câu thứ " + (target + 1) + " — đó là chỗ họ cần giấu nhất."), 0.4f);
        }

        // ------------------------------------------------------------------ autopilot

        private void AutopilotChoice(List<Statement> statements, List<Evidence> evidence, ref int selected, out int action, out int card)
        {
            action = 2; card = -1;
            var open = Enumerable.Range(0, statements.Count).Where(i => !statements[i].Disproved).ToList();
            float roll = Random.value;
            // Mostly sensible play: press anything that can still change, prove what can be proved, sometimes a mistake.
            var revisable = open.Where(i => statements[i].Revision.Text != null && !statements[i].Revised).ToList();
            if (revisable.Count > 0 && roll < 0.5f) { selected = revisable[Random.Range(0, revisable.Count)]; action = 1; return; }
            var provable = open.Where(i => Card(statements[i], evidence) >= 0).ToList();
            if (provable.Count > 0 && roll < 0.85f)
            {
                // Leave the key for last half of the time, so bonus contradictions get found too.
                var bonus = provable.Where(i => !statements[i].Key).ToList();
                selected = bonus.Count > 0 && Random.value < 0.6f ? bonus[Random.Range(0, bonus.Count)] : provable[Random.Range(0, provable.Count)];
                card = Card(statements[selected], evidence);
                return;
            }
            if (roll < 0.93f || StoryAutopilot.EndingTarget == "nghich_menh") { selected = open[Random.Range(0, open.Count)]; action = 1; return; }
            selected = open[Random.Range(0, open.Count)];
            card = WrongCard(statements[selected], evidence);
            if (card < 0) action = 1;
        }

        private static int Card(Statement statement, List<Evidence> evidence)
        {
            if (statement.NeedsRevision && !statement.Revised) return -1;
            for (int i = 0; i < evidence.Count; i++) if (statement.Proofs.Any(p => p.Evidence == evidence[i].Id)) return i;
            return -1;
        }

        private static int WrongCard(Statement statement, List<Evidence> evidence)
        {
            for (int i = 0; i < evidence.Count; i++) if (!statement.Proofs.Any(p => p.Evidence == evidence[i].Id)) return i;
            return -1;
        }

        // ------------------------------------------------------------------ the case file

        private List<Evidence> Available()
        {
            var d = _director;
            int retreated = d.GetInt("c7_lui");
            var list = new List<Evidence>
            {
                new Evidence { Id = "khe_hon", Title = "Tờ khế hôn ước", Body = "Tháng Tư. Mã Giám Sinh xin cưới Vương Thúy Kiều làm vợ, sính lễ 400 lạng." },
                // The contract is always in the case: Chapter 4 cannot end without it.
                new Evidence { Id = "giao_keo", Title = "Giao kèo Mã – Tú Bà", Body = "Mùng 9 tháng Giêng. Mỗi người Mã giao đến, Tú Bà trả 450 lạng. Ba tên đã gạch." }
            };
            if (d.GetInt("c4_van_tu") == 1) list.Add(new Evidence { Id = "van_tu", Title = "Văn tự bán thân", Body = "Người bán: Mã Giám Sinh. Người mua: Tú Bà. Người bị bán: Vương Thúy Kiều. 450 lạng." });
            if (d.GetBool("ev_giay_no")) list.Add(new Evidence { Id = "so_no", Title = "Trang sổ nợ", Body = "Vương Thúy Kiều nợ tiền mua 450 lạng. Áo lụa 30, son phấn 10. Chưa trả." });
            if (d.GetInt("c4_tien") == 2) list.Add(new Evidence { Id = "tui_bac", Title = "Túi bạc dấu \"T.\"", Body = "450 lạng nén, mỗi nén đóng dấu lầu Lâm Tri. Chưa tiêu một đồng." });
            if (d.GetBool("c6_so_bac")) list.Add(new Evidence { Id = "so_bac", Title = "Sổ hiệu bạc Phúc Ký", Body = "14 tháng Tư: người nhà Tú Bà giao 450 lạng cho \"Mã công tử\", có điểm chỉ." });
            if (d.GetBool("giu_mau_giay")) list.Add(new Evidence { Id = "mau_giay", Title = "Mẩu giấy trên đường", Body = "\"Bà T. ở Lâm Tri nhận hàng — giá 450 lạng.\"" });
            if (d.GetBool("c7_biet_hoi_lo")) list.Add(new Evidence { Id = "hoi_lo", Title = "Túi gấm đổi tay", Body = "Sáng nay, trước cổng: Tú Bà dúi túi gấm cho một nha dịch." });
            if (d.GetBool("nc_ba_nuoc") && retreated != 2) list.Add(new Evidence { Id = "ba_nuoc", Title = "Nhân chứng: bà hàng nước", Body = "Nghe Mã và tùy tùng nói chuyện \"giao hàng\" cho Tú Bà sau vách quán." });
            if (d.GetBool("nc_thu_lai") && retreated != 3) list.Add(new Evidence { Id = "thu_lai", Title = "Nhân chứng: thư lại", Body = "Sổ hôn thú của huyện: Mã \"cưới vợ\" bốn lần một năm; bằng giám sinh là bằng mua." });
            if (d.GetBool("nc_co_lam") && retreated != 1) list.Add(new Evidence { Id = "co_lam", Title = "Nhân chứng: chị Lam", Body = "Ba năm ở lầu Tú Bà, một món nợ không bao giờ trả hết." });
            if (d.GetInt("kt_tin_tuong") >= 3) list.Add(new Evidence { Id = "kim_trong", Title = "Lời Kim Trọng", Body = "Gặp Kiều ở cổng chùa Bạch Vân, đêm nàng trốn khỏi lầu." });
            return list;
        }

        private static Proof P(string evidence, int points, params Line[] lines) => new Proof { Evidence = evidence, Points = points, Lines = lines };

        private List<Statement> Build()
        {
            switch (_round)
            {
                case 1: return TuBaTheft();
                case 2: return MaGiamSinh();
                default: return TuBaDebt();
            }
        }

        private static List<Statement> TuBaTheft() => new List<Statement>
        {
            new Statement
            {
                Claim = Say("TuBa", "Con bé này là người tôi bỏ bạc ra mua về, giấy tờ đàng hoàng, quan lớn cứ xem."),
                Press = new[] { Say("TuBa", "Mua người ở, mua đứa giúp việc thôi ạ. Nhà nào có của mà chẳng thế."), Say("NoiTam", "Mụ nói chữ \"mua\" mà không chớp mắt.") },
                Proofs = new[]
                {
                    P("khe_hon", 1, Say("Kieu", "Ta là vợ có cưới hỏi của Mã Giám Sinh. Tờ khế hôn ước còn đây. Vợ của người ta, sao lại thành người bà \"mua về\"?"),
                        Say("QuanHuyen", "Vừa là vợ người này, vừa là của mua của người kia? Lạ thật.")),
                    P("van_tu", 1, Say("Kieu", "\"Giấy tờ đàng hoàng\" của bà đây: văn tự bán thân, người bán là Mã Giám Sinh. Có người chồng nào lại đem bán vợ mình?"),
                        Say("QuanHuyen", "Hừm. Bản quan ghi nhận."))
                }
            },
            new Statement
            {
                Claim = Say("TuBa", "Đêm mười sáu, nó cạy tủ lấy trộm năm trăm lạng bạc rồi trốn đi."),
                Revision = Say("TuBa", "Đêm mười sáu, nó cạy tủ lấy trộm bốn trăm năm mươi lạng bạc rồi trốn đi."),
                NeedsRevision = true,
                Press = new[] { Say("Kieu", "Năm trăm lạng? Bà chắc chứ?"), Say("TuBa", "Năm trăm... à, bốn trăm năm mươi lạng! Già rồi, nhớ nhầm. Nhưng là bạc của tôi!") },
                Proofs = new[]
                {
                    P("tui_bac", 1, Say("Kieu", "Bốn trăm năm mươi lạng, đúng bằng giá mua ta ghi trên giao kèo. Túi bạc còn nguyên dấu \"T.\", ta chưa tiêu một đồng. Ta mang nó theo để làm bằng."),
                        Say("QuanHuyen", "Bạc trộm mà không tiêu, lại đem nộp quan? Thật chưa thấy kẻ trộm nào như thế.")),
                    P("so_bac", 1, Say("Kieu", "Bốn trăm năm mươi lạng ấy đâu có ở trong tủ bà. Sổ hiệu bạc ghi: ngày mười bốn tháng Tư, bà đã giao đúng số đó cho Mã công tử."),
                        Say("QuanHuyen", "Tiền đã giao đi từ trước, thì đêm mười sáu lấy đâu ra mà mất?")),
                    P("kim_trong", 1, Say("Kieu", "Đêm ấy ta trốn đi gần như tay không. Kim công tử gặp ta ở cổng chùa, ngoài bộ áo ướt chẳng có gì."),
                        Say("KimTrong", "Học trò xin làm chứng: nàng không mang theo túi bạc nào cả."))
                }
            },
            new Statement
            {
                Claim = Say("TuBa", "Bạc ấy là tiền chợ của nhà tôi, làm ăn lương thiện cả."),
                Press = new[] { Say("TuBa", "Buôn son phấn, lụa là, cho khách thuê phòng... lương thiện cả, quan lớn ạ.") },
                Proofs = new[]
                {
                    P("so_bac", 1, Say("Kieu", "Sổ hiệu bạc Phúc Ký: ngày mười bốn tháng Tư, người nhà bà lấy bốn trăm năm mươi lạng giao cho \"Mã công tử\". Tiền chợ gì mà trả cho người mai mối đúng bằng giá một con người?"),
                        Say("QuanHuyen", "Sổ hiệu bạc có điểm chỉ... Tú Bà, mụ giải thích đi.")),
                    P("hoi_lo", 1, Say("Kieu", "Làm ăn lương thiện, vậy sáng nay bà dúi túi gấm cho nha dịch ngoài cổng làm gì?"),
                        Say("NhaDich", "Bẩm... bẩm quan, tiểu nhân..."), Say("QuanHuyen", "Câm miệng! Lát nữa bản quan xử ngươi sau.")),
                    P("tui_bac", 1, Say("Kieu", "Tiền chợ mà nén nào cũng đóng dấu riêng của lầu, cất trong tủ khóa cùng với văn tự mua người?"),
                        Say("QuanHuyen", "Hừm. Tiền chợ thì không cần giấu kỹ thế."))
                }
            },
            new Statement
            {
                Key = true,
                Claim = Say("TuBa", "Mã công tử chỉ là người mai mối giúp, chẳng dính dáng gì đến chuyện này."),
                Press = new[] { Say("TuBa", "Cậu ấy học hành tử tế, giúp người ta tìm chỗ nương thân thôi mà."), Say("NoiTam", "\"Chỗ nương thân.\" Họ gọi cái lồng là chỗ nương thân.") },
                Proofs = new[]
                {
                    P("giao_keo", 1, Say("Kieu", "Mai mối mà có giao kèo này? Lập từ tháng Giêng: Mã Giám Sinh lo việc hỏi vợ ở các huyện, mỗi người giao đến, Tú Bà trả bốn trăm năm mươi lạng. Đó không phải mai mối. Đó là buôn người."),
                        Say("TuBa", "Cái... cái giấy ấy...")),
                    P("mau_giay", 1, Say("Kieu", "Mẩu giấy rơi từ hòm của Mã Giám Sinh: \"Bà T. ở Lâm Tri nhận hàng, giá bốn trăm năm mươi lạng.\" Mai mối nào lại gọi cô dâu là \"hàng\"?"),
                        Say("TuBa", "Bà T. nào? Đất Lâm Tri thiếu gì người họ T.!")),
                    P("ba_nuoc", 1, Say("BaHangNuoc", "Bẩm quan, già này nghe tận tai: gã họ Mã hỏi \"Tú Bà đã chuẩn bị chưa\", rồi cười bảo giao hàng xong là có bạc."),
                        Say("TuBa", "Mụ già này ở đâu ra mà dám..."))
                }
            }
        };

        private static List<Statement> MaGiamSinh() => new List<Statement>
        {
            new Statement
            {
                Claim = Say("MaGiamSinh", "Ta là giám sinh, người đọc sách thánh hiền, có danh phận đàng hoàng."),
                Press = new[] { Say("MaGiamSinh", "Bằng giám sinh do triều đình cấp, há để một đứa đàn bà nghi ngờ?") },
                Proofs = new[]
                {
                    P("thu_lai", 1, Say("ThuLai", "Bẩm quan, sổ huyện ghi rõ: bằng giám sinh của hắn mua bằng ba trăm quan tiền năm ngoái. Hắn chưa đọc hết một bộ sách nào."),
                        Say("QuanHuyen", "Mua danh... Ghi vào biên bản."))
                }
            },
            new Statement
            {
                Claim = Say("MaGiamSinh", "Tháng Tư ta đến hỏi cưới Kiều, có mai mối, có sính lễ bốn trăm lạng, cả làng đều biết."),
                Press = new[] { Say("MaGiamSinh", "Ta đã đưa đủ bốn trăm lạng để chuộc cha nàng. Ta là ân nhân của nhà họ Vương!"), Say("NoiTam", "Ân nhân. Mua bốn trăm, bán bốn trăm năm mươi.") },
                Proofs = new[]
                {
                    P("so_bac", 1, Say("Kieu", "Mười ngày sau, chính ngươi nhận lại bốn trăm năm mươi lạng của Tú Bà, có điểm chỉ trong sổ hiệu bạc. \"Ân nhân\" lãi năm mươi lạng."),
                        Say("MaGiamSinh", "Đó... đó là tiền ta cho mụ vay!")),
                    P("van_tu", 1, Say("Kieu", "Mười ngày sau đám cưới, ngươi đứng tên người bán trên văn tự này. Bán chính người vợ ngươi vừa cưới."),
                        Say("QuanHuyen", "Cưới tháng Tư, bán cũng tháng Tư..."))
                }
            },
            new Statement
            {
                Key = true,
                Claim = Say("MaGiamSinh", "Trước ngày cưới, ta chưa từng gặp mặt mụ Tú Bà này."),
                Press = new[] { Say("MaGiamSinh", "Chỉ là lúc đưa Kiều qua Lâm Tri, tiện đường gửi nàng ở nhờ thôi.") },
                Proofs = new[]
                {
                    P("giao_keo", 1, Say("Kieu", "Giao kèo giữa ngươi và Tú Bà lập từ mùng chín tháng Giêng, ba tháng trước khi ngươi đến hỏi cưới ta. Chưa từng gặp mặt mà đã ký giao kèo với nhau?"),
                        Say("MaGiamSinh", "Ta... ta..."))
                }
            },
            new Statement
            {
                Claim = Say("MaGiamSinh", "Kiều là vợ ta. Chồng đưa vợ đi đâu, đó là quyền của chồng."),
                Press = new[] { Say("MaGiamSinh", "Ả bỏ nhà chồng chạy theo thằng họ Kim kia, thế là thất tiết!"), Say("KimTrong", "Học trò gặp nàng sau khi nàng đã tự thoát ra. Xin quan đừng nghe lời vu vạ.") },
                Proofs = new[]
                {
                    P("thu_lai", 1, Say("ThuLai", "Bẩm quan, sổ hôn thú ghi: năm nay hắn đã \"cưới vợ\" bốn lần, tháng Giêng, tháng Ba, tháng Tư... Bốn người vợ, không một người nào về đến nhà hắn."),
                        Say("QuanHuyen", "Bốn lần cưới vợ trong một năm. Mã Giám Sinh, vợ ngươi đâu cả rồi?")),
                    P("van_tu", 1, Say("Kieu", "Quyền của chồng ư? Văn tự này ghi ngươi là người bán, Tú Bà là người mua. Từ lúc điểm chỉ vào đây, ngươi đã tự xé bỏ chữ \"chồng\"."),
                        Say("MaGiamSinh", "Đó là... là giấy gửi người!"))
                }
            }
        };

        private static List<Statement> TuBaDebt() => new List<Statement>
        {
            new Statement
            {
                Key = true,
                Claim = Say("TuBa", "Nó nợ tôi bốn trăm năm mươi lạng tiền mua, chưa kể tiền ăn tiền mặc."),
                Press = new[] { Say("TuBa", "Áo lụa ba mươi lạng, son phấn mười lạng... sổ sách tôi ghi rành rành!") },
                Proofs = new[]
                {
                    P("so_no", 2, Say("Kieu", "Đây là trang sổ nợ của chính bà: \"nợ tiền mua bốn trăm năm mươi lạng\". Bà trả cho Mã Giám Sinh bao nhiêu thì bắt ta trả lại bấy nhiêu. Ta bị bán, rồi còn phải trả giá cho việc mình bị bán."),
                        Say("QuanHuyen", "Bắt người ta trả tiền mua chính mình... Thật chưa từng nghe.")),
                    P("giao_keo", 1, Say("Kieu", "Bốn trăm năm mươi lạng, đúng bằng giá bà trả cho Mã Giám Sinh trên giao kèo. Đó không phải nợ của ta. Đó là giá bà mua ta."),
                        Say("TuBa", "Thì... thì cũng là tiền tôi bỏ ra!")),
                    P("van_tu", 1, Say("Kieu", "Văn tự ghi rõ: bốn trăm năm mươi lạng là giá bà trả cho người bán. Người bị bán không ký vào món nợ nào cả."),
                        Say("QuanHuyen", "Giá mua người lại biến thành nợ của người bị mua. Hay cho cái sổ sách!"))
                }
            },
            new Statement
            {
                Claim = Say("TuBa", "Các cô ở nhà tôi đều tự nguyện, ăn sung mặc sướng."),
                Press = new[] { Say("TuBa", "Có đứa nào kêu ca đâu? Cứ hỏi chúng nó xem!") },
                Proofs = new[]
                {
                    P("co_lam", 1, Say("ChiLam", "Bẩm quan... ba năm. Ba năm em trả nợ mà nợ không bớt một dòng. Đứa nào trốn thì bị đánh. Ở đó, chưa ai hỏi tên em."),
                        Say("TuBa", "Con ranh này!"))
                }
            },
            new Statement
            {
                Claim = Say("TuBa", "Xưa nay chưa ai thấy tôi ép buộc ai bao giờ."),
                Press = new[] { Say("TuBa", "Lầu tôi mở cửa đón khách, ai muốn đi thì đi, có ai giữ đâu.") },
                Proofs = new[]
                {
                    P("thu_lai", 1, Say("ThuLai", "Bẩm quan, ba cái tên bị gạch trên giao kèo, Lý thị, Trần thị, Lưu thị, đều có trong sổ hôn thú của Mã Giám Sinh năm nay. Ba người \"vợ\", rồi không ai thấy về quê nữa."),
                        Say("QuanHuyen", "Ba người con gái biến mất... Ghi vào biên bản.")),
                    P("ba_nuoc", 1, Say("BaHangNuoc", "Già này thấy hai lần rồi, quan ạ: kiệu hoa đi ngang quán, cô dâu khóc không thành tiếng. Rồi kiệu rẽ về phía Lâm Tri."),
                        Say("TuBa", "Mụ già lắm chuyện!")),
                    P("co_lam", 1, Say("ChiLam", "Cổng trước khóa từ bên ngoài, ngõ sau có chó xích. Có người trèo tường trốn, bị bắt lại đánh gần chết."),
                        Say("QuanHuyen", "\"Ai muốn đi thì đi\" mà cổng khóa từ bên ngoài?"))
                }
            },
            new Statement
            {
                Claim = Say("TuBa", "Con Kiều trốn đi là bỏ nợ, là ăn cắp của tôi."),
                Press = new[] { Say("TuBa", "Tiền mua, tiền ăn, tiền mặc... không trả thì là ăn cắp chứ còn gì!"), Say("NoiTam", "Cái \"nợ\" ấy là sợi xích. Phải cho quan thấy nó từ đâu mà ra.") }
            }
        };
    }
}

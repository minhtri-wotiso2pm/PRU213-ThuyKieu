using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 2 deduction board: three questions Kiều settles from the clues she gathered. Every wrong option is one a
    /// careful person could reach from what she saw (his "Lâm Thanh", a debt, an official), and the board never says
    /// which is right. Writes "suy_luan" (how many match the truth) and which conclusions were wrong
    /// ("c2_sai_noi_den", "c2_sai_bac", "c2_sai_nguoi_nhan"), so a mistaken belief follows her to Lâm Tri.
    /// </summary>
    public class DeductionSequence : StorySequence
    {
        [SerializeField] private DeductionView _view;

        private static readonly string[] Mistakes = { "c2_sai_noi_den", "c2_sai_bac", "c2_sai_nguoi_nhan" };

        public override IEnumerator Run(StoryDirector director)
        {
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            if (director.Audio != null) director.Audio.PlayMusic("tension_low");
            bool readPaper = director.GetBool("mm_mau_giay");
            var questions = new List<DeductionView.Question>
            {
                new DeductionView.Question
                {
                    Prompt = "Mã Giám Sinh thật sự đưa Kiều đi đâu?",
                    Options = new[] { "Về quê hắn ở huyện Lâm Thanh, như lời hắn nói lúc dạm hỏi", "Đến Lâm Tri, nơi \"có người lo liệu\"", "Lên kinh, nơi hắn bảo có người quen làm quan" },
                    Correct = 1
                },
                new DeductionView.Question
                {
                    Prompt = readPaper
                        ? "Bốn trăm lạng trong hòm và giá bốn trăm năm mươi trên mẩu giấy nói lên điều gì?"
                        : "Tú Bà trả bốn trăm năm mươi, còn hắn chỉ đưa nhà Kiều bốn trăm. Khoản chênh lệch nói lên điều gì?",
                    Options = readPaper
                        ? new[] { "Hắn mắc nợ ở Lâm Tri, đưa Kiều đi gán nợ", "Hắn gom sính lễ, phần dư để lo cỗ cưới", "Hắn mua Kiều bốn trăm lạng để bán lại bốn trăm năm mươi" }
                        : new[] { "Hắn đang trả một món nợ cũ", "Hắn mua Kiều bốn trăm rồi bán lại bốn trăm năm mươi, ăn chênh lệch", "Hắn để dành tiền tổ chức lễ cưới" },
                    Correct = readPaper ? 2 : 1
                },
                new DeductionView.Question
                {
                    Prompt = "Ai đang chờ \"nhận người\" ở Lâm Tri?",
                    Options = new[] { "Tú Bà, chủ một lầu xanh", "Một viên quan muốn lấy Kiều làm thiếp", "Mẹ già của Mã Giám Sinh" },
                    Correct = 0
                }
            };
            int[] picks = null;
            _view.FastMode = director.Autopilot;
            yield return _view.Run(questions, Conclusion(questions), p => picks = p);
            int score = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                bool right = picks[i] == questions[i].Correct;
                if (right) score++;
                director.SetBool(Mistakes[i], !right);
            }
            director.SetInt("suy_luan", score);
            int suspicion = director.GetInt("nghi_ngo");
            int requestedChange = score == 3 ? -5 : (3 - score) * 10;
            int finalSuspicion = Mathf.Clamp(suspicion + requestedChange, 0, 100);
            int actualChange = finalSuspicion - suspicion;
            director.SetInt("nghi_ngo", finalSuspicion);
            director.Hud.ShowToast(
                score == 3 ? "SUY LUẬN CHÍNH XÁC" : "SUY LUẬN CÒN SƠ HỞ",
                score == 3
                    ? "Ba mảnh ghép khớp nhau · " + SuspicionChange(actualChange)
                    : score + "/3 kết luận đúng · Kiều để lộ dao động · " + SuspicionChange(actualChange));
            if (director.Audio != null) director.Audio.PlaySfx("clue");
        }

        private static string SuspicionChange(int change)
        {
            if (change == 0) return "Nghi ngờ không đổi";
            return "Nghi ngờ " + (change > 0 ? "+" : string.Empty) + change;
        }

        /// <summary>What she now believes, drawn from her answers about the money and the buyer.</summary>
        private static System.Func<int[], string> Conclusion(List<DeductionView.Question> questions) => picks =>
        {
            bool sold = picks[1] == questions[1].Correct || picks[2] == questions[2].Correct;
            return sold ? "MÃ GIÁM SINH KHÔNG CƯỚI KIỀU.\nHẮN ĐANG BÁN NÀNG." : "MÃ GIÁM SINH KHÔNG CƯỚI KIỀU.\nHẮN ĐANG GIẤU NÀNG ĐIỀU GÌ ĐÓ.";
        };
    }
}

using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core.Story;
using ThuyKieu.UI;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Chapter 2 deduction board: three questions answered from the gathered clues.
    /// Writes the number of correct answers to the Ink variable "suy_luan".
    /// </summary>
    public class DeductionSequence : StorySequence
    {
        [SerializeField] private DeductionView _view;

        public override IEnumerator Run(StoryDirector director)
        {
            yield return null; // after DialogueCursorController re-locks on dialogue end
            director.CameraController.SetCursorLocked(false);
            if (director.Audio != null) director.Audio.PlayMusic("tension_low");
            var questions = new List<DeductionView.Question>
            {
                new DeductionView.Question
                {
                    Prompt = "Mã Giám Sinh thật sự đưa Kiều đi đâu?",
                    Options = new[] { "Về quê nhà hắn để làm lễ cưới", "Đến Lâm Tri, nơi \"có người lo liệu\"", "Lên kinh thành ra mắt quan lớn" },
                    Correct = 1,
                    Hint = "Bắt chuyện khéo với hắn sẽ lộ ra điểm đến."
                },
                new DeductionView.Question
                {
                    Prompt = "Bạc trong hòm và mẩu giấy rơi trên đường cho thấy điều gì?",
                    Options = new[] { "Hắn nợ tiền một chủ nợ ở Lâm Tri", "Hắn tiết kiệm để lo cho gia đình", "Hắn mua Kiều bốn trăm lạng để bán lại bốn trăm năm mươi" },
                    Correct = 2,
                    Hint = "Mẩu giấy ghi rõ: \"nhận hàng — giá bốn trăm năm mươi lạng\"."
                },
                new DeductionView.Question
                {
                    Prompt = "Ai đang chờ \"nhận người\" ở Lâm Tri?",
                    Options = new[] { "Tú Bà, chủ một lầu xanh", "Mẹ chồng tương lai của Kiều", "Một vị quan muốn lấy Kiều làm thiếp" },
                    Correct = 0,
                    Hint = "Cái tên nghe được sau vách tre."
                }
            };
            int score = 0;
            _view.FastMode = director.Autopilot;
            yield return _view.Run(questions, "MÃ GIÁM SINH KHÔNG CƯỚI KIỀU.\nHẮN ĐANG BÁN NÀNG.", s => score = s);
            director.SetInt("suy_luan", score);
            if (director.Audio != null) director.Audio.PlaySfx(score >= 3 ? "clue" : "suspicion_up");
        }
    }
}

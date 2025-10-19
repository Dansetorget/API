using System.Collections.Generic;

namespace QuizApp.Models
{
    public class SubmitQuizViewModel
    {
        public int QuizId { get; set; }
        public List<QuestionResponseViewModel> Responses { get; set; } = new();
    }

    public class QuestionResponseViewModel
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new();
    }
}

using System.Collections.Generic;

namespace QuizApp.Models
{
    public class ResultViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int QuestionsCorrect { get; set; }
        public int TotalScore { get; set; }
        public int MaxScore { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; } = new();
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; }
        public bool IsCorrect { get; set; }
        public List<string> SelectedAnswers { get; set; } = new();
        public List<string> CorrectAnswers { get; set; } = new();
    }
}



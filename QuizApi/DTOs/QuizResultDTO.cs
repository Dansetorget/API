namespace QuizApi.DTOs;

public class QuizResultDTO
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int QuestionsCorrect { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public List<QuestionResultDTO> QuestionResults { get; set; } = new();
}

public class QuestionResultDTO
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsCorrect { get; set; }
    public List<string> SelectedAnswers { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
}
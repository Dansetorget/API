namespace QuizApi.Models;

public class Answer
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Foreign key
    public int QuestionId { get; set; }

    // Navigation property
    public virtual Question? Question { get; set; } = default;
}
namespace QuizApp.Models;

public class Answer
{
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation property
    public virtual Question? Question { get; set; } = default;
}
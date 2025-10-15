namespace QuizApp.Models;

public class Question
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;

    // Navigation property
    public virtual int QuizId { get; set; }
}
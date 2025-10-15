namespace QuizApp.Models;

public class Quiz
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    // Navigation property
    public virtual int QuestionId { get; set; }
}
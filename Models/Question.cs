namespace QuizApp.Models;

public class Question
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;

    // Foreign key
    public int QuizId { get; set; }
    // Navigation property
    public virtual List<Answer>? Answers { get; set; }
}
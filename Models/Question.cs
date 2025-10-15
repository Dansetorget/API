namespace QuizApp.Models;

public class Question
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }

    // Foreign key
    public int QuizId { get; set; }
    // Navigation property
    public virtual List<Answer>? Answers { get; set; }
}
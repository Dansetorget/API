namespace QuizApp.Models;

public class Attempt
{
    public int AttemptId { get; set; }
    public int CorrectCount { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    // Navigation property
    public virtual int QuizId { get; set; }

}
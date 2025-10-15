namespace QuizApp.Models;

public class Attempt
{
    public int AttemptId { get; set; }
    public int CorrectCount { get; set; }
    public string? DisplayName { get; set; }

    // Navigation property
    public virtual Quiz? Quiz { get; set; }

}
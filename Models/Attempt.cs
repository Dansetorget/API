namespace QuizApp.Models;

public class Attempt
{
    public int AttemptId { get; set; }
    public int CorrectCount { get; set; }
    public string? DisplayName { get; set; } // Hva er Display name, hvor legges det inn?

    // Navigation property
    public virtual Quiz? Quiz { get; set; }

}
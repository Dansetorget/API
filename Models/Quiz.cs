namespace QuizApi.Models;

public class Quiz
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = default!;
    public int TotalQuestions { get; set; }
    // Navigation property
    public virtual List<Question>? Questions { get; set; }
}
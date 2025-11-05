namespace QuizApp.DTOs;

public class QuizDetailDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionDto>? Questions { get; set; }
}


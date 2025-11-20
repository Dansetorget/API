namespace QuizApi.DTOs;

public class QuizAttemptDTO
{
    public int QuizId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeTaken { get; set; }
}
namespace QuizApi.DTOs;

public class QuizAttemptDTO
{
    public int QuizId { get; set; }
    public int Points { get; set; }
    public int TimeTaken { get; set; }
}
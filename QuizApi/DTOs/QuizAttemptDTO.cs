namespace QuizApi.DTOs;

public class CreateQuizAttemptDTO
{
    public int QuizId { get; set; }
    public int Points { get; set; }
    public int TimeTaken { get; set; }
}

public class QuizAttemptDTO
{
    public int QuizId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TimeTaken { get; set; }
}
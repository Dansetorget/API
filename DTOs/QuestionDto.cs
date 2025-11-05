namespace QuizApp.DTOs;

public class QuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<AnswerDto>? Answers { get; set; }
}


namespace QuizApp.DTOs;

/// <summary>
/// DTO for questions in quiz-taking scenarios (without correct answer indicators)
/// </summary>
public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<AnswerOptionDto>? Answers { get; set; }
}

public class AnswerOptionDto
{
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}


namespace QuizApi.DTO.Quiz;

public class QuizCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestionCreateDto> Questions { get; set; } = new();
}

public class QuestionCreateDto
{
    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<AnswerCreateDto> Answers { get; set; } = new();
}

public class AnswerCreateDto
{
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
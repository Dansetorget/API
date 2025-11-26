using System.ComponentModel.DataAnnotations;

namespace QuizApi.DTOs;

public class QuizDTO
{
    public int Id { get; set; } = 0;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }

    public List<QuestionDTO> Questions { get; set; } = new();
}

public class QuestionDTO
{
    public int Id { get; set; } = 0;

    public string QuestionText { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<AnswerDTO> Answers { get; set; } = new();
}

public class AnswerDTO
{
    public int Id { get; set; } = 0;
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateQuizDTO
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    // Description can be optional, but if provided, it should not be empty
    public string Description { get; set; } = string.Empty;

    public List<CreateQuestionDTO> Questions { get; set; } = new();
}

public class CreateQuestionDTO
{
    [Required(ErrorMessage = "Question text is required.")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Points must be in range 1 to 100")]
    public int Points { get; set; }

    public List<CreateAnswerDTO> Answers { get; set; } = new();
}

public class CreateAnswerDTO
{
    [Required(ErrorMessage = "Answer text is required.")]
    [StringLength(250, ErrorMessage = "Answer text cannot exceed 250 words.")]
    public string AnswerText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

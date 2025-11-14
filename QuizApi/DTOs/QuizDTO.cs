using System.ComponentModel.DataAnnotations;

namespace QuizApi.DTOs;

public class QuizDTO
{
    public int Id { get; set; } = 0;
    [Required(ErrorMessage = "Tittel er påkrevd")]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Beskrivelse er påkrevd")]
    public string Description { get; set; } = string.Empty;
    public List<QuestionDTO> Questions { get; set; } = new();
}

public class QuestionDTO
{
    public int Id { get; set; } = 0;
    [Required(ErrorMessage = "Spørsmålstekst er påkrevd.")]
    public string QuestionText { get; set; } = string.Empty;
    [Range(1, 100, ErrorMessage = "Poeng må være mellom 1 og 100.")] // Hvor mange poeng skal vi ha?
    public int Points { get; set; }
    public List<AnswerDTO> Answers { get; set; } = new();
}

public class AnswerDTO
{
    public int Id { get; set; } = 0;
    [Required(ErrorMessage = "Svar er påkrevd.")]
    [StringLength(250, ErrorMessage = "Svartekst kan ikke være lengre enn 250 tegn.")]
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateQuizDTO
{
    [Required(ErrorMessage = "Tittel er påkrevd")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivelse er påkrevd")]
    public string Description { get; set; } = string.Empty;

    public List<CreateQuestionDTO> Questions { get; set; } = new();
}

public class CreateQuestionDTO
{
    [Required(ErrorMessage = "Spørsmålstekst er påkrevd.")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Poeng må være mellom 1 og 100.")]
    public int Points { get; set; }

    public List<CreateAnswerDTO> Answers { get; set; } = new();
}

public class CreateAnswerDTO
{
    [Required(ErrorMessage = "Svar er påkrevd.")]
    [StringLength(250, ErrorMessage = "Svartekst kan ikke være lengre enn 250 tegn.")]
    public string AnswerText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

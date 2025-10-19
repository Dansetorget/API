using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class CreateQuizViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Tittel er påkrevd")]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Beskrivelse er påkrevd")]
    public string Description { get; set; } = string.Empty;
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Spørsmålstekst er påkrevd.")]
    public string QuestionText { get; set; } = string.Empty;
    [Range(1, 100, ErrorMessage = "Poeng må være mellom 1 og 100.")] // Hvor mange poeng skal vi ha?
    public int Points { get; set; }
    public List<AnswerViewModel> Answers { get; set; } = new();
}

public class AnswerViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Svar er påkrevd.")]
    [StringLength(250, ErrorMessage = "Svartekst kan ikke være lengre enn 250 tegn.")]
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}



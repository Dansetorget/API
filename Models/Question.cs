using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Question
{
    public int QuestionId { get; set; }
    [Required(ErrorMessage = "Spørsmålstekst er påkrevd.")]
    public string QuestionText { get; set; } = string.Empty;
    [Range(1, 100, ErrorMessage = "Poeng må være mellom 1 og 100.")] // Hvor mange poeng skal vi ha?
    public int Points { get; set; }

    // Foreign key
    public int QuizId { get; set; }
    // Navigation property
    public virtual List<Answer>? Answers { get; set; }
}
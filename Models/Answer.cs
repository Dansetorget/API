using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Answer
{
    public int AnswerId { get; set; }
    [Required(ErrorMessage = "Svar er påkrevd.")]
    [StringLength(250, ErrorMessage = "Svartekst kan ikke være lengre enn 250 tegn.")]
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation property
    public virtual Question? Question { get; set; } = default;
}
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Quiz
{
    public int QuizId { get; set; }
    [Required(ErrorMessage = "Tittel er påkrevd")]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Beskrivelse er påkrevd")]
    public string? Description { get; set; }
    [Range(1, 100, ErrorMessage = "Du må ha minst 1 og kan ha maks 100 spørsmål i en quiz")]
    public int TotalQuestions { get; set; }
    // Navigation property
    public virtual List<Question>? Questions { get; set; }
}
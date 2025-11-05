using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

public class CreateAnswerDto
{
    [Required]
    [StringLength(500)]
    public string AnswerText { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
}


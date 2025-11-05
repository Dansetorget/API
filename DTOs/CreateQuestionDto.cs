using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

public class CreateQuestionDto
{
    [Required]
    [StringLength(1000)]
    public string QuestionText { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int Points { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 answers are required")]
    public List<CreateAnswerDto> Answers { get; set; } = new();
}


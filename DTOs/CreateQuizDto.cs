using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

public class CreateQuizDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one question is required")]
    public List<CreateQuestionDto> Questions { get; set; } = new();
}


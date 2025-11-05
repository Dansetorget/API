using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

public class UpdateQuizDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
}


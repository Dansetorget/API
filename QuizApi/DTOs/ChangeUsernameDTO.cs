using System.ComponentModel.DataAnnotations;

namespace QuizApi.DTOs;

public class ChangeUsernameDTO
{
    [Required]
    [StringLength(20, MinimumLength = 3)] // Username must be between 3 and 20 characters
    public string NewUsername { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace QuizApi.DTOs;

public class ChangeEmailDTO
{
    [Required]
    [EmailAddress] // Ensures the new email is in a valid email format
    public string NewEmail { get; set; } = string.Empty;
}
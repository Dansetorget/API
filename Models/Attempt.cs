using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Attempt
{
    public int AttemptId { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Antall riktige svar må være 0 eller mer.")]
    public int CorrectCount { get; set; }
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [StringLength(100, ErrorMessage = "Navn kan ikke være lengre enn 100 tegn.")]
    public string? DisplayName { get; set; } // Hva er Display name, hvor legges det inn?

    // Navigation property
    public virtual Quiz? Quiz { get; set; }

}
using Microsoft.AspNetCore.Identity;

namespace QuizApi.Models
{
    public class AuthUser : IdentityUser
    {
        // Empty because it inherits the necessary properties for a user from the base IdentityUser class.
        // Properties like Id, UserName, Email, PhoneNumber, PasswordHash, etc. are already defined in IdentityUser.
        // Can add more custom properties, such as FirstName, LastName, etc.
        public int Streak { get; set; } = 0;
        public DateTime? LastQuizDate { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
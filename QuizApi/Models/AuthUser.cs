using Microsoft.AspNetCore.Identity;

namespace QuizApi.Models
{
    public class AuthUser : IdentityUser
    {
        // Includes properties like Id, UserName, Email, PhoneNumber, PasswordHash, etc. from IdentityUser.
        // Can add more custom properties
        public int? AvatarId { get; set; }
        public int Streak { get; set; } = 0;
        public DateTime? LastQuizDate { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
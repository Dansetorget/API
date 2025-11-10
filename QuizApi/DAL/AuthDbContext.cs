using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApi.Models;

namespace QuizApi.DAL
{
    public class AuthDbContext : IdentityDbContext<AuthUser>
    {
        // Constructor 
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
            // Empty because it inherits all the necessary database management logic for the Identity system from the IdentityDbContext base class.
        }
    }
}
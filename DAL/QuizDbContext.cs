using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApi.Models;

namespace QuizApi.DAL;

public class QuizDbContext : IdentityDbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}
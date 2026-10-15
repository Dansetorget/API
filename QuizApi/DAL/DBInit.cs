using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using QuizApi.Models;

namespace QuizApi.DAL;

public static class DBInit
{
    public static async Task Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        QuizDbContext context = serviceScope.ServiceProvider.GetRequiredService<QuizDbContext>();
        context.Database.EnsureCreated();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();
        var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new AuthUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                AvatarId = 6
            };

            var createResult = await userManager.CreateAsync(admin, "Password123!");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create admin user for seeding quizzes.");
            }
        }
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin")); // Create Admin role if it doesn't exist
        }
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin"); // Assign admin user to Admin role if not already assigned
        }

        // Seed initial quizzes if none exist
        if (!context.Quizzes.Any())
        {
            var quizzes = new List<Quiz>
        {
            new Quiz
            {
                Title = "C# Fundamentals",
                Description = "Test your knowledge of C# basics",
                TotalQuestions = 2,
                OwnerId = admin.Id,
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Which of the following is a value type in C#?",
                        Points = 10,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "int", IsCorrect = true},
                            new Answer { AnswerText = "string", IsCorrect = false},
                            new Answer { AnswerText = "object", IsCorrect = false},
                            new Answer { AnswerText = "dynamic", IsCorrect = false}
                        }
                    },
                    new Question
                    {
                        QuestionText = "What is the default access modifier for class members in C#?",
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "private", IsCorrect = true},
                            new Answer { AnswerText = "public", IsCorrect = false},
                            new Answer { AnswerText = "internal", IsCorrect = false},
                            new Answer { AnswerText = "protected", IsCorrect = false}
                        }
                    }
                }
            },
            new Quiz
            {
                Title = "ASP.NET MVC Basics",
                Description = "Questions about ASP.NET MVC framework",
                TotalQuestions = 1,
                OwnerId = admin.Id,
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Which method is used to return a View in a controller?",
                        Points = 15,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "View()", IsCorrect = true},
                            new Answer { AnswerText = "ReturnView()", IsCorrect = false},
                            new Answer { AnswerText = "GetView()", IsCorrect = false},
                            new Answer { AnswerText = "RenderView()", IsCorrect = false}
                        }
                    }
                }
            }
        };

            foreach (var quiz in quizzes)
            {
                quiz.TotalQuestions = quiz.Questions!.Count;
            }

            context.AddRange(quizzes);
            context.SaveChanges();
        }
    }

}

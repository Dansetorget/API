using Microsoft.AspNetCore.Identity;
using QuizApi.Models;

namespace QuizApi.DAL;

public static class DBInit
{
    public static async Task Seed(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        QuizDbContext context = serviceScope.ServiceProvider.GetRequiredService<QuizDbContext>();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var admin = await userManager.FindByNameAsync("admin");

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

            context.AddRange(quizzes);
            context.SaveChanges();
        }
    }

}

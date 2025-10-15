// using QuizApp.Models;

// namespace QuizApp.DAL;

// public static class DBInit
// {
//     public static async Task InitializeAsync(QuizDbContext context)
//     {
//         // Apply pending migrations
//         await context.Database.MigrateAsync();

//         // Return if data already exists
//         if (await context.Quizzes.AnyAsync()) return;

//         await SeedSampleDataAsync(context);
//     }

//     private static async Task SeedSampleDataAsync(QuizDbContext context)
//     {
//         var quizzes = new List<Quiz>
//         {
//             new Quiz
//             {
//                 Title = "C# Fundamentals",
//                 Description = "Test your knowledge of C# basics",
//                 Questions = new List<Question>
//                 {
//                     new Question
//                     {
//                         QuestionText = "Which of the following is a value type in C#?",
//                         Points = 10,
//                         Order = 1,
//                         Answers = new List<Answer>
//                         {
//                             new Answer { AnswerText = "int", IsCorrect = true, Order = 1 },
//                             new Answer { AnswerText = "string", IsCorrect = false, Order = 2 },
//                             new Answer { AnswerText = "object", IsCorrect = false, Order = 3 },
//                             new Answer { AnswerText = "dynamic", IsCorrect = false, Order = 4 }
//                         }
//                     },
//                     new Question
//                     {
//                         QuestionText = "What is the default access modifier for class members in C#?",
//                         Points = 10,
//                         Order = 2,
//                         Answers = new List<Answer>
//                         {
//                             new Answer { AnswerText = "private", IsCorrect = true, Order = 1 },
//                             new Answer { AnswerText = "public", IsCorrect = false, Order = 2 },
//                             new Answer { AnswerText = "internal", IsCorrect = false, Order = 3 },
//                             new Answer { AnswerText = "protected", IsCorrect = false, Order = 4 }
//                         }
//                     }
//                 }
//             },
//             new Quiz
//             {
//                 Title = "ASP.NET MVC Basics",
//                 Description = "Questions about ASP.NET MVC framework",
//                 CreatedDate = DateTime.Now,
//                 CreatedBy = "system",
//                 TimeLimit = 45,
//                 Questions = new List<Question>
//                 {
//                     new Question
//                     {
//                         QuestionText = "Which method is used to return a View in a controller?",
//                         Points = 15,
//                         Order = 1,
//                         Answers = new List<Answer>
//                         {
//                             new Answer { AnswerText = "View()", IsCorrect = true, Order = 1 },
//                             new Answer { AnswerText = "ReturnView()", IsCorrect = false, Order = 2 },
//                             new Answer { AnswerText = "GetView()", IsCorrect = false, Order = 3 },
//                             new Answer { AnswerText = "RenderView()", IsCorrect = false, Order = 4 }
//                         }
//                     }
//                 }
//             }
//         };

//         await context.Quizzes.AddRangeAsync(quizzes);
//         await context.SaveChangesAsync();
//     }
// }
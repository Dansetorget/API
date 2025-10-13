using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        var model = new QuizCreateViewModel
        {
            Title = "Min første quiz",
            Description = "Dette er en beskrivelse av quizen.",
            Questions = new()
            {
                new QuestionItemViewModel { Id = 1, Text = "Hvor mange tangenter er det på et keyboard?", Answers = new() {
                    new QuestionAnswerViewModel { Id = 1, Answer = "10", IsCorrect = true }, 
                    new QuestionAnswerViewModel { Id = 2, Answer = "20", IsCorrect = false } } 
                },
                new QuestionItemViewModel { Id = 2, Text = "Hva er hovedstaden i Norge?", Answers = new() {
                    new QuestionAnswerViewModel { Id = 3, Answer = "Oslo", IsCorrect = true }, 
                    new QuestionAnswerViewModel { Id = 4, Answer = "Bergen", IsCorrect = false },
                    new QuestionAnswerViewModel { Id = 5, Answer = "Trondheim", IsCorrect = false },
                    new QuestionAnswerViewModel { Id = 6, Answer = "Stavanger", IsCorrect = false }
                } },
                new QuestionItemViewModel { Id = 3, Text = "Hvilket år startet .NET Core?", Answers = new() {
                    new QuestionAnswerViewModel { Id = 7, Answer = "2016", IsCorrect = true }, 
                    new QuestionAnswerViewModel { Id = 8, Answer = "2014", IsCorrect = false },
                    new QuestionAnswerViewModel { Id = 9, Answer = "2012", IsCorrect = false },
                    new QuestionAnswerViewModel { Id = 10, Answer = "2010", IsCorrect = false }
                } }
            }
        };

        return View(model);
    }
}



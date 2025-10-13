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
                new QuestionItemViewModel { Id = 1, Text = "Hvor mange tangenter er det på et keyboard?" },
                new QuestionItemViewModel { Id = 2, Text = "Hva er hovedstaden i Norge?" },
                new QuestionItemViewModel { Id = 3, Text = "Hvilket år startet .NET Core?" }
            }
        };

        return View(model);
    }
}



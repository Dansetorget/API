using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private static readonly List<QuestionItemViewModel> Questions = new()
    {
        new QuestionItemViewModel { Id = 1, Text = "Hvor mange tangenter er det på et keyboard?" }
    };

    [HttpGet]
    public IActionResult Create()
    {
        return View(Questions);
    }

    [HttpPost]
    public IActionResult CreateQuestion(QuestionItemViewModel question)
    {
        Questions.Add(question);
        return RedirectToAction(nameof(Create));
    }

    [HttpPost]
    public IActionResult DeleteQuestion(QuestionItemViewModel question)
    {
        Questions.Remove(question);
        return RedirectToAction(nameof(Create));
    }
}



using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private static readonly List<QuestionItemViewModel> Questions = new()
    {
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
        return RedirectToAction("Create");
    }

}



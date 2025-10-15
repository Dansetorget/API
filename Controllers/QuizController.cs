using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        List<QuestionItemViewModel> questions = new List<QuestionItemViewModel>();
        questions.Add(new QuestionItemViewModel { Id = 1, Text = "Hvor mange tangenter er det på et keyboard?"});
        
        [HttpPost]
        IActionResult CreateQuestion(QuestionItemViewModel question)
        {
            questions.Add(question);
            return View(questions);
        }

        return View(questions);
    }
}



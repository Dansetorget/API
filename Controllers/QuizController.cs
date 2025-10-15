using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        

        return View();
    }
}



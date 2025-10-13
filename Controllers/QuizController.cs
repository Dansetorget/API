using Microsoft.AspNetCore.Mvc;

namespace QuizApp.Controllers;

public class QuizController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
}



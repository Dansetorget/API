using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.DAL;
using QuizApp.Models;


namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private readonly QuizDbContext _quizDbContext;

    public QuizController(QuizDbContext quizDbContext)
    {
        _quizDbContext = quizDbContext;
    }

    public async Task<IActionResult> Table()
    {
        List<Quiz> quizzes = await _quizDbContext.Quizzes.ToListAsync();
        ViewBag.CurrentViewName = "Table";
        return View(quizzes);
    }

    public async Task<IActionResult> Grid()
    {
        List<Quiz> quizzes = await _quizDbContext.Quizzes.ToListAsync();
        ViewBag.CurrentViewName = "Grid";
        return View(quizzes);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Quiz quiz)
    {
        List<QuestionViewModel> questions = [new QuestionViewModel { Id = 1, Text = "Hvor mange tangenter er det på et keyboard?" }];

        [HttpPost]
        IActionResult CreateQuestion(QuestionViewModel question)
        {
            questions.Add(question);
            return View(questions);
        }

        return View(questions);
    }
}



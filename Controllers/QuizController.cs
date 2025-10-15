using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.DAL;
using QuizApp.Models;


namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private readonly QuizDbContext _quizDbContext;

    private static List<QuestionViewModel> Questions = new List<QuestionViewModel>();

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

    public IActionResult AddQuestionToList(QuestionViewModel question){
        Questions.Add(question);
        return RedirectToAction("Create");
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View(Questions);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuizViewModel model)
    {
        if (ModelState.IsValid)
        {
            var quiz = new Quiz
            {
                Title = model.Title,
                Description = model.Description,
                Questions = new List<Question>()
            };

            // Add questions to the quiz
            foreach (var questionModel in model.Questions)
            {
                var question = new Question
                {
                    QuestionText = questionModel.QuestionText,
                    Points = questionModel.Points,
                    Answers = new List<Answer>()
                };
                // Add answers to the question
                foreach (var answerModel in questionModel.Answers)
                {
                    question.Answers.Add(new Answer
                    {
                        AnswerText = answerModel.AnswerText,
                        IsCorrect = answerModel.IsCorrect
                    });
                }
                quiz.Questions.Add(question);
            }

            await _quizDbContext.AddAsync(quiz);
            return RedirectToAction(nameof(Index));

        }
        return View(model);
    }

}



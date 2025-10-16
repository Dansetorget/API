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

    public async Task<IActionResult> Index()
    {
        var quizzes = _quizDbContext.Quizzes.ToListAsync();
        return View(quizzes);
    }


    public IActionResult AddQuestionToList(QuestionViewModel question)
    {
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

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var quiz = await _quizDbContext.Quizzes.FindAsync(id);

        if (quiz == null)
        {
            return BadRequest("Quiz not found for the QuizId");
        }

        // var model = new CreateQuizViewModel
        // {
        //     Id = quiz.QuizId,
        //     Title = quiz.Title,
        //     Description = quiz.Description!,
        //     Questions = quiz.Questions!.Select(q => new QuestionViewModel
        //     {
        //         Id = q.QuestionId,
        //         QuestionText = q.QuestionText,
        //         Points = q.Points,
        //         Answers = q.Answers!.Select(a => new AnswerViewModel
        //         {
        //             Id = a.AnswerId,
        //             AnswerText = a.AnswerText,
        //             IsCorrect = a.IsCorrect
        //         }).ToList()
        //     }).ToList()
        // };

        return View(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> Update(Quiz quiz)
    {
        if (ModelState.IsValid)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            _quizDbContext.Quizzes.Update(quiz);
            await _quizDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            // var quiz = await _quizDbContext.Quizzes
            //     .Include(q => q.Questions!)
            //     .ThenInclude(q => q.Answers!)
            //     .FirstOrDefaultAsync(q => q.QuizId == id);

            // if (quiz == null)
            // {
            //     return NotFound();
            // }

            // quiz.Title = model.Title;
            // quiz.Description = model.Description;

            // // Update questions
            // foreach (var questionModel in model.Questions)
            // {
            //     var question = quiz.Questions!.FirstOrDefault(q => q.QuestionId == questionModel.Id);
            //     if (question != null)
            //     {
            //         question.QuestionText = questionModel.QuestionText;
            //         question.Points = questionModel.Points;

            //         // Update answers
            //         foreach (var answerModel in questionModel.Answers)
            //         {
            //             var answer = question.Answers!.FirstOrDefault(a => a.AnswerId == answerModel.Id);
            //             if (answer != null)
            //             {
            //                 answer.AnswerText = answerModel.AnswerText;
            //                 answer.IsCorrect = answerModel.IsCorrect;
            //             }
            //         }
            //     }
            // }

            // await _quizDbContext.SaveChangesAsync();
            // return RedirectToAction(nameof(Index));
        }

        return View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizDbContext.Quizzes.FindAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var quiz = await _quizDbContext.Quizzes.FindAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        _quizDbContext.Quizzes.Remove(quiz);
        await _quizDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}



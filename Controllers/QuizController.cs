using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.DAL;
using QuizApp.Models;


namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private readonly IQuizRepository _quizRepository;

    private static List<QuestionViewModel> Questions = new List<QuestionViewModel>();

    public QuizController(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }
public async Task<IActionResult> Index()
{
    var allQuizzes = await _quizRepository.GetAll();
    var quizzes = allQuizzes?.ToList() ?? new List<Quiz>();

    // In-memory fallback so the view has something to render if DB is empty
    if (quizzes.Count == 0)
    {
        quizzes.Add(new Quiz
        {
            QuizId = 1,
            Title = "Testquiz",
            Description = "En liten test for å sjekke at visningen fungerer.",
            TotalQuestions = 5
        });
        quizzes.Add(new Quiz
        {
            QuizId = 2,
            Title = "Testquiz 2",
            Description = "En annen test for å sjekke at visningen fungerer.",
            TotalQuestions = 5
        });
    }
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
    public async Task<IActionResult> Create(Quiz quiz)
    {
        if (ModelState.IsValid)
        {
            // Add questions to the quiz
            foreach (var questionModel in quiz.Questions!)
            {
                var question = new Question
                {
                    QuestionText = questionModel.QuestionText,
                    Points = questionModel.Points,
                    Answers = new List<Answer>()
                };
                // Add answers to the question
                foreach (var answerModel in questionModel.Answers!)
                {
                    question.Answers.Add(new Answer
                    {
                        AnswerText = answerModel.AnswerText,
                        IsCorrect = answerModel.IsCorrect
                    });
                }
                quiz.Questions.Add(question);
            }

            await _quizRepository.Create(quiz);
            return RedirectToAction(nameof(Index));

        }
        return View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var quiz = await _quizRepository.GetQuizById(id);

        if (quiz == null)
        {
            return BadRequest("Quiz not found for the QuizId");
        }

        var model = new CreateQuizViewModel
        {
            Id = quiz.QuizId,
            Title = quiz.Title,
            Description = quiz.Description!,
            Questions = quiz.Questions!.Select(q => new QuestionViewModel
            {
                Id = q.QuestionId,
                QuestionText = q.QuestionText,
                Points = q.Points,
                Answers = q.Answers!.Select(a => new AnswerViewModel
                {
                    Id = a.AnswerId,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList()
        };

        return View(model);
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
            // Update questions
            foreach (var questionModel in quiz.Questions!)
            {
                var question = quiz.Questions!.FirstOrDefault(q => q.QuestionId == questionModel.QuestionId);
                if (question != null)
                {
                    question.QuestionText = questionModel.QuestionText;
                    question.Points = questionModel.Points;

                    // Update answers
                    foreach (var answerModel in questionModel.Answers!)
                    {
                        var answer = question.Answers!.FirstOrDefault(a => a.AnswerId == answerModel.AnswerId);
                        if (answer != null)
                        {
                            answer.AnswerText = answerModel.AnswerText;
                            answer.IsCorrect = answerModel.IsCorrect;
                        }
                    }
                }
            }

            await _quizRepository.Update(quiz);
            return RedirectToAction(nameof(Index));

        }

        return View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizRepository.GetQuizById(id);
        if (quiz == null)
        {
            // Log the error (not implemented here)
            return BadRequest("Quiz not found for the QuizId");
        }

        return View(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var returnOK = await _quizRepository.Delete(id);
        if (!returnOK)
        {
            // log the error (not implemented here)
            return BadRequest("Quiz deletion failed");
        }
        return RedirectToAction(nameof(Index));
    }

}



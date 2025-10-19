using Microsoft.AspNetCore.Mvc;
using QuizApp.DAL;
using QuizApp.Models;


namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;

    private readonly ILogger<QuizController> _logger;


    private static List<QuestionViewModel> Questions = new List<QuestionViewModel>();

    public QuizController(IQuizRepository quizRepository, IQuestionRepository questionRepository, IAnswerRepository answerRepository, ILogger<QuizController> logger)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _logger = logger;
    }
    public async Task<IActionResult> Index()
    {
        var quizzes = await _quizRepository.GetAll();

        // if no quizzes - show a message
        if (quizzes == null)
        {
            _logger.LogError("[QuizController] Index: No quizzes found in the database.");
            return View("NoQuizzes");
        }
        return View(quizzes);
    }

    [HttpGet]
    public async Task<IActionResult> Take(int id) // takes the quiz id and gets the quiz from the database
    {
        var quiz = await _quizRepository.GetQuizById(id);
        if (quiz == null)
        {
            _logger.LogError("[QuizController] Take: Quiz not found for the QuizId {QuizId}", id);
            return NotFound();
        }
        return View(quiz);
    }

    public IActionResult AddQuestionToList(QuestionViewModel question)
    {
        Questions.Add(question);
        return RedirectToAction("Create");
    }


    [HttpGet]
    public IActionResult Create()
    {
        // Changed this to match the POST, so they use the same model type
        return View(new CreateQuizViewModel { Questions = Questions });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuizViewModel quizModel)
    {
        if (!ModelState.IsValid)
        {
            return View(quizModel);
        }
        else
        {
            var quiz = new Quiz
            {
                Title = quizModel.Title,
                Description = quizModel.Description,
                TotalQuestions = quizModel.Questions.Count,
                Questions = new List<Question>()
            };
            // Add questions to the quiz
            // loop through the static Questions list
            foreach (var questionModel in quizModel.Questions)
            {
                var question = new Question
                {
                    QuestionText = questionModel.QuestionText,
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
                //await _quizRepository.AddQuestionToQuiz(quiz, question);
            }

            bool returnOK = await _quizRepository.Create(quiz);
            if (returnOK)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        _logger.LogError("[QuizController] Create: Quiz creation failed {@quiz}", quizModel);
        return View(quizModel);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var quiz = await _quizRepository.GetQuizById(id);

        if (quiz == null)
        {
            _logger.LogError("[QuizController] Update: Quiz not found when updating the QuizId {QuizId:0000}", id);
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
    public async Task<IActionResult> Update(CreateQuizViewModel quizModel)
    {
        // Input validation
        if (!ModelState.IsValid)
        {
            // Return view with validation errors
            return View(quizModel);
        }
        
        // Fetch quiz from database, and check if quiz exists
        var quiz = await _quizRepository.GetQuizById(quizModel.Id);
        if (quiz == null)
        {
            return BadRequest("Quiz not found for the QuizId");
        }

        // Update title and description
        quiz.Title = quizModel.Title;
        quiz.Description = quizModel.Description;

        // Update questions
        foreach (var questionModel in quizModel.Questions)
        {
            var question = quiz.Questions!.FirstOrDefault(q => q.QuestionId == questionModel.Id);
            if (question != null)
                {
                    question.QuestionText = questionModel.QuestionText;
                    question.Points = questionModel.Points;

                    // Update answers
                    foreach (var answerModel in questionModel.Answers!)
                    {
                        var answer = question.Answers!.FirstOrDefault(a => a.AnswerId == answerModel.Id);
                        if (answer != null)
                        {
                            answer.AnswerText = answerModel.AnswerText;
                            answer.IsCorrect = answerModel.IsCorrect;
                        }
                    }
                }
            }

            bool returnOK = await _quizRepository.Update(quiz);
            if (returnOK)
                return RedirectToAction(nameof(Index));


        }
        _logger.LogWarning("[QuizController] Update: Quiz update failed {@quiz}", quiz);
        return View(quiz);

    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _quizRepository.GetQuizById(id);
        if (quiz == null)
        {
            _logger.LogError("[QuizController] Delete: Quiz not found when deleting the QuizId {QuizId:0000}", id);
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
            _logger.LogError("[QuizController] DeleteConfirmed: Quiz deletion failed for the QuizId {QuizId:0000}", id);
            return BadRequest("Quiz deletion failed");
        }
        return RedirectToAction(nameof(Index));
    }

}



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

    public IActionResult DeleteQuestionFromList(int id)
    {
        var question = Questions.FirstOrDefault(q => q.Id == id);
        if (question != null)
        {
            Questions.Remove(question);
        }
        return RedirectToAction("Create");
    }

    public IActionResult UpdateQuestionInList(QuestionViewModel question)
    {
        var questionToUpdate = Questions[question.Id];
        if (questionToUpdate != null)
        {
            questionToUpdate.QuestionText = question.QuestionText;
            Console.WriteLine(question.Answers.Count+"-------");
            for (var i = 0; i < question.Answers.Count; i++)
            {
                questionToUpdate.Answers[i].AnswerText = question.Answers[i].AnswerText;
                questionToUpdate.Answers[i].IsCorrect = question.Answers[i].IsCorrect;
            }
        }
        return RedirectToAction("Create");
    }


    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateQuizViewModel
        {
            Title = string.Empty,
            Description = string.Empty,
            Questions = Questions // behold eksisterende midlertidige spørsmål om de finnes
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitQuizViewModel model)
    {
        if (model == null || model.QuizId <= 0)
        {
            return BadRequest("Invalid submission");
        }

        var quiz = await _quizRepository.GetQuizById(model.QuizId);
        if (quiz == null || quiz.Questions == null)
        {
            return NotFound();
        }

        var result = new ResultViewModel
        {
            QuizId = quiz.QuizId,
            QuizTitle = quiz.Title,
            TotalQuestions = quiz.Questions.Count,
            TotalScore = 0,
            MaxScore = quiz.Questions.Sum(q => q.Points)
        };

        foreach (var q in quiz.Questions)
        {
            var response = model.Responses?.FirstOrDefault(r => r.QuestionId == q.QuestionId);
            var selectedIds = response?.SelectedAnswerIds ?? new List<int>();
            var correctIds = q.Answers?.Where(a => a.IsCorrect).Select(a => a.AnswerId).ToHashSet() ?? new HashSet<int>();

            var isCorrect = selectedIds.Count > 0 && selectedIds.All(id => correctIds.Contains(id)) && correctIds.SetEquals(selectedIds);

            if (isCorrect)
            {
                result.QuestionsCorrect += 1;
                result.TotalScore += q.Points;
            }

            result.QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                Points = q.Points,
                IsCorrect = isCorrect,
                SelectedAnswers = q.Answers?.Where(a => selectedIds.Contains(a.AnswerId)).Select(a => a.AnswerText).ToList() ?? new List<string>(),
                CorrectAnswers = q.Answers?.Where(a => a.IsCorrect).Select(a => a.AnswerText).ToList() ?? new List<string>()
            });
        }

        return View("Result", result);
    }
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuizViewModel quizModel)
    {
        if (!ModelState.IsValid)
        {
            return View(quizModel);
        }

        var questionsVm = quizModel.Questions ?? new List<QuestionViewModel>();
        var quiz = new Quiz
        {
            Title = quizModel.Title,
            Description = quizModel.Description,
            TotalQuestions = questionsVm.Count,
            Questions = new List<Question>()
        };

        foreach (var questionModel in questionsVm)
        {
            var question = new Question
            {
                QuestionText = questionModel.QuestionText,
                Points = questionModel.Points,
                Answers = new List<Answer>()
            };

            foreach (var answerModel in (questionModel.Answers ?? new List<AnswerViewModel>()))
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
        // Tøm den midlertidige listen siden vi nå har lagret
        Questions.Clear();
        return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Update(CreateQuizViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Load existing quiz with graph
        var existingQuiz = await _quizRepository.GetQuizById(model.Id);
        if (existingQuiz == null)
        {
            return BadRequest("Quiz not found for the QuizId");
        }

        // Update scalar fields
        existingQuiz.Title = model.Title;
        existingQuiz.Description = model.Description;

        // Ensure collections
        existingQuiz.Questions ??= new List<Question>();

        var incomingQuestions = model.Questions ?? new List<QuestionViewModel>();

        // Remove deleted questions
        var incomingQuestionIds = incomingQuestions.Where(q => q.Id > 0).Select(q => q.Id).ToHashSet();
        var questionsToRemove = existingQuiz.Questions.Where(q => !incomingQuestionIds.Contains(q.QuestionId)).ToList();
        foreach (var qRemove in questionsToRemove)
        {
            existingQuiz.Questions.Remove(qRemove);
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

        // Add or update questions
        foreach (var qVm in incomingQuestions)
        {
            Question targetQuestion;
            if (qVm.Id > 0)
            {
                targetQuestion = existingQuiz.Questions.FirstOrDefault(q => q.QuestionId == qVm.Id) ?? new Question();
                if (targetQuestion.QuestionId == 0)
                {
                    existingQuiz.Questions.Add(targetQuestion);
                }
            }
            else
            {
                targetQuestion = new Question();
                existingQuiz.Questions.Add(targetQuestion);
            }

            targetQuestion.QuestionText = qVm.QuestionText;
            targetQuestion.Points = qVm.Points;

            // Sync answers
            targetQuestion.Answers ??= new List<Answer>();
            var incomingAnswers = qVm.Answers ?? new List<AnswerViewModel>();
            var incomingAnswerIds = incomingAnswers.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();
            var answersToRemove = targetQuestion.Answers.Where(a => !incomingAnswerIds.Contains(a.AnswerId)).ToList();
            foreach (var aRemove in answersToRemove)
            {
                targetQuestion.Answers.Remove(aRemove);
            }

            foreach (var aVm in incomingAnswers)
            {
                Answer targetAnswer;
                if (aVm.Id > 0)
                {
                    targetAnswer = targetQuestion.Answers.FirstOrDefault(a => a.AnswerId == aVm.Id) ?? new Answer();
                    if (targetAnswer.AnswerId == 0)
                    {
                        targetQuestion.Answers.Add(targetAnswer);
                    }
                }
                else
                {
                    targetAnswer = new Answer();
                    targetQuestion.Answers.Add(targetAnswer);
                }

                targetAnswer.AnswerText = aVm.AnswerText;
                targetAnswer.IsCorrect = aVm.IsCorrect;
            }
        }

        // Persist changes
        await _quizRepository.Update(existingQuiz);
        return RedirectToAction(nameof(Index));
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



using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.DAL;
using QuizApp.Models;


namespace QuizApp.Controllers;

public class QuizController : Controller
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;


    private static List<QuestionViewModel> Questions = new List<QuestionViewModel>();

    public QuizController(IQuizRepository quizRepository, IQuestionRepository questionRepository, IAnswerRepository answerRepository)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
    }
    public async Task<IActionResult> Index()
    {
        var allQuizzes = await _quizRepository.GetAll();
        var quizzes = allQuizzes?.ToList() ?? new List<Quiz>();

        // if no quizzes - show a message
        if (quizzes.Count == 0)
        {
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
        return View(Questions);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuizViewModel quizModel)
    {
        if (ModelState.IsValid)
        {
            var quiz = new Quiz
            {
                Title = quizModel.Title,
                Description = quizModel.Description,
                TotalQuestions = Questions.Count,
                Questions = new List<Question>()
            };
            // Add questions to the quiz
            // loop through the static Questions list
            foreach (var questionModel in Questions)
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

            await _quizRepository.Create(quiz);

        }
        return RedirectToAction(nameof(Index));
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



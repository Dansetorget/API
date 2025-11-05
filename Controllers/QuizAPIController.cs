using Microsoft.AspNetCore.Mvc;
using QuizApp.DAL;
using QuizApp.DTOs;
using QuizApp.Models;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizAPIController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;
    private readonly ILogger<QuizAPIController> _logger;

    public QuizAPIController(IQuizRepository quizRepository, ILogger<QuizAPIController> logger)
    {
        _quizRepository = quizRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all quizzes
    /// </summary>
    [HttpGet("quizlist")]
    [ProducesResponseType(typeof(List<QuizDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var quizzes = await _quizRepository.GetAll();

            if (quizzes == null || !quizzes.Any())
            {
                return Ok(new List<QuizDto>());
            }

            var quizDtos = quizzes.Select(q => new QuizDto
            {
                QuizId = q.QuizId,
                Title = q.Title,
                Description = q.Description,
                TotalQuestions = q.TotalQuestions
            }).ToList();

            return Ok(quizDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] GetAll: Error occurred while retrieving quizzes");
            return StatusCode(500, "An error occurred while retrieving quizzes");
        }
    }

    /// <summary>
    /// Get quiz by ID with all questions and answers
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuizDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var quiz = await _quizRepository.GetQuizById(id);

            if (quiz == null)
            {
                _logger.LogWarning("[QuizAPIController] GetById: Quiz not found for QuizId {QuizId}", id);
                return NotFound($"Quiz with ID {id} not found");
            }

            var quizDetailDto = new QuizDetailDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                TotalQuestions = quiz.TotalQuestions,
                Questions = quiz.Questions?.Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Points = q.Points,
                    Answers = q.Answers?.Select(a => new AnswerDto
                    {
                        AnswerId = a.AnswerId,
                        AnswerText = a.AnswerText
                        // Note: IsCorrect is intentionally excluded to prevent cheating
                    }).ToList()
                }).ToList()
            };

            return Ok(quizDetailDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] GetById: Error occurred while retrieving quiz {QuizId}", id);
            return StatusCode(500, "An error occurred while retrieving the quiz");
        }
    }

    /// <summary>
    /// Create a new quiz
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(QuizDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateQuizDto createQuizDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map DTO to Entity
            var quiz = new Quiz
            {
                Title = createQuizDto.Title,
                Description = createQuizDto.Description,
                TotalQuestions = createQuizDto.Questions?.Count ?? 0,
                Questions = createQuizDto.Questions?.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    Points = q.Points,
                    Answers = q.Answers?.Select(a => new Answer
                    {
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList() ?? new List<Answer>()
                }).ToList() ?? new List<Question>()
            };

            var result = await _quizRepository.Create(quiz);

            if (!result)
            {
                _logger.LogError("[QuizAPIController] Create: Failed to create quiz");
                return StatusCode(500, "Failed to create quiz");
            }

            var quizDto = new QuizDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                TotalQuestions = quiz.TotalQuestions
            };

            return CreatedAtAction(nameof(GetById), new { id = quiz.QuizId }, quizDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] Create: Error occurred while creating quiz");
            return StatusCode(500, "An error occurred while creating the quiz");
        }
    }

    /// <summary>
    /// Update an existing quiz
    /// </summary>
    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(QuizDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizDto updateQuizDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingQuiz = await _quizRepository.GetQuizById(id);

            if (existingQuiz == null)
            {
                _logger.LogWarning("[QuizAPIController] Update: Quiz not found for QuizId {QuizId}", id);
                return NotFound($"Quiz with ID {id} not found");
            }

            // Update scalar fields
            existingQuiz.Title = updateQuizDto.Title;
            existingQuiz.Description = updateQuizDto.Description;

            var result = await _quizRepository.Update(existingQuiz);

            if (!result)
            {
                _logger.LogError("[QuizAPIController] Update: Failed to update quiz {QuizId}", id);
                return StatusCode(500, "Failed to update quiz");
            }

            var quizDto = new QuizDto
            {
                QuizId = existingQuiz.QuizId,
                Title = existingQuiz.Title,
                Description = existingQuiz.Description,
                TotalQuestions = existingQuiz.TotalQuestions
            };

            return Ok(quizDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] Update: Error occurred while updating quiz {QuizId}", id);
            return StatusCode(500, "An error occurred while updating the quiz");
        }
    }

    /// <summary>
    /// Delete a quiz
    /// </summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var quiz = await _quizRepository.GetQuizById(id);

            if (quiz == null)
            {
                _logger.LogWarning("[QuizAPIController] Delete: Quiz not found for QuizId {QuizId}", id);
                return NotFound($"Quiz with ID {id} not found");
            }

            var result = await _quizRepository.Delete(id);

            if (!result)
            {
                _logger.LogError("[QuizAPIController] Delete: Failed to delete quiz {QuizId}", id);
                return StatusCode(500, "Failed to delete quiz");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] Delete: Error occurred while deleting quiz {QuizId}", id);
            return StatusCode(500, "An error occurred while deleting the quiz");
        }
    }

    /// <summary>
    /// Submit a quiz for grading
    /// </summary>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitQuizDto submitQuizDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the QuizId in the DTO matches the route parameter
            if (submitQuizDto.QuizId != id)
            {
                return BadRequest("Quiz ID in route does not match Quiz ID in body");
            }

            // Get the quiz from the database
            var quiz = await _quizRepository.GetQuizById(id);

            if (quiz == null || quiz.Questions == null)
            {
                _logger.LogWarning("[QuizAPIController] Submit: Quiz not found for QuizId {QuizId}", id);
                return NotFound($"Quiz with ID {id} not found");
            }

            // Evaluate the quiz
            var result = new QuizResultDto
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.Title,
                TotalQuestions = quiz.Questions.Count,
                TotalScore = 0,
                MaxScore = quiz.Questions.Sum(q => q.Points),
                QuestionsCorrect = 0,
                QuestionResults = new List<QuestionResultDto>()
            };

            // Evaluate each question
            foreach (var question in quiz.Questions)
            {
                var response = submitQuizDto.Responses?.FirstOrDefault(r => r.QuestionId == question.QuestionId);
                var selectedIds = response?.SelectedAnswerIds ?? new List<int>();
                var correctIds = question.Answers?.Where(a => a.IsCorrect).Select(a => a.AnswerId).ToHashSet() ?? new HashSet<int>();

                // Check if all selected answers are correct and all correct answers are selected
                var isCorrect = selectedIds.Count > 0
                    && selectedIds.All(id => correctIds.Contains(id))
                    && correctIds.SetEquals(selectedIds);

                if (isCorrect)
                {
                    result.QuestionsCorrect += 1;
                    result.TotalScore += question.Points;
                }

                // Prepare question result details
                result.QuestionResults?.Add(new QuestionResultDto
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    Points = question.Points,
                    IsCorrect = isCorrect,
                    SelectedAnswers = question.Answers?.Where(a => selectedIds.Contains(a.AnswerId))
                        .Select(a => a.AnswerText).ToList() ?? new List<string>(),
                    CorrectAnswers = question.Answers?.Where(a => a.IsCorrect)
                        .Select(a => a.AnswerText).ToList() ?? new List<string>()
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuizAPIController] Submit: Error occurred while submitting quiz {QuizId}", id);
            return StatusCode(500, "An error occurred while submitting the quiz");
        }
    }
}


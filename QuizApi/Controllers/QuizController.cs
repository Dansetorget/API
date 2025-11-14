using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.Controllers
{
    [Route("api/Quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ILogger<QuizController> _logger;
        private readonly UserManager<AuthUser> _userManager;


        public QuizController(IQuizRepository quizRepository, ILogger<QuizController> logger, UserManager<AuthUser> userManager)
        {
            _quizRepository = quizRepository;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: api/Quiz
        // Returns all quizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDTO>>> GetQuizzes()
        {
            try
            {
                var quizzes = await _quizRepository.GetAll();
                var quizDtos = quizzes.Adapt<List<QuizDTO>>();
                return Ok(quizDtos);

            }
            catch (Exception e)
            {
                // log error
                return StatusCode(500, $"An error occurred: {e.Message}");
            }


        }

        // GET: api/Quiz/5
        // Returns a specific quiz
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _quizRepository.GetQuizById(id);


            if (quiz == null)
            {
                return NotFound();
            }

            var quizDto = quiz.Adapt<QuizDTO>();
            return Ok(quizDto);
        }

        // Patch: api/Quiz/5
        // Update a quiz. To use it, first fetch a quiz with GetQuiz(int id) into a form. Then use this by clicking the Confirm button.
        [Authorize] // Only user who have created the quiz can update it
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, QuizDTO quizDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Brukeridentitet mangler.");
            }

            if (id != quizDto.Id)
            {
                _logger.LogWarning("ID mismatch in URL and body: {UrlId} vs {DtoId}", id, quizDto.Id);
                return BadRequest("ID mismatch");
            }

            // Get existing quiz from database
            var existingQuiz = await _quizRepository.GetQuizById(id);
            if (existingQuiz == null)
            {
                return NotFound();
            }

            if (!string.Equals(existingQuiz.OwnerId, userId, StringComparison.Ordinal)) // Only the owner/creator can update the quiz
            {
                return Forbid();
            }

            // Using Mapster to update existing entity with DTO data
            quizDto.Adapt(existingQuiz);
            try
            {
                var success = await _quizRepository.Update(existingQuiz);
                if (!success)
                {
                    // Log dette
                    return StatusCode(500, "Failed to update quiz");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await QuizExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Quiz
        // Creates a new quiz
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize] // Only logged in users can create quizzes
        [HttpPost]
        public async Task<ActionResult<QuizDTO>> CreateQuiz(CreateQuizDTO createQuizDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID from the JWT token
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User missing.");
            }

            var quiz = createQuizDto.Adapt<Quiz>();
            quiz.OwnerId = userId; // Set the owner of the quiz to the logged-in user

            var created = await _quizRepository.Create(quiz);
            if (!created)
            {
                // Log error
                return StatusCode(500, "Failed to create quiz");
            }

            // EF sets the generated Ids on the tracked entity after SaveChanges
            var createdQuizDto = quiz.Adapt<QuizDTO>();
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, createdQuizDto);
        }


        // DELETE: api/Quiz/5
        // Delete a specific quiz
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Brukeridentitet mangler.");
            }

            var quiz = await _quizRepository.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound();
            }

            if (!string.Equals(quiz.OwnerId, userId, StringComparison.Ordinal)) // Only the owner/creator can delete the quiz
            {
                return Forbid();
            }

            await _quizRepository.Delete(id);

            return NoContent();
        }
        // POST: api/Quiz/submit
        // Submits answers for a quiz and returns the result
        [HttpPost("submit")]
        public async Task<IActionResult> Submit(SubmitQuizDTO model)
        {
            // Basic validation to ensure we have a quiz id to work with.
            if (model == null || model.QuizId <= 0)
            {
                return BadRequest("Invalid submission");
            }

            // Load quiz including questions and answers so we can score the submission.
            var quiz = await _quizRepository.GetQuizById(model.QuizId);
            if (quiz?.Questions == null || quiz.Questions.Count == 0)
            {
                return NotFound();
            }

            // Flatten responses so that each question maps to a distinct set of selected answer ids.
            var responses = model.Responses?
                .GroupBy(r => r.QuestionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(r => r.SelectedAnswerIds ?? new List<int>())
                          .Where(id => id > 0)
                          .Distinct()
                          .ToList())
                ?? new Dictionary<int, List<int>>();

            var questionsById = quiz.Questions.ToDictionary(q => q.Id);

            // Use Mapster to populate the bulk of the result DTO (title, metadata, etc.).
            var result = quiz.Adapt<QuizResultDTO>();
            // Ensure we have mutable collections and reset scoring counters before we evaluate.
            result.QuestionResults = result.QuestionResults?.ToList() ?? new List<QuestionResultDTO>();
            result.QuestionsCorrect = 0;
            result.TotalScore = 0;

            foreach (var questionResult in result.QuestionResults)
            {
                if (!questionsById.TryGetValue(questionResult.QuestionId, out var question))
                {
                    continue;
                }

                // Fetch the selected answer ids for this question (empty list if user skipped).
                var selectedIds = responses.TryGetValue(question.Id, out var ids)
                    ? ids
                    : new List<int>();

                var correctAnswers = question.Answers?.Where(a => a.IsCorrect).ToList() ?? new List<Answer>();
                var correctIds = correctAnswers.Select(a => a.Id).ToHashSet();
                var selectedIdSet = selectedIds.ToHashSet();

                // Translate ids to readable answer text for the response payload.
                questionResult.SelectedAnswers = question.Answers?
                    .Where(a => selectedIdSet.Contains(a.Id))
                    .Select(a => a.AnswerText)
                    .ToList() ?? new List<string>();

                // Mark the question as correct only when the selection exactly matches the correct answer set.
                questionResult.IsCorrect = selectedIdSet.Count > 0 && correctIds.SetEquals(selectedIdSet);
                // Include the list of correct answers so the client can display what the user missed.
                questionResult.CorrectAnswers = correctAnswers.Select(a => a.AnswerText).ToList();

                if (questionResult.IsCorrect)
                {
                    result.QuestionsCorrect += 1;
                    result.TotalScore += question.Points;
                }
            }

            // Update aggregate fields after iterating all questions to keep the DTO consistent.
            result.TotalQuestions = result.QuestionResults.Count;
            result.MaxScore = quiz.Questions.Sum(q => q.Points);

            // Update streak for logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var today = DateTime.UtcNow.Date;
                    if (user.LastQuizDate == null) // First time taking a quiz
                    {
                        user.Streak = 1;
                    }
                    else
                    {
                        var lastDate = user.LastQuizDate.Value.Date;
                        if (lastDate == today) // Already taken a quiz today
                        {
                            // Do nothing
                        }
                        else if (lastDate == today.AddDays(-1)) // Took a quiz yesterday
                        {
                            user.Streak += 1; // Increase streak
                        }
                        else
                        {
                            user.Streak = 1; // Missed one or more days
                        }
                    }
                    user.LastQuizDate = today;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update user streak for user {UserId}", userId);
                    }
                }
            }
            return Ok(result);
        }

        private async Task<bool> QuizExists(int id)
        {
            return await _quizRepository.GetQuizById(id) != null;
        }

        // GET: api/Quiz/owner/{ownerId}
        // Returns all quizzes that belong to a given owner.
        [Authorize]
        [HttpGet("owner/{ownerId}")]
        public async Task<ActionResult<List<Quiz>>> GetQuizzesByOwner(string ownerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Brukeridentitet mangler.");
            }

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !string.Equals(ownerId, userId, StringComparison.Ordinal))
            {
                return Forbid(); // Forbid access if not admin and userid != ownerId
            }

            var quizzes = await _quizRepository.GetByOwnerId(ownerId); // Fetch quizzes by ownerId
            var quizDtos = quizzes.Adapt<List<QuizDTO>>();
            return Ok(quizDtos);
        }

    }
}

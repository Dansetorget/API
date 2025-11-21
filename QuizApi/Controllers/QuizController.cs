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
                _logger.LogError("[QuizController] GetQuizzes: Error retrieving all quizzes. Exception: {ExceptionMessage}", e.Message);
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
                    _logger.LogError("[QuizController] Failed to update quiz with ID {QuizId}", id);
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
                _logger.LogError("[QuizController] CreateQuiz: Error creating new quiz for user {UserId}", userId);
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
        public async Task<IActionResult> SubmitQuizAttempt(CreateQuizAttemptDTO model)
        {
            // Basic validation to ensure we have a quiz id to work with.
            if (model == null || model.QuizId <= 0)
            {
                return BadRequest("Invalid submission");
            }

            // Update streak for logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var today = DateTime.Now.Date;
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

            var attempt = model.Adapt<QuizAttempt>();
            attempt.UserId = userId!; // Can be null for anonymous users;
            Console.WriteLine($"Saving quiz attempt for QuizId {attempt.QuizId} by UserId {attempt.UserId}");


            var saveSuccess = await _quizRepository.SaveQuizAttempt(attempt);
            if (!saveSuccess)
            {
                _logger.LogWarning("Failed to save quiz attempt for QuizId {QuizId}", attempt.QuizId);
                return StatusCode(500, "Failed to save quiz attempt");
            }

            return Ok(new { message = "Quiz attempt saved successfully" });
        }

        [HttpGet("results/{quizId}")]
        public async Task<IActionResult> GetQuizResults(int quizId)
        {
            // Fetch quiz attempts for the given quizId and userId
            var attempts = await _quizRepository.GetQuizAttempts(quizId);
            if (attempts == null || !attempts.Any())
            {
                return NotFound("No quiz attempts found for this quiz and user.");
            }


            var count = 0;
            var attemptDtos = attempts.Adapt<List<QuizAttemptDTO>>();
            foreach (var dto in attemptDtos)
            {
                var user = await _userManager.FindByIdAsync(attempts.ElementAt(count)!.UserId);
                dto.UserName = user != null ? user.UserName ?? "Unknown" : "Anonymous";
                count++;
            }



            return Ok(attemptDtos);
        }


        // GET: api/Quiz/owner/
        // Returns all quizzes that belong to a given owner.
        [Authorize]
        [HttpGet("owner")]
        public async Task<ActionResult<List<Quiz>>> GetQuizzesByOwner()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Brukeridentitet mangler.");
            }

            var ownerId = userId; // For now, only allow fetching own quizzes

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !string.Equals(ownerId, userId, StringComparison.Ordinal))
            {
                return Forbid(); // Forbid access if not admin and userid != ownerId
            }

            var quizzes = await _quizRepository.GetByOwnerId(ownerId); // Fetch quizzes by ownerId
            var quizDtos = quizzes.Adapt<List<QuizDTO>>();
            return Ok(quizDtos);
        }

        private async Task<bool> QuizExists(int id)
        {
            return await _quizRepository.GetQuizById(id) != null;
        }

    }
}

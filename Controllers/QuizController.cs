using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.DAL;
using QuizApi.DTOs;
using QuizApi.Mappers;
using QuizApi.Models;

namespace QuizApi.Controllers
{
    [Route("api/Quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly QuizDbContext _context;
        private readonly IQuizRepository _quizRepository;
        private readonly ILogger<QuizController> _logger;


        public QuizController(QuizDbContext context, IQuizRepository quizRepository, ILogger<QuizController> logger)
        {
            _context = context;
            _quizRepository = quizRepository;
            _logger = logger;
        }

        // GET: api/Quiz
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDTO>>> GetQuizzes()
        {
            var quizzes = await _quizRepository.GetAll();
            var quizDtos = quizzes.Adapt<List<QuizDTO>>();
            return Ok(quizDtos);

        }

        // GET: api/Quiz/5
        // Use this to take a specific quiz
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
        // For å bruke denne metoden, hent først en quiz med GetQuiz(int id) inn i et form
        // Bruk så denne ved å trykke på Bekreft-knappen.
        [Authorize] // Only user who have created the quiz can update it
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, QuizDTO quizDto)
        {
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize] // Only logged in users can create quizzes
        [HttpPost]
        public async Task<ActionResult<Quiz>> CreateQuiz(QuizDTO quizDto)
        {
            var quiz = quizDto.Adapt<Quiz>();

            var createdQuiz = await _quizRepository.Create(quiz);
            if (!createdQuiz)
            {
                // Log error
                return StatusCode(500, "Failed to create quiz");
            }

            var createdQuizDto = createdQuiz.Adapt<QuizDTO>();
            return CreatedAtAction(nameof(GetQuiz), new { id = createdQuizDto.Id }, createdQuizDto);
        }

        // DELETE: api/Quiz/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _quizRepository.GetQuizById(id);
            if (quiz == null)
            {
                return NotFound();
            }

            await _quizRepository.Delete(id);

            return NoContent();
        }

        private async Task<bool> QuizExists(int id)
        {
            return await _quizRepository.GetQuizById(id) != null;
        }



    }
}

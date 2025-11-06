using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Http;
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
            var quizDtos = quizzes!.Select(QuizToDTO).ToList();
            return Ok(quizDtos);

        }

        // GET: api/Quiz/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _quizRepository.GetQuizById(id);


            if (quiz == null)
            {
                return NotFound();
            }

            var quizDto = QuizToDTO(quiz);
            return Ok(quizDto);
        }

        // PUT: api/Quiz/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.QuizId)
            {
                return BadRequest();
            }

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
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
        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuiz", new { id = quiz.QuizId }, quiz);
        }

        // DELETE: api/Quiz/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.QuizId == id);
        }

        private static QuizDTO QuizToDTO(Quiz quiz) =>
         new QuizDTO
         {
             Id = quiz.QuizId,
             Title = quiz.Title,
             Description = quiz.Description ?? string.Empty,
             Questions = quiz.Questions?.Select(QuestionToDTO).ToList() ?? new List<QuestionDTO>()
         };

        private static QuestionDTO QuestionToDTO(Question question) =>
            new QuestionDTO
            {
                Id = question.QuestionId,
                QuestionText = question.QuestionText,
                Points = question.Points,
                Answers = question.Answers?.Select(AnswerToDTO).ToList() ?? new List<AnswerDTO>()
            };

        private static AnswerDTO AnswerToDTO(Answer answer) =>
            new AnswerDTO
            {
                Id = answer.AnswerId,
                AnswerText = answer.AnswerText,
                IsCorrect = answer.IsCorrect
            };

    }
}

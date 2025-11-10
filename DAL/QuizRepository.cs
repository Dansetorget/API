using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuizApi.Models;

namespace QuizApi.DAL;

public class QuizRepository : IQuizRepository
{
    private readonly QuizDbContext _db;
    private readonly ILogger<QuizRepository> _logger;

    public QuizRepository(QuizDbContext db, ILogger<QuizRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Quiz>?> GetAll()
    {
        try
        {
            return await _db.Quizzes
                .Include(q => q.Questions!)
                .ThenInclude(a => a.Answers)
                .ToListAsync();
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] GetAll: Error retrieving all quizzes from the database.");
            return null;
        }
    }
    public async Task<Quiz?> GetQuizById(int id)
    {
        try
        {
            // Include questions and answers
            return await _db.Quizzes
                .Include(q => q.Questions!)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == id);
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] GetQuizById: Error retrieving quiz with ID {QuizId:0000}", id);
            return null;
        }
    }
    public async Task<IEnumerable<Quiz>?> GetByOwnerId(string ownerId)
    {
        try
        {
            return await _db.Quizzes
                .Where(q => q.OwnerId == ownerId)
                .Include(q => q.Questions!)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] GetByOwnerId: Error retrieving quizzes for owner {OwnerId}", ownerId);
            return null;
        }
    }
    public async Task<bool> Create(Quiz quiz)
    {
        try
        {
            await _db.Quizzes.AddAsync(quiz);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] Create: Error creating a new quiz.");
            return false;
        }
    }
    public async Task<bool> AddQuestionToQuiz(Quiz quiz, Question question)
    {
        try
        {
            if (quiz.Questions == null)
            {
                quiz.Questions = new List<Question>();
            }

            quiz.Questions.Add(question);
            _db.Quizzes.Update(quiz);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] AddQuestionToQuiz: Error adding question to quiz with ID {QuizId:0000}", quiz.QuizId);
            return false;
        }
    }


    public async Task<bool> Update(Quiz quiz)
    {
        try
        {
            _db.Quizzes.Update(quiz);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] Update: Error updating quiz with ID {QuizId:0000}", quiz.QuizId);
            return false;
        }
    }
    public async Task<bool> Delete(int id)
    {
        try
        {
            var quiz = await GetQuizById(id);
            if (quiz == null)
            {
                return false;
            }

            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] Delete: Error deleting quiz with ID {QuizId:0000}", id);
            return false;
        }
    }
}

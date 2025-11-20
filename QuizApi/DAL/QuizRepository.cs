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
                .FirstOrDefaultAsync(q => q.Id == id);
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


    public async Task<bool> SaveQuizAttempt(QuizAttempt attempt)
    {
        try
        {
            await _db.QuizAttempts.AddAsync(attempt);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[QuizRepository] Error saving quiz attempt. QuizId={QuizId} , Points={Points}, TimeTaken={TimeTaken}. Error message: {e}",
                attempt.QuizId, attempt.Points, attempt.TimeTaken, e.Message);


            // Show the inner exception, which often has the real cause
            if (e.InnerException != null)
                Console.WriteLine("Inner Exception: " + e.InnerException.Message);
            return false;
        }
    }

    public async Task<IEnumerable<QuizAttempt?>> GetQuizAttempts(int quizId, string userId)
    {
        try
        {
            var attempts = await _db.QuizAttempts
                .Where(qa => qa.QuizId == quizId && qa.UserId == userId)
                .ToListAsync();
            Console.WriteLine($"Retrieved {attempts.Count} attempts for QuizId {quizId} and UserId {userId}");
            return attempts;

        }
        catch (Exception)
        {
            _logger.LogError("[QuizRepository] GetQuizAttempt: Error retrieving quiz attempt for QuizId {QuizId} and UserId {UserId}", quizId, userId);
            return null!;
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
            _logger.LogError("[QuizRepository] Update: Error updating quiz with ID {QuizId:0000}", quiz.Id);
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

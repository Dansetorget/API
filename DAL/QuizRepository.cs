using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.DAL;

public class QuizRepository : IQuizRepository
{
    private readonly QuizDbContext _db;

    public QuizRepository(QuizDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Quiz>?> GetAll()
    {
        try
        {
            return await _db.Quizzes.ToListAsync();
        }
        catch (Exception)
        {
            // Log the exception (not implemented here)
            return null;
        }
    }
    public async Task<Quiz?> GetQuizById(int id)
    {
        try
        {
            return await _db.Quizzes.FindAsync(id);
        }
        catch (Exception)
        {
            // Log the exception (not implemented here)
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
            // Log the exception (not implemented here)
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
            // Log the exception (not implemented here)
            return false;
        }
    }
    public async Task<bool> Delete(int id)
    {
        try
        {
            var quiz = await _db.Quizzes.FindAsync(id);
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
            // Log the exception (not implemented here)
            return false;
        }
    }
}
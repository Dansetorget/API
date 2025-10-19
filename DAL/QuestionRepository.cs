using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.DAL;

public class QuestionRepository : IQuestionRepository
{
    private readonly QuizDbContext _db;

    public QuestionRepository(QuizDbContext db)
    {
        _db = db;
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
        catch
        {
            // log the exception (not implemented here)
            return false;
        }
    }

    public async Task<bool> Update(Quiz quiz, Question question)
    {
        try
        {
            var existingQuestion = quiz.Questions?.FirstOrDefault(q => q.QuestionId == question.QuestionId);
            if (existingQuestion != null)
            {
                existingQuestion.QuestionText = question.QuestionText;
                existingQuestion.Points = question.Points;
                _db.Quizzes.Update(quiz);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Delete(int questionId)
    {
        try
        {
            var question = await _db.Questions.FindAsync(questionId);
            if (question != null)
            {
                _db.Questions.Remove(question);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
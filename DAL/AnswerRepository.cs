using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.DAL;

public class AnswerRepository : IAnswerRepository
{
    private readonly QuizDbContext _db;
    public AnswerRepository(QuizDbContext db)
    {
        _db = db;
    }
    public async Task<bool> AddAnswerToQuestion(Question question, Answer answer)
    {
        try
        {

            if (question.Answers == null)
            {
                question.Answers = new List<Answer>();
            }

            question.Answers.Add(new Answer
            {
                AnswerText = answer.AnswerText,
                IsCorrect = answer.IsCorrect
            });

            _db.Answers.Update(answer);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }


    }
    public async Task<bool> Update(Question question, Answer answer)
    {
        try
        {
            var existingAnswer = question.Answers?.FirstOrDefault(a => a.AnswerId == answer.AnswerId);
            if (existingAnswer != null)
            {
                existingAnswer.AnswerText = answer.AnswerText;
                existingAnswer.IsCorrect = answer.IsCorrect;
                _db.Answers.Update(existingAnswer);
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

    public async Task<bool> Delete(int answerId)
    {
        try
        {
            var answer = await _db.Answers.FindAsync(answerId);
            if (answer != null)
            {
                _db.Answers.Remove(answer);
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
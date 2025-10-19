using QuizApp.Models;

namespace QuizApp.DAL;

public interface IAnswerRepository
{
    Task<bool> AddAnswerToQuestion(Question question, Answer answer);
    Task<bool> Update(Question question, Answer answer);
    Task<bool> Delete(int answerId);
}
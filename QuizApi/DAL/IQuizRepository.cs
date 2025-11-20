using QuizApi.Models;

namespace QuizApi.DAL;

public interface IQuizRepository
{
    Task<IEnumerable<Quiz>?> GetAll();
    Task<Quiz?> GetQuizById(int id);
    Task<IEnumerable<Quiz>?> GetByOwnerId(string ownerId);
    Task<bool> SaveQuizAttempt(QuizAttempt attempt);
    Task<bool> Create(Quiz quiz);
    Task<bool> Update(Quiz quiz);
    Task<bool> Delete(int id);
}

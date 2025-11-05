using QuizApp.Models;

namespace QuizApp.DAL;

public interface IQuizRepository
{
    Task<IEnumerable<Quiz>?> GetAll();
    Task<Quiz?> GetQuizById(int id);
    Task<bool> Create(Quiz quiz);
    Task<bool> AddQuestionToQuiz(Quiz quiz, Question question);
    Task<bool> Update(Quiz quiz);
    Task<bool> Delete(int id);
}
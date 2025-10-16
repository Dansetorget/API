using QuizApp.Models;

namespace QuizApp.DAL;

public interface IQuestionRepository
{
    Task<bool> AddQuestionToQuiz(Quiz quiz, Question question);
    Task<bool> Update(Quiz quiz, Question question);
    Task<bool> Delete(int questionId);
}
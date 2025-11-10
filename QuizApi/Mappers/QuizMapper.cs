using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.Mappers;


public class QuizMapper()
{
    public static QuizDTO QuizToDTO(Quiz quiz) =>
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
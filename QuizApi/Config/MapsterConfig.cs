using System.Collections.Generic;
using System.Linq;
using Mapster;
using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.MapsterConfig.cs;

public static class MapsterConfig
{
    public static void Configure()
    {
        // Quiz mappings
        TypeAdapterConfig<Quiz, QuizDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Questions, src => src.Questions ?? new List<Question>());

        TypeAdapterConfig<QuizDTO, Quiz>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Questions, src => src.Questions ?? new List<QuestionDTO>());

        // Question mappings
        TypeAdapterConfig<Question, QuestionDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Answers, src => src.Answers ?? new List<Answer>());

        TypeAdapterConfig<QuestionDTO, Question>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Answers, src => src.Answers ?? new List<AnswerDTO>());

        // Answer mappings
        TypeAdapterConfig<Answer, AnswerDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AnswerDTO, Answer>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        // Quiz result mappings
        TypeAdapterConfig<Quiz, QuizResultDTO>
            .NewConfig()
            .Map(dest => dest.QuizId, src => src.Id)
            .Map(dest => dest.QuizTitle, src => src.Title)
            .Map(dest => dest.TotalQuestions, src => src.Questions == null ? 0 : src.Questions.Count)
            .Map(dest => dest.MaxScore, src => src.Questions == null ? 0 : src.Questions.Sum(q => q.Points))
            .Map(dest => dest.QuestionResults, src => src.Questions ?? new List<Question>());

        TypeAdapterConfig<Question, QuestionResultDTO>
            .NewConfig()
            .Map(dest => dest.QuestionId, src => src.Id)
            .Map(dest => dest.QuestionText, src => src.QuestionText)
            .Map(dest => dest.Points, src => src.Points)
            .Map(dest => dest.CorrectAnswers,
                src => src.Answers != null
                    ? src.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerText).ToList()
                    : new List<string>());

        TypeAdapterConfig<QuizAttemptDTO, QuizAttempt>
            .NewConfig();


        // Ignore null values when updating
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}
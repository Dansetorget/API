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
            .Map(dest => dest.Id, src => src.QuizId)
            .Map(dest => dest.Questions, src => src.Questions ?? new List<Question>());

        TypeAdapterConfig<QuizDTO, Quiz>
            .NewConfig()
            .Map(dest => dest.QuizId, src => src.Id)
            .Map(dest => dest.Questions, src => src.Questions ?? new List<QuestionDTO>());

        // Question mappings
        TypeAdapterConfig<Question, QuestionDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.QuestionId)
            .Map(dest => dest.Answers, src => src.Answers ?? new List<Answer>());

        TypeAdapterConfig<QuestionDTO, Question>
            .NewConfig()
            .Map(dest => dest.QuestionId, src => src.Id)
            .Map(dest => dest.Answers, src => src.Answers ?? new List<AnswerDTO>());

        // Answer mappings
        TypeAdapterConfig<Answer, AnswerDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.AnswerId);

        TypeAdapterConfig<AnswerDTO, Answer>
            .NewConfig()
            .Map(dest => dest.AnswerId, src => src.Id);

        // Ignore null values when updating
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}
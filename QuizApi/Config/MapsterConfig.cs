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



        TypeAdapterConfig<QuizAttemptDTO, QuizAttempt>
            .NewConfig();


        // Ignore null values when updating
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }
}
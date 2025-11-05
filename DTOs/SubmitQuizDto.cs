using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

public class SubmitQuizDto
{
    // QuizId is set from URL route parameter, not required in body
    public int QuizId { get; set; }
    
    public List<QuestionResponseDto>? Responses { get; set; }
}

public class QuestionResponseDto
{
    [Required]
    public int QuestionId { get; set; }
    
    public List<int>? SelectedAnswerIds { get; set; }
}


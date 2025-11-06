namespace QuizApp.DTOs;

public class SubmitQuizDTO
{
    public int QuizId { get; set; }
    public List<QuestionResponseDTO> Responses { get; set; } = new();
}

public class QuestionResponseDTO
{
    public int QuestionId { get; set; }
    public List<int> SelectedAnswerIds { get; set; } = new();
}
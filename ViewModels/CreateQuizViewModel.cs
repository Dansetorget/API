using System.Collections.Generic;

namespace QuizApp.Models;

public class CreateQuizViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;

    public int Points { get; set; }
    public List<QuestionAnswerViewModel> Answers { get; set; } = new();
}

public class QuestionAnswerViewModel
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}



using System.Collections.Generic;

namespace QuizApp.Models;

public class CreateQuizViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;

    public int Points { get; set; }
    public List<AnswerViewModel> Answers { get; set; } = new();
}

public class AnswerViewModel
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}



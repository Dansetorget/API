using System.Collections.Generic;

namespace QuizApp.Models;

public class QuizCreateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<QuestionAnswerViewModel> Answers { get; set; } = new();
}

public class QuestionAnswerViewModel
{
    public int Id { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}



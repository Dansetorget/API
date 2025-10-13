using System.Collections.Generic;

namespace QuizApp.Models;

public class QuizCreateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestionItemViewModel> Questions { get; set; } = new();
}

public class QuestionItemViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}



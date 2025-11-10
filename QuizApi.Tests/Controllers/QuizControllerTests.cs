using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuizApi.Controllers;
using QuizApi.DAL;
using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.Tests.Controllers;

public class QuizControllerTests
{
    [Fact]
    public async Task GetQuizzes_ShouldReturnOk_WithListOfQuizDTOs()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();

        var quizzes = new List<Quiz>
        {
            new Quiz { Id = 1, Title = "Math"},
            new Quiz { Id = 2, Title = "Science"}
        };


        mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(quizzes);

        var mockLogger = new Mock<ILogger<QuizController>>();
        var quizController = new QuizController(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await quizController.GetQuizzes();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var value = Assert.IsAssignableFrom<IEnumerable<QuizDTO>>(okResult!.Value);

        // Verify the returned list
        Assert.Equal(2, value.Count());
        Assert.Equal("Math", value.First().Title);

        // Verify repository was called
        mockRepo.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetQuizzes_ShouldReturnStatus500_WhenRepositoryThrows()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        mockRepo.Setup(repo => repo.GetAll())
                    .ThrowsAsync(new Exception("Database failure"));

        var mockLogger = new Mock<ILogger<QuizController>>();
        var controller = new QuizController(mockRepo.Object, mockLogger.Object);


        // Act
        var result = await controller.GetQuizzes();


        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        var message = Assert.IsType<string>(statusResult.Value);
        Assert.Contains("Database failure", message);

        mockRepo.Verify(repo => repo.GetAll(), Times.Once);
    }


    [Fact]
    public async Task GetQuizById_ShouldRetunOk_WhenQuizExists()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var quiz = new Quiz { Id = 42, Title = "History" };
        mockRepo.Setup(repo => repo.GetQuizById(42)).ReturnsAsync(quiz);


        var mockLogger = new Mock<ILogger<QuizController>>();
        var controller = new QuizController(mockRepo.Object, mockLogger.Object);


        // Act
        var result = await controller.GetQuiz(42);


        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDto = Assert.IsType<QuizDTO>(okResult.Value);

        Assert.Equal(42, quizDto.Id);
        Assert.Equal("History", quizDto.Title);


        mockRepo.Verify(repo => repo.GetQuizById(42), Times.Once);
    }


    [Fact]
    public async Task GetQuizById_GetQuizById_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        mockRepo.Setup(repo => repo.GetQuizById(42)).ReturnsAsync((Quiz?)null); // simulate not found

        var mockLogger = new Mock<ILogger<QuizController>>();
        var controller = new QuizController(mockRepo.Object, mockLogger.Object);


        // Act 
        var result = await controller.GetQuiz(42);


        // Assert
        Assert.IsType<NotFoundResult>(result.Result);


        // Verify that repository was called exactly once
        mockRepo.Verify(repo => repo.GetQuizById(42), Times.Once);
    }

}
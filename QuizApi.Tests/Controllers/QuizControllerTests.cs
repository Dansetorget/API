using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuizApi.Controllers;
using QuizApi.DAL;
using QuizApi.DTOs;
using QuizApi.Models;

namespace QuizApi.Tests.Controllers;

// Testing CRUD operations for QuizController
public class QuizControllerTests
{
    // Private mocks at the top
    private readonly Mock<IQuizRepository> _repoMock = new();
    private readonly Mock<ILogger<QuizController>> _loggerMock = new();
    private readonly Mock<UserManager<AuthUser>> _userManagerMock;
    private readonly QuizController _controller;

    public QuizControllerTests()
    {
        // Minimal UserManager mock
        var store = new Mock<IUserStore<AuthUser>>();
        _userManagerMock = new Mock<UserManager<AuthUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        _controller = new QuizController(_repoMock.Object, _loggerMock.Object, _userManagerMock.Object);

        // Default authenticated user
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "mockAuth");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    // Test for CreateQuiz
    [Fact]
    public async Task CreateQuiz_ShouldReturnCreatedAtAction_WithMappedQuiz()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        var dto = new CreateQuizDTO
        {
            Title = "Geography",
            Questions = new()
        {
            new()
            {
                QuestionText = "What is the capital of France?",
                Answers = new()
                {
                    new() { AnswerText = "Paris", IsCorrect = true },
                    new() { AnswerText = "London", IsCorrect = false }
                }
            }
        }
        };

        Quiz? captured = null;

        mockRepo.Setup(r => r.Create(It.IsAny<Quiz>()))
                .Callback<Quiz>(q => captured = q)
                .ReturnsAsync(true);

        // Act
        var result = await controller.CreateQuiz(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<QuizDTO>(created.Value);

        // Validate basic fields
        Assert.Equal(dto.Title, returned.Title);
        Assert.Equal(dto.Questions.Count, returned.Questions.Count);

        // Validate first question/answers
        var qDto = dto.Questions[0];
        var qReturned = returned.Questions[0];

        Assert.Equal(qDto.QuestionText, qReturned.QuestionText);
        Assert.Equal(qDto.Answers.Count, qReturned.Answers.Count);

        // Validate an answer
        Assert.Equal(qDto.Answers[0].AnswerText, qReturned.Answers[0].AnswerText);
        Assert.Equal(qDto.Answers[0].IsCorrect, qReturned.Answers[0].IsCorrect);

        // Repo should be called once
        mockRepo.Verify(r => r.Create(It.IsAny<Quiz>()), Times.Once);

        // Ensure title was correctly mapped into entity
        Assert.Equal(dto.Title, captured!.Title);
    }

    // 

    // Negative test for CreateQuiz
    [Fact]
    public async Task CreateQuiz_ShouldReturn500_WhenRepositoryFails()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        var quizDto = new CreateQuizDTO
        {
            Title = "Invalid Quiz"
        };

        // Setup repo to return false indicating failure
        mockRepo.Setup(r => r.Create(It.IsAny<Quiz>())).ReturnsAsync(false);

        // Act
        var result = await controller.CreateQuiz(quizDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Failed to create quiz", statusCodeResult.Value);

        // Repo should have been called once
        mockRepo.Verify(r => r.Create(It.IsAny<Quiz>()), Times.Once);
    }

    // Test for read by id
    [Fact]
    public async Task GetQuizById_ShouldRetunOk_WhenQuizExists()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);


        var quiz = new Quiz { Id = 42, Title = "History" };
        mockRepo.Setup(r => r.GetQuizById(42)).ReturnsAsync(quiz);


        // Act
        var result = await controller.GetQuiz(42);


        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDto = Assert.IsType<QuizDTO>(okResult.Value);

        Assert.Equal(42, quizDto.Id);
        Assert.Equal("History", quizDto.Title);

        mockRepo.Verify(r => r.GetQuizById(42), Times.Once);
    }

    // Negative test for read by id
    [Fact]
    public async Task GetQuizById_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);


        mockRepo.Setup(r => r.GetQuizById(42)).ReturnsAsync((Quiz?)null); // simulate not found


        // Act 
        var result = await controller.GetQuiz(42);


        // Assert
        Assert.IsType<NotFoundResult>(result.Result);


        // Verify that repository was called exactly once
        mockRepo.Verify(r => r.GetQuizById(42), Times.Once);
    }



    // Test for update quiz
    [Fact]
    public async Task UpdateQuiz_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        var quizDto = new QuizDTO
        {
            Id = 1,
            Title = "Updated Quiz Title"
        };

        // Mock existing quiz returned from repository
        var existingQuiz = new Quiz
        {
            Id = 1,
            Title = "Old Title",
            OwnerId = "1"
        };

        mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        // Capture the Quiz entity passed to Update()
        Quiz? capturedQuiz = null;

        mockRepo.Setup(r => r.Update(It.IsAny<Quiz>()))
                .Callback<Quiz>(q => capturedQuiz = q)
                .ReturnsAsync(true);

        // Act
        var result = await controller.UpdateQuiz(1, quizDto);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify Update was called exactly once
        mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Once);

        // Check that the updated quiz contains the new values
        Assert.NotNull(capturedQuiz);
        Assert.Equal(1, capturedQuiz!.Id);
        Assert.Equal("Updated Quiz Title", capturedQuiz.Title);
    }

    /* Negative tests for update quiz */
    // Test for unauthorized update
    [Fact]
    public async Task UpdateQuiz_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        // No user identity
        controller.ControllerContext.HttpContext.User =
            new ClaimsPrincipal(new ClaimsIdentity());

        var dto = new QuizDTO { Id = 1, Title = "Test" };

        // Act
        var result = await controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }


    // Test for id mismatch
    [Fact]
    public async Task UpdateQuiz_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        var dto = new QuizDTO { Id = 2, Title = "Test" };

        // Act
        var result = await controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }

    // Test for quiz not found
    [Fact]
    public async Task UpdateQuiz_ShouldReturnNotFound_WhenQuizNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        mockRepo.Setup(r => r.GetQuizById(1)).ReturnsAsync((Quiz)null!);

        var dto = new QuizDTO { Id = 1, Title = "Test" };

        // Act
        var result = await controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // Test for forbidden update
    [Fact]
    public async Task UpdateQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        mockRepo.Setup(r => r.GetQuizById(1)).ReturnsAsync(new Quiz
        {
            Id = 1,
            OwnerId = "999"
        });

        var dto = new QuizDTO { Id = 1, Title = "Updated" };

        // Act
        var result = await controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);
        mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }

    /* End of Negative test for update quiz */

    /* Delete quiz tests */
    // Test for delete quiz
    [Fact]
    public async Task DeleteQuiz_ShouldReturnNoContent_WhenDeletionIsSuccessful()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        // Mock existing quiz owned by the user
        var existingQuiz = new Quiz
        {
            Id = 1,
            OwnerId = "1"
        };

        mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        mockRepo.Setup(r => r.Delete(1))
                .ReturnsAsync(true);

        // Act
        var result = await controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify Delete was called exactly once
        mockRepo.Verify(r => r.Delete(1), Times.Once);
    }


    // Negative test for delete quiz
    [Fact]
    public async Task DeleteQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        // Mock existing quiz NOT owned by the user
        var existingQuiz = new Quiz
        {
            Id = 1,
            OwnerId = "999"   // <-- Not owned by the logged-in user
        };

        mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        // Act
        var result = await controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<ForbidResult>(result);

        // Ensure Delete() was never called
        mockRepo.Verify(r => r.Delete(1), Times.Never);
    }

    /* End of delete quiz tests */

    /* Helper method */
    // Method to create controller with mocked dependencies
    private QuizController CreateController(Mock<IQuizRepository> repoMock = null!)
    {
        repoMock ??= new Mock<IQuizRepository>();

        var logger = new Mock<ILogger<QuizController>>();

        // Minimal UserManager mock (constructor requires it)
        var store = new Mock<IUserStore<AuthUser>>();
        var userManager = new Mock<UserManager<AuthUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        var controller = new QuizController(repoMock.Object, logger.Object, userManager.Object);

        // Default authenticated user
        var identity = new ClaimsIdentity(
            new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
            },
            "mockAuth"
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }


}
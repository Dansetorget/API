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
    private readonly Mock<IQuizRepository> _mockRepo = new();
    private readonly Mock<ILogger<QuizController>> _mockLogger = new();
    private readonly Mock<UserManager<AuthUser>> _mockUserManager;
    private readonly QuizController _controller;

    public QuizControllerTests()
    {
        // Minimal UserManager mock
        var store = new Mock<IUserStore<AuthUser>>();
        _mockUserManager = new Mock<UserManager<AuthUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        _controller = new QuizController(_mockRepo.Object, _mockLogger.Object, _mockUserManager.Object);

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


    /*---------------------------------------------
    | Tests for CREATE Quiz, 1 positive, 2 negative
     --------------------------------------------*/
    // Test for CreateQuiz
    [Fact]
    public async Task CreateQuiz_ShouldReturnCreatedAtAction_WithMappedQuiz()
    {
        // Arrange
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

        _mockRepo.Setup(r => r.Create(It.IsAny<Quiz>()))
                .Callback<Quiz>(q => captured = q)
                .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateQuiz(dto);

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
        _mockRepo.Verify(r => r.Create(It.IsAny<Quiz>()), Times.Once);

        // Ensure title was correctly mapped into entity
        Assert.Equal(dto.Title, captured!.Title);
    }

    // Negative test for CreateQuiz when user is missing
    [Fact]
    public async Task CreateQuiz_ShouldReturnUnauthorized_WhenUserMissing()
    {
        // Arrange
        SetUser(null);
        var dto = new CreateQuizDTO();

        // Act
        var result = await _controller.CreateQuiz(dto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("User missing.", unauthorized.Value);
    }

    // Negative test for CreateQuiz
    [Fact]
    public async Task CreateQuiz_ShouldReturn500_WhenRepositoryFails()
    {
        // Arrange
        var quizDto = new CreateQuizDTO
        {
            Title = "Invalid Quiz"
        };

        // Setup repo to return false indicating failure
        _mockRepo.Setup(r => r.Create(It.IsAny<Quiz>())).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateQuiz(quizDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Failed to create quiz", statusCodeResult.Value);

        // Repo should have been called once
        _mockRepo.Verify(r => r.Create(It.IsAny<Quiz>()), Times.Once);
    }

    /*---------------------------------------------
    | End of CreateQuiz tests
     --------------------------------------------*/

    /*--------------------------------------------- 
    | Tests for Read, 1 positive, 1 negative
     --------------------------------------------*/

    // Positive test for read by id
    [Fact]
    public async Task GetQuizById_ShouldRetunOk_WhenQuizExists()
    {
        // Arrange

        var quiz = new Quiz { Id = 42, Title = "History" };
        _mockRepo.Setup(r => r.GetQuizById(42)).ReturnsAsync(quiz);


        // Act
        var result = await _controller.GetQuiz(42);


        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDto = Assert.IsType<QuizDTO>(okResult.Value);

        Assert.Equal(42, quizDto.Id);
        Assert.Equal("History", quizDto.Title);

        _mockRepo.Verify(r => r.GetQuizById(42), Times.Once);
    }

    // Negative test for read by id
    [Fact]
    public async Task GetQuizById_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetQuizById(42)).ReturnsAsync((Quiz?)null); // simulate not found


        // Act 
        var result = await _controller.GetQuiz(42);


        // Assert
        Assert.IsType<NotFoundResult>(result.Result);


        // Verify that repository was called exactly once
        _mockRepo.Verify(r => r.GetQuizById(42), Times.Once);
    }

    // Positive test for read all quizzes
    [Fact]
    public async Task GetAllQuizzes_ShouldReturnOk_WithListOfQuizzes()
    {
        // Arrange
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = 1, Title = "Math" },
            new Quiz { Id = 2, Title = "Science" }
        };

        _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(quizzes);

        // Act
        var result = await _controller.GetQuizzes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDtos = Assert.IsType<List<QuizDTO>>(okResult.Value);

        Assert.Equal(2, quizDtos.Count);
        Assert.Equal("Math", quizDtos[0].Title);
        Assert.Equal("Science", quizDtos[1].Title);

        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    // Negative test for read all quizzes
    [Fact]
    public async Task GetAllQuizzes_ShouldReturnNotFound_WhenNoQuizzesExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAll()).ReturnsAsync((IEnumerable<Quiz>?)null);

        // Act
        var result = await _controller.GetQuizzes();

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);

        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    /*---------------------------------------------
    | End of Read tests
     --------------------------------------------*/

    /*---------------------------------------------
    | Tests for UPDATE Quiz, 1 positive, 4 negative
     --------------------------------------------*/
    // Positive test for update quiz
    [Fact]
    public async Task UpdateQuiz_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
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

        _mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        // Capture the Quiz entity passed to Update()
        Quiz? capturedQuiz = null;

        _mockRepo.Setup(r => r.Update(It.IsAny<Quiz>()))
                .Callback<Quiz>(q => capturedQuiz = q)
                .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateQuiz(1, quizDto);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify Update was called exactly once
        _mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Once);

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
        // No user identity
        SetUser(null);

        var dto = new QuizDTO { Id = 1, Title = "Test" };

        // Act
        var result = await _controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        _mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }


    // Test for id mismatch
    [Fact]
    public async Task UpdateQuiz_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange
        var dto = new QuizDTO { Id = 2, Title = "Test" };

        // Act
        var result = await _controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }

    // Test for quiz not found
    [Fact]
    public async Task UpdateQuiz_ShouldReturnNotFound_WhenQuizNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetQuizById(1)).ReturnsAsync((Quiz)null!);

        var dto = new QuizDTO { Id = 1, Title = "Test" };

        // Act
        var result = await _controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // Test for forbidden update
    [Fact]
    public async Task UpdateQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetQuizById(1)).ReturnsAsync(new Quiz
        {
            Id = 1,
            OwnerId = "999"
        });

        var dto = new QuizDTO { Id = 1, Title = "Updated" };

        // Act
        var result = await _controller.UpdateQuiz(1, dto);

        // Assert
        Assert.IsType<ForbidResult>(result);
        _mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }

    /*---------------------------------------------
    | End of Update Quiz tests
     --------------------------------------------*/

    /*---------------------------------------------
    | Tests for DELETE Quiz, 1 positive, 1 negative
      --------------------------------------------*/
    // Positive test for delete quiz
    [Fact]
    public async Task DeleteQuiz_ShouldReturnNoContent_WhenDeletionIsSuccessful()
    {
        // Arrange
        // Mock existing quiz owned by the user
        var existingQuiz = new Quiz
        {
            Id = 1,
            OwnerId = "1"
        };

        _mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        _mockRepo.Setup(r => r.Delete(1))
                .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        // Verify Delete was called exactly once
        _mockRepo.Verify(r => r.Delete(1), Times.Once);
    }


    // Negative test for delete quiz
    [Fact]
    public async Task DeleteQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
        // Mock existing quiz NOT owned by the user
        var existingQuiz = new Quiz
        {
            Id = 1,
            OwnerId = "not-owned-by-user-by-logged-in-user"
        };

        _mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(existingQuiz);

        // Act
        var result = await _controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<ForbidResult>(result);

        // Ensure Delete() was never called
        _mockRepo.Verify(r => r.Delete(1), Times.Never);
    }
    /*---------------------------------------------
    | End of Delete Quiz tests
      --------------------------------------------*/

    /* Helper methods */
    private void SetUser(string? userId)
    {
        var identity = new ClaimsIdentity();
        if (!string.IsNullOrEmpty(userId))
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }


}
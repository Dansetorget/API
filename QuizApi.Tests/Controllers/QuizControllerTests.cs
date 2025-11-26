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
    | Tests for CREATE Quiz, 2 positive, 5 negative
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

    // Positive test for submitting a quiz attempt
    [Fact]
    public async Task SubmitQuizAttempt_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var attemptDto = new CreateQuizAttemptDTO
        {
            QuizId = 1,
            Points = 1,
            TimeTaken = 300,
        };

        _mockRepo.Setup(r => r.GetQuizById(attemptDto.QuizId))
                 .ReturnsAsync(new Quiz { Id = attemptDto.QuizId, Title = "Sample Quiz" });
        _mockRepo.Setup(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitQuizAttempt(attemptDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Quiz attempt saved successfully", okResult.Value);

        _mockRepo.Verify(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()), Times.Once);
    }

    // Negative test for submitting a quiz attempt with invalid submission
    [Fact]
    public async Task SubmitQuizAttempt_ShouldReturnBadRequest_WhenInvalidSubmission()
    {
        // Arrange
        var attemptDto = (CreateQuizAttemptDTO?)null;

        // Act
        var result = await _controller.SubmitQuizAttempt(attemptDto!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid quiz attempt submission", badRequestResult.Value);

        _mockRepo.Verify(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()), Times.Never);
    }

    // Negative test for submitting a quiz attempt when quiz not found
    [Fact]
    public async Task SubmitQuizAttempt_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        var attemptDto = new CreateQuizAttemptDTO
        {
            QuizId = 999, // Non-existent quiz
            Points = 1,
            TimeTaken = 300,
        };

        _mockRepo.Setup(r => r.GetQuizById(attemptDto.QuizId))
                 .ReturnsAsync((Quiz?)null); // Simulate not found

        // Act
        var result = await _controller.SubmitQuizAttempt(attemptDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);

        _mockRepo.Verify(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()), Times.Never);
    }

    // Negative test for submitting a quiz attempt when saving fails
    [Fact]
    public async Task SubmitQuizAttempt_ShouldReturn500_WhenSavingFails()
    {
        // Arrange
        var attemptDto = new CreateQuizAttemptDTO
        {
            QuizId = 1,
            Points = 1,
            TimeTaken = 300,
        };

        _mockRepo.Setup(r => r.GetQuizById(attemptDto.QuizId))
                 .ReturnsAsync(new Quiz { Id = attemptDto.QuizId, Title = "Sample Quiz" });
        _mockRepo.Setup(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()))
                 .ReturnsAsync(false); // Simulate failure

        // Act
        var result = await _controller.SubmitQuizAttempt(attemptDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Failed to save quiz attempt", objectResult.Value);

        _mockRepo.Verify(r => r.SaveQuizAttempt(It.IsAny<QuizAttempt>()), Times.Once);
    }

    /*---------------------------------------------
    | End of CREATE tests
     --------------------------------------------*/

    /*--------------------------------------------- 
    | Tests for Read, 9 positive, 3 negative
     --------------------------------------------*/

    // Positive test for read quiz attempts by quiz id
    [Fact]
    public async Task GetQuizResults_ShouldReturnOk_WithListOfAttempts()
    {
        // Arrange
        var quizId = 1;
        var attempts = new List<QuizAttempt>
        {
            new QuizAttempt { QuizId = quizId, Points = 5, TimeTaken = 300, FirstAttempt = true },
            new QuizAttempt { QuizId = quizId, Points = 3, TimeTaken = 400, FirstAttempt = false },
            new QuizAttempt { QuizId = quizId, Points = 3, TimeTaken = 350, FirstAttempt = true }
        };

        _mockRepo.Setup(r => r.GetQuizAttempts(quizId)).ReturnsAsync(attempts);

        // Act
        var result = await _controller.GetQuizResults(quizId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var attemptDtos = Assert.IsType<List<QuizAttemptDTO>>(okResult.Value);

        Assert.Equal(2, attemptDtos.Count);
        Assert.Equal(5, attemptDtos[0].Points);
        Assert.Equal(3, attemptDtos[1].Points);

        _mockRepo.Verify(r => r.GetQuizAttempts(quizId), Times.Once);
    }

    // Positive test for read quiz attempts when none exist
    [Fact]
    public async Task GetQuizResults_ShouldReturnOk_WhenNoAttemptsExist()
    {
        // Arrange
        var quizId = 1;
        var attempts = new List<QuizAttempt>(); // No attempts

        _mockRepo.Setup(r => r.GetQuizAttempts(quizId)).ReturnsAsync(attempts);

        // Act
        var result = await _controller.GetQuizResults(quizId);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result);

        Assert.Equal("No quiz attempts found for this quiz.", response.Value);

        _mockRepo.Verify(r => r.GetQuizAttempts(quizId), Times.Once);
    }

    // Positive test for read quiz attempts when no logged in user attempts exist
    [Fact]
    public async Task GetQuizResults_ShouldReturnOk_WhenNoLoggedInUserAttemptsExist()
    {
        // Arrange
        var quizId = 1;
        var attempts = new List<QuizAttempt>
        {
            new QuizAttempt { QuizId = quizId, Points = 5, TimeTaken = 300, FirstAttempt = false },
            new QuizAttempt { QuizId = quizId, Points = 3, TimeTaken = 400, FirstAttempt = false }
        };

        _mockRepo.Setup(r => r.GetQuizAttempts(quizId)).ReturnsAsync(attempts);

        // Act
        var result = await _controller.GetQuizResults(quizId);

        // Assert
        var response = Assert.IsType<OkObjectResult>(result);

        Assert.Equal("No attempts from logged in users found for this quiz.", response.Value);

        _mockRepo.Verify(r => r.GetQuizAttempts(quizId), Times.Once);
    }

    // Negative test for read quiz attempts when databse error occurs
    [Fact]
    public async Task GetQuizResults_ShouldReturn500_WhenDatabaseErrorOccurs()
    {
        // Arrange
        var quizId = 1;
        var exceptionMessage = "Database error";

        _mockRepo.Setup(r => r.GetQuizAttempts(quizId))
                 .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetQuizResults(quizId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal($"An error occurred: {exceptionMessage}", objectResult.Value);

        _mockRepo.Verify(r => r.GetQuizAttempts(quizId), Times.Once);
    }

    // Positive test for read quiz by owner
    [Fact]
    public async Task GetQuizzesByOwner_ShouldReturnOk_WithListOfQuizzes()
    {
        // Arrange
        var ownerId = "1";
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = 1, Title = "Math", OwnerId = ownerId },
            new Quiz { Id = 2, Title = "Science", OwnerId = ownerId }
        };

        _mockRepo.Setup(r => r.GetByOwnerId(ownerId)).ReturnsAsync(quizzes);

        // Act
        var result = await _controller.GetQuizzesByOwner();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDtos = Assert.IsType<List<QuizDTO>>(okResult.Value);

        Assert.Equal(2, quizDtos.Count);
        Assert.Equal("Math", quizDtos[0].Title);
        Assert.Equal("Science", quizDtos[1].Title);

        _mockRepo.Verify(r => r.GetByOwnerId(ownerId), Times.Once);
    }

    // Positive test for read quiz by owner when role admin
    [Fact]
    public async Task GetQuizzesByOwner_ShouldReturnOk_WithAllQuizzes_WhenUserIsAdmin()
    {
        // Arrange
        SetUser("1", "Admin");
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = 1, Title = "Math", OwnerId = "2" },
            new Quiz { Id = 2, Title = "Science", OwnerId = "3" }
        };

        _mockRepo.Setup(r => r.GetAll()).ReturnsAsync(quizzes);

        // Act
        var result = await _controller.GetQuizzesByOwner();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDtos = Assert.IsType<List<QuizDTO>>(okResult.Value);

        Assert.Equal(2, quizDtos.Count);
        Assert.Equal("Math", quizDtos[0].Title);
        Assert.Equal("Science", quizDtos[1].Title);

        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    // Positive test for read quiz by owner when no quizzes exist
    [Fact]
    public async Task GetQuizzesByOwner_ShouldReturnOk_WhenNoQuizzesExist()
    {
        // Arrange
        var ownerId = "1";

        _mockRepo.Setup(r => r.GetByOwnerId(ownerId)).ReturnsAsync((IEnumerable<Quiz>?)null);

        // Act
        var result = await _controller.GetQuizzesByOwner();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("No quizzes found for this owner.", ((OkObjectResult)result.Result).Value);

        _mockRepo.Verify(r => r.GetByOwnerId(ownerId), Times.Once);
    }

    // Negative test for read quiz by owner when user missing
    [Fact]
    public async Task GetQuizzesByOwner_ShouldReturnUnauthorized_WhenUserMissing()
    {
        // Arrange
        SetUser(null);

        // Act
        var result = await _controller.GetQuizzesByOwner();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("User missing.", unauthorizedResult.Value);
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

    // Positive test for read all quizzes, when no quizzes exist
    [Fact]
    public async Task GetAllQuizzes_ShouldReturnNoContent_WhenNoQuizzesExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAll()).ReturnsAsync((IEnumerable<Quiz>?)null);

        // Act
        var result = await _controller.GetQuizzes();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    // Negative test for read all quizzes, when exception occurs
    [Fact]
    public async Task GetAllQuizzes_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var exceptionMessage = "Database error";
        _mockRepo.Setup(r => r.GetAll()).ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetQuizzes();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal($"An error occurred: {exceptionMessage}", objectResult.Value);

        _mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

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
    | Tests for DELETE Quiz, 2 positive, 3 negative
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

    // Positive test for delete quiz when user is admin
    [Fact]
    public async Task DeleteQuiz_ShouldReturnNoContent_WhenUserIsAdmin()
    {
        // Arrange
        SetUser("1", "Admin");

        // Mock existing quiz NOT owned by the user
        var existingQuiz = new Quiz
        {
            Id = 1,
            OwnerId = "not-owned-by-logged-in-user"
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

    // Negative test for delete quiz when user is unauthorized
    [Fact]
    public async Task DeleteQuiz_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        SetUser(null);

        // Act
        var result = await _controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);

        // Ensure Delete() was never called
        _mockRepo.Verify(r => r.Delete(1), Times.Never);
    }

    // Negative test for delete quiz when user does not own quiz
    [Fact]
    public async Task DeleteQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
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

    // Negative test for delete quiz when quiz not found
    [Fact]
    public async Task DeleteQuiz_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _controller.DeleteQuiz(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        // Ensure Delete() was never called
        _mockRepo.Verify(r => r.Delete(1), Times.Never);
    }


    /*---------------------------------------------
    | End of Delete Quiz tests
      --------------------------------------------*/

    /* Helper methods */
    private void SetUser(string? userId, params string[] roles)
    {
        var identity = new ClaimsIdentity("mockAuth");

        if (!string.IsNullOrEmpty(userId))
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

        // Add roles
        foreach (var role in roles)
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }



}
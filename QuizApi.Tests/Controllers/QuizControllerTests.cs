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
    // Method to create controller with mocked dependencies
    private QuizController CreateController(Mock<IQuizRepository> mockRepo = null!)
    {
        mockRepo ??= new Mock<IQuizRepository>();
        var mockLogger = new Mock<ILogger<QuizController>>();

        // Mock UserManager
        var store = new Mock<IUserStore<AuthUser>>();

        var mockUserManager = new Mock<UserManager<AuthUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),   // IOptions<IdentityOptions>
            new Mock<IPasswordHasher<AuthUser>>().Object,
            new IUserValidator<AuthUser>[0],
            new IPasswordValidator<AuthUser>[0],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,  // IServiceProvider
            new Mock<ILogger<UserManager<AuthUser>>>().Object
        );

        var controller = new QuizController(mockRepo.Object, mockLogger.Object, mockUserManager.Object);

        // Mock authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        return controller;
    }
    // Test for getting all quizzes, commented out for now
    // [Fact]
    // public async Task GetQuizzes_ShouldReturnOk_WithListOfQuizDTOs()
    // {
    //     // Arrange
    //     var mockRepo = new Mock<IQuizRepository>();

    //     var quizzes = new List<Quiz>
    //     {
    //         new Quiz { Id = 1, Title = "Math"},
    //         new Quiz { Id = 2, Title = "Science"}
    //     };


    //     mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(quizzes);

    //     var mockLogger = new Mock<ILogger<QuizController>>();
    //     var quizController = new QuizController(mockRepo.Object, mockLogger.Object);

    //     // Act
    //     var result = await quizController.GetQuizzes();

    //     // Assert
    //     var okResult = result.Result as OkObjectResult;
    //     var value = Assert.IsAssignableFrom<IEnumerable<QuizDTO>>(okResult!.Value);

    //     // Verify the returned list
    //     Assert.Equal(2, value.Count());
    //     Assert.Equal("Math", value.First().Title);

    //     // Verify repository was called
    //     mockRepo.Verify(repo => repo.GetAll(), Times.Once);
    // }

    // [Fact]
    // public async Task GetQuizzes_ShouldReturnStatus500_WhenRepositoryThrows()
    // {
    //     // Arrange
    //     var mockRepo = new Mock<IQuizRepository>();
    //     mockRepo.Setup(repo => repo.GetAll())
    //                 .ThrowsAsync(new Exception("Database failure"));

    //     var mockLogger = new Mock<ILogger<QuizController>>();
    //     var controller = new QuizController(mockRepo.Object, mockLogger.Object);


    //     // Act
    //     var result = await controller.GetQuizzes();


    //     // Assert
    //     var statusResult = Assert.IsType<ObjectResult>(result.Result);
    //     Assert.Equal(500, statusResult.StatusCode);
    //     var message = Assert.IsType<string>(statusResult.Value);
    //     Assert.Contains("Database failure", message);

    //     mockRepo.Verify(repo => repo.GetAll(), Times.Once);
    // }

    [Fact]

    public async Task CreateQuiz_ShouldReturnCreatedAtActionResult_WithCorrectQuiz_WhenQuizIsCreated()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var mockLogger = new Mock<ILogger<QuizController>>();
        var controller = CreateController(mockRepo);

        var quizDto = new CreateQuizDTO
        {
            Title = "Geography",
            Questions = new List<CreateQuestionDTO>
        {
            new CreateQuestionDTO
            {
                QuestionText = "What is the capital of France?",
                Answers = new List<CreateAnswerDTO>
                {
                    new CreateAnswerDTO { AnswerText = "Paris", IsCorrect = true },
                    new CreateAnswerDTO { AnswerText = "London", IsCorrect = false }
                }
            }
        }
        };


        Quiz? capturedQuiz = null;

        // Setup repo: return true and capture the created quiz
        mockRepo.Setup(r => r.Create(It.IsAny<Quiz>()))
                .Callback<Quiz>(q =>
                {
                    capturedQuiz = q;
                    q.Id = 1; // set Id for questions
                              // Set Ids for questions and answers
                    if (q.Questions != null)
                    {
                        int qId = 1;
                        int aId = 1;
                        foreach (var question in q.Questions)
                        {
                            question.Id = qId++;
                            if (question.Answers != null)
                            {
                                foreach (var answer in question.Answers)
                                {
                                    answer.Id = aId++;
                                }
                            }
                        }
                    }
                })
                .ReturnsAsync(true);

        // Act
        var result = await controller.CreateQuiz(quizDto);

        // Assert
        // 1. Result should be CreatedAtActionResult
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        // 2. Result value should be a Quiz
        var returnedQuiz = Assert.IsType<QuizDTO>(createdAtActionResult.Value);

        // 3. Quiz should have correct content
        Assert.Equal(quizDto.Title, returnedQuiz.Title);

        // Questions
        Assert.Equal(quizDto.Questions.Count, returnedQuiz.Questions.Count);
        for (int i = 0; i < quizDto.Questions.Count; i++)
        {
            Assert.Equal(quizDto.Questions[i].QuestionText, returnedQuiz.Questions[i].QuestionText);

            // Answers
            Assert.Equal(quizDto.Questions[i].Answers.Count, returnedQuiz.Questions[i].Answers.Count);
            for (int j = 0; j < quizDto.Questions[i].Answers.Count; j++)
            {
                Assert.Equal(quizDto.Questions[i].Answers[j].AnswerText, returnedQuiz.Questions[i].Answers[j].AnswerText);
                Assert.Equal(quizDto.Questions[i].Answers[j].IsCorrect, returnedQuiz.Questions[i].Answers[j].IsCorrect);
            }
        }
        // 4. Repo Create should have been called once
        mockRepo.Verify(r => r.Create(It.IsAny<Quiz>()), Times.Once);

        // 5. Captured quiz should have correct title
        Assert.Equal(quizDto.Title, capturedQuiz!.Title);
    }

    // Negative test for CreateQuiz
    [Fact]
    public async Task CreateQuiz_ShouldReturn500_WhenReposFails()
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
        mockRepo.Setup(repo => repo.GetQuizById(42)).ReturnsAsync(quiz);


        // Act
        var result = await controller.GetQuiz(42);


        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quizDto = Assert.IsType<QuizDTO>(okResult.Value);

        Assert.Equal(42, quizDto.Id);
        Assert.Equal("History", quizDto.Title);

        mockRepo.Verify(repo => repo.GetQuizById(42), Times.Once);
    }

    // Negative test for read by id
    [Fact]
    public async Task GetQuizById_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);


        mockRepo.Setup(repo => repo.GetQuizById(42)).ReturnsAsync((Quiz?)null); // simulate not found


        // Act 
        var result = await controller.GetQuiz(42);


        // Assert
        Assert.IsType<NotFoundResult>(result.Result);


        // Verify that repository was called exactly once
        mockRepo.Verify(repo => repo.GetQuizById(42), Times.Once);
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

    // Negative test for update quiz
    [Fact]
    public async Task UpdateQuiz_ShouldReturnForbid_WhenUserDoesNotOwnQuiz()
    {
        // Arrange
        var mockRepo = new Mock<IQuizRepository>();
        var controller = CreateController(mockRepo);

        var quizDto = new QuizDTO
        {
            Id = 1,
            Title = "Updated Title"
        };

        // Quiz exists, but its OwnerId does NOT match authenticated user ("1")
        mockRepo.Setup(r => r.GetQuizById(1))
                .ReturnsAsync(new Quiz
                {
                    Id = 1,
                    OwnerId = "999"   // <-- Not owned by the logged-in user
                });

        // Act
        var result = await controller.UpdateQuiz(1, quizDto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        // Ensure Update() was never called
        mockRepo.Verify(r => r.Update(It.IsAny<Quiz>()), Times.Never);
    }


}
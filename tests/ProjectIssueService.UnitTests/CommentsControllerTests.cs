using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

using ProjectIssueService.Controllers;
using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;
using ProjectIssueService.RequestHelpers;
using ProjectIssueService.Services;

namespace ProjectIssueService.UnitTests;

public class CommentsControllerTests : BaseControllerTests
{
    private readonly Mock<ICommentRepository> _mockRepo;
    private readonly Mock<IIssueRepository> _mockIssueRepo;
    private readonly IMapper _mapper;
    private readonly Mock<IProjectAssignmentServices> _mockProjectAssignmentServices;
    private readonly CommentsController _controller;
    private readonly Mock<IUserService> _mockUserService;

    public CommentsControllerTests()
    {
        _mockRepo = new Mock<ICommentRepository>();
        _mockIssueRepo = new Mock<IIssueRepository>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _mockProjectAssignmentServices = new Mock<IProjectAssignmentServices>();
        _mockUserService = new Mock<IUserService>();

        _controller = new CommentsController(
            _mockRepo.Object,
            _mockIssueRepo.Object,
            _mapper,
            _mockProjectAssignmentServices.Object,
            _mockUserService.Object
        );
    }

    [Fact]
    public async Task GetComments_Valid_ReturnsComments()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new CommentParams()
        {
            IssueId = issueId
        };
        var expectedComments = new List<CommentDto>
        {
            new () { Id = Guid.NewGuid(), Content = "Content" },
            new () { Id = Guid.NewGuid(), Content = "Content" },
        };
        var issueDto = new IssueDto()
        {
            Name = "Issue"
        };

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockRepo
            .Setup(r => r.GetCommentsAsync(It.IsAny<CommentParams>()))
            .ReturnsAsync(expectedComments);

        // Act
        var result = await _controller.GetComments(parameters);

        // Assert
        Assert.Equal(expectedComments.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<CommentDto>>>(result);
    }

    [Fact]
    public async Task GetComments_IssueIdNotGiven_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new CommentParams();

        // Act
        var result = await _controller.GetComments(parameters);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetComments_IssueNotFound_ReturnsEmpty()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new CommentParams()
        {
            IssueId = issueId
        };
        IssueDto issueDto = null;

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);

        // Act
        var result = await _controller.GetComments(parameters);

        // Assert
        Assert.IsType<ActionResult<List<CommentDto>>>(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetComments_IssueNoAccess_ReturnsEmpty()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new CommentParams()
        {
            IssueId = issueId
        };
        var issueDto = new IssueDto()
        {
            Name = "Issue",
            ProjectId = Guid.NewGuid(),
        };

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetComments(parameters);

        // Assert
        Assert.IsType<ActionResult<List<CommentDto>>>(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetComment_Valid_ReturnsComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var issue = new Issue()
        {
            Name = "Issue",
            ProjectId = Guid.NewGuid(),
            CreatedBy = "System"
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            Id = commentId
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(comment);
        _mockIssueRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await _controller.GetComment(commentId);

        // Assert
        Assert.IsType<ActionResult<CommentDto>>(result);
        Assert.Equal(comment.Content, result.Value.Content);
        Assert.Equal(comment.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetComment_NotFound_NotFound()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        CommentDto comment = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(comment);

        // Act
        var result = await _controller.GetComment(commentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetComment_NoAccess_NotFound()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var issue = new Issue()
        {
            Name = "Issue",
            ProjectId = Guid.NewGuid(),
            CreatedBy = "System"
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            Id = commentId
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(comment);
        _mockIssueRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetComment(commentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateComment_Valid_CommentCreated()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var commentCreateDto = new CommentCreateDto()
        {
            Content = "Content",
            IssueId = issueId
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            IssueId = issueId,
            CreatedBy = "system"
        };
        var issueDto = new IssueDto()
        {
            Name = "Issue",
            Id = issueId,
            ProjectId = Guid.NewGuid(),
        };

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockRepo.Setup(r => r.AddComment(It.IsAny<Comment>()));
        _mockRepo.Setup(r => r.GetCommentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(comment);

        // Act
        var result = await _controller.CreateComment(commentCreateDto);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRepo.Verify(r => r.AddComment(It.IsAny<Comment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateComment_IssueNotFound_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var commentCreateDto = new CommentCreateDto()
        {
            Content = "Content",
            IssueId = issueId
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            IssueId = issueId,
            CreatedBy = "system"
        };
        IssueDto issueDto = null;

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);

        // Act
        var result = await _controller.CreateComment(commentCreateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateComment_IssueNoAccess_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var commentCreateDto = new CommentCreateDto()
        {
            Content = "Content",
            IssueId = issueId
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            IssueId = issueId,
            CreatedBy = "system"
        };
        var issueDto = new IssueDto()
        {
            Name = "Issue",
            Id = issueId,
            ProjectId = Guid.NewGuid(),
        };

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateComment(commentCreateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateComment_SaveFailed_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var commentCreateDto = new CommentCreateDto()
        {
            Content = "Content",
            IssueId = issueId
        };
        var comment = new CommentDto()
        {
            Content = "Content",
            IssueId = issueId,
            CreatedBy = "system"
        };
        var issueDto = new IssueDto()
        {
            Name = "Issue",
            Id = issueId,
            ProjectId = Guid.NewGuid(),
        };

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddComment(It.IsAny<Comment>()));

        // Act
        var result = await _controller.CreateComment(commentCreateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.AddComment(It.IsAny<Comment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateComment_Valid_Ok()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };
        var commentUpdateDto = new CommentUpdateDto()
        {
            Content = "New Content"
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("alice");
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateComment(commentId, commentUpdateDto);

        // Assert
        Assert.Equal(commentUpdateDto.Content, commentEntity.Content);
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateComment_NotFound_NotFound()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        Comment commentEntity = null;
        var commentUpdateDto = new CommentUpdateDto()
        {
            Content = "New Content"
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);

        // Act
        var result = await _controller.UpdateComment(commentId, commentUpdateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateComment_NoAccess_Forbid()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };
        var commentUpdateDto = new CommentUpdateDto()
        {
            Content = "New Content"
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("bob");

        // Act
        var result = await _controller.UpdateComment(commentId, commentUpdateDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommment_IsAdmin_CommentDeleted()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("bob");
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(true);
        _mockRepo.Setup(r => r.RemoveComment(It.IsAny<Comment>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.RemoveComment(It.IsAny<Comment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommment_IsOwner_CommentDeleted()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("alice");
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(false);
        _mockRepo.Setup(r => r.RemoveComment(It.IsAny<Comment>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.RemoveComment(It.IsAny<Comment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommment_NotFound_ReturnsNotFound()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        Comment commentEntity = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockRepo.Verify(r => r.RemoveComment(It.IsAny<Comment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommment_NoAccess_Forbid()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("bob");
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(false);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<ForbidResult>(result);
        _mockRepo.Verify(r => r.RemoveComment(It.IsAny<Comment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommment_SaveFailed_BadRequest()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var commentEntity = new Comment()
        {
            Content = "Content",
            CreatedBy = "alice",
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetCommentEntityById(It.IsAny<Guid>())).ReturnsAsync(commentEntity);
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("alice");
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(false);
        _mockRepo.Setup(r => r.RemoveComment(It.IsAny<Comment>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepo.Verify(r => r.RemoveComment(It.IsAny<Comment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
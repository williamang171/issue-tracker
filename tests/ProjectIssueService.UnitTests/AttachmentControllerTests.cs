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

public class AttachmentsControllerTests : BaseControllerTests
{
    private readonly Mock<IAttachmentRepository> _mockRepo;
    private readonly Mock<IIssueRepository> _mockIssueRepo;
    private readonly IMapper _mapper;
    private readonly Mock<IProjectAssignmentServices> _mockProjectAssignmentServices;
    private readonly AttachmentsController _controller;

    public AttachmentsControllerTests()
    {
        _mockRepo = new Mock<IAttachmentRepository>();
        _mockIssueRepo = new Mock<IIssueRepository>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _mockProjectAssignmentServices = new Mock<IProjectAssignmentServices>();

        _controller = new AttachmentsController(
            _mapper,
            _mockRepo.Object,
            _mockIssueRepo.Object,
            _mockProjectAssignmentServices.Object
        );
    }

    [Fact]
    public async Task GetAttachments_Valid_ReturnsAttachments()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new AttachmentParams()
        {
            IssueId = issueId
        };
        var expectedList = new List<AttachmentDto>
        {
            new () { Id = Guid.NewGuid(), PublicId = "1", Url = "sample", Name = "filename.png" },
            new () { Id = Guid.NewGuid(), PublicId = "2", Url = "sample", Name = "filename.png" },
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
            .Setup(r => r.GetAttachmentsAsync(It.IsAny<AttachmentParams>()))
            .ReturnsAsync(expectedList);

        // Act
        var result = await _controller.GetAttachments(parameters);

        // Assert
        Assert.Equal(expectedList.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<AttachmentDto>>>(result);
    }

    [Fact]
    public async Task GetAttachments_IssueIdNotGiven_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new AttachmentParams();

        // Act
        var result = await _controller.GetAttachments(parameters);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAttachments_IssueNotFound_ReturnsEmpty()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new AttachmentParams()
        {
            IssueId = issueId
        };
        IssueDto issueDto = null;

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);

        // Act
        var result = await _controller.GetAttachments(parameters);

        // Assert
        Assert.IsType<ActionResult<List<AttachmentDto>>>(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAttachments_IssueNoAccess_ReturnsEmpty()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var parameters = new AttachmentParams()
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
        var result = await _controller.GetAttachments(parameters);

        // Assert
        Assert.IsType<ActionResult<List<AttachmentDto>>>(result);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAttachment_Valid_ReturnsAttachment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var issueId = Guid.NewGuid();
        var issue = new Issue()
        {
            Name = "Issue",
            ProjectId = Guid.NewGuid(),
            CreatedBy = "System"
        };
        var attachment = new AttachmentDto()
        {
            Id = id,
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).ReturnsAsync(attachment);
        _mockIssueRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await _controller.GetAttachment(id);

        // Assert
        Assert.IsType<ActionResult<AttachmentDto>>(result);
        Assert.Equal(attachment.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetAttachment_NotFound_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        AttachmentDto dto = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetAttachment(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAttachment_NoAccess_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var issue = new Issue()
        {
            Id = Guid.NewGuid(),
            Name = "Issue",
            ProjectId = Guid.NewGuid(),
            CreatedBy = "System"
        };
        var comment = new AttachmentDto()
        {
            Id = id,
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issue.Id
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).ReturnsAsync(comment);
        _mockIssueRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetAttachment(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAttachment_Valid_AttachmentCreated()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var attachmentCreateDto = new AttachmentCreateDto()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
        };
        var attachment = new AttachmentDto()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
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
        _mockRepo.Setup(r => r.AddAttachment(It.IsAny<Attachment>()));
        _mockRepo.Setup(r => r.GetAttachmentById(It.IsAny<Guid>())).ReturnsAsync(attachment);

        // Act
        var result = await _controller.CreateAttachment(attachmentCreateDto);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRepo.Verify(r => r.AddAttachment(It.IsAny<Attachment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAttachment_IssueNotFound_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var attachmentCreateDto = new AttachmentCreateDto()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
        };
        IssueDto issueDto = null;

        // Setup mocks
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issueDto);

        // Act
        var result = await _controller.CreateAttachment(attachmentCreateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.AddAttachment(It.IsAny<Attachment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAttachment_IssueNoAccess_Forbid()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var attachmentCreateDto = new AttachmentCreateDto()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
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
        var result = await _controller.CreateAttachment(attachmentCreateDto);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        _mockRepo.Verify(r => r.AddAttachment(It.IsAny<Attachment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAttachment_SaveFailed_BadRequest()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var attachmentCreateDto = new AttachmentCreateDto()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            IssueId = issueId
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
        _mockRepo.Setup(r => r.AddAttachment(It.IsAny<Attachment>()));

        // Act
        var result = await _controller.CreateAttachment(attachmentCreateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.AddAttachment(It.IsAny<Attachment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAttachment_Valid_CommentDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Attachment()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            Id = id,
            CreatedBy = "System"
        };
        var issue = new IssueDto()
        {
            Name = "Issue",
            Id = Guid.NewGuid()
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.RemoveAttachment(It.IsAny<Attachment>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAttachment(id);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.RemoveAttachment(It.IsAny<Attachment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAttachment_NotFound_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        Attachment entity = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.DeleteAttachment(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockRepo.Verify(r => r.RemoveAttachment(It.IsAny<Attachment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAttachment_NoAccess_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Attachment()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            Id = id,
            CreatedBy = "System"
        };
        var issue = new IssueDto()
        {
            Name = "Issue",
            Id = Guid.NewGuid()
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAttachment(id);

        // Assert
        Assert.IsType<ForbidResult>(result);
        _mockRepo.Verify(r => r.RemoveAttachment(It.IsAny<Attachment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAttachment_SaveFailed_Forbid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new Attachment()
        {
            Name = "filename.png",
            PublicId = "1",
            Url = "test.com",
            Id = id,
            CreatedBy = "System"
        };
        var issue = new IssueDto()
        {
            Name = "Issue",
            Id = Guid.NewGuid()
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetAttachmentEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<Guid>())).ReturnsAsync(issue);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockRepo.Setup(r => r.RemoveAttachment(It.IsAny<Attachment>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAttachment(id);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepo.Verify(r => r.RemoveAttachment(It.IsAny<Attachment>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
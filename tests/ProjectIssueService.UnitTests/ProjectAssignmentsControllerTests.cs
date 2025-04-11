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
using ProjectIssueService.Data.Migrations;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;
using ProjectIssueService.RequestHelpers;
using ProjectIssueService.Services;

namespace ProjectIssueService.UnitTests;

public class ProjectAssignmentsControllerTests : BaseControllerTests
{
    private readonly Mock<IProjectAssignmentRepository> _mockRepo;
    private readonly Mock<IIssueRepository> _mockIssueRepo;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly IMapper _mapper;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IProjectAssignmentServices> _mockProjectAssignmentServices;
    private readonly Mock<IPublishBatchService> _mockPublishBatchService;
    private readonly ProjectAssignmentsController _controller;

    public ProjectAssignmentsControllerTests()
    {
        _mockRepo = new Mock<IProjectAssignmentRepository>();
        _mockIssueRepo = new Mock<IIssueRepository>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);

        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockProjectAssignmentServices = new Mock<IProjectAssignmentServices>();
        _mockPublishBatchService = new Mock<IPublishBatchService>();

        _controller = new ProjectAssignmentsController(
            _mockRepo.Object,
            _mockIssueRepo.Object,
            _mockProjectRepo.Object,
            _mockUserRepo.Object,
            _mapper,
            _mockPublishEndpoint.Object,
            _mockProjectAssignmentServices.Object,
            _mockPublishBatchService.Object
        );
    }

    [Fact]
    public async Task GetProjectAssignmentsPaginated_Valid_ReturnsProjectAssignments()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parameters = new ProjectAssignmentParams()
        {
            ProjectId = projectId
        };
        var expectedList = new List<ProjectAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), UserName = "alice", ProjectId = projectId },
                new() { Id = Guid.NewGuid(), UserName = "bob", ProjectId = projectId },
            };
        var expectedPagedList = new PagedList<ProjectAssignmentDto>(expectedList, 6, 1, 2);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup mocks
        _mockRepo.Setup(repo => repo.GetProjectAssignmentsPaginatedAsync(
            It.IsAny<ProjectAssignmentParams>()
             ))
        .ReturnsAsync(expectedPagedList);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectAssignmentsPaginated(parameters);

        // Assert
        Assert.Equal(expectedPagedList.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectAssignmentDto>>>(result);
        Assert.Equal("6", _controller.HttpContext.Response.Headers["x-total-count"]);
        _mockRepo.Verify(repo => repo.GetProjectAssignmentsPaginatedAsync(parameters), Times.Once);
    }

    [Fact]
    public async Task GetProjectAssignmentsPaginated_NoProjectId_ReturnsEmpty()
    {
        // Arrange
        var parameters = new ProjectAssignmentParams();

        // Act
        var result = await _controller.GetProjectAssignmentsPaginated(parameters);

        // Assert
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProjectAssignmentsPaginated_NoAccess_ReturnsEmpty()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parameters = new ProjectAssignmentParams()
        {
            ProjectId = projectId
        };
        var expectedList = new List<ProjectAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), UserName = "alice", ProjectId = projectId },
                new() { Id = Guid.NewGuid(), UserName = "bob", ProjectId = projectId },
            };
        var expectedPagedList = new PagedList<ProjectAssignmentDto>(expectedList, 6, 1, 2);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup mocks
        _mockRepo
            .Setup(repo => repo.GetProjectAssignmentsPaginatedAsync(
            It.IsAny<ProjectAssignmentParams>()))
            .ReturnsAsync(expectedPagedList);
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetProjectAssignmentsPaginated(parameters);

        // Assert
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProjectAssignments_Valid_ReturnsProjectAssignments()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parameters = new ProjectAssignmentParams()
        {
            ProjectId = projectId
        };
        var expectedList = new List<ProjectAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), UserName = "alice", ProjectId = projectId },
                new() { Id = Guid.NewGuid(), UserName = "bob", ProjectId = projectId },
            };
        var expectedPagedList = new PagedList<ProjectAssignmentDto>(expectedList, 6, 1, 2);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup mocks
        _mockRepo.Setup(repo => repo.GetProjectAssignmentsAsync(
            It.IsAny<ProjectAssignmentParams>()
             ))
        .ReturnsAsync(expectedPagedList);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectAssignments(parameters);

        // Assert
        Assert.Equal(expectedPagedList.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectAssignmentDto>>>(result);
        _mockRepo.Verify(repo => repo.GetProjectAssignmentsAsync(parameters), Times.Once);
    }

    [Fact]
    public async Task GetProjectAssignments_NoProjectId_ReturnsEmpty()
    {
        // Arrange
        var parameters = new ProjectAssignmentParams();

        // Act
        var result = await _controller.GetProjectAssignments(parameters);

        // Assert
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProjectAssignments_NoAccess_ReturnsEmpty()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var parameters = new ProjectAssignmentParams()
        {
            ProjectId = projectId
        };
        var expectedList = new List<ProjectAssignmentDto>
            {
                new() { Id = Guid.NewGuid(), UserName = "alice", ProjectId = projectId },
                new() { Id = Guid.NewGuid(), UserName = "bob", ProjectId = projectId },
            };

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup mocks
        _mockRepo
            .Setup(repo => repo.GetProjectAssignmentsAsync(
            It.IsAny<ProjectAssignmentParams>()))
            .ReturnsAsync(expectedList);
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetProjectAssignments(parameters);

        // Assert
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProjectAssignmentById_Valid_ReturnProjectAssignment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProjectAssignmentDto()
        {
            UserName = "alice",
            ProjectId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(dto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectAssignmentById(id);

        // Assert
        Assert.IsType<ActionResult<ProjectAssignmentDto>>(result);
        Assert.Equal("alice", result.Value.UserName);
        _mockRepo.Verify(r => r.GetProjectAssignmentByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectAssignmentById_NotFound_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        ProjectAssignmentDto dto = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(dto);

        // Act
        var result = await _controller.GetProjectAssignmentById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProjectAssignmentById_NoAccess_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProjectAssignmentDto()
        {
            UserName = "alice",
            ProjectId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(dto);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.GetProjectAssignmentById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateBulkProjectAssignments_Valid_RecordsCreated()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var dto = new BulkProjectAssignmentCreateDto()
        {
            ProjectId = projectId,
            UserNames =
            [
                "alice",
                "bob",
                "charlie",
                "daniel"
            ]
        };
        var projectDto = new ProjectDto()
        {
            Name = "Project 1",
            Id = projectId
        };
        var project = new Project()
        {
            Name = "Project 1",
            Id = projectId,
            CreatedBy = "System"
        };
        var projectAssignments = new List<ProjectAssignment>
        {
            new ProjectAssignment()
            {
                Id = Guid.NewGuid(),
                CreatedBy = "System",
                ProjectId = projectId,
                UserName = "alice",
                Project = project
            },
        };
        var user = new User()
        {
            CreatedBy = "System",
            UserName = "alice"
        };

        // Setup mocks
        _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>())).ReturnsAsync(projectDto);
        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync((string userName) =>
        {
            if (userName == "daniel")
            {
                return null;
            }
            return user;
        });
        _mockRepo.Setup(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()));
        _mockRepo.Setup(r => r.GetProjectAssignmentsByProjectIdAsync(It.IsAny<Guid>())).ReturnsAsync(projectAssignments);
        _mockPublishBatchService.Setup(p => p.PublishBatchProjectAssignments(It.IsAny<List<ProjectAssignmentCreated>>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateBulkProjectAssignments(dto);

        // Assert
        Assert.IsType<ActionResult<BulkProjectAssignmentResultDto>>(result);
        _mockRepo.Verify(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()), Times.Exactly(2));
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishBatchService.Verify(r => r.PublishBatchProjectAssignments(It.IsAny<List<ProjectAssignmentCreated>>()), Times.Once);
    }

    [Fact]
    public async Task CreateBulkProjectAssignments_ProjectNotFound_BadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var dto = new BulkProjectAssignmentCreateDto()
        {
            ProjectId = projectId,
            UserNames =
            [
                "alice",
                "bob",
                "charlie",
                "daniel"
            ]
        };
        ProjectDto projectDto = null;

        // Setup mocks
        _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>())).ReturnsAsync(projectDto);

        // Act
        var result = await _controller.CreateBulkProjectAssignments(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishBatchService.Verify(r => r.PublishBatchProjectAssignments(It.IsAny<List<ProjectAssignmentCreated>>()), Times.Never);
    }

    [Fact]
    public async Task CreateBulkProjectAssignments_NoUsersAssigned_BadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var dto = new BulkProjectAssignmentCreateDto()
        {
            ProjectId = projectId,
            UserNames = []
        };
        var projectDto = new ProjectDto()
        {
            Name = "Project 1",
            Id = projectId
        };

        // Setup mocks
        _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>())).ReturnsAsync(projectDto);

        // Act
        var result = await _controller.CreateBulkProjectAssignments(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishBatchService.Verify(r => r.PublishBatchProjectAssignments(It.IsAny<List<ProjectAssignmentCreated>>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectAssignment_Valid_Ok()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectAssignmentId = Guid.NewGuid();
        var project = new Project()
        {
            CreatedBy = "System",
            Id = projectId,
            Name = "Project 1"
        };
        var projectAssignment = new ProjectAssignment()
        {
            CreatedBy = "System",
            Id = projectAssignmentId,
            UserName = "bob",
            ProjectId = projectId,
            Project = project
        };
        var issuesAssignedToUser = new List<Issue>();
        var issue1 = new Issue()
        {
            CreatedBy = "System",
            Name = "Issue 1",
            Assignee = "bob"
        };
        var issue2 = new Issue()
        {
            CreatedBy = "System",
            Name = "Issue 2",
            Assignee = "bob"
        };
        issuesAssignedToUser.Add(issue1);
        issuesAssignedToUser.Add(issue2);

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentEntityById(It.IsAny<Guid>())).ReturnsAsync(projectAssignment);
        _mockRepo.Setup(r => r.RemoveProjectAssignment(It.IsAny<ProjectAssignment>()));
        _mockIssueRepo.Setup(r => r.GetIssueEntitiesAssignedToUser(It.IsAny<string>())).ReturnsAsync(issuesAssignedToUser);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectAssignmentDeleted>(), default));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProjectAssignment(projectAssignmentId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.RemoveProjectAssignment(It.IsAny<ProjectAssignment>()), Times.Once);
        Assert.Null(issue1.Assignee);
        Assert.Null(issue2.Assignee);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectAssignmentDeleted>(), default), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAssignment_NotFound_NotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectAssignmentId = Guid.NewGuid();
        var project = new Project()
        {
            CreatedBy = "System",
            Id = projectId,
            Name = "Project 1"
        };
        ProjectAssignment projectAssignment = null;

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentEntityById(It.IsAny<Guid>())).ReturnsAsync(projectAssignment);

        // Act
        var result = await _controller.DeleteProjectAssignment(projectAssignmentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectAssignmentDeleted>(), default), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectAssignment_SaveFailed_BadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var projectAssignmentId = Guid.NewGuid();
        var project = new Project()
        {
            CreatedBy = "System",
            Id = projectId,
            Name = "Project 1"
        };
        var projectAssignment = new ProjectAssignment()
        {
            CreatedBy = "System",
            Id = projectAssignmentId,
            UserName = "bob",
            ProjectId = projectId,
            Project = project
        };
        var issuesAssignedToUser = new List<Issue>();
        var issue1 = new Issue()
        {
            CreatedBy = "System",
            Name = "Issue 1",
            Assignee = "bob"
        };
        var issue2 = new Issue()
        {
            CreatedBy = "System",
            Name = "Issue 2",
            Assignee = "bob"
        };
        issuesAssignedToUser.Add(issue1);
        issuesAssignedToUser.Add(issue2);

        // Setup mocks
        _mockRepo.Setup(r => r.GetProjectAssignmentEntityById(It.IsAny<Guid>())).ReturnsAsync(projectAssignment);
        _mockRepo.Setup(r => r.RemoveProjectAssignment(It.IsAny<ProjectAssignment>()));
        _mockIssueRepo.Setup(r => r.GetIssueEntitiesAssignedToUser(It.IsAny<string>())).ReturnsAsync(issuesAssignedToUser);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectAssignmentDeleted>(), default));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteProjectAssignment(projectAssignmentId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepo.Verify(r => r.RemoveProjectAssignment(It.IsAny<ProjectAssignment>()), Times.Once);
        Assert.Null(issue1.Assignee);
        Assert.Null(issue2.Assignee);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectAssignmentDeleted>(), default), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
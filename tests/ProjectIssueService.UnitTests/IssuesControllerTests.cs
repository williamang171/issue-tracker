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

public class IssuesControllerTests : BaseControllerTests
{
    private readonly Mock<IIssueRepository> _mockRepo;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly IMapper _mapper;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IProjectAssignmentServices> _mockProjectAssignmentServices;
    private readonly Mock<IUserService> _mockUserService;
    private readonly IssuesController _controller;

    public IssuesControllerTests()
    {
        _mockRepo = new Mock<IIssueRepository>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);

        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockProjectAssignmentServices = new Mock<IProjectAssignmentServices>();
        _mockUserService = new Mock<IUserService>();
        _controller = new IssuesController(
            _mockRepo.Object,
            _mockProjectRepo.Object,
            _mockUserRepo.Object,
            _mapper,
            _mockPublishEndpoint.Object,
            _mockProjectAssignmentServices.Object,
            _mockUserService.Object
        );
    }

    [Fact]
    public async Task GetIssues_WithAdminRole_ReturnsIssues()
    {
        // Arrange
        var userName = "testAdmin";
        var parameters = new IssueParams();
        var expectedItemsForAdmin = new List<IssueDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Issue 1" },
                new() { Id = Guid.NewGuid(), Name = "Issue 2" },
            };
        var expectedPagedItemsForAdmin = new PagedList<IssueDto>(expectedItemsForAdmin, 4, 1, 3);
        var expectedItemsForUser = new List<IssueDto>();
        var expectedPagedItemsForUser = new PagedList<IssueDto>(expectedItemsForUser, 0, 1, 2);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetIssuesPaginatedAsync(
            It.IsAny<IssueParams>(),
            It.IsAny<string>()))
        .ReturnsAsync(expectedPagedItemsForUser);
        _mockRepo.Setup(repo => repo.GetIssuesPaginatedAsync(
                It.IsAny<IssueParams>(),
                null))
        .ReturnsAsync(expectedPagedItemsForAdmin);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(true);

        // Act
        var result = await _controller.GetIssues(parameters);

        // Assert
        Assert.Equal(expectedPagedItemsForAdmin.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<IssueDto>>>(result);
        Assert.Equal("4", _controller.HttpContext.Response.Headers["x-total-count"]);
        _mockRepo.Verify(repo => repo.GetIssuesPaginatedAsync(parameters, null), Times.Once);
        _mockRepo.Verify(repo => repo.GetIssuesPaginatedAsync(parameters, userName), Times.Never);
    }

    [Fact]
    public async Task GetIssues_WithNonAdminRole_ReturnsUserIssues()
    {
        // Arrange
        var userName = "testAdmin";
        var parameters = new IssueParams();
        var expectedItemsForAdmin = new List<IssueDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Issue 1" },
                new() { Id = Guid.NewGuid(), Name = "Issue 2" },
            };
        var expectedPagedItemsForAdmin = new PagedList<IssueDto>(expectedItemsForAdmin, 4, 1, 3);
        var expectedItemsForUser = new List<IssueDto>()
        {
            new() { Id = Guid.NewGuid(), Name = "Issue 1" },
        };
        var expectedPagedItemsForUser = new PagedList<IssueDto>(expectedItemsForUser, 2, 1, 1);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetIssuesPaginatedAsync(
            It.IsAny<IssueParams>(),
            It.IsAny<string>()))
        .ReturnsAsync(expectedPagedItemsForUser);
        _mockRepo.Setup(repo => repo.GetIssuesPaginatedAsync(
                It.IsAny<IssueParams>(),
                null))
        .ReturnsAsync(expectedPagedItemsForAdmin);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(false);

        // Act
        var result = await _controller.GetIssues(parameters);

        // Assert
        Assert.Equal(expectedPagedItemsForUser.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<IssueDto>>>(result);
        Assert.Equal("2", _controller.HttpContext.Response.Headers["x-total-count"]);
        _mockRepo.Verify(repo => repo.GetIssuesPaginatedAsync(parameters, null), Times.Never);
        _mockRepo.Verify(repo => repo.GetIssuesPaginatedAsync(parameters, userName), Times.Once);
    }

    [Fact]
    public async Task GetIssues_WithNoUserName_ReturnsEmptyList()
    {
        // Arrange
        var parameters = new IssueParams();

        // Setup HttpContext
        SetupHttpContext(_controller);

        // Act
        var result = await _controller.GetIssues(parameters);

        // Assert
        Assert.Empty(result.Value);
        _mockRepo.Verify(repo => repo.GetIssuesPaginatedAsync(
            It.IsAny<IssueParams>(),
            It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetIssueById_ValidCase_ReturnsIssue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mockDto = new IssueDto()
        {
            Name = "Issue 1",
            Id = id,
        };

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetIssueByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockDto);

        // Setup mock project assignment service
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetIssue(id);

        // Assert
        Assert.IsType<ActionResult<IssueDto>>(result);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Issue 1", result.Value.Name);
    }

    [Fact]
    public async Task GetIssueById_IssueNotExists_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        IssueDto mockIssue = null;

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetIssueByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockIssue);

        // Act
        var result = await _controller.GetIssue(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetIssueById_NoAccess_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mockIssue = new IssueDto()
        {
            Name = "Issue 1",
            Id = id,
        };

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetIssueByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockIssue);

        // Setup mock project assignment service
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetIssue(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateIssue_Valid_IssueCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System" };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);
        _mockRepo.Setup(r => r.AddIssue(It.IsAny<Issue>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetIssueByIdAsync(entity.Id)).ReturnsAsync(issueDto);

        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(s => s.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);

        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateIssue(dto);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.IsType<CreatedAtActionResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateIssue_ProjectNotFound_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        Project project = null;

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);

        // Act
        var result = await _controller.CreateIssue(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateIssue_NoAccessToProject_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(r => r.CanCurrentUserAccessProject(projectId)).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateIssue(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateIssue_AssigneeNotExists_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        User user = null;

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(r => r.CanCurrentUserAccessProject(projectId)).ReturnsAsync(true);

        // Setup userRepo mocks
        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _controller.CreateIssue(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateIssue_AssigneeNotAssignedToProject_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(r => r.CanCurrentUserAccessProject(projectId)).ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(r => r.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(false);

        // Setup userRepo mocks
        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _controller.CreateIssue(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateIssue_SaveFailed_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueCreateDto { Name = "New Issue", Assignee = "alice", ProjectId = projectId };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System" };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);
        _mockRepo.Setup(r => r.AddIssue(It.IsAny<Issue>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        _mockRepo.Setup(r => r.GetIssueByIdAsync(entity.Id)).ReturnsAsync(issueDto);

        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(s => s.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);

        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateIssue(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);

        _mockRepo.Verify(r => r.AddIssue(It.IsAny<Issue>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateIssue_Valid_Ok()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto
        {
            Name = "New Issue Updated",
            Assignee = "bob",
            Description = "Description updated",
            Priority = Entities.IssuePriority.Critical,
            Status = Entities.IssueStatus.InProgress,
            Type = Entities.IssueType.FeatureRequest,
        };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetIssueByIdAsync(entity.Id)).ReturnsAsync(issueDto);

        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(s => s.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);

        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.Description, entity.Description);
        Assert.Equal(dto.Status, entity.Status);
        Assert.Equal(dto.Type, entity.Type);
        Assert.Equal(dto.Priority, entity.Priority);
        Assert.Equal(dto.Assignee, entity.Assignee);
        Assert.IsType<OkResult>(result);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateIssue_IssueNotExists_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        Issue entity = null;
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);


        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateIssue_NoAccess_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateIssue_AssigneeNotFound_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        User user = null;

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);

        // Setup userRepo mocks
        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateIssue_AssigneeNotAssignedToProject_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);

        // Setup projectAssignmentServices mocks
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(s => s.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(false);

        // Setup userRepo mocks
        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateIssue_SaveFailed_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockProjectRepo.Setup(r => r.GetProjectEntityById(It.IsAny<Guid>())).ReturnsAsync(project);
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        _mockRepo.Setup(r => r.GetIssueByIdAsync(entity.Id)).ReturnsAsync(issueDto);

        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockProjectAssignmentServices.Setup(s => s.IsUserAssignedToProject(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);

        _mockUserRepo.Setup(r => r.GetUserEntityByUserName(It.IsAny<string>())).ReturnsAsync(user);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateIssue(id, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueUpdated>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteIssue_Valid_Ok()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(true);
        _mockRepo.Setup(r => r.RemoveIssue(It.IsAny<Issue>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueDeleted>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteIssue(id);

        // Assert
        Assert.IsType<OkResult>(result);

        _mockRepo.Verify(r => r.RemoveIssue(It.IsAny<Issue>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueDeleted>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteIssue_NotFound_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        Issue entity = null;
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);

        // Act
        var result = await _controller.DeleteIssue(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _mockRepo.Verify(r => r.RemoveIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueDeleted>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteIssue_NoAccess_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteIssue(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _mockRepo.Verify(r => r.RemoveIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueDeleted>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteIssue_CannotDelete_Forbid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(false);

        // Act
        var result = await _controller.DeleteIssue(id);

        // Assert
        Assert.IsType<ForbidResult>(result);

        _mockRepo.Verify(r => r.RemoveIssue(It.IsAny<Issue>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueDeleted>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteIssue_SaveFailed_BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var dto = new IssueUpdateDto { Name = "New Issue", Assignee = "alice" };
        var entity = new Issue { Id = id, Name = "New Issue", CreatedBy = "System", ProjectId = projectId };
        var issueDto = new IssueDto { Id = id, Name = "New Issue" };
        var project = new Project { Id = projectId, Name = "Sample", CreatedBy = "System" };
        var user = new User { UserName = "user", CreatedBy = "System" };

        // Setup mocks
        _mockRepo.Setup(r => r.GetIssueEntityById(It.IsAny<Guid>())).ReturnsAsync(entity);
        _mockProjectAssignmentServices.Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>())).ReturnsAsync(true);
        _mockUserService.Setup(s => s.CurrentUserRoleIsAdmin()).Returns(true);
        _mockRepo.Setup(r => r.RemoveIssue(It.IsAny<Issue>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<IssueDeleted>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteIssue(id);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);

        _mockRepo.Verify(r => r.RemoveIssue(It.IsAny<Issue>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<IssueDeleted>(), default), Times.Once);
    }
}

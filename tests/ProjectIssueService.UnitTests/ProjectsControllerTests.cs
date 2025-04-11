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

public class ProjectsControllerTests : BaseControllerTests
{
    private readonly Mock<IProjectRepository> _mockRepo;
    private readonly Mock<IProjectAssignmentRepository> _mockProjectAssignmentRepo;
    private readonly IMapper _mapper;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IProjectAssignmentServices> _mockProjectAssignmentServices;
    private readonly Mock<IUserService> _mockUserService;
    private readonly ProjectsController _controller;

    public ProjectsControllerTests()
    {
        _mockRepo = new Mock<IProjectRepository>();
        _mockProjectAssignmentRepo = new Mock<IProjectAssignmentRepository>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);

        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockProjectAssignmentServices = new Mock<IProjectAssignmentServices>();
        _mockUserService = new Mock<IUserService>();
        _controller = new ProjectsController(
            _mockRepo.Object,
            _mockProjectAssignmentRepo.Object,
            _mapper,
            _mockPublishEndpoint.Object,
            _mockProjectAssignmentServices.Object,
            _mockUserService.Object
        );
    }

    [Fact]
    public async Task GetProjects_WithAdminRole_ReturnsProjects()
    {
        // Arrange
        var userName = "testAdmin";
        var parameters = new ProjectParams();
        var expectedProjectsForAdmin = new List<ProjectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1" },
                new() { Id = Guid.NewGuid(), Name = "Project 2" },
                new() { Id = Guid.NewGuid(), Name = "Project 3" },
            };
        var expectedProjectsPagedListForAdmin = new PagedList<ProjectDto>(expectedProjectsForAdmin, 6, 1, 3);
        var expectedProjectsForUser = new List<ProjectDto>();
        var expectedProjectsPagedListForUser = new PagedList<ProjectDto>(expectedProjectsForUser, 0, 1, 2);

        // Setup HttpContext
        var mockHttpContext = SetupHttpContext(_controller);

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetProjectsPaginatedAsync(
            It.IsAny<ProjectParams>(),
            It.IsAny<string>()))
        .ReturnsAsync(expectedProjectsPagedListForUser);
        _mockRepo.Setup(repo => repo.GetProjectsPaginatedAsync(
                It.IsAny<ProjectParams>(),
                null))
        .ReturnsAsync(expectedProjectsPagedListForAdmin);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(true);

        // Act
        var result = await _controller.GetProjects(parameters);

        // Assert
        Assert.Equal(expectedProjectsPagedListForAdmin.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectDto>>>(result);
        Assert.Equal("6", _controller.HttpContext.Response.Headers["x-total-count"]);
        _mockRepo.Verify(repo => repo.GetProjectsPaginatedAsync(parameters, null), Times.Once);
        _mockRepo.Verify(repo => repo.GetProjectsPaginatedAsync(parameters, userName), Times.Never);
    }

    [Fact]
    public async Task GetProjects_WithNonAdminRole_ReturnsUserProjects()
    {
        // Arrange
        var userName = "testUser";
        var parameters = new ProjectParams();
        var expectedProjects = new List<ProjectDto>
        {
            new ProjectDto { Id = Guid.NewGuid(), Name = "User Project" }
        };
        var expectedProjectsPagedList = new PagedList<ProjectDto>(expectedProjects, 3, 1, 1);

        // Setup HttpContext
        SetupHttpContext(_controller);

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetProjectsPaginatedAsync(
                It.IsAny<ProjectParams>(),
                userName))
            .ReturnsAsync(expectedProjectsPagedList);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(false);

        // Act
        var result = await _controller.GetProjects(parameters);

        // Assert
        Assert.Equal(expectedProjects.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectDto>>>(result);
        Assert.Equal("3", _controller.HttpContext.Response.Headers["x-total-count"]);
        _mockRepo.Verify(repo => repo.GetProjectsPaginatedAsync(parameters, userName), Times.Once);
        _mockRepo.Verify(repo => repo.GetProjectsPaginatedAsync(parameters, null), Times.Never);
    }

    [Fact]
    public async Task GetProjects_WithNoUserName_ReturnsEmptyList()
    {
        // Arrange
        var parameters = new ProjectParams();

        // Setup HttpContext
        SetupHttpContext(_controller);

        // Act
        var result = await _controller.GetProjects(parameters);

        // Assert
        Assert.Empty(result.Value);
        _mockRepo.Verify(repo => repo.GetProjectsPaginatedAsync(
            It.IsAny<ProjectParams>(),
            It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProjectById_ValidCase_ReturnsProject()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mockProject = new ProjectDto()
        {
            Name = "Project 1",
            Id = id,
        };

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetProjectByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockProject);

        // Setup mock project assignment service
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectById(id);

        // Assert
        Assert.IsType<ActionResult<ProjectDto>>(result);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Project 1", result.Value.Name);
    }

    [Fact]
    public async Task GetProjectById_ProjectNotExists_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        ProjectDto mockProject = null;

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetProjectByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockProject);

        // Act
        var result = await _controller.GetProjectById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProjectById_NoAccess_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mockProject = new ProjectDto()
        {
            Name = "Project 1",
            Id = id,
        };

        // Setup mock repo
        _mockRepo
            .Setup(repo => repo.GetProjectByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mockProject);

        // Setup mock project assignment service
        _mockProjectAssignmentServices
            .Setup(s => s.CanCurrentUserAccessProject(It.IsAny<Guid>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetProjectById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProjectsAll_WithAdminRole_ReturnsProjects()
    {
        // Arrange
        var userName = "testAdmin";
        var expectedProjects = new List<ProjectForSelectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1" },
                new() { Id = Guid.NewGuid(), Name = "Project 2" },
            };

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetProjectsForSelectAsync(null))
            .ReturnsAsync(expectedProjects);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(true);

        // Act
        var result = await _controller.GetProjectsAll();

        // Assert
        Assert.Equal(expectedProjects.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectForSelectDto>>>(result);
        _mockRepo.Verify(repo => repo.GetProjectsForSelectAsync(null), Times.Once);
        _mockRepo.Verify(repo => repo.GetProjectsForSelectAsync(userName), Times.Never);
    }

    [Fact]
    public async Task GetProjectsAll_WithNonAdminRole_ReturnsProjects()
    {
        // Arrange
        var userName = "testUser";
        var expectedProjects = new List<ProjectForSelectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1" },
            };

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetProjectsForSelectAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedProjects);

        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns(userName);
        _mockUserService.Setup(svc => svc.CurrentUserRoleIsAdmin()).Returns(false);

        // Act
        var result = await _controller.GetProjectsAll();

        // Assert
        Assert.Equal(expectedProjects.Count, result?.Value.Count);
        Assert.IsType<ActionResult<List<ProjectForSelectDto>>>(result);
        _mockRepo.Verify(repo => repo.GetProjectsForSelectAsync(userName), Times.Once);
        _mockRepo.Verify(repo => repo.GetProjectsForSelectAsync(null), Times.Never);
    }

    [Fact]
    public async Task GetProjectsAll_UserNameNotExists_ReturnsEmpty()
    {
        // Arrange
        var expectedProjects = new List<ProjectForSelectDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1" },
            };

        // Setup repository mock
        _mockRepo.Setup(repo => repo.GetProjectsForSelectAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedProjects);

        // Arrange
        // Setup userService mock
        _mockUserService.Setup(svc => svc.GetCurrentUserName()).Returns("");

        // Act
        var result = await _controller.GetProjectsAll();

        // Assert
        Assert.Empty(result.Value);
        _mockRepo.Verify(repo => repo.GetProjectsForSelectAsync(
            It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateProject_Valid_ProjectCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProjectCreateDto { Name = "New Project", Description = "Test description" };
        var projectEntity = new Project { Id = id, Name = "New Project", Description = "Test description", CreatedBy = "System" };
        var projectDto = new ProjectDto { Id = id, Name = "New Project", Description = "Test description" };

        SetupHttpContext(_controller);

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.AddProject(It.IsAny<Project>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);
        _mockRepo.Setup(r => r.GetProjectByIdAsync(projectEntity.Id)).ReturnsAsync(projectDto);

        _mockProjectAssignmentRepo.Setup(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()));
        _mockProjectAssignmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectCreated>(), default)).Returns(Task.CompletedTask);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectAssignmentCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProject(dto);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.IsType<CreatedAtActionResult>(result.Result);

        _mockRepo.Verify(r => r.AddProject(It.IsAny<Project>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectCreated>(), default), Times.Once);
        _mockProjectAssignmentRepo.Verify(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()), Times.Once);
        _mockProjectAssignmentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectAssignmentCreated>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateProject_Failure_ProjectNotCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ProjectCreateDto { Name = "New Project", Description = "Test description" };
        var projectEntity = new Project { Id = id, Name = "New Project", Description = "Test description", CreatedBy = "System" };
        var projectDto = new ProjectDto { Id = id, Name = "New Project", Description = "Test description" };

        SetupHttpContext(_controller);

        // Setup userService mocks
        _mockUserService.Setup(s => s.GetCurrentUserName()).Returns("user");

        // Setup repo mocks
        _mockRepo.Setup(r => r.AddProject(It.IsAny<Project>()));
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);
        _mockRepo.Setup(r => r.GetProjectByIdAsync(projectEntity.Id)).ReturnsAsync(projectDto);

        _mockProjectAssignmentRepo.Setup(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()));
        _mockProjectAssignmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Setup publish endpoint mock
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectCreated>(), default)).Returns(Task.CompletedTask);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectAssignmentCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateProject(dto);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepo.Verify(r => r.AddProject(It.IsAny<Project>()), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectCreated>(), default), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockProjectAssignmentRepo.Verify(r => r.AddProjectAssignment(It.IsAny<ProjectAssignment>()), Times.Never);
        _mockProjectAssignmentRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectAssignmentCreated>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateProject_ProjectExists_UpdatesAndReturnsOk()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = "System"
        };

        var updateDto = new ProjectUpdateDto
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        SetupHttpContext(_controller);

        // Setup repo mock
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync(existingProject);
        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProject(projectId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);

        // Verify project was updated with new values
        Assert.Equal(updateDto.Name, existingProject.Name);
        Assert.Equal(updateDto.Description, existingProject.Description);

        // Verify repository methods were called
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_ProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var updateDto = new ProjectUpdateDto
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        SetupHttpContext(_controller);

        // Setup repo mock to return null (project not found)
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync((Project)null);

        // Act
        var result = await _controller.UpdateProject(projectId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        // Verify repository method was called but save was not
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateProject_SaveChangesFails_ReturnsBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Original Name",
            Description = "Original Description",
            CreatedBy = "System"
        };

        var updateDto = new ProjectUpdateDto
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        SetupHttpContext(_controller);

        // Setup repo mock
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync(existingProject);

        // Setup SaveChangesAsync to return false (failure)
        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateProject(projectId, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Problem saving changes", badRequestResult.Value);

        // Verify repository methods were called
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_ProjectExists_DeletesAndReturnsOk()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Project to Delete",
            Description = "This project will be deleted",
            CreatedBy = "System"
        };

        var projectDto = new ProjectDto
        {
            Id = projectId,
            Name = "Project to Delete",
            Description = "This project will be deleted"
        };

        var projectDeleted = new ProjectDeleted
        {
            Id = projectId,
            Name = "Project to Delete"
        };

        SetupHttpContext(_controller);

        // Setup repo mock
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync(existingProject);
        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Setup publish endpoint
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectDeleted>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);

        // Verify repository methods were called
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.RemoveProject(existingProject), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // Verify messages were published
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectDeleted>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_ProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        SetupHttpContext(_controller);

        // Setup repo mock to return null (project not found)
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync((Project)null);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        // Verify repository method was called but save was not
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.RemoveProject(It.IsAny<Project>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);

        // Verify no messages were published
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectDeleted>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteProject_SaveChangesFails_ReturnsBadRequest()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            Name = "Project to Delete",
            Description = "This project will be deleted",
            CreatedBy = "System"
        };

        var projectDto = new ProjectDto
        {
            Id = projectId,
            Name = "Project to Delete",
            Description = "This project will be deleted"
        };

        var projectDeleted = new ProjectDeleted
        {
            Id = projectId,
            Name = "Project to Delete"
        };

        // Setup HttpContext with admin role
        SetupHttpContext(_controller);

        // Setup repo mock
        _mockRepo.Setup(r => r.GetProjectEntityById(projectId))
            .ReturnsAsync(existingProject);

        // Setup SaveChangesAsync to return false (failure)
        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(false);


        // Setup publish endpoint
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProjectDeleted>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Could not update DB", badRequestResult.Value);

        // Verify repository methods were called
        _mockRepo.Verify(r => r.GetProjectEntityById(projectId), Times.Once);
        _mockRepo.Verify(r => r.RemoveProject(existingProject), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // Verify message was published even though save failed
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProjectDeleted>(), default), Times.Once);
    }
}
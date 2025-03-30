using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;
using ProjectIssueService.Extensions;
using ProjectIssueService.Services;
using MassTransit;
using Contracts;

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectAssignmentsController(
    IProjectAssignmentRepository repo,
    IIssueRepository issueRepo,
    IProjectRepository projectRepo,
    IUserRepository userRepo,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IProjectAssignmentServices projectAssignmentServices
    )
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProjectAssignmentDto>>> GetProjectAssignmentsPaginated([FromQuery] ProjectAssignmentParams parameters)
    {
        // If no projectId is given, return an empty list
        if (parameters.ProjectId == null) return new List<ProjectAssignmentDto>();

        // If user is not assigned to project and is not an admin, return an empty list
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(parameters.ProjectId);
        if (!hasAccess) return new List<ProjectAssignmentDto>();

        var response = await repo.GetProjectAssignmentsPaginatedAsync(parameters);
        Response.AddPaginationHeader(response.TotalCount);
        return response;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<List<ProjectAssignmentDto>>> GetProjectAssignments([FromQuery] ProjectAssignmentParams parameters)
    {
        // If no projectId is given, return an empty list
        if (parameters.ProjectId == null) return new List<ProjectAssignmentDto>();

        // If user is not assigned to project and is not an admin, return an empty list
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(parameters.ProjectId);
        if (!hasAccess) return new List<ProjectAssignmentDto>();

        var response = await repo.GetProjectAssignmentsAsync(parameters);
        return response;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectAssignmentDto>> GetProjectAssignmentById(Guid id)
    {
        var projectAssignment = await repo.GetProjectAssignmentByIdAsync(id);
        if (projectAssignment == null) return NotFound();

        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(projectAssignment.ProjectId);
        if (!hasAccess) return NotFound();

        return projectAssignment;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkProjectAssignmentResultDto>> CreateBulkProjectAssignments(BulkProjectAssignmentCreateDto dto)
    {
        var projectId = dto.ProjectId;
        var project = await projectRepo.GetProjectByIdAsync(projectId);

        if (project == null)
        {
            return BadRequest("Project not found");
        }

        if (dto.UserNames == null || dto.UserNames.Count == 0)
        {
            return BadRequest("No users provided for assignment");
        }

        var existingAssignments = await repo.GetProjectAssignmentsByProjectIdAsync(projectId);
        var existingUserNames = existingAssignments.Select(a => a.UserName).ToHashSet();
        var result = new BulkProjectAssignmentResultDto();
        var toPublish = new List<ProjectAssignmentCreated>();

        foreach (var userName in dto.UserNames)
        {
            if (existingUserNames.Contains(userName))
            {
                result.FailedAssignments.Add($"{userName} - already assigned to project");
                continue;
            }

            if (await userRepo.GetUserEntityByUserName(userName) == null)
            {
                result.FailedAssignments.Add($"{userName} was not found.");
                continue;
            }

            var projectAssignmentCreateDto = new ProjectAssignmentCreateDto
            {
                ProjectId = projectId,
                UserName = userName,
            };
            var projectAssignment = mapper.Map<ProjectAssignment>(projectAssignmentCreateDto);
            var version = Guid.NewGuid();
            projectAssignment.Version = version;
            repo.AddProjectAssignment(projectAssignment);
            result.SuccessfulAssignments.Add(mapper.Map<BulkProjectAssignmentDto>(projectAssignment));
            toPublish.Add(new ProjectAssignmentCreated()
            {
                ProjectId = projectId,
                UserName = userName,
                Version = version,
            });
        }

        await publishEndpoint.PublishBatch(toPublish);
        await repo.SaveChangesAsync();
        result.Summary = $"Successfully assigned {result.SuccessfulAssignments.Count} out of {dto.UserNames.Count} users";
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProjectAssignmentDto>> CreateProjectAssignment(ProjectAssignmentCreateDto dto)
    {
        var projectId = dto.ProjectId;
        var project = await projectRepo.GetProjectByIdAsync(projectId);

        if (project == null)
        {
            return BadRequest("Project not found");
        }

        var existing = await repo.GetProjectAssignmentByProjectIdAndUserNameAsync(projectId, dto.UserName);

        if (existing != null)
        {
            return BadRequest("User has already been assigned to project");
        }

        if (await userRepo.GetUserEntityByUserName(dto.UserName) == null)
        {
            return BadRequest("User not found");
        }

        var projectAssignment = mapper.Map<ProjectAssignment>(dto);
        projectAssignment.Version = Guid.NewGuid();

        repo.AddProjectAssignment(projectAssignment);

        var newProjectAssignment = mapper.Map<ProjectAssignmentDto>(projectAssignment);

        var toPublish = new ProjectAssignmentCreated()
        {
            ProjectId = project.Id,
            UserName = dto.UserName,
            Version = newProjectAssignment.Version,
        };
        await publishEndpoint.Publish(toPublish);

        if (await repo.SaveChangesAsync())
        {
            var newDto = await repo.GetProjectAssignmentByIdAsync(newProjectAssignment.Id);
            return CreatedAtAction(
                nameof(GetProjectAssignmentById),
                new { id = newProjectAssignment.Id },
                newDto);
        }

        return BadRequest("Failed to create project assignment");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProjectAssignment(Guid id)
    {
        var entity = await repo.GetProjectAssignmentEntityById(id);

        if (entity == null) return NotFound();

        var userName = entity.UserName;
        repo.RemoveProjectAssignment(entity);

        var tasksAssignedToUser = await issueRepo.GetIssueEntitiesAssignedToUser(userName);
        foreach (var task in tasksAssignedToUser)
        {
            task.Assignee = null;
        }

        var toPublish = new ProjectAssignmentDeleted()
        {
            ProjectId = entity.ProjectId,
            UserName = entity.UserName,
            Version = entity.Version
        };
        await publishEndpoint.Publish(toPublish);

        var result = await repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
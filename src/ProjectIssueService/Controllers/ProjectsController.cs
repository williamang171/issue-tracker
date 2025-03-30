using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using Contracts;
using ProjectIssueService.Helpers;
using ProjectIssueService.Extensions;
using ProjectIssueService.Services;

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(
    IProjectRepository repo,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IProjectAssignmentServices projectAssignmentServices
    )
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects([FromQuery] ProjectParams parameters)
    {
        // Return empty list if userName not found
        var userName = HttpContext.GetCurrentUserName();
        if (string.IsNullOrEmpty(userName)) return Ok(Array.Empty<string>());

        // Get projects based on role (if isAdmin get all projects)
        var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
        var response = isAdmin ?
            await repo.GetProjectsPaginatedAsync(parameters, null) :
            await repo.GetProjectsPaginatedAsync(parameters, userName);
        Response.AddPaginationHeader(response.TotalCount);
        return response;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
    {
        // Check if project exists
        var project = await repo.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        // Check if user has access to project
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(project.Id);
        if (!hasAccess) return NotFound();

        return project;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<List<ProjectForSelectDto>>> GetProjectsAll()
    {
        // Return empty list if userName not found
        var userName = HttpContext.GetCurrentUserName();
        if (string.IsNullOrEmpty(userName)) return Ok(Array.Empty<string>());

        var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
        var response = isAdmin ? await repo.GetProjectsForSelectAsync(null) : await repo.GetProjectsForSelectAsync(userName);
        return response;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> CreateProject(ProjectCreateDto dto)
    {
        var project = mapper.Map<Project>(dto);
        var version = Guid.NewGuid();
        project.Version = version;
        repo.AddProject(project);
        var newProject = mapper.Map<ProjectDto>(project);

        await publishEndpoint.Publish(mapper.Map<ProjectCreated>(newProject));

        if (await repo.SaveChangesAsync())
        {
            var projectDto = await repo.GetProjectByIdAsync(newProject.Id);
            return CreatedAtAction(
                nameof(GetProjectById),
                new { id = newProject.Id },
                projectDto);
        }

        return BadRequest("Failed to create project");
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateProject(Guid id, ProjectUpdateDto dto)
    {
        var project = await repo.GetProjectEntityById(id);

        if (project == null) return NotFound();

        project.Name = dto.Name ?? project.Name;
        project.Description = dto.Description ?? project.Description;

        var result = await repo.SaveChangesAsync();

        if (result) return Ok();

        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProject(Guid id)
    {
        var project = await repo.GetProjectEntityById(id);

        if (project == null) return NotFound();

        var projectDto = mapper.Map<ProjectDto>(project);

        repo.RemoveProject(project);

        await publishEndpoint.Publish(mapper.Map<ProjectDeleted>(projectDto));

        var result = await repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
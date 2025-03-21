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

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects([FromQuery] ProjectParams parameters)
    {
        var response = await repo.GetProjectsPaginatedAsync(parameters);
        Response.AddPaginationHeader(response.TotalCount);
        return response;
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<List<ProjectForSelectDto>>> GetProjectAssignmentsAll()
    {
        var response = await repo.GetProjectsForSelectAsync();
        return response;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
    {
        var project = await repo.GetProjectByIdAsync(id);

        if (project == null) return NotFound();

        return project;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(ProjectCreateDto dto)
    {
        var project = mapper.Map<Project>(dto);

        repo.AddProject(project);

        var newProject = mapper.Map<ProjectDto>(project);

        // await publishEndpoint.Publish(mapper.Map<ProjectCreated>(newProject));

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

    [Authorize]
    [HttpPatch("{id:guid}")]
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

    [Authorize]
    [HttpDelete("{id}")]
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
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using Contracts;

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
    {
        return await repo.GetProjectsAsync();
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

        var result = await repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not save changes to DB");

        return CreatedAtAction(nameof(GetProjectById),
            new { Id = project.Id }, newProject);
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

        repo.RemoveProject(project);

        var result = await repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
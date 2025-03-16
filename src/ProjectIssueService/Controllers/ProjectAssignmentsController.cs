using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;
using ProjectIssueService.Extensions;

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectAssignmentsController(IProjectAssignmentRepository repo, IProjectRepository projectRepo, IMapper mapper)
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ProjectAssignmentDto>>> GetProjectAssignments([FromQuery] PaginationParams parameters)
    {
        var response = await repo.GetProjectAssignmentsPaginatedAsync(parameters);
        Response.AddPaginationHeader(response.TotalCount);
        return response;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectAssignmentDto>> GetProjectAssignmentById(Guid id)
    {
        var projectAssignment = await repo.GetProjectAssignmentByIdAsync(id);

        if (projectAssignment == null) return NotFound();

        return projectAssignment;
    }

    [Authorize]
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

        var projectAssignment = mapper.Map<ProjectAssignment>(dto);

        repo.AddProjectAssignment(projectAssignment);

        var newProjectAssignment = mapper.Map<ProjectAssignmentDto>(projectAssignment);

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

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProjectAssignment(Guid id)
    {
        var entity = await repo.GetProjectAssignmentEntityById(id);

        if (entity == null) return NotFound();

        repo.RemoveProjectAssignment(entity);

        var result = await repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not update DB");

        return Ok();
    }
}
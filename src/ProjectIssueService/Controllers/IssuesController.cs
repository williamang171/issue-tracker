using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MassTransit;

using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Data;
using Microsoft.AspNetCore.Authorization;
using Contracts;
using ProjectIssueService.Helpers;
using ProjectIssueService.Extensions;
using ProjectIssueService.Services;

namespace ProjectIssueService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssuesController(
    IIssueRepository issueRepo,
    IProjectRepository projectRepo,
    IUserRepository userRepo,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IProjectAssignmentServices projectAssignmentServices
    ) : ControllerBase
{
    private readonly IIssueRepository _issueRepo = issueRepo;
    private readonly IProjectRepository _projectRepo = projectRepo;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<IssueDto>>> GetIssues([FromQuery] IssueParams parameters)
    {
        // Return empty list if userName not found
        var userName = HttpContext.GetCurrentUserName();
        if (string.IsNullOrEmpty(userName)) return Ok(Array.Empty<string>());

        // Get issues based on role (if isAdmin get all issues)
        var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
        var response = isAdmin ?
            await _issueRepo.GetIssuesPaginatedAsync(parameters, null) :
            await _issueRepo.GetIssuesPaginatedAsync(parameters, userName);
        return response;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IssueDto>> GetIssue(Guid id)
    {
        // Check if issue exists
        var issue = await _issueRepo.GetIssueByIdAsync(id);
        if (issue == null) return NotFound();

        // Check if current user has access to this issue
        var hasAccess = await projectAssignmentServices
            .CanCurrentUserAccessProject(issue.ProjectId);
        if (!hasAccess) return NotFound();

        return issue;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Member")]
    public async Task<ActionResult<IssueDto>> CreateIssue(IssueCreateDto createIssueDto)
    {
        // Check if project with given Id exists
        var project = await _projectRepo.GetProjectEntityById(createIssueDto.ProjectId);
        if (project == null) return BadRequest("Project not found");

        // Check if user has access to this project
        var hasAccess = await projectAssignmentServices
            .CanCurrentUserAccessProject(project.Id);
        if (!hasAccess) return BadRequest("Project not found");

        // Check if assignee exists
        var assignee = createIssueDto.Assignee;
        if (assignee != null)
        {
            // Check if assignee exists
            if (await userRepo.GetUserEntityByUserName(assignee) == null) return BadRequest("Invalid Assignee");

            // Check if assignee is assigned to project
            var assigneeIsAssignedToProject = await projectAssignmentServices
                .IsUserAssignedToProject(project.Id, assignee);
            if (!assigneeIsAssignedToProject) return BadRequest("Invalid Assignee");
        }

        // Create issue and publish it
        var issue = _mapper.Map<Issue>(createIssueDto);
        _issueRepo.AddIssue(issue);
        issue.Version = Guid.NewGuid();
        var newIssue = _mapper.Map<IssueDto>(issue);
        await publishEndpoint.Publish(_mapper.Map<IssueCreated>(newIssue));
        if (await _issueRepo.SaveChangesAsync())
        {
            var issueDto = await _issueRepo.GetIssueByIdAsync(issue.Id);
            return CreatedAtAction(
                nameof(GetIssue),
                new { id = issue.Id },
                issueDto);
        }

        return BadRequest("Failed to create issue");
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Member")]
    public async Task<IActionResult> UpdateIssue(Guid id, IssueUpdateDto dto)
    {
        // Check if issue exists
        var issue = await _issueRepo.GetIssueEntityById(id);
        if (issue == null) return NotFound();

        // Check if user has access to issue
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(issue.ProjectId);
        if (!hasAccess) return NotFound();

        // Check if given assignee actually exists
        var assignee = dto.Assignee;
        if (assignee != null)
        {
            // Check if assignee exists
            if (await userRepo.GetUserEntityByUserName(assignee) == null) return BadRequest("Invalid Assignee");

            // Check if assignee is assigned to project
            var assigneeIsAssignedToProject = await projectAssignmentServices
            .IsUserAssignedToProject(issue.ProjectId, assignee);
            if (!assigneeIsAssignedToProject) return BadRequest("Invalid Assignee");
        }

        // Update entity fields
        var oldIssue = _mapper.Map<IssueDto>(issue);
        var oldVersion = oldIssue.Version;
        var newVersion = Guid.NewGuid();
        issue.Name = dto.Name ?? issue.Name;
        issue.Description = dto.Description ?? issue.Description;
        issue.Status = dto.Status ?? issue.Status;
        issue.Priority = dto.Priority ?? issue.Priority;
        issue.Type = dto.Type ?? issue.Type;
        if (dto.UnassignUser.HasValue && dto.UnassignUser == true)
        {
            issue.Assignee = null;
        }
        else
        {
            issue.Assignee = dto.Assignee ?? issue.Assignee;
        }
        issue.Version = newVersion;
        var newIssue = _mapper.Map<IssueDto>(issue);

        // Publish IssueUpdated and save changes
        IssueUpdated issueUpdated = new()
        {
            Id = id,
            OldValues = _mapper.Map<IssueValues>(oldIssue),
            NewValues = _mapper.Map<IssueValues>(newIssue),
            ProjectId = issue.ProjectId,
            OldVersion = oldVersion,
            NewVersion = newVersion,
        };
        await publishEndpoint.Publish(issueUpdated);
        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to update issue");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Member")]
    public async Task<IActionResult> DeleteIssue(Guid id)
    {
        // Check if issue exists
        var issue = await _issueRepo.GetIssueEntityById(id);
        if (issue == null) return NotFound();

        // Check if user has access to this issue
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(issue.ProjectId);
        if (!hasAccess) return NotFound();

        // Check if user has permission to delete this issue
        // Admin can delete all issues, while members can delete their own issues
        var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
        var canDelete = isAdmin || issue.CreatedBy == HttpContext.GetCurrentUserName();
        if (!canDelete) return Forbid();

        // Delete issue and publish event
        var issueDto = _mapper.Map<IssueDto>(issue);
        var issueDeleted = _mapper.Map<IssueDeleted>(issueDto);
        _issueRepo.RemoveIssue(issue);
        await publishEndpoint.Publish(issueDeleted);
        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to delete issue");
    }
}
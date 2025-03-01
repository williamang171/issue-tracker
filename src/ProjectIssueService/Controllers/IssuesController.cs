using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Data;
using Microsoft.AspNetCore.Authorization;

namespace ProjectIssueService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IssuesController(
    IIssueRepository issueRepo,
    IProjectRepository projectRepo,
    IMapper mapper) : ControllerBase
{
    private readonly IIssueRepository _issueRepo = issueRepo;
    private readonly IProjectRepository _projectRepo = projectRepo;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<IssueDto>>> GetIssues()
    {
        return await _issueRepo.GetIssuesAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IssueDto>> GetIssue(Guid id)
    {
        var issue = await _issueRepo.GetIssueByIdAsync(id);

        if (issue == null) return NotFound();

        return issue;
    }

    [HttpPost]
    public async Task<ActionResult<IssueDto>> CreateIssue(IssueCreateDto createIssueDto)
    {
        var project = await _projectRepo.GetProjectEntityById(createIssueDto.ProjectId);
        if (project == null) return BadRequest("Invalid ProjectId");

        var issue = _mapper.Map<Issue>(createIssueDto);

        _issueRepo.AddIssue(issue);

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
    public async Task<IActionResult> UpdateIssue(Guid id, IssueUpdateDto dto)
    {
        var issue = await _issueRepo.GetIssueEntityById(id);

        if (issue == null) return NotFound();

        issue.Name = dto.Name ?? issue.Name;
        issue.Description = dto.Description ?? issue.Description;
        issue.Status = dto.Status ?? issue.Status;
        issue.Priority = dto.Priority ?? issue.Priority;
        issue.Type = dto.Type ?? issue.Type;

        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to update issue");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIssue(Guid id)
    {
        var issue = await _issueRepo.GetIssueEntityById(id);

        if (issue == null) return NotFound();

        _issueRepo.RemoveIssue(issue);

        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to delete issue");
    }
}
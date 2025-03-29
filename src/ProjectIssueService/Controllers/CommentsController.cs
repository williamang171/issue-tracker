using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Data;
using Microsoft.AspNetCore.Authorization;
using ProjectIssueService.Helpers;
using ProjectIssueService.Services;
using ProjectIssueService.Extensions;

namespace ProjectIssueService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CommentsController(
    ICommentRepository commentRepo,
    IIssueRepository issueRepo,
    IMapper mapper,
    IProjectAssignmentServices projectAssignmentServices
    ) : ControllerBase
{
    private readonly IIssueRepository _issueRepo = issueRepo;
    private readonly ICommentRepository _commentRepo = commentRepo;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<CommentDto>>> GetComments([FromQuery] CommentParams parameters)
    {
        if (parameters.IssueId == null) return BadRequest("IssueId is required");

        // Return empty list if given issue not found
        Guid issueId = parameters.IssueId ?? Guid.Empty;
        var issue = await _issueRepo.GetIssueByIdAsync(issueId);
        if (issue == null) return new List<CommentDto>();

        // Return empty list if user doesn't have access to project of issue
        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(issue.ProjectId);
        if (!hasAccess) return new List<CommentDto>();

        // Return response
        var response = await _commentRepo.GetCommentsAsync(parameters);
        return response;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(Guid id)
    {
        // Check if comment exists
        var comment = await _commentRepo.GetCommentByIdAsync(id);
        if (comment == null) return NotFound();

        // Check if user has access to the project
        var issue = await _issueRepo.GetIssueEntityById(comment.IssueId);
        if (issue != null)
        {
            var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(issue.ProjectId);
            if (!hasAccess) return NotFound();
        }
        else
        {
            // Handle edge case where a comment exists without corresponding issue
            var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
            if (isAdmin) return comment;
            else return NotFound();
        }

        return comment;
    }

    [Authorize(Roles = "Admin,Member")]
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(CommentCreateDto createCommentDto)
    {
        var issue = await _issueRepo.GetIssueByIdAsync(createCommentDto.IssueId);
        if (issue == null) return BadRequest("Issue not found");

        var hasAccess = await projectAssignmentServices.CanCurrentUserAccessProject(issue.ProjectId);
        if (!hasAccess) return BadRequest("Issue not found");

        var comment = _mapper.Map<Comment>(createCommentDto);
        _commentRepo.AddComment(comment);

        if (await _commentRepo.SaveChangesAsync())
        {
            var commentDto = await _commentRepo.GetCommentByIdAsync(comment.Id);
            return CreatedAtAction(
                nameof(GetComment),
                new { id = comment.Id },
                commentDto);
        }

        return BadRequest("Failed to create comment");
    }

    [Authorize(Roles = "Admin,Member")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateComment(Guid id, CommentUpdateDto dto)
    {
        // Check if comment exists
        var comment = await _commentRepo.GetCommentEntityById(id);
        if (comment == null) return NotFound();

        // A comment can only be updated by the owner of it
        var userName = HttpContext.GetCurrentUserName();
        if (comment.CreatedBy != userName) return Forbid();

        comment.Content = dto.Content ?? comment.Content;

        await _commentRepo.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Admin,Member")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        // Check if comment exists
        var comment = await _commentRepo.GetCommentEntityById(id);
        if (comment == null) return NotFound();

        // Admin can remove all comments
        // Members can only remove their own comments
        var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
        var userName = HttpContext.GetCurrentUserName();
        if (!isAdmin && userName != comment.CreatedBy) return Forbid();

        _commentRepo.RemoveComment(comment);

        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to delete comment");
    }
}
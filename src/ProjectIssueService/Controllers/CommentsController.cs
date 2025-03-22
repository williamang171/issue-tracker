using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Data;
using Microsoft.AspNetCore.Authorization;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CommentsController(
    ICommentRepository commentRepo,
    IIssueRepository issueRepo,
    IMapper mapper) : ControllerBase
{
    private readonly IIssueRepository _issueRepo = issueRepo;
    private readonly ICommentRepository _commentRepo = commentRepo;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<CommentDto>>> GetComments([FromQuery] CommentParams parameters)
    {
        var response = await _commentRepo.GetCommentsAsync(parameters);
        return response;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(Guid id)
    {
        var comment = await _commentRepo.GetCommentByIdAsync(id);
        if (comment == null) return NotFound();
        return comment;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(CommentCreateDto createCommentDto)
    {
        var issue = await _issueRepo.GetIssueByIdAsync(createCommentDto.IssueId);
        if (issue == null) return BadRequest("Issue not found");

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

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateComment(Guid id, CommentUpdateDto dto)
    {
        var comment = await _commentRepo.GetCommentEntityById(id);
        if (comment == null) return NotFound();

        comment.Content = dto.Content ?? comment.Content;

        await _commentRepo.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _commentRepo.GetCommentEntityById(id);
        if (comment == null) return NotFound();

        _commentRepo.RemoveComment(comment);

        if (await _issueRepo.SaveChangesAsync()) return Ok();

        return BadRequest("Failed to delete comment");
    }
}
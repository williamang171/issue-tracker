using System;
using System.ComponentModel.DataAnnotations;
namespace ProjectIssueService.DTOs;

public class CommentCreateDto
{
    public required string Content { get; set; }
    [Required]
    public Guid IssueId { get; set; }
}

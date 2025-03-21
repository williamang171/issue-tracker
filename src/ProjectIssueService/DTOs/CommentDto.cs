using System;
using System.ComponentModel.DataAnnotations;
using ProjectIssueService.Entities;
namespace ProjectIssueService.DTOs;

public class CommentDto : BaseDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
}

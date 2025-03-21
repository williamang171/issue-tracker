using System;
using System.ComponentModel.DataAnnotations;
using ProjectIssueService.Entities;
namespace ProjectIssueService.DTOs;

public class IssueUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueType? Type { get; set; }
    public string? Assignee { get; set; }
}
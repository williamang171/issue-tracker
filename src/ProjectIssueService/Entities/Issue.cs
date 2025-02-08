using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

[Table("Issues")]
public class Issue
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueType Type { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}

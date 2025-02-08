using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

[Table("Projects")]
public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Issue> Issues { get; } = [];
}

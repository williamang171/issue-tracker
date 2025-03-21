using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

[Table("Comments")]
public class Comment : BaseEntity
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
}

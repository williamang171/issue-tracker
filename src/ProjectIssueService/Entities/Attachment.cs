using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

[Table("Attachments")]
public class Attachment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
    public required string Name { get; set; }
    public required string PublicId { get; set; }
    public required string Url { get; set; }
}

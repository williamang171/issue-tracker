using System;

namespace ProjectIssueService.DTOs;

public class AttachmentCreateDto
{
    public Guid IssueId { get; set; }
    public required string PublicId { get; set; }
    public required string Url { get; set; }
    public required string Name { get; set; }
}

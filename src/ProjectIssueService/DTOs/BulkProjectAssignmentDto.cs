using System;

namespace ProjectIssueService.DTOs;

public class BulkProjectAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public required string UserName { get; set; }
}

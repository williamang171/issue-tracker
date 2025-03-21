using System;

namespace ProjectIssueService.DTOs;

public class ProjectAssignmentCreateDto
{
    public Guid ProjectId { get; set; }
    public required string UserName { get; set; }
}

using System;

namespace ProjectIssueService.DTOs;

public class ProjectForSelectDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}

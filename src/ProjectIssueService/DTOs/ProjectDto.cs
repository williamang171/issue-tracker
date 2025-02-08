using System;
using System.ComponentModel.DataAnnotations;
namespace ProjectIssueService.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

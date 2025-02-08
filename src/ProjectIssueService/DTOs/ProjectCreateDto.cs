using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssueService.DTOs;

public class ProjectCreateDto
{
    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }
}

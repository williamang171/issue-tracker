using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssueService.DTOs;

public class ProjectUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

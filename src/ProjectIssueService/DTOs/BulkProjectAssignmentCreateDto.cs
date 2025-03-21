using System;
using System.ComponentModel.DataAnnotations;
using ProjectIssueService.Entities;
namespace ProjectIssueService.DTOs;

public class BulkProjectAssignmentCreateDto
{
    public Guid ProjectId { get; set; }
    public List<string> UserNames { get; set; } = new List<string>();
}
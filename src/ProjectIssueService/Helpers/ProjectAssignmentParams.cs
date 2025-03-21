using System;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Helpers;

public class ProjectAssignmentParams : PaginationParams
{
    public Guid? ProjectId { get; set; }
    public string? UserName_Like { get; set; }
}

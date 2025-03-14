using System;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Helpers;

public class IssueParams : PaginationParams
{
    public string? Name_Like { get; set; }
    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueType? Type { get; set; }
    public Guid? ProjectId { get; set; }
}

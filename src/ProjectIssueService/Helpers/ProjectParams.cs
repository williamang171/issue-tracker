using System;

namespace ProjectIssueService.Helpers;

public class ProjectParams : PaginationParams
{
    public string? Name_Like { get; set; }
}

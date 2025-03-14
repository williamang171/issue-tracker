using System;

namespace Contracts;

public class IssueValues
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueType? Type { get; set; }
}

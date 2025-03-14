using System;

namespace Contracts;

public class IssueUpdated
{
    public Guid Id { get; set; }
    public IssueValues NewValues { get; set; }
    public IssueValues OldValues { get; set; }
    public Guid ProjectId { get; set; }
}

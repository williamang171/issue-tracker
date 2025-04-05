using System;

namespace ProjectIssueService.Helpers;

public class AttachmentParams : PaginationParams
{
    public Guid? IssueId { get; set; }
}

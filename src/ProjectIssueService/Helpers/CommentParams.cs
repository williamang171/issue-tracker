using System;

namespace ProjectIssueService.Helpers;

public class CommentParams : PaginationParams
{
  public Guid? IssueId { get; set; }
}

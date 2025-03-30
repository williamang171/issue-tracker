using System;

namespace Contracts;

public class ProjectAssignmentDeleted : BaseDeleteMessage
{
    public Guid ProjectId { get; set; }
    public string UserName { get; set; }
}

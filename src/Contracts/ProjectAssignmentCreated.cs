using System;

namespace Contracts;

public class ProjectAssignmentCreated : BaseCreateMessage
{
    public Guid ProjectId { get; set; }
    public string UserName { get; set; }
}

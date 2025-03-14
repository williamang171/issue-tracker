using System;

namespace Contracts;

public class ProjectDeleted
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

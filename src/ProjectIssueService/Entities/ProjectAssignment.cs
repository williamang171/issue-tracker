using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

[Table("ProjectAssignments")]
public class ProjectAssignment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public required string UserName { get; set; }
    public required Project Project { get; set; }
}

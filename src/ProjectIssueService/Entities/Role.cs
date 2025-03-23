using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProjectIssueService.Entities;

[Table("Roles")]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Code), IsUnique = true)]
public class Role : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Entities;

[Table("Roles")]
[Index(nameof(Name), IsUnique = true)]
public class Role : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public required string Name { get; set; }
}

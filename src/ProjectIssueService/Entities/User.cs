using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProjectIssueService.Entities;

[Table("Users")]
[Index(nameof(UserName), IsUnique = true)]
public class User
{
    [Key]
    public Guid Id { get; set; }
    public required string UserName { get; set; }
}

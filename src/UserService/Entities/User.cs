using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Entities;

[Table("Users")]
[Index(nameof(UserName), IsUnique = true)]
public class User : BaseEntity
{
    [Key]
    public Guid Guid { get; set; }
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
}

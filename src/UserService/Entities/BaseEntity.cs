using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Entities;

public class BaseEntity
{
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public required string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    [ConcurrencyCheck]
    public Guid Version { get; set; }
}
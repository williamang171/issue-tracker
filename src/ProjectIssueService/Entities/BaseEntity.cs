using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectIssueService.Entities;

public class BaseEntity
{
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public required string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

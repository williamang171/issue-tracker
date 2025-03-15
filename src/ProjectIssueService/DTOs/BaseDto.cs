using System;

namespace ProjectIssueService.DTOs;

public class BaseDto
{
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public required string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

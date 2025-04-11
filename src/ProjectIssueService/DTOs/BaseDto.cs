using System;

namespace ProjectIssueService.DTOs;

public class BaseDto
{
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public Guid Version { get; set; }
}

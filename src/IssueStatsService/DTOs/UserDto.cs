using System;

namespace IssueStatsService.DTOs;

public class UserDto
{
    public required string UserName { get; set; }
    public string? RoleCode { get; set; }
    public bool? IsActive { get; set; }
    public Guid Version { get; set; }
}

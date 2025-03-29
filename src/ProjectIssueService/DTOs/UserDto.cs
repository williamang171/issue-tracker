using System;

namespace ProjectIssueService.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

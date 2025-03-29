using System;

namespace UserService.DTOs;

public class UserSyncLastLoginDto
{
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleCode { get; set; }
    public bool IsActive { get; set; }
}

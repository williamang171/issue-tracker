using System;

namespace UserService.DTOs;

public class UserSyncDto
{
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleCode { get; set; }
    public bool IsActive { get; set; }
    public Guid Version { get; set; }
    public bool IsRoot { get; set; }
}

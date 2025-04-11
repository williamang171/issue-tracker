using System;

namespace UserService.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleCode { get; set; }
    public bool IsActive { get; set; }
    public Guid Version { get; set; }
    public bool IsRoot { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

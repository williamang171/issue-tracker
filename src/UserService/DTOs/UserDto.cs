using System;

namespace UserService.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
}

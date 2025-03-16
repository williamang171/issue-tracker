using System;

namespace UserService.DTOs;

public class UserDto
{
    public Guid Guid { get; set; }
    public required string UserName { get; set; }
}

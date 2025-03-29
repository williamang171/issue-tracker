using System;

namespace UserService.DTOs;

public class UserUpdateDto
{
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

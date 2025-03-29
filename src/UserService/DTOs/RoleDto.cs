using System;

namespace UserService.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}

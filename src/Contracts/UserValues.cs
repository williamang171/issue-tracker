using System;

namespace Contracts;

public class UserValues
{
    public DateTime? LastLoginTime { get; set; }
    public Guid? RoleId { get; set; }
    public string RoleCode { get; set; }
    public bool? IsActive { get; set; }
}

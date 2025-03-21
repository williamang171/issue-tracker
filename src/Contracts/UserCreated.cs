using System;

namespace Contracts;

public class UserCreated
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public DateTime? LastLoginTime { get; set; }
}

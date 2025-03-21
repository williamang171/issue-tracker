using System;

namespace Contracts;

public class UserUpdated
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public UserValues OldValues { get; set; }
    public UserValues NewValues { get; set; }
}

using System;

namespace Contracts;

public class BaseUpdateMessage
{
    public Guid OldVersion { get; set; }
    public Guid NewVersion { get; set; }
}

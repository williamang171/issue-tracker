using System;
using UserService.Entities;

namespace UserService.Helpers;

public class UserParams : PaginationParams
{
    public Guid? ProjectId { get; set; }
    public string? UserName_Like { get; set; }
    public bool? Pagination { get; set; }
}

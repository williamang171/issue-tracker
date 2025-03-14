using System;

namespace ProjectIssueService.Helpers;

public class PaginationParams()
{
    public int _end { get; set; } = 10;
    public int _start { get; set; } = 0;
    public int PageSize => _end - _start;
    public int PageNumber => (_start / PageSize) + 1;
    public string? _sort { get; set; }
    public string _order { get; set; } = "ASC";
}

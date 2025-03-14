using System;

namespace ProjectIssueService.Helpers;

public static class PaginationHelper
{
    public static (int PageNumber, int PageSize) CalculatePagination(int start, int end)
    {
        if (end <= start)
        {
            throw new ArgumentException("End must be greater than start.");
        }

        int pageSize = end - start;
        int pageNumber = (start / pageSize) + 1;

        return (pageNumber, pageSize);
    }

}

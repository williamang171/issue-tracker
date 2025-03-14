using System.Text.Json;
using ProjectIssueService.Helpers;
using Microsoft.AspNetCore.Http;

namespace ProjectIssueService.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int totalItems)
        {
            response.Headers.Append("x-total-count", totalItems.ToString());
            response.Headers.Append("Access-Control-Expose-Headers", "x-total-count");
        }
    }
}
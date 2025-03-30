using System;

namespace IssueStatsService.Helpers;

public static class Constants
{
    public static string GetProjectKey(Guid projectId) => $"project:{projectId}:id";
    public static string GetProjectIssueVersionKey(Guid projectId) => $"project:{projectId}:issue-version";
    public static string GetUserKey(string userName) => $"user:{userName}";
    public static string GetProjectAssignmenstKey(Guid projectId) => $"project:{projectId}:users";
    public static string GetProjectIssueStatusCountsKey(Guid projectId) => $"project:{projectId}:status-counts";
    public static string GetProjectIssueTypeCountsKey(Guid projectId) => $"project:{projectId}:type-counts";
    public static string GetProjectIssuePriorityCountsKey(Guid projectId) => $"project:{projectId}:priority-counts";
}

using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Contracts;

using Microsoft.AspNetCore.Authorization;

namespace IssueStatsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IssueStatsController(IConnectionMultiplexer muxer) : ControllerBase
{

    [HttpGet("status/{projectId}")]
    public async Task<IActionResult> GetIssueStatusCount(string projectId)
    {
        IDatabase db = muxer.GetDatabase();
        var statusCounts = await db.HashGetAllAsync($"project:{projectId}:status-counts");

        // Transform Redis data to our response object
        var result = statusCounts
            .Select(kvp => new
            {
                label = kvp.Name.ToString(),
                key = (IssueStatus)System.Enum.Parse(typeof(IssueStatus), kvp.Name.ToString(), true),
                value = (int)kvp.Value
            })
            .OrderBy(x => x.key)
            .ToList();

        return Ok(result);
    }

    [HttpGet("type/{projectId}")]
    public async Task<IActionResult> GetIssueTypeCount(string projectId)
    {
        IDatabase db = muxer.GetDatabase();
        var counts = await db.HashGetAllAsync($"project:{projectId}:type-counts");

        // Transform Redis data to our response object
        var result = counts
            .Select(kvp => new
            {
                label = kvp.Name.ToString(),
                key = (IssueType)System.Enum.Parse(typeof(IssueType), kvp.Name.ToString(), true),
                value = (int)kvp.Value
            })
            .OrderBy(x => x.key)
            .ToList();

        return Ok(result);
    }

    [HttpGet("priority/{projectId}")]
    public async Task<IActionResult> GetIssuePriorityCount(string projectId)
    {
        IDatabase db = muxer.GetDatabase();
        var counts = await db.HashGetAllAsync($"project:{projectId}:priority-counts");

        // Transform Redis data to our response object
        var result = counts
            .Select(kvp => new
            {
                label = kvp.Name.ToString(),
                key = (IssuePriority)System.Enum.Parse(typeof(IssuePriority), kvp.Name.ToString(), true),
                value = (int)kvp.Value
            })
            .OrderBy(x => x.key)
            .ToList();

        return Ok(result);
    }
}
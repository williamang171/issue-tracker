using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Contracts;

using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using IssueStatsService.DTOs;

namespace IssueStatsService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IssueStatsController(IConnectionMultiplexer muxer, IHttpContextAccessor _httpContextAccessor) : ControllerBase
{

    [HttpGet("status/{projectId}")]
    public async Task<IActionResult> GetIssueStatusCount(string projectId)
    {
        if (!await HasAccess(projectId))
        {
            return Forbid();
        }
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
        if (!await HasAccess(projectId))
        {
            return Forbid();
        }
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
        if (!await HasAccess(projectId))
        {
            return Forbid();
        }
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

    private async Task<bool> HasAccess(string projectId)
    {
        var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (currentUsername == null)
        {
            return false;
        }

        string userKey = $"user:{currentUsername}";
        IDatabase db = muxer.GetDatabase();

        // If key doesn't exist, throw
        RedisValue storedUser = await db.StringGetAsync(userKey);
        if (!storedUser.HasValue)
        {
            return false;
        }

        // If failed to create userDto from source, throw
        var userDto = JsonSerializer.Deserialize<UserDto>(storedUser!);
        if (userDto == null)
        {
            Console.WriteLine($"Failed to create userDto for ${currentUsername}");
            return false;
        }

        // Admin can view all stats data
        if (userDto.RoleCode == "Admin")
        {
            return true;
        }

        string projectAssignmentKey = $"project:{projectId}:users";
        return await db.SetContainsAsync(projectAssignmentKey, currentUsername);
    }
}
using Microsoft.AspNetCore.Mvc;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IIssueRepository
{
    Task<List<IssueDto>> GetIssuesAsync();
    Task<PagedList<IssueDto>> GetIssuesPaginatedAsync(IssueParams parameters, string? projectAssignee);
    Task<IssueDto?> GetIssueByIdAsync(Guid id);
    Task<Issue?> GetIssueEntityById(Guid id);
    void AddIssue(Issue issue);
    void RemoveIssue(Issue issue);
    Task<bool> SaveChangesAsync();
}
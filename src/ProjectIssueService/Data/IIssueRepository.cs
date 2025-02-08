using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Data;

public interface IIssueRepository
{
    Task<List<IssueDto>> GetIssuesAsync();
    Task<IssueDto?> GetIssueByIdAsync(Guid id);
    Task<Issue?> GetIssueEntityById(Guid id);
    void AddIssue(Issue issue);
    void RemoveIssue(Issue issue);
    Task<bool> SaveChangesAsync();
}
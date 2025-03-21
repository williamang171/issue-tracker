using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public class IssueRepository(ApplicationDbContext context, IMapper mapper) : IIssueRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public void AddIssue(Issue issue)
    {
        _context.Issues.Add(issue);
    }

    public async Task<IssueDto?> GetIssueByIdAsync(Guid id)
    {
        return await _context.Issues
            .ProjectTo<IssueDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Issue?> GetIssueEntityById(Guid id)
    {
        return await _context.Issues
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<IssueDto>> GetIssuesAsync()
    {
        var query = _context.Issues.AsQueryable();
        return await query.ProjectTo<IssueDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<PagedList<IssueDto>> GetIssuesPaginatedAsync(IssueParams parameters)
    {
        var query = _context.Issues.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Name_Like))
        {
            query = query.Where(s => s.Name.Contains(parameters.Name_Like));
        }
        if (parameters.Priority.HasValue)
        {
            query = query.Where(s => s.Priority.Equals(parameters.Priority.Value));
        }
        if (parameters.Status.HasValue)
        {
            query = query.Where(s => s.Status.Equals(parameters.Status.Value));
        }
        if (parameters.Type.HasValue)
        {
            query = query.Where(s => s.Type.Equals(parameters.Type.Value));
        }
        if (parameters.ProjectId.HasValue)
        {
            query = query.Where(s => s.ProjectId.Equals(parameters.ProjectId.Value));
        }

        switch (parameters._sort)
        {
            case "name":
                if (parameters._order.ToUpper().Equals("ASC"))
                {
                    query = query.OrderBy(s => s.Name);
                }
                else if (parameters._order.ToUpper().Equals("DESC"))
                {
                    query = query.OrderByDescending(s => s.Name);
                }
                break;
            default:
                break;
        }

        return await PagedList<IssueDto>.CreateAsync
            (query.ProjectTo<IssueDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public void RemoveIssue(Issue issue)
    {
        _context.Issues.Remove(issue);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
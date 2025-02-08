using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

    public void RemoveIssue(Issue issue)
    {
        _context.Issues.Remove(issue);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
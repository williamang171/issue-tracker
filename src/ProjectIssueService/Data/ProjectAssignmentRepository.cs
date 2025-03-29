using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public class ProjectAssignmentRepository(ApplicationDbContext context, IMapper mapper) : IProjectAssignmentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    public void AddProjectAssignment(ProjectAssignment projectAssignment)
    {
        _context.ProjectAssignments.Add(projectAssignment);
    }

    public async Task<ProjectAssignmentDto?> GetProjectAssignmentByIdAsync(Guid id)
    {
        return await _context.ProjectAssignments
            .ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProjectAssignmentDto?> GetProjectAssignmentByProjectIdAndUserNameAsync(Guid projectId, string userName)
    {
        return await _context.ProjectAssignments
            .ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserName == userName);
    }

    public async Task<ProjectAssignment?> GetProjectAssignmentEntityById(Guid id)
    {
        return await _context.ProjectAssignments
           .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ProjectAssignmentDto>> GetProjectAssignmentsAsync(ProjectAssignmentParams parameters)
    {
        var query = _context.ProjectAssignments.AsQueryable();

        if (parameters.ProjectId.HasValue)
        {
            query = query.Where(s => s.ProjectId.Equals(parameters.ProjectId.Value));
        }
        if (!string.IsNullOrEmpty(parameters.UserName_Like))
        {
            query = query.Where(s => s.UserName.Contains(parameters.UserName_Like));
        }

        return await query.ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<PagedList<ProjectAssignmentDto>> GetProjectAssignmentsPaginatedAsync(ProjectAssignmentParams parameters)
    {
        var query = _context.ProjectAssignments.AsQueryable();

        if (parameters.ProjectId.HasValue)
        {
            query = query.Where(s => s.ProjectId.Equals(parameters.ProjectId.Value));
        }
        if (!string.IsNullOrEmpty(parameters.UserName_Like))
        {
            query = query.Where(s => s.UserName.Contains(parameters.UserName_Like));
        }

        return await PagedList<ProjectAssignmentDto>.CreateAsync
            (query.ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectAssignments
            .Where(pa => pa.ProjectId == projectId)
            .ToListAsync();
    }

    public void RemoveProjectAssignment(ProjectAssignment projectAssignment)
    {
        _context.ProjectAssignments.Remove(projectAssignment);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<ProjectAssignment?> GetProjectAssignmentEntityByProjectIdAndUserName(Guid projectId, string userName)
    {
        return await _context.ProjectAssignments
            .Where(x => x.ProjectId == projectId && x.UserName == userName)
            .FirstOrDefaultAsync();
    }
}

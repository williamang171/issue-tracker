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

    public async Task<List<ProjectAssignmentDto>> GetProjectAssignmentsAsync()
    {
        var query = _context.ProjectAssignments.AsQueryable();
        return await query.ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<PagedList<ProjectAssignmentDto>> GetProjectAssignmentsPaginatedAsync(PaginationParams parameters)
    {
        var query = _context.ProjectAssignments.AsQueryable();
        return await PagedList<ProjectAssignmentDto>.CreateAsync
            (query.ProjectTo<ProjectAssignmentDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public void RemoveProjectAssignment(ProjectAssignment projectAssignment)
    {
        _context.ProjectAssignments.Remove(projectAssignment);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

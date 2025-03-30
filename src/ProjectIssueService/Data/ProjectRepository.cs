using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public class ProjectRepository(ApplicationDbContext context, IMapper mapper) : IProjectRepository
{
    public void AddProject(Project project)
    {
        context.Projects.Add(project);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        return await context.Projects
            .ProjectTo<ProjectDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProjectDto?> GetProjectByIdAndProjectAssigneeAsync(Guid id, string projectAssignee)
    {
        return await context.Projects
            .Where(x => x.ProjectAssignments.Any(pa => pa.UserName == projectAssignee))
            .ProjectTo<ProjectDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Project?> GetProjectEntityById(Guid id)
    {
        return await context.Projects
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        var query = context.Projects.AsQueryable();
        return await query.ProjectTo<ProjectDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<List<ProjectForSelectDto>> GetProjectsForSelectAsync(string? projectAssignee)
    {
        var query = context.Projects.AsQueryable();

        if (projectAssignee != null)
        {
            query = query.Where(x => x.ProjectAssignments.Any(pa => pa.UserName == projectAssignee));
        }

        return await query.ProjectTo<ProjectForSelectDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<PagedList<ProjectDto>> GetProjectsPaginatedAsync(ProjectParams parameters, string? projectAssignee)
    {
        var query = context.Projects.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Name_Like))
        {
            query = query.Where(s => s.Name.Contains(parameters.Name_Like));
        }

        if (projectAssignee != null)
        {
            query = query.Where(x => x.ProjectAssignments.Any(pa => pa.UserName == projectAssignee));
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

        return await PagedList<ProjectDto>.CreateAsync
            (query.ProjectTo<ProjectDto>(mapper.ConfigurationProvider).AsNoTracking(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public void RemoveProject(Project project)
    {
        context.Projects.Remove(project);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}

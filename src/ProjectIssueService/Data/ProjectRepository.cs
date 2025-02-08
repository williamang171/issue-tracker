using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Project?> GetProjectEntityById(Guid id)
    {
        return await context.Projects
            // .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        var query = context.Projects.AsQueryable();

        return await query.ProjectTo<ProjectDto>(mapper.ConfigurationProvider).ToListAsync();
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

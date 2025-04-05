using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public class CommentRepository(ApplicationDbContext context, IMapper mapper) : ICommentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public void AddComment(Comment comment)
    {
        _context.Comments.Add(comment);
    }

    public async Task<CommentDto?> GetCommentByIdAsync(Guid id)
    {
        return await _context.Comments
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Comment?> GetCommentEntityById(Guid id)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<CommentDto>> GetCommentsAsync(CommentParams parameters)
    {
        var query = _context.Comments.AsQueryable();

        if (parameters.IssueId.HasValue)
        {
            query = query.Where(x => x.IssueId == parameters.IssueId);
        }
        bool isAscending = parameters._order.Equals("ASC", StringComparison.CurrentCultureIgnoreCase);
        query = parameters._sort switch
        {
            "createdTime" => isAscending
                ? query.OrderBy(s => s.CreatedTime)
                : query.OrderByDescending(s => s.CreatedTime),
            _ => query // Default case returns query unchanged
        };

        return await query.ProjectTo<CommentDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveComment(Comment issue)
    {
        _context.Comments.Remove(issue);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

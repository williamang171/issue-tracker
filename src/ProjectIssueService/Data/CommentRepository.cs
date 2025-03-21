using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<CommentDto>> GetCommentsAsync()
    {
        var query = _context.Comments.AsQueryable();
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

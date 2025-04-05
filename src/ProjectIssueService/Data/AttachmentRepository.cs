using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public class AttachmentRepository(ApplicationDbContext context, IMapper mapper) : IAttachmentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public void AddAttachment(Attachment attachment)
    {
        _context.Attachments.Add(attachment);
    }

    public async Task<Attachment?> GetAttachmentEntityById(Guid id)
    {
        return await _context.Attachments
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<AttachmentDto?> GetAttachmentById(Guid id)
    {
        return await _context.Attachments
            .ProjectTo<AttachmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AttachmentDto>> GetAttachmentsAsync(AttachmentParams parameters)
    {
        var query = _context.Attachments.AsQueryable();

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

        return await query.ProjectTo<AttachmentDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveAttachment(Attachment attachment)
    {
        _context.Attachments.Remove(attachment);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

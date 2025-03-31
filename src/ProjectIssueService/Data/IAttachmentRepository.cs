using Microsoft.AspNetCore.Mvc;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IAttachmentRepository
{
    Task<List<AttachmentDto>> GetAttachmentsAsync(AttachmentParams parameters);
    Task<Attachment?> GetAttachmentEntityById(Guid id);
    Task<AttachmentDto?> GetAttachmentById(Guid id);
    void AddAttachment(Attachment attachment);
    void RemoveAttachment(Attachment attachment);
    Task<bool> SaveChangesAsync();
}
using Microsoft.AspNetCore.Mvc;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface ICommentRepository
{
    Task<List<CommentDto>> GetCommentsAsync(CommentParams parameters);
    Task<CommentDto?> GetCommentByIdAsync(Guid id);
    Task<Comment?> GetCommentEntityById(Guid id);
    void AddComment(Comment comment);
    void RemoveComment(Comment comment);
    Task<bool> SaveChangesAsync();
}
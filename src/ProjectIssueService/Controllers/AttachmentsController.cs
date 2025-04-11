using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectIssueService.Services;
using Microsoft.AspNetCore.Authorization;
using ProjectIssueService.DTOs;
using ProjectIssueService.Data;
using AutoMapper;
using ProjectIssueService.Extensions;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController(
    IMapper mapper,
    IAttachmentRepository attachmentRepository,
    IIssueRepository issueRepository,
    IProjectAssignmentServices projectAssignmentService
    ) : ControllerBase
    {
        private readonly IIssueRepository _issueRepo = issueRepository;
        private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
        private readonly IProjectAssignmentServices _projectAssignmentService = projectAssignmentService;
        private readonly IMapper _mapper = mapper;

        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        public async Task<ActionResult<AttachmentDto>> CreateAttachment(AttachmentCreateDto dto)
        {
            var issueId = dto.IssueId;
            var issue = await _issueRepo.GetIssueByIdAsync(issueId);

            if (issue == null) return BadRequest("Issue not found");

            var hasAccess = await _projectAssignmentService.CanCurrentUserAccessProject(issue.ProjectId);
            if (!hasAccess) return Forbid();

            var attachment = _mapper.Map<Attachment>(dto);
            attachment.Version = Guid.NewGuid();
            _attachmentRepository.AddAttachment(attachment);

            if (await _attachmentRepository.SaveChangesAsync())
            {
                var commentDto = await _attachmentRepository.GetAttachmentById(attachment.Id);

                return CreatedAtAction(
                    nameof(GetAttachment),
                    new { id = attachment.Id },
                    commentDto);
            }

            return BadRequest("Failed to create attachment");
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AttachmentDto>> GetAttachment(Guid id)
        {
            // Check if attachment exists
            var attachment = await _attachmentRepository.GetAttachmentById(id);
            if (attachment == null) return NotFound();

            // Check if user has access to the project
            var issue = await _issueRepo.GetIssueEntityById(attachment.IssueId);
            if (issue != null)
            {
                var hasAccess = await _projectAssignmentService.CanCurrentUserAccessProject(issue.ProjectId);
                if (!hasAccess) return NotFound();
            }
            else
            {
                // Handle edge case where an attachment exists without corresponding issue
                var isAdmin = HttpContext.CurrentUserRoleIsAdmin();
                if (isAdmin) return attachment;
                else return NotFound();
            }

            return attachment;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AttachmentDto>>> GetAttachments([FromQuery] AttachmentParams parameters)
        {
            if (parameters.IssueId == null) return BadRequest("IssueId is required");

            // Return empty list if given issue not found
            Guid issueId = parameters.IssueId ?? Guid.Empty;
            var issue = await _issueRepo.GetIssueByIdAsync(issueId);
            if (issue == null) return new List<AttachmentDto>();

            // Return empty list if user doesn't have access to project of issue
            var hasAccess = await _projectAssignmentService.CanCurrentUserAccessProject(issue.ProjectId);
            if (!hasAccess) return new List<AttachmentDto>();

            // Return response
            var response = await _attachmentRepository.GetAttachmentsAsync(parameters);
            return response;
        }

        [Authorize(Roles = "Admin,Member")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(Guid id)
        {
            // Check if entity exists
            var entity = await _attachmentRepository.GetAttachmentEntityById(id);
            if (entity == null) return NotFound();

            // Admin can remove all attachments
            // Member can remove attachments if they belong to the project that contains the attachment
            var issue = await _issueRepo.GetIssueByIdAsync(entity.IssueId);
            if (issue != null)
            {
                var hasAccess = await _projectAssignmentService.CanCurrentUserAccessProject(issue.ProjectId);
                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            _attachmentRepository.RemoveAttachment(entity);

            if (await _attachmentRepository.SaveChangesAsync()) return Ok();

            return BadRequest("Failed to delete attachment");
        }
    }
}
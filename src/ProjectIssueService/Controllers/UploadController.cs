using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectIssueService.Services;
using ProjectIssueService.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ProjectIssueService.Controllers
{
    [Authorize(Roles = "Admin,Member")]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public UploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        public async Task<ActionResult<UploadDto>> UploadSingleFile(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("No file was provided");

                var dto = await _fileUploadService.UploadFileAsync(file, "attachments");

                return dto;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
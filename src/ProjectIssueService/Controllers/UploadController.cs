using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProjectIssueService.Services;
using ProjectIssueService.DTOs;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;

namespace ProjectIssueService.Controllers
{
    [Authorize(Roles = "Admin,Member")]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly long _fileSizeLimit = 2 * 1024 * 1024; // 2MB limit
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".jfif" };

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

                // Check file size
                if (file.Length > _fileSizeLimit)
                {
                    return BadRequest($"File size exceeds the limit of 2MB.");
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !Array.Exists(_permittedExtensions, e => e == extension))
                {
                    return BadRequest("Invalid file extension. Only image files are allowed.");
                }

                // Verify the file is actually an image
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        stream.Position = 0;

                        // This will throw an exception if the file is not a valid image
                        using (var image = await Image.LoadAsync(stream))
                        {
                            // Reset the position so the file can be read again by the upload service
                            stream.Position = 0;

                            // Now that we've verified it's an image, proceed with the upload
                            var dto = await _fileUploadService.UploadFileAsync(file, "attachments");
                            return dto;
                        }
                    }
                }
                catch (Exception ex) when (
                    ex is UnknownImageFormatException ||
                    ex is InvalidImageContentException ||
                    ex is NotSupportedException)
                {
                    return BadRequest("The uploaded file is not a valid image.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
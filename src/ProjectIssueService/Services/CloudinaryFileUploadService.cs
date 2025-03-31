using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProjectIssueService.DTOs;
using System;
using System.IO;
using System.Threading.Tasks;


namespace ProjectIssueService.Services
{
    public interface IFileUploadService
    {
        Task<UploadDto> UploadFileAsync(IFormFile file, string folder = "");
    }

    public class CloudinaryFileUploadService : IFileUploadService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileUploadService(IConfiguration configuration)
        {
            _cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
        }

        public async Task<UploadDto> UploadFileAsync(IFormFile file, string folder = "")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided");

            using var stream = file.OpenReadStream();

            // Create upload parameters
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
            };

            // Add folder if specified
            if (!string.IsNullOrEmpty(folder))
            {
                uploadParams.Folder = folder;
            }

            // Upload to Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Check if upload was successful
            if (uploadResult.Error != null)
            {
                throw new Exception($"Failed to upload file: {uploadResult.Error.Message}");
            }

            var dto = new UploadDto()
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString(),
            };
            return dto;
        }
    }
}
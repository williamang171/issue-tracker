using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectIssueService.Controllers;
using ProjectIssueService.DTOs;
using ProjectIssueService.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ProjectIssueService.UnitTests
{
    public class UploadControllerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly UploadController _controller;

        public UploadControllerTests()
        {
            _mockFileUploadService = new Mock<IFileUploadService>();
            _controller = new UploadController(_mockFileUploadService.Object);
        }

        private MemoryStream CreateMockJpegImage()
        {
            // Create a small valid JPEG image in memory
            var ms = new MemoryStream();
            using (var image = new Image<Rgba32>(10, 10))
            {
                // Set each pixel manually
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        image[x, y] = new Rgba32(255, 0, 0); // Red color
                    }
                }
                // Save as JPEG
                image.SaveAsJpeg(ms);
            }
            ms.Position = 0;
            return ms;
        }

        [Fact]
        public async Task UploadSingleFile_WithValidImage_ReturnsUploadDto()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var ms = CreateMockJpegImage();

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Setup CopyToAsync to work with MemoryStream
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);

            var expectedDto = new UploadDto
            {
                PublicId = "1",
                Url = "test.com"
            };

            _mockFileUploadService
                .Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), "attachments"))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _controller.UploadSingleFile(fileMock.Object);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UploadDto>>(result);
            var returnValue = Assert.IsType<UploadDto>(actionResult.Value);
            Assert.Equal(expectedDto.PublicId, returnValue.PublicId);
            Assert.Equal(expectedDto.Url, returnValue.Url);

            _mockFileUploadService.Verify(
                s => s.UploadFileAsync(It.IsAny<IFormFile>(), "attachments"),
                Times.Once);
        }

        [Fact]
        public async Task UploadSingleFile_WithInvalidExtension_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("This is not an image");
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Act
            var result = await _controller.UploadSingleFile(fileMock.Object);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UploadDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Contains("Invalid file extension", badRequestResult.Value.ToString());

            _mockFileUploadService.Verify(
                s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadSingleFile_WithOversizedFile_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";

            // Mock a file larger than 2MB
            long fileSize = 3 * 1024 * 1024; // 3MB

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(fileSize);

            // Act
            var result = await _controller.UploadSingleFile(fileMock.Object);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UploadDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Contains("File size exceeds", badRequestResult.Value.ToString());

            _mockFileUploadService.Verify(
                s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadSingleFile_WithNullFile_ReturnsBadRequest()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.UploadSingleFile(file);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UploadDto>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            _mockFileUploadService.Verify(
                s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadSingleFile_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var ms = CreateMockJpegImage();

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Setup CopyToAsync to work with MemoryStream
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);

            var exceptionMessage = "Test exception";

            _mockFileUploadService
                .Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UploadSingleFile(fileMock.Object);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UploadDto>>(result);
            Assert.IsType<ObjectResult>(actionResult.Result);

            var statusCodeResult = (ObjectResult)actionResult.Result;
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
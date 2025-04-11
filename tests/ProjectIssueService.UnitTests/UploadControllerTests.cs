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

        [Fact]
        public async Task UploadSingleFile_WithValidFile_ReturnsUploadDto()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

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
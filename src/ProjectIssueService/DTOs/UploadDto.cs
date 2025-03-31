using System;

namespace ProjectIssueService.DTOs;

public class UploadDto
{
    public required string Url { get; set; }
    public required string PublicId { get; set; }
}

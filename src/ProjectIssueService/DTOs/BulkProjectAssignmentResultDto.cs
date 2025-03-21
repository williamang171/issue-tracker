namespace ProjectIssueService.DTOs;

public class BulkProjectAssignmentResultDto
{
    public List<BulkProjectAssignmentDto> SuccessfulAssignments { get; set; } = [];
    public List<string> FailedAssignments { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}
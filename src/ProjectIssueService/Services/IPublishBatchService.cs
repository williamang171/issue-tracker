using System;
using Contracts;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Services;

public interface IPublishBatchService
{
    public Task PublishBatchProjectAssignments(List<ProjectAssignmentCreated> projectAssignmentsCreated);
}

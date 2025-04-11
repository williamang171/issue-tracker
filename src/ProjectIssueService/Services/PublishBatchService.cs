using System;
using Contracts;
using MassTransit;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Services;

public class PublishBatchService(IPublishEndpoint publishEndpoint) : IPublishBatchService
{
    public async Task PublishBatchProjectAssignments(List<ProjectAssignmentCreated> projectAssignments)
    {
        await publishEndpoint.PublishBatch(projectAssignments);
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

using ProjectIssueService.Controllers;
using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;
using ProjectIssueService.RequestHelpers;
using ProjectIssueService.Services;

namespace ProjectIssueService.UnitTests;

public class BaseControllerTests
{
    protected Mock<HttpContext> SetupHttpContext(ControllerBase controller)
    {
        // Create mock HttpContext
        var mockHttpContext = new Mock<HttpContext>();

        // Setup response for header testing
        var mockResponse = new Mock<HttpResponse>();
        mockResponse.SetupGet(r => r.Headers).Returns(new HeaderDictionary());
        mockHttpContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        // Assign HttpContext to controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        return mockHttpContext;
    }
}

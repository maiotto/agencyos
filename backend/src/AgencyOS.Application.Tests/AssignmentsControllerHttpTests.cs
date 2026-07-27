using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace AgencyOS.Application.Tests;

public class AssignmentsControllerHttpTests : IClassFixture<AssignmentApiFactory>
{
    private readonly AssignmentApiFactory _factory;

    public AssignmentsControllerHttpTests(AssignmentApiFactory factory)
    {
        _factory = factory;
        _factory.AssignmentService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.AssignmentService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AssignmentQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AssignmentResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/assignments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenAssignmentExists()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(assignmentId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/assignments/{assignmentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAssignmentMissing()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.GetByIdAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Assignment with id '{assignmentId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/assignments/{assignmentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/assignments", new CreateAssignmentRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(assignmentId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/assignments", CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenTaskMissing()
    {
        var taskId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Task with id '{taskId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/assignments", CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenExecutionResourceInactive()
    {
        _factory.AssignmentService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "Only Active Execution Resources can receive assignments."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/assignments", CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.UpdateAsync(
                assignmentId,
                It.IsAny<UpdateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(assignmentId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/assignments/{assignmentId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenCancelledAssignmentCannotBeEdited()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.UpdateAsync(
                assignmentId,
                It.IsAny<UpdateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Cancelled Assignments cannot be edited."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/assignments/{assignmentId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenAssignmentMissing()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.UpdateAsync(
                assignmentId,
                It.IsAny<UpdateAssignmentRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Assignment with id '{assignmentId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/assignments/{assignmentId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.CancelAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(assignmentId, AssignmentStatus.Cancelled));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/assignments/{assignmentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ReturnsOk()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.CancelAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(assignmentId, AssignmentStatus.Cancelled));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/assignments/{assignmentId}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ReturnsNotFound_WhenAssignmentMissing()
    {
        var assignmentId = Guid.NewGuid();
        _factory.AssignmentService
            .Setup(service => service.CancelAsync(assignmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Assignment with id '{assignmentId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/assignments/{assignmentId}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static CreateAssignmentRequest CreateValidCreateRequest()
    {
        return new CreateAssignmentRequest
        {
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = Guid.NewGuid(),
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Planned
        };
    }

    private static UpdateAssignmentRequest CreateValidUpdateRequest()
    {
        return new UpdateAssignmentRequest
        {
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = AssignmentStatus.Confirmed
        };
    }

    private static AssignmentResponse CreateResponse(
        Guid assignmentId,
        string status = AssignmentStatus.Planned)
    {
        return new AssignmentResponse
        {
            Id = assignmentId,
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = Guid.NewGuid(),
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = 24,
            PlannedStartDate = new DateOnly(2026, 1, 5),
            PlannedEndDate = new DateOnly(2026, 1, 9),
            AllocationPercentage = 50,
            Status = status
        };
    }
}

public sealed class AssignmentApiFactory : WebApplicationFactory<Program>
{
    public Mock<IAssignmentService> AssignmentService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=127.0.0.1;Port=54322;Database=postgres;Username=postgres;Password=postgres"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAssignmentService>();
            services.AddSingleton(AssignmentService.Object);
        });
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;

namespace AgencyOS.Application.Tests;

public class AllocationConflictControllerHttpTests : IClassFixture<AllocationConflictApiFactory>
{
    private readonly AllocationConflictApiFactory _factory;

    public AllocationConflictControllerHttpTests(AllocationConflictApiFactory factory)
    {
        _factory = factory;
        _factory.AllocationConflictDetectionService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.AllocationConflictDetectionService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AllocationConflictResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildConflictUrl("/allocation-conflicts"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/allocation-conflicts");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            "/allocation-conflicts?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.AllocationConflictDetectionService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSummaryResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildConflictUrl("/allocation-conflicts/summary"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/allocation-conflicts/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsOk_WhenResourceExists()
    {
        var resourceId = Guid.NewGuid();
        _factory.AllocationConflictDetectionService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateConflictResponse(resourceId) });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildConflictUrl($"/allocation-conflicts/{resourceId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.AllocationConflictDetectionService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildConflictUrl($"/allocation-conflicts/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceIsNotActive()
    {
        var resourceId = Guid.NewGuid();
        _factory.AllocationConflictDetectionService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AllocationConflictQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Allocation conflicts are detected only for Active Execution Resources. Resource '{resourceId}' is not active."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildConflictUrl($"/allocation-conflicts/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var resourceId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/allocation-conflicts/{resourceId}?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildConflictUrl(string path)
    {
        return $"{path}?periodStartDate=2026-07-01&periodEndDate=2026-07-07";
    }

    private static AllocationConflictResponse CreateConflictResponse(Guid resourceId)
    {
        return new AllocationConflictResponse
        {
            ConflictId = Guid.NewGuid(),
            ConflictType = "Capacity Exceeded",
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = "RES-001",
            ExecutionResourceName = "Senior Delivery Consultant",
            RelatedAssignments =
            [
                new AllocationConflictAssignmentReference
                {
                    AssignmentId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    AssignmentRole = "Responsible",
                    PlannedHours = 30,
                    PlannedStartDate = new DateOnly(2026, 7, 1),
                    PlannedEndDate = new DateOnly(2026, 7, 7)
                }
            ],
            Severity = "High",
            Description = "Planned workload of 30 hours exceeds available capacity of 20 hours.",
            SuggestedResolution =
                "Reduce planned hours, extend the assignment period, or reassign work to another resource."
        };
    }

    private static AllocationConflictSummaryResponse CreateSummaryResponse()
    {
        return new AllocationConflictSummaryResponse
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            ActiveResourceCount = 1,
            TotalConflictCount = 1,
            CriticalConflictCount = 0,
            HighConflictCount = 1,
            MediumConflictCount = 0,
            LowConflictCount = 0,
            ResourcesWithConflicts = 1
        };
    }
}

public sealed class AllocationConflictApiFactory : WebApplicationFactory<Program>
{
    public Mock<IAllocationConflictDetectionService> AllocationConflictDetectionService { get; } = new();

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
            services.RemoveAll<IAllocationConflictDetectionService>();
            services.AddSingleton(AllocationConflictDetectionService.Object);
        });
    }
}

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

public class WorkloadControllerHttpTests : IClassFixture<WorkloadApiFactory>
{
    private readonly WorkloadApiFactory _factory;

    public WorkloadControllerHttpTests(WorkloadApiFactory factory)
    {
        _factory = factory;
        _factory.WorkloadCalculatorService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.WorkloadCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildWorkloadUrl("/workload"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            "/workload?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.WorkloadCalculatorService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSummaryResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildWorkloadUrl("/workload/summary"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsOk_WhenResourceExists()
    {
        var resourceId = Guid.NewGuid();
        _factory.WorkloadCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateWorkloadResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildWorkloadUrl($"/workload/{resourceId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.WorkloadCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildWorkloadUrl($"/workload/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceIsNotActive()
    {
        var resourceId = Guid.NewGuid();
        _factory.WorkloadCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<WorkloadQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Workload is calculated only for Active Execution Resources. Resource '{resourceId}' is not active."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildWorkloadUrl($"/workload/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var resourceId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/workload/{resourceId}?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildWorkloadUrl(string path)
    {
        return $"{path}?periodStartDate=2026-07-01&periodEndDate=2026-07-07";
    }

    private static WorkloadResponse CreateWorkloadResponse(Guid resourceId)
    {
        return new WorkloadResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = "RES-001",
            ExecutionResourceName = "Senior Delivery Consultant",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalPlannedHours = 24,
            AssignmentCount = 2,
            AverageHoursPerAssignment = 12,
            WorkloadPercentage = 60
        };
    }

    private static WorkloadSummaryResponse CreateSummaryResponse()
    {
        return new WorkloadSummaryResponse
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            ActiveResourceCount = 1,
            TotalPlannedHours = 24,
            TotalAssignmentCount = 2,
            AverageHoursPerAssignment = 12,
            OverallWorkloadPercentage = 60
        };
    }
}

public sealed class WorkloadApiFactory : WebApplicationFactory<Program>
{
    public Mock<IWorkloadCalculatorService> WorkloadCalculatorService { get; } = new();

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
            services.RemoveAll<IWorkloadCalculatorService>();
            services.AddSingleton(WorkloadCalculatorService.Object);
        });
    }
}

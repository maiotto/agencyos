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

public class WorkloadHistoryControllerHttpTests : IClassFixture<WorkloadHistoryApiFactory>
{
    private readonly WorkloadHistoryApiFactory _factory;

    public WorkloadHistoryControllerHttpTests(WorkloadHistoryApiFactory factory)
    {
        _factory = factory;
        _factory.WorkloadHistoryService.Reset();
    }

    [Fact]
    public async Task GetHistory_ReturnsOk()
    {
        _factory.WorkloadHistoryService
            .Setup(service => service.QueryAsync(
                It.IsAny<WorkloadHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadHistoryResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryById_ReturnsOk()
    {
        var historyId = Guid.NewGuid();
        _factory.WorkloadHistoryService
            .Setup(service => service.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateHistory(historyId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/workload/history/{historyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryById_ReturnsNotFound()
    {
        var historyId = Guid.NewGuid();
        _factory.WorkloadHistoryService
            .Setup(service => service.GetByIdAsync(historyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Workload history with id '{historyId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/workload/history/{historyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryByResource_ReturnsOk()
    {
        var resourceId = Guid.NewGuid();
        _factory.WorkloadHistoryService
            .Setup(service => service.GetByExecutionResourceIdAsync(
                resourceId,
                It.IsAny<WorkloadHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadHistoryResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/workload/history/resource/{resourceId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryByCompany_ReturnsOk()
    {
        var companyId = Guid.NewGuid();
        _factory.WorkloadHistoryService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.IsAny<WorkloadHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkloadHistoryResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/workload/history/company/{companyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompareHistory_ReturnsOk()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        _factory.WorkloadHistoryService
            .Setup(service => service.CompareAsync(
                It.IsAny<WorkloadHistoryCompareQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryCompareResponse
            {
                Left = CreateHistory(leftId),
                Right = CreateHistory(rightId),
                AllocatedHoursDelta = 8
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/workload/history/compare?leftHistoryId={leftId}&rightHistoryId={rightId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompareHistory_ReturnsBadRequest_WhenIdsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload/history/compare");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryAggregate_ReturnsOk()
    {
        _factory.WorkloadHistoryService
            .Setup(service => service.AggregateAsync(
                It.IsAny<WorkloadHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryAggregateResponse { RecordCount = 0 });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload/history/aggregate");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHistoryTrends_ReturnsOk()
    {
        _factory.WorkloadHistoryService
            .Setup(service => service.TrendsAsync(
                It.IsAny<WorkloadHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryTrendResponse { PointCount = 0, Points = [] });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/workload/history/trends");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static WorkloadHistoryResponse CreateHistory(Guid historyId) => new()
    {
        HistoryId = historyId,
        ExecutionResourceId = Guid.NewGuid(),
        CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        CalculationDate = DateTimeOffset.UtcNow,
        PeriodStart = new DateOnly(2026, 7, 1),
        PeriodEnd = new DateOnly(2026, 7, 7),
        AllocatedHours = 24,
        CapacityHours = 40,
        WorkloadPercentage = 60,
        WorkingDays = 5,
        HolidayDays = 0,
        AvailableDays = 5,
        CalculationVersion = "1.1.0",
        CreatedAt = DateTimeOffset.UtcNow
    };
}

public sealed class WorkloadHistoryApiFactory : WebApplicationFactory<Program>
{
    public Mock<IWorkloadHistoryService> WorkloadHistoryService { get; } = new();

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
            services.RemoveAll<IWorkloadHistoryService>();
            services.AddSingleton(WorkloadHistoryService.Object);
        });
    }
}

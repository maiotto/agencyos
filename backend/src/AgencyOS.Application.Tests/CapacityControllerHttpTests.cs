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

public class CapacityControllerHttpTests : IClassFixture<CapacityApiFactory>
{
    private readonly CapacityApiFactory _factory;

    public CapacityControllerHttpTests(CapacityApiFactory factory)
    {
        _factory = factory;
        _factory.CapacityCalculatorService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.CapacityCalculatorService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CapacityResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildCapacityUrl("/capacity"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/capacity");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            "/capacity?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.CapacityCalculatorService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSummaryResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildCapacityUrl("/capacity/summary"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/capacity/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsOk_WhenResourceExists()
    {
        var resourceId = Guid.NewGuid();
        _factory.CapacityCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCapacityResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildCapacityUrl($"/capacity/{resourceId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.CapacityCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildCapacityUrl($"/capacity/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceIsNotActive()
    {
        var resourceId = Guid.NewGuid();
        _factory.CapacityCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Capacity is calculated only for Active Execution Resources. Resource '{resourceId}' is not active."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildCapacityUrl($"/capacity/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var resourceId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/capacity/{resourceId}?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsBadRequest_WhenOperationalConfigurationMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.CapacityCalculatorService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "No active Resource Availability configuration exists for execution resource 'RES-001' in the planning period. Capacity cannot fall back to Monday–Friday defaults."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildCapacityUrl($"/capacity/{resourceId}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildCapacityUrl(string path)
    {
        return $"{path}?periodStartDate=2026-07-01&periodEndDate=2026-07-07";
    }

    private static CapacityResponse CreateCapacityResponse(Guid resourceId)
    {
        return new CapacityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = "RES-001",
            ExecutionResourceName = "Senior Delivery Consultant",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            TotalCapacityHours = 40,
            AllocatedHours = 10,
            AvailableHours = 30,
            UtilizationPercentage = 25,
            RemainingCapacityHours = 30
        };
    }

    private static CapacitySummaryResponse CreateSummaryResponse()
    {
        return new CapacitySummaryResponse
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            ActiveResourceCount = 1,
            TotalCapacityHours = 40,
            TotalAllocatedHours = 10,
            TotalAvailableHours = 30,
            OverallUtilizationPercentage = 25,
            TotalRemainingCapacityHours = 30
        };
    }
}

public sealed class CapacityApiFactory : WebApplicationFactory<Program>
{
    public Mock<ICapacityCalculatorService> CapacityCalculatorService { get; } = new();

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
            services.RemoveAll<ICapacityCalculatorService>();
            services.AddSingleton(CapacityCalculatorService.Object);
        });
    }
}

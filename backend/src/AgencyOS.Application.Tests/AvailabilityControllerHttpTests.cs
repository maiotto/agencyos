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

public class AvailabilityControllerHttpTests : IClassFixture<AvailabilityApiFactory>
{
    private readonly AvailabilityApiFactory _factory;

    public AvailabilityControllerHttpTests(AvailabilityApiFactory factory)
    {
        _factory = factory;
        _factory.AvailabilityEngineService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.AvailabilityEngineService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AvailabilityResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildAvailabilityUrl("/availability"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/availability");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            "/availability?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_WhenPeriodIsValid()
    {
        _factory.AvailabilityEngineService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSummaryResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildAvailabilityUrl("/availability/summary"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsBadRequest_WhenPeriodIsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/availability/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsOk_WhenResourceExists()
    {
        var resourceId = Guid.NewGuid();
        _factory.AvailabilityEngineService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAvailabilityResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildAvailabilityUrl($"/availability/{resourceId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.AvailabilityEngineService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildAvailabilityUrl($"/availability/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenResourceIsNotActive()
    {
        var resourceId = Guid.NewGuid();
        _factory.AvailabilityEngineService
            .Setup(service => service.GetByResourceIdAsync(
                resourceId,
                It.IsAny<AvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(
                $"Availability is calculated only for Active Execution Resources. Resource '{resourceId}' is not active."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            BuildAvailabilityUrl($"/availability/{resourceId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var resourceId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/availability/{resourceId}?periodStartDate=2026-07-10&periodEndDate=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildAvailabilityUrl(string path)
    {
        return $"{path}?periodStartDate=2026-07-01&periodEndDate=2026-07-07";
    }

    private static AvailabilityResponse CreateAvailabilityResponse(Guid resourceId)
    {
        return new AvailabilityResponse
        {
            ExecutionResourceId = resourceId,
            ExecutionResourceCode = "RES-001",
            ExecutionResourceName = "Senior Delivery Consultant",
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            NextAvailableDate = new DateOnly(2026, 7, 1),
            AvailableHours = 30,
            OccupiedHours = 10,
            AvailabilityPercentage = 75,
            AvailableTimeSlots =
            [
                new AvailabilityTimeSlotResponse
                {
                    StartDate = new DateOnly(2026, 7, 1),
                    EndDate = new DateOnly(2026, 7, 3),
                    AvailableHours = 18
                }
            ]
        };
    }

    private static AvailabilitySummaryResponse CreateSummaryResponse()
    {
        return new AvailabilitySummaryResponse
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            ActiveResourceCount = 1,
            TotalAvailableHours = 30,
            TotalOccupiedHours = 10,
            OverallAvailabilityPercentage = 75,
            ResourcesWithAvailability = 1
        };
    }
}

public sealed class AvailabilityApiFactory : WebApplicationFactory<Program>
{
    public Mock<IAvailabilityEngineService> AvailabilityEngineService { get; } = new();

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
            services.RemoveAll<IAvailabilityEngineService>();
            services.AddSingleton(AvailabilityEngineService.Object);
        });
    }
}

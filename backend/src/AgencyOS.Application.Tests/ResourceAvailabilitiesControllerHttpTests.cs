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
using System.Net.Http.Json;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ResourceAvailabilitiesControllerHttpTests : IClassFixture<ResourceAvailabilityApiFactory>
{
    private readonly ResourceAvailabilityApiFactory _factory;

    public ResourceAvailabilitiesControllerHttpTests(ResourceAvailabilityApiFactory factory)
    {
        _factory = factory;
        _factory.ResourceAvailabilityService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.ResourceAvailabilityService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ResourceAvailabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ResourceAvailabilityResponse>());

        var response = await _factory.CreateClient().GetAsync("/resource-availabilities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/resource-availabilities",
            new CreateResourceAvailabilityRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var id = Guid.NewGuid();
        _factory.ResourceAvailabilityService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateResourceAvailabilityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/resource-availabilities",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsConflict_WhenOverlap()
    {
        var id = Guid.NewGuid();
        _factory.ResourceAvailabilityService
            .Setup(service => service.ActivateAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "Only one Active Availability configuration may exist for the same Resource during the same effective period."));

        var response = await _factory.CreateClient().PostAsync($"/resource-availabilities/{id}/activate", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetOperational_ReturnsBadRequest_WhenConfigurationMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.ResourceAvailabilityService
            .Setup(service => service.GetOperationalAvailabilityAsync(
                resourceId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "No active Resource Availability configuration exists."));

        var response = await _factory.CreateClient().GetAsync(
            $"/resource-availabilities/operational?executionResourceId={resourceId}&date=2026-07-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateResourceAvailabilityRequest CreateValidRequest() => new()
    {
        ExecutionResourceId = Guid.NewGuid(),
        WorkingCalendarId = Guid.NewGuid(),
        WorkingHoursId = Guid.NewGuid(),
        Name = "Standard Availability",
        EffectiveFrom = new DateOnly(2026, 1, 1),
        WeeklyAvailability = WorkingDayNames.Ordered.Select(day => new ResourceAvailabilityWeekDayRequest
        {
            DayOfWeek = day,
            Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
        }).ToList(),
        DailyOverrides = []
    };

    private static ResourceAvailabilityResponse CreateResponse(Guid id) => new()
    {
        Id = id,
        ExecutionResourceId = Guid.NewGuid(),
        WorkingCalendarId = Guid.NewGuid(),
        WorkingHoursId = Guid.NewGuid(),
        Name = "Standard Availability",
        Status = ResourceAvailabilityStatus.Inactive,
        EffectiveFrom = new DateOnly(2026, 1, 1),
        WeeklyAvailability = [],
        DailyOverrides = [],
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}

public class ResourceAvailabilityApiFactory : WebApplicationFactory<Program>
{
    public Mock<IResourceAvailabilityService> ResourceAvailabilityService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=127.0.0.1;Port=5432;Database=agencyos_test;Username=postgres;Password=postgres"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IResourceAvailabilityService>();
            services.AddSingleton(ResourceAvailabilityService.Object);
        });
    }
}

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

public class WorkingHoursControllerHttpTests : IClassFixture<WorkingHoursApiFactory>
{
    private readonly WorkingHoursApiFactory _factory;

    public WorkingHoursControllerHttpTests(WorkingHoursApiFactory factory)
    {
        _factory = factory;
        _factory.WorkingHoursService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.WorkingHoursService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkingHoursQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkingHoursResponse>());

        var response = await _factory.CreateClient().GetAsync("/working-hours");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/working-hours",
            new CreateWorkingHoursRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var id = Guid.NewGuid();
        _factory.WorkingHoursService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateWorkingHoursRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/working-hours",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsConflict_WhenOverlap()
    {
        var id = Guid.NewGuid();
        _factory.WorkingHoursService
            .Setup(service => service.ActivateAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "Only one active Working Hours configuration may exist for a Working Calendar during the same effective period."));

        var response = await _factory.CreateClient().PostAsync($"/working-hours/{id}/activate", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenActive()
    {
        var id = Guid.NewGuid();
        _factory.WorkingHoursService
            .Setup(service => service.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only inactive Working Hours can be deleted."));

        var response = await _factory.CreateClient().DeleteAsync($"/working-hours/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var id = Guid.NewGuid();
        _factory.WorkingHoursService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Working Hours with id '{id}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/working-hours/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static CreateWorkingHoursRequest CreateValidRequest() =>
        new()
        {
            WorkingCalendarId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            Name = "Standard Office Hours",
            EffectiveFrom = new DateOnly(2099, 1, 1),
            Days = WorkingDayNames.DefaultWeekdays
                .Select(day => new WorkingHoursDayRequest
                {
                    DayOfWeek = day,
                    Enabled = true,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(18, 0),
                    BreakStart = new TimeOnly(12, 0),
                    BreakEnd = new TimeOnly(13, 0)
                })
                .ToList()
        };

    private static WorkingHoursResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            WorkingCalendarId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            Name = "Standard Office Hours",
            Status = WorkingHoursStatus.Inactive,
            EffectiveFrom = new DateOnly(2099, 1, 1),
            Days = [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class WorkingHoursApiFactory : WebApplicationFactory<Program>
{
    public Mock<IWorkingHoursService> WorkingHoursService { get; } = new();

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
            services.RemoveAll<IWorkingHoursService>();
            services.AddSingleton(WorkingHoursService.Object);
        });
    }
}

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

public class WorkingCalendarsControllerHttpTests : IClassFixture<WorkingCalendarApiFactory>
{
    private readonly WorkingCalendarApiFactory _factory;

    public WorkingCalendarsControllerHttpTests(WorkingCalendarApiFactory factory)
    {
        _factory = factory;
        _factory.WorkingCalendarService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.WorkingCalendarService
            .Setup(service => service.GetAllAsync(
                It.IsAny<WorkingCalendarQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkingCalendarResponse>());

        var response = await _factory.CreateClient().GetAsync("/working-calendars");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.GetByIdAsync(calendarId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Working Calendar with id '{calendarId}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/working-calendars/{calendarId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/working-calendars",
            new CreateWorkingCalendarRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateWorkingCalendarRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(calendarId));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/working-calendars",
            CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.UpdateAsync(
                calendarId,
                It.IsAny<UpdateWorkingCalendarRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(calendarId));

        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/working-calendars/{calendarId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsConflict_WhenPeriodOverlaps()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.ActivateAsync(calendarId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Only one active calendar may exist for the same period."));

        var response = await _factory.CreateClient().PostAsync(
            $"/working-calendars/{calendarId}/activate",
            null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsNoContent()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.ActivateAsync(calendarId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync(
            $"/working-calendars/{calendarId}/activate",
            null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.DeactivateAsync(calendarId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync(
            $"/working-calendars/{calendarId}/deactivate",
            null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.DeleteAsync(calendarId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().DeleteAsync($"/working-calendars/{calendarId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenActive()
    {
        var calendarId = Guid.NewGuid();
        _factory.WorkingCalendarService
            .Setup(service => service.DeleteAsync(calendarId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only inactive calendars can be deleted."));

        var response = await _factory.CreateClient().DeleteAsync($"/working-calendars/{calendarId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetActive_ReturnsOk_WhenCalendarExists()
    {
        var companyId = Guid.Parse("22222222-2222-4222-8222-000000000001");
        _factory.WorkingCalendarService
            .Setup(service => service.GetActiveForCompanyAsync(
                companyId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(Guid.NewGuid()));

        var response = await _factory.CreateClient().GetAsync(
            $"/working-calendars/active?companyId={companyId}&date=2026-09-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static CreateWorkingCalendarRequest CreateValidCreateRequest() =>
        new()
        {
            CompanyId = Guid.Parse("22222222-2222-4222-8222-000000000001"),
            Name = "Standard Business Week",
            EffectiveFrom = new DateOnly(2026, 8, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };

    private static UpdateWorkingCalendarRequest CreateValidUpdateRequest() =>
        new()
        {
            Name = "Standard Business Week",
            EffectiveFrom = new DateOnly(2026, 8, 1),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };

    private static WorkingCalendarResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            CompanyId = Guid.Parse("22222222-2222-4222-8222-000000000001"),
            Name = "Standard Business Week",
            Status = WorkingCalendarStatus.Inactive,
            EffectiveFrom = new DateOnly(2026, 8, 1),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };
}

public sealed class WorkingCalendarApiFactory : WebApplicationFactory<Program>
{
    public Mock<IWorkingCalendarService> WorkingCalendarService { get; } = new();

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
            services.RemoveAll<IWorkingCalendarService>();
            services.AddSingleton(WorkingCalendarService.Object);
        });
    }
}

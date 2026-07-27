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

namespace AgencyOS.Application.Tests;

public class PlanningTemplatesControllerHttpTests : IClassFixture<PlanningTemplatesApiFactory>
{
    private readonly PlanningTemplatesApiFactory _factory;

    public PlanningTemplatesControllerHttpTests(PlanningTemplatesApiFactory factory)
    {
        _factory = factory;
        _factory.PlanningTemplateService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.PlanningTemplateService
            .Setup(service => service.GetAllAsync(
                It.IsAny<PlanningTemplateQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlanningTemplateResponse>());

        var response = await _factory.CreateClient().GetAsync("/planning-templates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.PlanningTemplateService
            .Setup(service => service.FilterAsync(
                It.IsAny<PlanningTemplateQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlanningTemplateResponse>());

        var response = await _factory.CreateClient().GetAsync("/planning-templates/filter?search=weekly");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.PlanningTemplateService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));

        var response = await _factory.CreateClient().GetAsync($"/planning-templates/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.PlanningTemplateService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreatePlanningTemplateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/planning-templates",
            new CreatePlanningTemplateRequest
            {
                CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                Name = "Standard Weekly",
                WorkingCalendarId = Guid.NewGuid(),
                WorkingHoursId = Guid.NewGuid(),
                DefaultPlanningWindowDays = 7
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _factory.PlanningTemplateService
            .Setup(service => service.ActivateAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/planning-templates/{id}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenActive()
    {
        var id = Guid.NewGuid();
        _factory.PlanningTemplateService
            .Setup(service => service.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Deleting Active Templates is prohibited."));

        var response = await _factory.CreateClient().DeleteAsync($"/planning-templates/{id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Clone_ReturnsCreated()
    {
        var id = Guid.NewGuid();
        var clone = CreateResponse(Guid.NewGuid());
        clone.Name = "Copy";
        _factory.PlanningTemplateService
            .Setup(service => service.CloneAsync(
                id,
                It.IsAny<ClonePlanningTemplateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(clone);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/planning-templates/{id}/clone",
            new ClonePlanningTemplateRequest { Name = "Copy" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Apply_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.PlanningTemplateService
            .Setup(service => service.ApplyAsync(
                id,
                It.IsAny<ApplyPlanningTemplateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppliedPlanningConfigurationResponse
            {
                SourceTemplateId = id,
                SourceTemplateName = "Standard Weekly",
                CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                WorkingCalendarId = Guid.NewGuid(),
                WorkingHoursId = Guid.NewGuid(),
                ResourceAvailabilityStrategy = "RequireActiveConfiguration",
                PeriodStartDate = new DateOnly(2026, 7, 1),
                PeriodEndDate = new DateOnly(2026, 7, 7)
            });

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/planning-templates/{id}/apply",
            new ApplyPlanningTemplateRequest { CalculateCapacity = false });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Apply_ReturnsBadRequest_WhenInactive()
    {
        var id = Guid.NewGuid();
        _factory.PlanningTemplateService
            .Setup(service => service.ApplyAsync(
                id,
                It.IsAny<ApplyPlanningTemplateRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Inactive templates cannot be applied."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/planning-templates/{id}/apply",
            new ApplyPlanningTemplateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static PlanningTemplateResponse CreateResponse(Guid id) => new()
    {
        Id = id,
        CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        Name = "Standard Weekly",
        Status = "Inactive",
        WorkingCalendarId = Guid.NewGuid(),
        WorkingHoursId = Guid.NewGuid(),
        ResourceAvailabilityStrategy = "RequireActiveConfiguration",
        DefaultPlanningWindowDays = 7,
        DefaultPeriodStartOffsetDays = 0,
        IncludeAssignmentDistribution = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}

public sealed class PlanningTemplatesApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPlanningTemplateService> PlanningTemplateService { get; } = new();

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
            services.RemoveAll<IPlanningTemplateService>();
            services.AddSingleton(PlanningTemplateService.Object);
        });
    }
}

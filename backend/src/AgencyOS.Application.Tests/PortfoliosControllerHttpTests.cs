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

public class PortfoliosControllerHttpTests : IClassFixture<PortfoliosApiFactory>
{
    private readonly PortfoliosApiFactory _factory;

    public PortfoliosControllerHttpTests(PortfoliosApiFactory factory)
    {
        _factory = factory;
        _factory.PortfolioService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.PortfolioService
            .Setup(service => service.GetAllAsync(
                It.IsAny<PortfolioQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PortfolioResponse>());

        var response = await _factory.CreateClient().GetAsync("/portfolios");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.PortfolioService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreatePortfolioRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/portfolios",
            new CreatePortfolioRequest
            {
                CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                Name = "Q3 Portfolio",
                PlanningPeriodStart = new DateOnly(2026, 7, 1),
                PlanningPeriodEnd = new DateOnly(2026, 9, 30),
                Missions = [new PortfolioMissionRequest { MissionId = Guid.NewGuid(), Priority = 1 }]
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.PortfolioService
            .Setup(service => service.GetSummaryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioSummaryResponse
            {
                PortfolioId = id,
                Name = "Q3",
                Status = "Active",
                PortfolioHealth = "Healthy"
            });

        var response = await _factory.CreateClient().GetAsync($"/portfolios/{id}/summary");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.PortfolioService
            .Setup(service => service.GetHealthAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioHealthResponse
            {
                PortfolioId = id,
                PortfolioHealth = "Healthy",
                CalculatedAt = DateTimeOffset.UtcNow
            });

        var response = await _factory.CreateClient().GetAsync($"/portfolios/{id}/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenActive()
    {
        var id = Guid.NewGuid();
        _factory.PortfolioService
            .Setup(service => service.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Deleting Active Portfolios is prohibited."));

        var response = await _factory.CreateClient().DeleteAsync($"/portfolios/{id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AssociateMission_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.PortfolioService
            .Setup(service => service.AssociateMissionAsync(
                id,
                It.IsAny<PortfolioMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/portfolios/{id}/missions",
            new PortfolioMissionRequest { MissionId = Guid.NewGuid(), Priority = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static PortfolioResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Q3 Portfolio",
            Status = "Active",
            PlanningPeriodStart = new DateOnly(2026, 7, 1),
            PlanningPeriodEnd = new DateOnly(2026, 9, 30),
            PortfolioHealth = "Unknown",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Missions = []
        };
}

public sealed class PortfoliosApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPortfolioService> PortfolioService { get; } = new();

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
            services.RemoveAll<IPortfolioService>();
            services.AddSingleton(PortfolioService.Object);
        });
    }
}

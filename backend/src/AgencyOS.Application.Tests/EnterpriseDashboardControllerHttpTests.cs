using System.Net;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace AgencyOS.Application.Tests;

public class EnterpriseDashboardControllerHttpTests : IClassFixture<EnterpriseDashboardApiFactory>
{
    private readonly EnterpriseDashboardApiFactory _factory;

    public EnterpriseDashboardControllerHttpTests(EnterpriseDashboardApiFactory factory)
    {
        _factory = factory;
        _factory.EnterpriseDashboardService.Reset();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        EnterpriseDashboardQueryParameters? captured = null;

        _factory.EnterpriseDashboardService
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<EnterpriseDashboardQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new EnterpriseDashboardResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/enterprise-dashboard?companyId={companyId}&periodStart=2026-06-01&periodEnd=2026-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(new DateOnly(2026, 6, 1), captured.PeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 30), captured.PeriodEnd);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardSummaryResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanning_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetPlanningAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPlanningResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/planning");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPortfolio_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetPortfolioAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardPortfolioResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/portfolio");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCapacity_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetCapacityAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardCapacityResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/capacity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkload_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetWorkloadAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardWorkloadResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/workload");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommendations_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetRecommendationsAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardRecommendationsResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/recommendations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDecisions_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetDecisionsAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardDecisionsResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/decisions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAi_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetAiAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAiResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/ai");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAudit_ReturnsOk()
    {
        _factory.EnterpriseDashboardService
            .Setup(service => service.GetAuditAsync(
                It.IsAny<EnterpriseDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnterpriseDashboardAuditResponse());

        var response = await _factory.CreateClient().GetAsync("/enterprise-dashboard/audit");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_ReturnsBadRequest_WhenPeriodEndBeforePeriodStart()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/enterprise-dashboard?periodStart=2026-06-30&periodEnd=2026-06-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class EnterpriseDashboardApiFactory : WebApplicationFactory<Program>
{
    public Mock<IEnterpriseDashboardService> EnterpriseDashboardService { get; } = new();

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
            services.RemoveAll<IEnterpriseDashboardService>();
            services.AddSingleton(EnterpriseDashboardService.Object);
        });
    }
}

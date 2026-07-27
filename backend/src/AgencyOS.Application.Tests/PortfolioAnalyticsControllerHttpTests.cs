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

public class PortfolioAnalyticsControllerHttpTests : IClassFixture<PortfolioAnalyticsApiFactory>
{
    private readonly PortfolioAnalyticsApiFactory _factory;

    public PortfolioAnalyticsControllerHttpTests(PortfolioAnalyticsApiFactory factory)
    {
        _factory = factory;
        _factory.PortfolioAnalyticsService.Reset();
    }

    [Fact]
    public async Task GetOverview_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetOverviewAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioAnalyticsOverviewResponse());

        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOverview_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        PortfolioAnalyticsQueryParameters? captured = null;

        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetOverviewAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<PortfolioAnalyticsQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new PortfolioAnalyticsOverviewResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/portfolio-analytics?companyId={companyId}&periodStart=2026-06-01&periodEnd=2026-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(new DateOnly(2026, 6, 1), captured.PeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 30), captured.PeriodEnd);
    }

    [Fact]
    public async Task GetOverview_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/portfolio-analytics?from=2026-06-30&to=2026-06-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTrends_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetTrendsAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioTrendsResponse());

        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics/trends");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTrends_ScopedToPortfolioId_PassesThrough()
    {
        var portfolioId = Guid.NewGuid();
        PortfolioAnalyticsQueryParameters? captured = null;

        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetTrendsAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<PortfolioAnalyticsQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new PortfolioTrendsResponse());

        var response = await _factory.CreateClient().GetAsync($"/portfolio-analytics/trends?portfolioId={portfolioId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(portfolioId, captured!.PortfolioId);
    }

    [Fact]
    public async Task GetCompare_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetComparisonAsync(It.IsAny<PortfolioCompareQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioComparisonResponse());

        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();

        var response = await _factory.CreateClient().GetAsync(
            $"/portfolio-analytics/compare?leftPortfolioId={leftId}&rightPortfolioId={rightId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompare_ReturnsBadRequest_WhenIdsMissing()
    {
        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics/compare");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRanking_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetRankingAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioRankingResponse());

        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics/ranking");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetHealthAnalyticsAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioHealthAnalyticsResponse());

        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPerformance_ReturnsOk()
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetPerformanceAsync(It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioPerformanceResponse());

        var response = await _factory.CreateClient().GetAsync("/portfolio-analytics/performance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDetail_ReturnsOk_ForGuidRoute()
    {
        var portfolioId = Guid.NewGuid();
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetDetailAsync(portfolioId, It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PortfolioAnalyticsDetailResponse { PortfolioId = portfolioId });

        var response = await _factory.CreateClient().GetAsync($"/portfolio-analytics/{portfolioId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/portfolio-analytics/trends")]
    [InlineData("/portfolio-analytics/compare")]
    [InlineData("/portfolio-analytics/ranking")]
    [InlineData("/portfolio-analytics/health")]
    [InlineData("/portfolio-analytics/performance")]
    public async Task NamedRoutes_AreNotShadowedByGuidRoute(string path)
    {
        _factory.PortfolioAnalyticsService
            .Setup(service => service.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<PortfolioAnalyticsQueryParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("GetDetailAsync should not be invoked for named routes."));

        var response = await _factory.CreateClient().GetAsync(path);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}

public sealed class PortfolioAnalyticsApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPortfolioAnalyticsService> PortfolioAnalyticsService { get; } = new();

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
            services.RemoveAll<IPortfolioAnalyticsService>();
            services.AddSingleton(PortfolioAnalyticsService.Object);
        });
    }
}

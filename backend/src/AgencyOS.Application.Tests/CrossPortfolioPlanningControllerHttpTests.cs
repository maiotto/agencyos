using System.Net;
using System.Net.Http.Json;
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

public class CrossPortfolioPlanningControllerHttpTests : IClassFixture<CrossPortfolioPlanningApiFactory>
{
    private readonly CrossPortfolioPlanningApiFactory _factory;

    public CrossPortfolioPlanningControllerHttpTests(CrossPortfolioPlanningApiFactory factory)
    {
        _factory = factory;
        _factory.CrossPortfolioPlanningService.Reset();
    }

    [Fact]
    public async Task GetOverview_ReturnsOk()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetOverviewAsync(It.IsAny<CrossPortfolioPlanningQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioOverviewResponse());

        var response = await _factory.CreateClient().GetAsync("/cross-portfolio-planning");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOverview_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/cross-portfolio-planning?from=2026-06-30&to=2026-06-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetScenarios_ReturnsOk()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetScenariosAsync(It.IsAny<CrossPortfolioPlanningQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioScenarioListResponse());

        var response = await _factory.CreateClient().GetAsync("/cross-portfolio-planning/scenarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetConflicts_ReturnsOk_WhenPortfolioIdsProvided()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetConflictsAsync(It.IsAny<CrossPortfolioSelectionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConflictSummaryResponse());

        var portfolioId = Guid.NewGuid();
        var response = await _factory.CreateClient().GetAsync(
            $"/cross-portfolio-planning/conflicts?portfolioIds={portfolioId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetConflicts_ReturnsBadRequest_WhenPortfolioIdsMissing()
    {
        var response = await _factory.CreateClient().GetAsync("/cross-portfolio-planning/conflicts");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetConflicts_PassesCommaSeparatedPortfolioIds_Through()
    {
        CrossPortfolioSelectionQueryParameters? captured = null;
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetConflictsAsync(It.IsAny<CrossPortfolioSelectionQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<CrossPortfolioSelectionQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new ConflictSummaryResponse());

        var portfolioIdOne = Guid.NewGuid();
        var portfolioIdTwo = Guid.NewGuid();
        var response = await _factory.CreateClient().GetAsync(
            $"/cross-portfolio-planning/conflicts?portfolioIds={portfolioIdOne},{portfolioIdTwo}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(2, captured!.PortfolioIds.Count);
        Assert.Contains(portfolioIdOne, captured.PortfolioIds);
        Assert.Contains(portfolioIdTwo, captured.PortfolioIds);
    }

    [Fact]
    public async Task GetBalance_ReturnsOk_WhenPortfolioIdsProvided()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetBalanceAsync(It.IsAny<CrossPortfolioSelectionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioBalanceResponse());

        var portfolioId = Guid.NewGuid();
        var response = await _factory.CreateClient().GetAsync(
            $"/cross-portfolio-planning/balance?portfolioIds={portfolioId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBalance_ReturnsBadRequest_WhenPortfolioIdsMissing()
    {
        var response = await _factory.CreateClient().GetAsync("/cross-portfolio-planning/balance");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Simulate_ReturnsOk_ForValidRequest()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.SimulateAsync(It.IsAny<SimulateCrossPortfolioPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimulationSummaryResponse());

        var request = new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()]
        };

        var response = await _factory.CreateClient().PostAsJsonAsync("/cross-portfolio-planning/simulate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Simulate_ReturnsBadRequest_WhenFewerThanTwoPortfolioIds()
    {
        var request = new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid()]
        };

        var response = await _factory.CreateClient().PostAsJsonAsync("/cross-portfolio-planning/simulate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Simulate_ResponseIncludesRequiresHumanApproval()
    {
        var scenarioId = Guid.NewGuid();
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.SimulateAsync(It.IsAny<SimulateCrossPortfolioPlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimulationSummaryResponse
            {
                ScenarioId = scenarioId,
                RequiresHumanApproval = true,
                AdvisoryOnly = true
            });

        var request = new SimulateCrossPortfolioPlanRequest
        {
            PortfolioIds = [Guid.NewGuid(), Guid.NewGuid()]
        };

        var response = await _factory.CreateClient().PostAsJsonAsync("/cross-portfolio-planning/simulate", request);
        var payload = await response.Content.ReadFromJsonAsync<SimulationSummaryResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.True(payload!.RequiresHumanApproval);
        Assert.True(payload.AdvisoryOnly);
    }

    [Fact]
    public async Task Compare_ReturnsOk_ForDistinctScenarioIds()
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.CompareAsync(It.IsAny<CompareCrossPortfolioScenariosRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioComparisonResponse());

        var request = new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = Guid.NewGuid(),
            RightScenarioId = Guid.NewGuid()
        };

        var response = await _factory.CreateClient().PostAsJsonAsync("/cross-portfolio-planning/compare", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Compare_ReturnsBadRequest_WhenScenarioIdsIdentical()
    {
        var scenarioId = Guid.NewGuid();
        var request = new CompareCrossPortfolioScenariosRequest
        {
            LeftScenarioId = scenarioId,
            RightScenarioId = scenarioId
        };

        var response = await _factory.CreateClient().PostAsJsonAsync("/cross-portfolio-planning/compare", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/cross-portfolio-planning/scenarios")]
    [InlineData("/cross-portfolio-planning/conflicts?portfolioIds=" + "00000000-0000-0000-0000-000000000001")]
    [InlineData("/cross-portfolio-planning/balance?portfolioIds=" + "00000000-0000-0000-0000-000000000001")]
    public async Task NamedRoutes_ReturnSuccessfully(string path)
    {
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetScenariosAsync(It.IsAny<CrossPortfolioPlanningQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioScenarioListResponse());
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetConflictsAsync(It.IsAny<CrossPortfolioSelectionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConflictSummaryResponse());
        _factory.CrossPortfolioPlanningService
            .Setup(service => service.GetBalanceAsync(It.IsAny<CrossPortfolioSelectionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CrossPortfolioBalanceResponse());

        var response = await _factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class CrossPortfolioPlanningApiFactory : WebApplicationFactory<Program>
{
    public Mock<ICrossPortfolioPlanningService> CrossPortfolioPlanningService { get; } = new();

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
            services.RemoveAll<ICrossPortfolioPlanningService>();
            services.AddSingleton(CrossPortfolioPlanningService.Object);
        });
    }
}

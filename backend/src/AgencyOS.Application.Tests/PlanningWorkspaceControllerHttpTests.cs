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

public class PlanningWorkspaceControllerHttpTests : IClassFixture<PlanningWorkspaceApiFactory>
{
    private readonly PlanningWorkspaceApiFactory _factory;

    public PlanningWorkspaceControllerHttpTests(PlanningWorkspaceApiFactory factory)
    {
        _factory = factory;
        _factory.PlanningWorkspaceService.Reset();
    }

    [Fact]
    public async Task GetWorkspace_ReturnsOk()
    {
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<PlanningWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync("/planning-workspace");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        PlanningWorkspaceQueryParameters? captured = null;

        _factory.PlanningWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<PlanningWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<PlanningWorkspaceQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new PlanningWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/planning-workspace?companyId={companyId}&periodStart=2026-06-01&periodEnd=2026-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(new DateOnly(2026, 6, 1), captured.PeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 30), captured.PeriodEnd);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenPeriodEndBeforePeriodStart()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/planning-workspace?periodStart=2026-06-30&periodEnd=2026-06-01");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/planning-workspace/overview")]
    [InlineData("/planning-workspace/templates")]
    [InlineData("/planning-workspace/capacity")]
    [InlineData("/planning-workspace/workload")]
    [InlineData("/planning-workspace/portfolios")]
    [InlineData("/planning-workspace/history")]
    [InlineData("/planning-workspace/scenarios")]
    public async Task SectionEndpoints_ReturnOk(string path)
    {
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetOverviewAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningOverviewResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetTemplatesAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningTemplatesSectionResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetCapacityAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningCapacitySectionResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningWorkloadSectionResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetPortfoliosAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningPortfoliosSectionResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetHistoryAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningHistoryResponse());
        _factory.PlanningWorkspaceService
            .Setup(service => service.GetScenariosAsync(It.IsAny<PlanningWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanningScenariosSectionResponse());

        var response = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class PlanningWorkspaceApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPlanningWorkspaceService> PlanningWorkspaceService { get; } = new();

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
            services.RemoveAll<IPlanningWorkspaceService>();
            services.AddSingleton(PlanningWorkspaceService.Object);
        });
    }
}

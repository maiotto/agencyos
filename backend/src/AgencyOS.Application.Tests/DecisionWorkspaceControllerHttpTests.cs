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

public class DecisionWorkspaceControllerHttpTests : IClassFixture<DecisionWorkspaceApiFactory>
{
    private readonly DecisionWorkspaceApiFactory _factory;

    public DecisionWorkspaceControllerHttpTests(DecisionWorkspaceApiFactory factory)
    {
        _factory = factory;
        _factory.DecisionWorkspaceService.Reset();
    }

    [Fact]
    public async Task GetWorkspace_ReturnsOk()
    {
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<DecisionWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync("/decision-workspace");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        var decisionId = Guid.NewGuid();
        DecisionWorkspaceQueryParameters? captured = null;

        _factory.DecisionWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<DecisionWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<DecisionWorkspaceQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new DecisionWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/decision-workspace?companyId={companyId}&decisionId={decisionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(decisionId, captured.DecisionId);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var response = await _factory.CreateClient().GetAsync(
            $"/decision-workspace?from={Uri.EscapeDataString(now.ToString("O"))}&to={Uri.EscapeDataString(now.AddDays(-1).ToString("O"))}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/decision-workspace/overview")]
    [InlineData("/decision-workspace/decisions")]
    [InlineData("/decision-workspace/timeline")]
    [InlineData("/decision-workspace/outcomes")]
    [InlineData("/decision-workspace/audit")]
    [InlineData("/decision-workspace/kpis")]
    public async Task SectionEndpoints_ReturnOk(string path)
    {
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetOverviewAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionOverviewResponse());
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetDecisionsAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionsSectionResponse());
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetTimelineAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionTimelineSectionResponse());
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetOutcomesAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionOutcomesSectionResponse());
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetAuditAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionAuditSectionResponse());
        _factory.DecisionWorkspaceService
            .Setup(service => service.GetKpisAsync(It.IsAny<DecisionWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecisionKpiSummaryResponse());

        var response = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class DecisionWorkspaceApiFactory : WebApplicationFactory<Program>
{
    public Mock<IDecisionWorkspaceService> DecisionWorkspaceService { get; } = new();

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
            services.RemoveAll<IDecisionWorkspaceService>();
            services.AddSingleton(DecisionWorkspaceService.Object);
        });
    }
}

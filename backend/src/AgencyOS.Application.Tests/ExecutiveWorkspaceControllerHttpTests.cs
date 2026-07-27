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

public class ExecutiveWorkspaceControllerHttpTests : IClassFixture<ExecutiveWorkspaceApiFactory>
{
    private readonly ExecutiveWorkspaceApiFactory _factory;

    public ExecutiveWorkspaceControllerHttpTests(ExecutiveWorkspaceApiFactory factory)
    {
        _factory = factory;
        _factory.ExecutiveWorkspaceService.Reset();
    }

    [Fact]
    public async Task GetWorkspace_ReturnsOk()
    {
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<ExecutiveWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync("/executive-workspace");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        ExecutiveWorkspaceQueryParameters? captured = null;

        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<ExecutiveWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutiveWorkspaceQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new ExecutiveWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync($"/executive-workspace?companyId={companyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var response = await _factory.CreateClient().GetAsync(
            $"/executive-workspace?from={Uri.EscapeDataString(now.ToString("O"))}&to={Uri.EscapeDataString(now.AddDays(-1).ToString("O"))}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenPeriodEndBeforePeriodStart()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await _factory.CreateClient().GetAsync(
            $"/executive-workspace?periodStart={today:O}&periodEnd={today.AddDays(-1):O}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/executive-workspace/overview")]
    [InlineData("/executive-workspace/enterprise")]
    [InlineData("/executive-workspace/portfolios")]
    [InlineData("/executive-workspace/recommendations")]
    [InlineData("/executive-workspace/decisions")]
    [InlineData("/executive-workspace/capacity")]
    [InlineData("/executive-workspace/workload")]
    [InlineData("/executive-workspace/ai")]
    [InlineData("/executive-workspace/audit")]
    public async Task SectionEndpoints_ReturnOk(string path)
    {
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetOverviewAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveOverviewResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetEnterpriseAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveEnterpriseSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetPortfoliosAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutivePortfoliosSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetRecommendationsAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveRecommendationsSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetDecisionsAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveDecisionsSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetCapacityAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveCapacitySectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveWorkloadSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetAiAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveAiSectionResponse());
        _factory.ExecutiveWorkspaceService
            .Setup(service => service.GetAuditAsync(It.IsAny<ExecutiveWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveAuditSectionResponse());

        var response = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class ExecutiveWorkspaceApiFactory : WebApplicationFactory<Program>
{
    public Mock<IExecutiveWorkspaceService> ExecutiveWorkspaceService { get; } = new();

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
            services.RemoveAll<IExecutiveWorkspaceService>();
            services.AddSingleton(ExecutiveWorkspaceService.Object);
        });
    }
}

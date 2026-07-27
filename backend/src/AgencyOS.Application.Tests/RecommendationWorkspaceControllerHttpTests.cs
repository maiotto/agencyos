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

public class RecommendationWorkspaceControllerHttpTests : IClassFixture<RecommendationWorkspaceApiFactory>
{
    private readonly RecommendationWorkspaceApiFactory _factory;

    public RecommendationWorkspaceControllerHttpTests(RecommendationWorkspaceApiFactory factory)
    {
        _factory = factory;
        _factory.RecommendationWorkspaceService.Reset();
    }

    [Fact]
    public async Task GetWorkspace_ReturnsOk()
    {
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<RecommendationWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecommendationWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync("/recommendation-workspace");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        RecommendationWorkspaceQueryParameters? captured = null;

        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetWorkspaceAsync(
                It.IsAny<RecommendationWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<RecommendationWorkspaceQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new RecommendationWorkspaceResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/recommendation-workspace?companyId={companyId}&leftRecommendationId={leftId}&rightRecommendationId={rightId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(leftId, captured.LeftRecommendationId);
        Assert.Equal(rightId, captured.RightRecommendationId);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var response = await _factory.CreateClient().GetAsync(
            $"/recommendation-workspace?from={Uri.EscapeDataString(now.ToString("O"))}&to={Uri.EscapeDataString(now.AddDays(-1).ToString("O"))}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsBadRequest_WhenOnlyOneCompareIdProvided()
    {
        var response = await _factory.CreateClient().GetAsync(
            $"/recommendation-workspace?leftRecommendationId={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/recommendation-workspace/overview")]
    [InlineData("/recommendation-workspace/recommendations")]
    [InlineData("/recommendation-workspace/approval")]
    [InlineData("/recommendation-workspace/history")]
    [InlineData("/recommendation-workspace/compare")]
    [InlineData("/recommendation-workspace/ai")]
    [InlineData("/recommendation-workspace/executive-summary")]
    public async Task SectionEndpoints_ReturnOk(string path)
    {
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetOverviewAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecommendationOverviewResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetRecommendationsSectionAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecommendationsSectionResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetApprovalSectionAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApprovalSectionResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetHistorySectionAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HistorySectionResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetCompareSectionAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompareSectionResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetAiSectionAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiSectionResponse());
        _factory.RecommendationWorkspaceService
            .Setup(service => service.GetExecutiveSummaryAsync(It.IsAny<RecommendationWorkspaceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveSummarySectionResponse());

        var response = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class RecommendationWorkspaceApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRecommendationWorkspaceService> RecommendationWorkspaceService { get; } = new();

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
            services.RemoveAll<IRecommendationWorkspaceService>();
            services.AddSingleton(RecommendationWorkspaceService.Object);
        });
    }
}

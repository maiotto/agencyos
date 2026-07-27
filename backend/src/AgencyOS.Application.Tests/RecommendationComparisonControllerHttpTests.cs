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

namespace AgencyOS.Application.Tests;

public class RecommendationComparisonControllerHttpTests : IClassFixture<RecommendationComparisonApiFactory>
{
    private readonly RecommendationComparisonApiFactory _factory;

    public RecommendationComparisonControllerHttpTests(RecommendationComparisonApiFactory factory)
    {
        _factory = factory;
        _factory.ComparisonService.Reset();
    }

    [Fact]
    public async Task Compare_ReturnsOk()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        _factory.ComparisonService
            .Setup(service => service.CompareAsync(
                It.Is<RecommendationComparisonQueryParameters>(parameters =>
                    parameters.LeftId == leftId && parameters.RightId == rightId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(leftId, rightId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/recommendations/compare?leftId={leftId}&rightId={rightId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Compare_ReturnsBadRequest_WhenIdsMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/recommendations/compare");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompareByIds_ReturnsOk()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        _factory.ComparisonService
            .Setup(service => service.CompareByIdsAsync(leftId, rightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(leftId, rightId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/recommendations/compare/{leftId}/{rightId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompareByIds_ReturnsNotFound()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        _factory.ComparisonService
            .Setup(service => service.CompareByIdsAsync(leftId, rightId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("not found"));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/recommendations/compare/{leftId}/{rightId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompareVersions_ReturnsOk()
    {
        _factory.ComparisonService
            .Setup(service => service.CompareVersionsAsync(
                "REC-1",
                It.IsAny<RecommendationVersionComparisonQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(Guid.NewGuid(), Guid.NewGuid()));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            "/recommendations/compare/version/REC-1?leftVersion=1&rightVersion=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static RecommendationComparisonResponse CreateResponse(Guid leftId, Guid rightId) =>
        new()
        {
            HasDifferences = true,
            ScoreDelta = 5m,
            RankDelta = -1,
            VersionDelta = 1,
            Left = new RecommendationHistoryResponse
            {
                Id = leftId,
                RecommendationId = Guid.NewGuid(),
                RecommendationNumber = "REC-1",
                RecommendationVersion = 1,
                Title = "Left",
                EventType = "VersionCreated",
                RecommendationStatus = "Active",
                DecisionEngineVersion = "1.1.0",
                CapacitySnapshot = "{}",
                WorkloadSnapshot = "{}",
                RecommendationPayload = "{}",
                CreatedBy = "tests",
                CreatedAt = DateTimeOffset.UtcNow
            },
            Right = new RecommendationHistoryResponse
            {
                Id = rightId,
                RecommendationId = Guid.NewGuid(),
                RecommendationNumber = "REC-1",
                RecommendationVersion = 2,
                Title = "Right",
                EventType = "VersionCreated",
                RecommendationStatus = "Active",
                DecisionEngineVersion = "1.1.0",
                CapacitySnapshot = "{}",
                WorkloadSnapshot = "{}",
                RecommendationPayload = "{}",
                CreatedBy = "tests",
                CreatedAt = DateTimeOffset.UtcNow
            },
            Differences =
            [
                new RecommendationComparisonFieldDiffResponse
                {
                    Section = "Metadata",
                    Path = "score",
                    LeftValue = "80",
                    RightValue = "85",
                    Changed = true
                }
            ],
            Sections = []
        };
}

public sealed class RecommendationComparisonApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRecommendationComparisonService> ComparisonService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Database=agencyos_test;Username=test;Password=test"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRecommendationComparisonService>();
            services.AddSingleton(ComparisonService.Object);
        });
    }
}

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

public class RecommendationHistoryControllerHttpTests : IClassFixture<RecommendationHistoryApiFactory>
{
    private readonly RecommendationHistoryApiFactory _factory;

    public RecommendationHistoryControllerHttpTests(RecommendationHistoryApiFactory factory)
    {
        _factory = factory;
        _factory.RecommendationHistoryService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.RecommendationHistoryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationHistoryResponse>());

        var response = await _factory.CreateClient().GetAsync("/recommendations/history");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationHistoryService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Recommendation history with id '{id}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/recommendations/history/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVersions_ReturnsOk()
    {
        var recommendationId = Guid.NewGuid();
        _factory.RecommendationHistoryService
            .Setup(service => service.GetVersionsAsync(recommendationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateResponse(Guid.NewGuid(), recommendationId)]);

        var response = await _factory.CreateClient()
            .GetAsync($"/recommendations/history/{recommendationId}/versions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeline_ReturnsOk()
    {
        var recommendationId = Guid.NewGuid();
        _factory.RecommendationHistoryService
            .Setup(service => service.GetTimelineAsync(recommendationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new RecommendationHistoryTimelineEntryResponse
                {
                    Id = Guid.NewGuid(),
                    EventType = "VersionCreated",
                    RecommendationVersion = 1,
                    RecommendationStatus = "Active",
                    CreatedBy = "decision-engine",
                    CreatedAt = DateTimeOffset.UtcNow,
                    Title = "Human + AI"
                }
            ]);

        var response = await _factory.CreateClient()
            .GetAsync($"/recommendations/history/{recommendationId}/timeline");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.RecommendationHistoryService
            .Setup(service => service.FilterAsync(
                It.IsAny<RecommendationHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationHistoryResponse>());

        var response = await _factory.CreateClient().GetAsync("/recommendations/history/filter?search=Alpha");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static RecommendationHistoryResponse CreateResponse(Guid id, Guid recommendationId) =>
        new()
        {
            Id = id,
            RecommendationId = recommendationId,
            RecommendationNumber = "REC-TEST",
            RecommendationVersion = 1,
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DeliveryStrategyId = Guid.NewGuid(),
            Title = "Human + AI",
            EventType = "VersionCreated",
            RecommendationStatus = "Active",
            DecisionEngineVersion = "1.1.0-decision-engine",
            CapacitySnapshot = "{}",
            WorkloadSnapshot = "{}",
            RecommendationPayload = "{}",
            CreatedBy = "decision-engine",
            CreatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class RecommendationHistoryApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRecommendationHistoryService> RecommendationHistoryService { get; } = new();

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
            services.RemoveAll<IRecommendationHistoryService>();
            services.AddSingleton(RecommendationHistoryService.Object);
        });
    }
}

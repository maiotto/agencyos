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

public class AIRecommendationsControllerHttpTests : IClassFixture<AIRecommendationApiFactory>
{
    private readonly AIRecommendationApiFactory _factory;

    public AIRecommendationsControllerHttpTests(AIRecommendationApiFactory factory)
    {
        _factory = factory;
        _factory.Service.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.Service
            .Setup(service => service.GetAllAsync(
                It.IsAny<AIRecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AIRecommendationResponse>());

        var response = await _factory.CreateClient().GetAsync("/ai-recommendations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/ai-recommendations/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Generate_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.Service
            .Setup(service => service.GenerateAsync(
                It.IsAny<GenerateAIRecommendationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/ai-recommendations/generate",
            new GenerateAIRecommendationRequest
            {
                RecommendationId = Guid.NewGuid(),
                GeneratedBy = "planner"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveAndCompareAndByRecommendation_ReturnOk()
    {
        var id = Guid.NewGuid();
        var recommendationId = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.ArchiveAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));
        _factory.Service
            .Setup(service => service.CompareAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIRecommendationComparisonResponse
            {
                AIRecommendation = CreateResponse(id),
                RecommendationId = recommendationId,
                RecommendationTitle = "Human + AI",
                RecommendationVersion = 1,
                HasDifferences = true,
                Differences = []
            });
        _factory.Service
            .Setup(service => service.GetByRecommendationIdAsync(
                recommendationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateResponse(id) });

        var client = _factory.CreateClient();
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsync($"/ai-recommendations/{id}/archive", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync($"/ai-recommendations/{id}/compare")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync($"/ai-recommendations/recommendation/{recommendationId}")).StatusCode);
    }

    private static AIRecommendationResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            RecommendationId = Guid.NewGuid(),
            RecommendationVersion = 1,
            GenerationVersion = 1,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = "planner",
            ConfidenceScore = 80m,
            ExecutiveSummary = "summary",
            Reasoning = "reasoning",
            Assumptions = "[]",
            Risks = "[]",
            Alternatives = "[]",
            SuggestedDeliveryStrategy = "Balanced",
            SuggestedCapacityImpact = "{}",
            SuggestedWorkloadImpact = "{}",
            ModelVersion = "1.1.0-ai-advisor",
            PromptVersion = "1.0.0-deterministic-advisor",
            Status = "Active"
        };
}

public sealed class AIRecommendationApiFactory : WebApplicationFactory<Program>
{
    public Mock<IAIRecommendationService> Service { get; } = new();

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
            services.RemoveAll<IAIRecommendationService>();
            services.AddSingleton(Service.Object);
        });
    }
}

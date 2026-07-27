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

public class ExplainabilityControllerHttpTests : IClassFixture<ExplainabilityApiFactory>
{
    private readonly ExplainabilityApiFactory _factory;

    public ExplainabilityControllerHttpTests(ExplainabilityApiFactory factory)
    {
        _factory = factory;
        _factory.Service.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.Service
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExplainabilityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExplainabilityResponse>());

        var response = await _factory.CreateClient().GetAsync("/explainability");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/explainability/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Generate_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.Service
            .Setup(service => service.GenerateAsync(
                It.IsAny<GenerateExplainabilityRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/explainability/generate",
            new GenerateExplainabilityRequest
            {
                RecommendationId = Guid.NewGuid(),
                GeneratedBy = "planner"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveAndByRecommendation_ReturnOk()
    {
        var id = Guid.NewGuid();
        var recommendationId = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.ArchiveAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));
        _factory.Service
            .Setup(service => service.GetByRecommendationIdAsync(
                recommendationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateResponse(id) });

        var client = _factory.CreateClient();
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsync($"/explainability/{id}/archive", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync($"/explainability/recommendation/{recommendationId}")).StatusCode);
    }

    private static ExplainabilityResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            RecommendationId = Guid.NewGuid(),
            ExplanationType = "Recommendation",
            GenerationVersion = 1,
            ExecutiveSummary = "summary",
            DetailedExplanation = "detailed",
            DecisionFactors = "[]",
            Assumptions = "[]",
            Risks = "[]",
            ConfidenceExplanation = "confidence",
            CapacityExplanation = "capacity",
            WorkloadExplanation = "workload",
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = "planner",
            ModelVersion = "1.1.0-explainability",
            PromptVersion = "1.0.0-deterministic-explainer",
            Status = "Active"
        };
}

public sealed class ExplainabilityApiFactory : WebApplicationFactory<Program>
{
    public Mock<IExplainabilityService> Service { get; } = new();

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
            services.RemoveAll<IExplainabilityService>();
            services.AddSingleton(Service.Object);
        });
    }
}

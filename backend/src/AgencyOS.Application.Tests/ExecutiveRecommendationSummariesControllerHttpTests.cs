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

public class ExecutiveRecommendationSummariesControllerHttpTests
    : IClassFixture<ExecutiveRecommendationSummaryApiFactory>
{
    private readonly ExecutiveRecommendationSummaryApiFactory _factory;

    public ExecutiveRecommendationSummariesControllerHttpTests(
        ExecutiveRecommendationSummaryApiFactory factory)
    {
        _factory = factory;
        _factory.Service.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.Service
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExecutiveRecommendationSummaryResponse>());

        var response = await _factory.CreateClient().GetAsync("/executive-summaries");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/executive-summaries/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Generate_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.Service
            .Setup(service => service.GenerateAsync(
                It.IsAny<GenerateExecutiveRecommendationSummaryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/executive-summaries/generate",
            new GenerateExecutiveRecommendationSummaryRequest
            {
                RecommendationId = Guid.NewGuid(),
                GeneratedBy = "exec"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveVersionsCompareAndByRecommendation_ReturnExpected()
    {
        var id = Guid.NewGuid();
        var recommendationId = Guid.NewGuid();
        _factory.Service
            .Setup(service => service.ArchiveAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));
        _factory.Service
            .Setup(service => service.CreateNewVersionAsync(
                id,
                It.IsAny<CreateExecutiveRecommendationSummaryVersionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(Guid.NewGuid()));
        _factory.Service
            .Setup(service => service.CompareAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutiveRecommendationSummaryComparisonResponse
            {
                Summary = CreateResponse(id),
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
            (await client.PostAsync($"/executive-summaries/{id}/archive", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(
                $"/executive-summaries/{id}/versions",
                new CreateExecutiveRecommendationSummaryVersionRequest { GeneratedBy = "exec" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync($"/executive-summaries/{id}/compare")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync($"/executive-summaries/recommendation/{recommendationId}")).StatusCode);
    }

    private static ExecutiveRecommendationSummaryResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            RecommendationId = Guid.NewGuid(),
            SummaryVersion = 1,
            ExecutiveSummary = "summary",
            KeyDecisionFactors = "[]",
            BusinessImpact = "business",
            CapacityImpact = "capacity",
            WorkloadImpact = "workload",
            Risks = "[]",
            Assumptions = "[]",
            ConfidenceLevel = 80m,
            RecommendedActions = "[]",
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = "exec",
            ModelVersion = "1.1.0-executive-briefing",
            PromptVersion = "1.0.0-deterministic-executive-summary",
            Status = "Active"
        };
}

public sealed class ExecutiveRecommendationSummaryApiFactory : WebApplicationFactory<Program>
{
    public Mock<IExecutiveRecommendationSummaryService> Service { get; } = new();

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
            services.RemoveAll<IExecutiveRecommendationSummaryService>();
            services.AddSingleton(Service.Object);
        });
    }
}

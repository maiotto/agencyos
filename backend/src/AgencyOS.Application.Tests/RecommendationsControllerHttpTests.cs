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

public class RecommendationsControllerHttpTests : IClassFixture<RecommendationsApiFactory>
{
    private readonly RecommendationsApiFactory _factory;

    public RecommendationsControllerHttpTests(RecommendationsApiFactory factory)
    {
        _factory = factory;
        _factory.RecommendationService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.RecommendationService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationResponse>());

        var response = await _factory.CreateClient().GetAsync("/recommendations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.RecommendationService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateRecommendationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/recommendations",
            new CreateRecommendationRequest
            {
                CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                MissionId = Guid.NewGuid(),
                ContractId = Guid.NewGuid(),
                DeliveryStrategyId = Guid.NewGuid(),
                Title = "Human + AI",
                RecommendationPayload = "{\"ok\":true}",
                CapacitySnapshot = "{}",
                WorkloadSnapshot = "{}",
                GeneratedBy = "planner@agencyos.local"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Archive_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationService
            .Setup(service => service.ArchiveAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id, archived: true));

        var response = await _factory.CreateClient().PostAsync($"/recommendations/{id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Restore_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationService
            .Setup(service => service.RestoreAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id));

        var response = await _factory.CreateClient().PostAsync($"/recommendations/{id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Recommendation with id '{id}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/recommendations/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVersions_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationService
            .Setup(service => service.GetVersionsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateResponse(id)]);

        var response = await _factory.CreateClient().GetAsync($"/recommendations/{id}/versions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static RecommendationResponse CreateResponse(Guid id, bool archived = false) =>
        new()
        {
            Id = id,
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DeliveryStrategyId = Guid.NewGuid(),
            RecommendationNumber = "REC-TEST",
            Title = "Human + AI",
            Status = archived ? "Archived" : "Active",
            Version = 1,
            DecisionEngineVersion = "1.1.0-decision-engine",
            CapacitySnapshot = "{}",
            WorkloadSnapshot = "{}",
            RecommendationPayload = "{}",
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = "decision-engine",
            Archived = archived,
            CreatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class RecommendationsApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRecommendationService> RecommendationService { get; } = new();

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
            services.RemoveAll<IRecommendationService>();
            services.AddSingleton(RecommendationService.Object);
        });
    }
}

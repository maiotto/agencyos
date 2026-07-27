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

public class DecisionsControllerHttpTests : IClassFixture<DecisionApiFactory>
{
    private readonly DecisionApiFactory _factory;

    public DecisionsControllerHttpTests(DecisionApiFactory factory)
    {
        _factory = factory;
        _factory.DecisionService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.DecisionService
            .Setup(service => service.GetAllAsync(
                It.IsAny<DecisionQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DecisionResponse>());

        var response = await _factory.CreateClient().GetAsync("/decisions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.DecisionService
            .Setup(service => service.FilterAsync(
                It.IsAny<DecisionQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DecisionResponse>());

        var response = await _factory.CreateClient().GetAsync("/decisions/filter?search=test");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.DecisionService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/decisions/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeline_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.DecisionService
            .Setup(service => service.GetTimelineAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DecisionTimelineEntryResponse>());

        var response = await _factory.CreateClient().GetAsync($"/decisions/{id}/timeline");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.DecisionService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateDecisionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/decisions",
            new CreateDecisionRequest
            {
                RecommendationId = Guid.NewGuid(),
                CreatedBy = "planner"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task StartCompleteCancelOutcome_ReturnOk()
    {
        var id = Guid.NewGuid();
        var responseBody = CreateResponse(id);
        _factory.DecisionService
            .Setup(service => service.StartImplementationAsync(
                id,
                It.IsAny<DecisionActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseBody);
        _factory.DecisionService
            .Setup(service => service.CompleteAsync(
                id,
                It.IsAny<DecisionActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseBody);
        _factory.DecisionService
            .Setup(service => service.CancelAsync(
                id,
                It.IsAny<DecisionActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseBody);
        _factory.DecisionService
            .Setup(service => service.RecordOutcomeAsync(
                id,
                It.IsAny<RecordDecisionOutcomeRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseBody);

        var client = _factory.CreateClient();
        var action = new DecisionActionRequest { Actor = "planner" };

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/decisions/{id}/start", action)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/decisions/{id}/complete", action)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/decisions/{id}/cancel", action)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync(
                $"/decisions/{id}/outcome",
                new RecordDecisionOutcomeRequest
                {
                    Outcome = "Done",
                    Actor = "planner"
                })).StatusCode);
    }

    private static DecisionResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            RecommendationId = Guid.NewGuid(),
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DecisionStatus = "Created",
            ImplementationStatus = "NotStarted",
            DecisionDate = DateTimeOffset.UtcNow,
            CreatedBy = "planner",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Timeline = []
        };
}

public sealed class DecisionApiFactory : WebApplicationFactory<Program>
{
    public Mock<IDecisionService> DecisionService { get; } = new();

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
            services.RemoveAll<IDecisionService>();
            services.RemoveAll<IDecisionTimelineService>();
            services.AddSingleton(DecisionService.Object);
        });
    }
}

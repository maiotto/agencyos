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

public class RecommendationWorkflowControllerHttpTests : IClassFixture<RecommendationWorkflowApiFactory>
{
    private readonly RecommendationWorkflowApiFactory _factory;

    public RecommendationWorkflowControllerHttpTests(RecommendationWorkflowApiFactory factory)
    {
        _factory = factory;
        _factory.RecommendationWorkflowService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.RecommendationWorkflowService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationWorkflowResponse>());

        var response = await _factory.CreateClient().GetAsync("/recommendations/workflow");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var created = CreateResponse(Guid.NewGuid());
        _factory.RecommendationWorkflowService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateRecommendationWorkflowRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/recommendations/workflow",
            new CreateRecommendationWorkflowRequest
            {
                RecommendationId = Guid.NewGuid(),
                CreatedBy = "planner@agencyos.local"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationWorkflowService
            .Setup(service => service.SubmitAsync(
                id,
                It.IsAny<RecommendationWorkflowActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id, "PendingApproval"));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/recommendations/workflow/{id}/submit",
            new RecommendationWorkflowActionRequest { Actor = "planner" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ReturnsBadRequest_WhenNotPending()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationWorkflowService
            .Setup(service => service.ApproveAsync(
                id,
                It.IsAny<ApproveRecommendationWorkflowRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "Transition from 'Draft' to 'Approved' is not allowed."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/recommendations/workflow/{id}/approve",
            new ApproveRecommendationWorkflowRequest { Approver = "approver" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeline_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationWorkflowService
            .Setup(service => service.GetTimelineAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationWorkflowTransitionResponse>());

        var response = await _factory.CreateClient().GetAsync($"/recommendations/workflow/{id}/timeline");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationWorkflowService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Recommendation workflow with id '{id}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/recommendations/workflow/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Cancel_Reopen_ReturnOk()
    {
        var id = Guid.NewGuid();
        _factory.RecommendationWorkflowService
            .Setup(service => service.RejectAsync(
                id,
                It.IsAny<RecommendationWorkflowActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id, "Rejected"));
        _factory.RecommendationWorkflowService
            .Setup(service => service.CancelAsync(
                id,
                It.IsAny<RecommendationWorkflowActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id, "Cancelled"));
        _factory.RecommendationWorkflowService
            .Setup(service => service.ReopenAsync(
                id,
                It.IsAny<RecommendationWorkflowActionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(id, "Reopened"));

        var client = _factory.CreateClient();
        var action = new RecommendationWorkflowActionRequest { Actor = "user" };

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/recommendations/workflow/{id}/reject", action)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/recommendations/workflow/{id}/cancel", action)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsJsonAsync($"/recommendations/workflow/{id}/reopen", action)).StatusCode);
    }

    private static RecommendationWorkflowResponse CreateResponse(Guid id, string status = "Draft") =>
        new()
        {
            Id = id,
            RecommendationId = Guid.NewGuid(),
            DeliveryStrategyId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            Title = "Balanced delivery mix",
            Status = status,
            CreatedBy = "planner@agencyos.local",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class RecommendationWorkflowApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRecommendationWorkflowService> RecommendationWorkflowService { get; } = new();

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
            services.RemoveAll<IRecommendationWorkflowService>();
            services.AddSingleton(RecommendationWorkflowService.Object);
        });
    }
}

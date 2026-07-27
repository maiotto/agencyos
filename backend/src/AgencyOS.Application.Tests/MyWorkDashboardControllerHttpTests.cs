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

public class MyWorkDashboardControllerHttpTests : IClassFixture<MyWorkDashboardApiFactory>
{
    private readonly MyWorkDashboardApiFactory _factory;

    public MyWorkDashboardControllerHttpTests(MyWorkDashboardApiFactory factory)
    {
        _factory = factory;
        _factory.MyWorkDashboardService.Reset();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkDashboardResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_PassesQueryParametersThrough()
    {
        var companyId = Guid.NewGuid();
        var executionResourceId = Guid.NewGuid();
        MyWorkDashboardQueryParameters? captured = null;

        _factory.MyWorkDashboardService
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<MyWorkDashboardQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new MyWorkDashboardResponse());

        var response = await _factory.CreateClient().GetAsync(
            $"/my-work?companyId={companyId}&userId=alice&executionResourceId={executionResourceId}"
            + "&periodStart=2026-06-01&periodEnd=2026-06-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal("alice", captured.UserId);
        Assert.Equal(executionResourceId, captured.ExecutionResourceId);
        Assert.Equal(new DateOnly(2026, 6, 1), captured.PeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 30), captured.PeriodEnd);
    }

    [Fact]
    public async Task GetDashboard_ReturnsBadRequest_WhenPeriodEndBeforePeriodStart()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/my-work?periodStart=2026-06-30&periodEnd=2026-06-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_ReturnsBadRequest_WhenToBeforeFrom()
    {
        var response = await _factory.CreateClient().GetAsync(
            "/my-work?from=2026-06-30&to=2026-06-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSummary_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkSummaryResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetTasksAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkTasksResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMissions_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetMissionsAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkMissionsResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/missions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommendations_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetRecommendationsAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkRecommendationsResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/recommendations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDecisions_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetDecisionsAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkDecisionsResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/decisions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetActivity_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetActivityAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkActivityResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/activity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetKpis_ReturnsOk()
    {
        _factory.MyWorkDashboardService
            .Setup(service => service.GetKpisAsync(
                It.IsAny<MyWorkDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkKpiSummaryResponse());

        var response = await _factory.CreateClient().GetAsync("/my-work/kpis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class MyWorkDashboardApiFactory : WebApplicationFactory<Program>
{
    public Mock<IMyWorkDashboardService> MyWorkDashboardService { get; } = new();

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
            services.RemoveAll<IMyWorkDashboardService>();
            services.AddSingleton(MyWorkDashboardService.Object);
        });
    }
}

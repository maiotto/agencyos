using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
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

public class PersonalProductivityDashboardControllerHttpTests
    : IClassFixture<PersonalProductivityDashboardApiFactory>
{
    private readonly PersonalProductivityDashboardApiFactory _factory;

    public PersonalProductivityDashboardControllerHttpTests(PersonalProductivityDashboardApiFactory factory)
    {
        _factory = factory;
        _factory.Service.Reset();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        _factory.Service
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivityDashboardResponse
            {
                CompanyId = AgencyOSCompanies.DefaultCompanyId,
                UserId = "planner",
                GeneratedAt = DateTimeOffset.UtcNow
            });

        var response = await _factory.CreateClient().GetAsync("/personal-dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SectionEndpoints_ReturnOk()
    {
        _factory.Service
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivitySummaryResponse { UserId = "planner" });
        _factory.Service
            .Setup(service => service.GetKpisAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivityKpiResponse());
        _factory.Service
            .Setup(service => service.GetTrendsAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivityTrendsResponse());
        _factory.Service
            .Setup(service => service.GetCapacityAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkCapacitySummaryResponse());
        _factory.Service
            .Setup(service => service.GetWorkloadAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkWorkloadSummaryResponse());
        _factory.Service
            .Setup(service => service.GetActivityAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivityActivitySummaryResponse());
        _factory.Service
            .Setup(service => service.GetStatisticsAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonalProductivityStatisticsResponse());

        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/kpis")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/trends")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/capacity")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/workload")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/activity")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/personal-dashboard/statistics")).StatusCode);
    }

    [Fact]
    public async Task GetDashboard_ReturnsNotFound_WhenCompanyMissing()
    {
        _factory.Service
            .Setup(service => service.GetDashboardAsync(
                It.IsAny<PersonalProductivityDashboardQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing company"));

        var response = await _factory.CreateClient().GetAsync("/personal-dashboard");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public sealed class PersonalProductivityDashboardApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPersonalProductivityDashboardService> Service { get; } = new();

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
            services.RemoveAll<IPersonalProductivityDashboardService>();
            services.AddSingleton(Service.Object);
        });
    }
}

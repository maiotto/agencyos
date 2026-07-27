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

public class AuditControllerHttpTests : IClassFixture<AuditApiFactory>
{
    private readonly AuditApiFactory _factory;

    public AuditControllerHttpTests(AuditApiFactory factory)
    {
        _factory = factory;
        _factory.AuditQueryService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.AuditQueryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<AuditEventQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());

        var response = await _factory.CreateClient().GetAsync("/audit");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.AuditQueryService
            .Setup(service => service.FilterAsync(
                It.IsAny<AuditEventQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());

        var response = await _factory.CreateClient().GetAsync("/audit/filter?search=decision");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.AuditQueryService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/audit/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByEntityCorrelationUserCompany_ReturnOk()
    {
        _factory.AuditQueryService
            .Setup(service => service.GetByEntityIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());
        _factory.AuditQueryService
            .Setup(service => service.GetByCorrelationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());
        _factory.AuditQueryService
            .Setup(service => service.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());
        _factory.AuditQueryService
            .Setup(service => service.GetByCompanyIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEventResponse>());

        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/audit/entity/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/audit/correlation/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/audit/user/planner")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/audit/company/{Guid.NewGuid()}")).StatusCode);
    }
}

public sealed class AuditApiFactory : WebApplicationFactory<Program>
{
    public Mock<IAuditQueryService> AuditQueryService { get; } = new();

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
            services.RemoveAll<IAuditQueryService>();
            services.AddSingleton(AuditQueryService.Object);
        });
    }
}

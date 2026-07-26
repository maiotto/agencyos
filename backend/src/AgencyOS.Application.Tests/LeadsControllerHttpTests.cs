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
using System.Net.Http.Json;

namespace AgencyOS.Application.Tests;

public class LeadsControllerHttpTests : IClassFixture<LeadApiFactory>
{
    private readonly LeadApiFactory _factory;

    public LeadsControllerHttpTests(LeadApiFactory factory)
    {
        _factory = factory;
        _factory.LeadService.Reset();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenLeadExists()
    {
        var leadId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadResponse
            {
                Id = leadId,
                Code = "LED-TEST001",
                CompanyName = "Acme",
                LeadName = "Alex",
                Source = "Referral",
                Status = LeadStatus.Prospect
            });

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/leads/{leadId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<LeadResponse>();
        Assert.NotNull(payload);
        Assert.Equal(leadId, payload!.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenLeadMissing()
    {
        var leadId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Lead with id '{leadId}' was not found."));

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/leads/{leadId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/leads", new CreateLeadRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var leadId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.CreateAsync(It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadResponse
            {
                Id = leadId,
                Code = "LED-TEST002",
                CompanyName = "Acme",
                LeadName = "Alex",
                Source = "Referral",
                Status = LeadStatus.Prospect
            });

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/leads", new CreateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = "Alex",
            Source = "Referral",
            Status = LeadStatus.Prospect
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Convert_ReturnsOk_WhenConversionSucceeds()
    {
        var leadId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.ConvertAsync(
                leadId,
                It.IsAny<ConvertLeadRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConvertLeadResponse
            {
                ClientId = clientId,
                Lead = new LeadResponse
                {
                    Id = leadId,
                    Code = "LED-TEST003",
                    CompanyName = "Acme",
                    LeadName = "Alex",
                    Source = "Referral",
                    Status = LeadStatus.Converted,
                    ClientId = clientId
                }
            });

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/leads/{leadId}/convert", new ConvertLeadRequest
        {
            LegalName = "Acme Corporation Ltd",
            TaxIdentifier = "12.345.678/0001-90"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ConvertLeadResponse>();
        Assert.NotNull(payload);
        Assert.Equal(clientId, payload!.ClientId);
    }

    [Fact]
    public async Task Convert_ReturnsBadRequest_WhenBusinessRuleFails()
    {
        var leadId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.ConvertAsync(
                leadId,
                It.IsAny<ConvertLeadRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only Leads in status Won can be converted."));

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/leads/{leadId}/convert", new ConvertLeadRequest
        {
            LegalName = "Acme Corporation Ltd",
            TaxIdentifier = "12.345.678/0001-90"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Archive_ReturnsNoContent_WhenArchiveSucceeds()
    {
        var leadId = Guid.NewGuid();
        _factory.LeadService
            .Setup(service => service.ArchiveAsync(leadId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/leads/{leadId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateEmail()
    {
        _factory.LeadService
            .Setup(service => service.CreateAsync(It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("A Lead with email 'alex@acme.example' already exists."));

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/leads", new CreateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = "Alex",
            Email = "alex@acme.example",
            Source = "Referral",
            Status = LeadStatus.Prospect
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

public sealed class LeadApiFactory : WebApplicationFactory<Program>
{
    public Mock<ILeadService> LeadService { get; } = new();

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
            services.RemoveAll<ILeadService>();
            services.AddSingleton(LeadService.Object);
        });
    }
}

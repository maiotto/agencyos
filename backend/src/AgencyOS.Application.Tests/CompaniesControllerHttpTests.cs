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

public class CompaniesControllerHttpTests : IClassFixture<CompanyApiFactory>
{
    private readonly CompanyApiFactory _factory;

    public CompaniesControllerHttpTests(CompanyApiFactory factory)
    {
        _factory = factory;
        _factory.CompanyService.Reset();
        _factory.CompanyContextService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.CompanyService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CompanyQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyResponse>());

        var response = await _factory.CreateClient().GetAsync("/companies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Company with id '{companyId}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/companies/{companyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetActive_ReturnsOk()
    {
        _factory.CompanyContextService
            .Setup(service => service.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(Guid.NewGuid()));

        var response = await _factory.CreateClient().GetAsync("/companies/active");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/companies",
            new CreateCompanyRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateCompanyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(companyId));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/companies",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateCode()
    {
        _factory.CompanyService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateCompanyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("A Company with CompanyCode 'ACME' already exists (BR-2002)."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/companies",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.UpdateAsync(
                companyId,
                It.IsAny<UpdateCompanyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(companyId));

        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/companies/{companyId}",
            new UpdateCompanyRequest { CompanyName = "Updated", Timezone = "UTC" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsNoContent()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.ActivateAsync(companyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/companies/{companyId}/activate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.DeactivateAsync(companyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/companies/{companyId}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Archive_ReturnsNoContent()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyService
            .Setup(service => service.ArchiveAsync(companyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/companies/{companyId}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Select_ReturnsNoContent_WhenValid()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyContextService
            .Setup(service => service.SelectAsync(companyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/companies/select",
            new SelectCompanyRequest { CompanyId = companyId });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Select_ReturnsBadRequest_WhenCompanyNotSelectable_BR2003()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyContextService
            .Setup(service => service.SelectAsync(companyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only Active Companies may be selected (BR-2003)."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/companies/select",
            new SelectCompanyRequest { CompanyId = companyId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithCompanyIdHeader_InvokesCompanyContextMiddleware_ReturnsBadRequestWhenInvalid()
    {
        var companyId = Guid.NewGuid();
        _factory.CompanyContextService
            .Setup(service => service.SelectAsync(companyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Company with id '{companyId}' was not found."));

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Company-Id", companyId.ToString());
        _factory.CompanyService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CompanyQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyResponse>());

        var response = await client.GetAsync("/companies");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestWithMalformedCompanyIdHeader_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Company-Id", "not-a-guid");
        _factory.CompanyService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CompanyQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyResponse>());

        var response = await client.GetAsync("/companies");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateCompanyRequest CreateValidRequest() =>
        new()
        {
            CompanyCode = "ACME",
            CompanyName = "Acme Agency",
            Timezone = "UTC"
        };

    private static CompanyResponse CreateResponse(Guid companyId) =>
        new()
        {
            Id = companyId,
            CompanyCode = "ACME",
            CompanyName = "Acme Agency",
            Status = CompanyStatus.Active,
            Timezone = "UTC",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class CompanyApiFactory : WebApplicationFactory<Program>
{
    public Mock<ICompanyService> CompanyService { get; } = new();
    public Mock<ICompanyContextService> CompanyContextService { get; } = new();

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
            services.RemoveAll<ICompanyService>();
            services.AddSingleton(CompanyService.Object);
            services.RemoveAll<ICompanyContextService>();
            services.AddSingleton(CompanyContextService.Object);
        });
    }
}

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

public class ClientsControllerHttpTests : IClassFixture<ClientApiFactory>
{
    private readonly ClientApiFactory _factory;

    public ClientsControllerHttpTests(ClientApiFactory factory)
    {
        _factory = factory;
        _factory.ClientService.Reset();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk()
    {
        _factory.ClientService
            .Setup(service => service.GetPagedAsync(
                It.IsAny<ClientQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<ClientResponse>
            {
                Items = [],
                Page = 1,
                PageSize = 20,
                TotalCount = 0,
                TotalPages = 0
            });

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/clients?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenClientExists()
    {
        var clientId = Guid.NewGuid();
        _factory.ClientService
            .Setup(service => service.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientResponse
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/clients/{clientId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenClientMissing()
    {
        var clientId = Guid.NewGuid();
        _factory.ClientService
            .Setup(service => service.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Client with id '{clientId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/clients/{clientId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/clients", new CreateClientRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var clientId = Guid.NewGuid();
        _factory.ClientService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateClientRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientResponse
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/clients", new CreateClientRequest
        {
            LegalName = "Acme",
            Status = ClientStatus.Active
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenTaxIdentifierDuplicates()
    {
        _factory.ClientService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateClientRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("A Client with tax identifier '12-3456789' already exists."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/clients", new CreateClientRequest
        {
            LegalName = "Acme",
            TaxIdentifier = "12-3456789",
            Status = ClientStatus.Active
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var clientId = Guid.NewGuid();
        _factory.ClientService
            .Setup(service => service.UpdateAsync(
                clientId,
                It.IsAny<UpdateClientRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientResponse
            {
                Id = clientId,
                LegalName = "Acme Updated",
                Status = ClientStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/clients/{clientId}", new UpdateClientRequest
        {
            LegalName = "Acme Updated",
            Status = ClientStatus.Active
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var clientId = Guid.NewGuid();
        _factory.ClientService
            .Setup(service => service.DeactivateAsync(clientId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/clients/{clientId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

public sealed class ClientApiFactory : WebApplicationFactory<Program>
{
    public Mock<IClientService> ClientService { get; } = new();

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
            services.RemoveAll<IClientService>();
            services.AddSingleton(ClientService.Object);
        });
    }
}

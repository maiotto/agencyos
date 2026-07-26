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

public class ContractsControllerHttpTests : IClassFixture<ContractApiFactory>
{
    private readonly ContractApiFactory _factory;

    public ContractsControllerHttpTests(ContractApiFactory factory)
    {
        _factory = factory;
        _factory.ContractService.Reset();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk()
    {
        _factory.ContractService
            .Setup(service => service.GetPagedAsync(
                It.IsAny<ContractQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<ContractResponse>
            {
                Items = [],
                Page = 1,
                PageSize = 20,
                TotalCount = 0,
                TotalPages = 0
            });

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/contracts?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenContractExists()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                ClientId = Guid.NewGuid(),
                ContractCode = "CTR-2026-001",
                ContractName = "Acme Retainer 2026",
                Status = ContractStatus.Draft
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/contracts/{contractId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contract with id '{contractId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/contracts/{contractId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contracts", new CreateContractRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var contractId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateContractRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                ClientId = clientId,
                ContractCode = "CTR-2026-001",
                ContractName = "Acme Retainer 2026",
                Status = ContractStatus.Draft
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contracts", new CreateContractRequest
        {
            ClientId = clientId,
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = ContractStatus.Draft,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenContractCodeDuplicates()
    {
        _factory.ContractService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateContractRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("A Contract with code 'CTR-2026-001' already exists."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contracts", new CreateContractRequest
        {
            ClientId = Guid.NewGuid(),
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer 2026",
            ContractType = ContractType.MonthlyRetainer,
            Status = ContractStatus.Draft,
            EstimatedValue = 120000,
            StartDate = new DateOnly(2026, 1, 1)
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.UpdateAsync(
                contractId,
                It.IsAny<UpdateContractRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                ClientId = Guid.NewGuid(),
                ContractCode = "CTR-2026-001",
                ContractName = "Acme Retainer Updated",
                Status = ContractStatus.Draft
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/contracts/{contractId}", new UpdateContractRequest
        {
            ContractCode = "CTR-2026-001",
            ContractName = "Acme Retainer Updated",
            ContractType = ContractType.MonthlyRetainer,
            Status = ContractStatus.Draft,
            EstimatedValue = 125000,
            StartDate = new DateOnly(2026, 1, 1)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_ReturnsNoContent()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.CancelAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                Status = ContractStatus.Cancelled
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/contracts/{contractId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsOk()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.ActivateAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                Status = ContractStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contracts/{contractId}/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Close_ReturnsOk()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.CloseAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                Status = ContractStatus.Closed
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contracts/{contractId}/close", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelContract_ReturnsOk()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.CancelAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractResponse
            {
                Id = contractId,
                Status = ContractStatus.Cancelled
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contracts/{contractId}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsBadRequest_WhenBusinessRuleFails()
    {
        var contractId = Guid.NewGuid();
        _factory.ContractService
            .Setup(service => service.ActivateAsync(contractId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only Draft Contracts can be activated."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contracts/{contractId}/activate", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class ContractApiFactory : WebApplicationFactory<Program>
{
    public Mock<IClientContractService> ContractService { get; } = new();

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
            services.RemoveAll<IClientContractService>();
            services.AddSingleton(ContractService.Object);
        });
    }
}

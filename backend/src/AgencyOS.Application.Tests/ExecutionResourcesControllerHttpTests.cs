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

public class ExecutionResourcesControllerHttpTests : IClassFixture<ExecutionResourceApiFactory>
{
    private readonly ExecutionResourceApiFactory _factory;

    public ExecutionResourcesControllerHttpTests(ExecutionResourceApiFactory factory)
    {
        _factory = factory;
        _factory.ExecutionResourceService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.ExecutionResourceService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExecutionResourceResponse>());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/execution-resources");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenResourceExists()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/execution-resources/{resourceId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/execution-resources/{resourceId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/execution-resources",
            new CreateExecutionResourceRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateExecutionResourceRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/execution-resources", CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenResourceCodeDuplicates()
    {
        _factory.ExecutionResourceService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateExecutionResourceRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("An Execution Resource with code 'RES-001' already exists."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/execution-resources", CreateValidCreateRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.UpdateAsync(
                resourceId,
                It.IsAny<UpdateExecutionResourceRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(resourceId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/execution-resources/{resourceId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.UpdateAsync(
                resourceId,
                It.IsAny<UpdateExecutionResourceRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/execution-resources/{resourceId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsConflict_WhenResourceCodeDuplicates()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.UpdateAsync(
                resourceId,
                It.IsAny<UpdateExecutionResourceRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("An Execution Resource with code 'RES-001' already exists."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync(
            $"/execution-resources/{resourceId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.DeactivateAsync(resourceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/execution-resources/{resourceId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNotFound_WhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _factory.ExecutionResourceService
            .Setup(service => service.DeactivateAsync(resourceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Execution Resource with id '{resourceId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/execution-resources/{resourceId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static CreateExecutionResourceRequest CreateValidCreateRequest()
    {
        return new CreateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40
        };
    }

    private static UpdateExecutionResourceRequest CreateValidUpdateRequest()
    {
        return new UpdateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40
        };
    }

    private static ExecutionResourceResponse CreateResponse(Guid resourceId)
    {
        return new ExecutionResourceResponse
        {
            Id = resourceId,
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40
        };
    }
}

public sealed class ExecutionResourceApiFactory : WebApplicationFactory<Program>
{
    public Mock<IExecutionResourceService> ExecutionResourceService { get; } = new();

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
            services.RemoveAll<IExecutionResourceService>();
            services.AddSingleton(ExecutionResourceService.Object);
        });
    }
}

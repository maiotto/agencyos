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

public class MissionsControllerHttpTests : IClassFixture<MissionApiFactory>
{
    private readonly MissionApiFactory _factory;

    public MissionsControllerHttpTests(MissionApiFactory factory)
    {
        _factory = factory;
        _factory.MissionService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.MissionService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MissionResponse>());

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/missions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenMissionExists()
    {
        var missionId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissionResponse
            {
                Id = missionId,
                Code = "MSN-2026-001",
                Name = "Acme Delivery Mission"
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/missions/{missionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Mission with id '{missionId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/missions/{missionId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/missions", new CreateMissionRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var missionId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissionResponse
            {
                Id = missionId,
                Code = "MSN-2026-001",
                Name = "Acme Delivery Mission"
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/missions", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenMissionCodeDuplicates()
    {
        _factory.MissionService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("A Mission with code 'MSN-2026-001' already exists."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/missions", CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenContractIsNotActive()
    {
        _factory.MissionService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Only Active Contracts may authorize Mission creation."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/missions", CreateValidRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contract with id '{contractId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/missions", CreateValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var missionId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.UpdateAsync(
                missionId,
                It.IsAny<UpdateMissionRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissionResponse
            {
                Id = missionId,
                Code = "MSN-2026-001",
                Name = "Updated Mission"
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/missions/{missionId}", new UpdateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Updated Mission",
            MissionTypeId = Guid.NewGuid(),
            MissionStatusId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var missionId = Guid.NewGuid();
        _factory.MissionService
            .Setup(service => service.DeleteAsync(missionId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/missions/{missionId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static CreateMissionRequest CreateValidRequest()
    {
        return new CreateMissionRequest
        {
            ClientContractId = Guid.NewGuid(),
            Code = "MSN-2026-001",
            Name = "Acme Delivery Mission",
            MissionTypeId = Guid.NewGuid(),
            MissionStatusId = Guid.NewGuid(),
            Priority = "NORMAL"
        };
    }
}

public sealed class MissionApiFactory : WebApplicationFactory<Program>
{
    public Mock<IMissionService> MissionService { get; } = new();

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
            services.RemoveAll<IMissionService>();
            services.AddSingleton(MissionService.Object);
        });
    }
}

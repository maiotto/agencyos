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

public class DecisionProfilesControllerHttpTests : IClassFixture<DecisionProfilesApiFactory>
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly DecisionProfilesApiFactory _factory;

    public DecisionProfilesControllerHttpTests(DecisionProfilesApiFactory factory)
    {
        _factory = factory;
        _factory.DecisionProfileService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.DecisionProfileService
            .Setup(service => service.GetAllAsync(
                It.IsAny<CompanyDecisionProfileQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyDecisionProfileResponse>());

        var response = await _factory.CreateClient().GetAsync("/decision-profiles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.DecisionProfileService
            .Setup(service => service.FilterAsync(
                It.IsAny<CompanyDecisionProfileQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyDecisionProfileResponse>());

        var response = await _factory.CreateClient().GetAsync("/decision-profiles/filter?status=Active");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Company Decision Profile with id '{profileId}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/decision-profiles/{profileId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCompanyId_ReturnsOk()
    {
        _factory.DecisionProfileService
            .Setup(service => service.GetByCompanyIdAsync(CompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CompanyDecisionProfileResponse>());

        var response = await _factory.CreateClient().GetAsync($"/decision-profiles/company/{CompanyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDefaultActive_ReturnsNotFound_WhenMissing()
    {
        _factory.DecisionProfileService
            .Setup(service => service.GetDefaultActiveAsync(CompanyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("No default Active Company Decision Profile was found."));

        var response = await _factory.CreateClient().GetAsync($"/decision-profiles/company/{CompanyId}/default");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/decision-profiles",
            new CreateCompanyDecisionProfileRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateCompanyDecisionProfileRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(profileId));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/decision-profiles",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateName()
    {
        _factory.DecisionProfileService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateCompanyDecisionProfileRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("An Active Company Decision Profile named 'X' already exists (BR-1902)."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/decision-profiles",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithNewVersionId()
    {
        var originalId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.UpdateAsync(
                originalId,
                It.IsAny<UpdateCompanyDecisionProfileRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(newId, version: 2));

        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/decision-profiles/{originalId}",
            CreateValidUpdateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CompanyDecisionProfileResponse>();
        Assert.Equal(newId, body!.Id);
        Assert.Equal(2, body.Version);
    }

    [Fact]
    public async Task Clone_ReturnsCreated()
    {
        var sourceId = Guid.NewGuid();
        var cloneId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.CloneAsync(
                sourceId,
                It.IsAny<CloneCompanyDecisionProfileRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(cloneId));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            $"/decision-profiles/{sourceId}/clone",
            new CloneCompanyDecisionProfileRequest { Name = "Clone", Code = "CloneCode" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsNoContent()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.ActivateAsync(profileId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/decision-profiles/{profileId}/activate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsConflict_WhenDefault_BR1901()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.DeactivateAsync(profileId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Default profile cannot be deactivated."));

        var response = await _factory.CreateClient().PostAsync($"/decision-profiles/{profileId}/deactivate", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Archive_ReturnsNoContent()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.ArchiveAsync(profileId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/decision-profiles/{profileId}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SetDefault_ReturnsOk()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.SetDefaultAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(profileId, defaultProfile: true));

        var response = await _factory.CreateClient().PostAsync($"/decision-profiles/{profileId}/set-default", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ClearDefault_ReturnsOk()
    {
        var profileId = Guid.NewGuid();
        _factory.DecisionProfileService
            .Setup(service => service.ClearDefaultAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(profileId, defaultProfile: false));

        var response = await _factory.CreateClient().PostAsync($"/decision-profiles/{profileId}/clear-default", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static CreateCompanyDecisionProfileRequest CreateValidRequest() =>
        new()
        {
            CompanyId = CompanyId,
            Code = "TestCode",
            Name = "Test Profile",
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m },
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.OperationalRisk, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m
        };

    private static UpdateCompanyDecisionProfileRequest CreateValidUpdateRequest() =>
        new()
        {
            Name = "Updated",
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m },
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.OperationalRisk, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m
        };

    private static CompanyDecisionProfileResponse CreateResponse(
        Guid profileId,
        int version = 1,
        bool defaultProfile = false) =>
        new()
        {
            Id = profileId,
            CompanyId = CompanyId,
            ProfileFamilyId = profileId,
            Code = "TestCode",
            Name = "Test Profile",
            Status = CompanyDecisionProfileStatus.Active,
            Dimensions =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m,
            DefaultProfile = defaultProfile,
            Version = version,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class DecisionProfilesApiFactory : WebApplicationFactory<Program>
{
    public Mock<ICompanyDecisionProfileService> DecisionProfileService { get; } = new();

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
            services.RemoveAll<ICompanyDecisionProfileService>();
            services.AddSingleton(DecisionProfileService.Object);
        });
    }
}

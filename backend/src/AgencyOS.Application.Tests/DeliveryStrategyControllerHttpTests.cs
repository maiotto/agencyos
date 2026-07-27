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

public class DeliveryStrategyControllerHttpTests : IClassFixture<DeliveryStrategyApiFactory>
{
    private readonly DeliveryStrategyApiFactory _factory;

    public DeliveryStrategyControllerHttpTests(DeliveryStrategyApiFactory factory)
    {
        _factory = factory;
        _factory.DeliveryStrategyEvaluatorService.Reset();
        _factory.DeliveryStrategyRankingService.Reset();
        _factory.DeliveryStrategyExplanationService.Reset();
    }

    [Fact]
    public async Task Evaluate_ReturnsOk_WhenRequestIsValid()
    {
        _factory.DeliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEvaluateResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            CreateValidEvaluateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            new EvaluateDeliveryStrategyRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            new EvaluateDeliveryStrategyRequest
            {
                ContractId = Guid.NewGuid(),
                MissionId = Guid.NewGuid(),
                PeriodStartDate = new DateOnly(2026, 7, 31),
                PeriodEndDate = new DateOnly(2026, 7, 1)
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_ReturnsNotFound_WhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _factory.DeliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contract with id '{contractId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            CreateValidEvaluateRequest(contractId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_ReturnsNotFound_WhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _factory.DeliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Mission with id '{missionId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            CreateValidEvaluateRequest(missionId: missionId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_ReturnsBadRequest_WhenBusinessRuleExceptionIsThrown()
    {
        _factory.DeliveryStrategyEvaluatorService
            .Setup(service => service.EvaluateAsync(
                It.IsAny<EvaluateDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "No delivery strategies were generated for evaluation."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/evaluate",
            CreateValidEvaluateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsOk_WhenRequestIsValid()
    {
        _factory.DeliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRankResponse());

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            CreateValidRankRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            new RankDeliveryStrategyRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            new RankDeliveryStrategyRequest
            {
                ContractId = Guid.NewGuid(),
                MissionId = Guid.NewGuid(),
                PeriodStartDate = new DateOnly(2026, 7, 31),
                PeriodEndDate = new DateOnly(2026, 7, 1),
                CompanyDecisionProfileId = Guid.NewGuid()
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsNotFound_WhenContractMissing()
    {
        var contractId = Guid.NewGuid();
        _factory.DeliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contract with id '{contractId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            CreateValidRankRequest(contractId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsNotFound_WhenMissionMissing()
    {
        var missionId = Guid.NewGuid();
        _factory.DeliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Mission with id '{missionId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            CreateValidRankRequest(missionId: missionId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsNotFound_WhenDecisionProfileMissing()
    {
        var profileId = Guid.NewGuid();
        _factory.DeliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Company Decision Profile not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            CreateValidRankRequest(profileId: profileId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rank_ReturnsBadRequest_WhenBusinessRuleExceptionIsThrown()
    {
        _factory.DeliveryStrategyRankingService
            .Setup(service => service.RankAsync(
                It.IsAny<RankDeliveryStrategyRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "At least one evaluated delivery strategy is required for ranking."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            "/delivery-strategies/rank",
            CreateValidRankRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsOk_WhenRequestIsValid()
    {
        var strategyId = Guid.NewGuid();
        _factory.DeliveryStrategyExplanationService
            .Setup(service => service.GetExplanationAsync(
                strategyId,
                It.IsAny<DeliveryStrategyExplanationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateExplanationResponse(strategyId));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildExplanationUrl(strategyId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsBadRequest_WhenRequiredFieldsAreMissing()
    {
        var strategyId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/delivery-strategies/{strategyId}/explanation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsBadRequest_WhenPeriodIsInvalid()
    {
        var strategyId = Guid.NewGuid();
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(
            $"/delivery-strategies/{strategyId}/explanation" +
            $"?contractId={Guid.NewGuid()}" +
            $"&missionId={Guid.NewGuid()}" +
            $"&periodStartDate=2026-07-31" +
            $"&periodEndDate=2026-07-01" +
            $"&companyDecisionProfileId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsNotFound_WhenStrategyMissing()
    {
        var strategyId = Guid.NewGuid();
        _factory.DeliveryStrategyExplanationService
            .Setup(service => service.GetExplanationAsync(
                strategyId,
                It.IsAny<DeliveryStrategyExplanationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Delivery strategy not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildExplanationUrl(strategyId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsNotFound_WhenDecisionProfileMissing()
    {
        var strategyId = Guid.NewGuid();
        _factory.DeliveryStrategyExplanationService
            .Setup(service => service.GetExplanationAsync(
                strategyId,
                It.IsAny<DeliveryStrategyExplanationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Company Decision Profile not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildExplanationUrl(strategyId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExplanation_ReturnsBadRequest_WhenBusinessRuleExceptionIsThrown()
    {
        var strategyId = Guid.NewGuid();
        _factory.DeliveryStrategyExplanationService
            .Setup(service => service.GetExplanationAsync(
                strategyId,
                It.IsAny<DeliveryStrategyExplanationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException(
                "At least one evaluated delivery strategy is required for ranking."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(BuildExplanationUrl(strategyId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string BuildExplanationUrl(Guid strategyId)
    {
        return $"/delivery-strategies/{strategyId}/explanation" +
               $"?contractId={Guid.NewGuid()}" +
               $"&missionId={Guid.NewGuid()}" +
               $"&periodStartDate=2026-07-01" +
               $"&periodEndDate=2026-07-31" +
               $"&companyDecisionProfileId={Guid.NewGuid()}";
    }

    private static EvaluateDeliveryStrategyRequest CreateValidEvaluateRequest(
        Guid? contractId = null,
        Guid? missionId = null)
    {
        return new EvaluateDeliveryStrategyRequest
        {
            ContractId = contractId ?? Guid.NewGuid(),
            MissionId = missionId ?? Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31)
        };
    }

    private static RankDeliveryStrategyRequest CreateValidRankRequest(
        Guid? contractId = null,
        Guid? missionId = null,
        Guid? profileId = null)
    {
        return new RankDeliveryStrategyRequest
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ContractId = contractId ?? Guid.NewGuid(),
            MissionId = missionId ?? Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31),
            CompanyDecisionProfileId = profileId ?? Guid.NewGuid()
        };
    }

    private static EvaluateDeliveryStrategyResponse CreateEvaluateResponse()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();

        return new EvaluateDeliveryStrategyResponse
        {
            ContractId = contractId,
            MissionId = missionId,
            EvaluatedStrategyCount = 1,
            ElapsedMilliseconds = 42,
            EvaluatedStrategies =
            [
                CreateEvaluatedStrategy(contractId, missionId)
            ]
        };
    }

    private static RankDeliveryStrategyResponse CreateRankResponse()
    {
        var contractId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        return new RankDeliveryStrategyResponse
        {
            ContractId = contractId,
            MissionId = missionId,
            CompanyDecisionProfileId = profileId,
            CompanyDecisionProfileName = "Balanced Strategy",
            RankedStrategyCount = 1,
            ElapsedMilliseconds = 55,
            RankedStrategies =
            [
                new RankedDeliveryStrategyResponse
                {
                    RecommendationId = Guid.NewGuid(),
                    RankPosition = 1,
                    FinalScore = 82.5m,
                    EvaluatedStrategy = CreateEvaluatedStrategy(contractId, missionId),
                    DecisionFactors = new DeliveryStrategyDecisionFactorsResponse
                    {
                        Factors =
                        [
                            new DeliveryStrategyDecisionFactorResponse
                            {
                                Dimension = "EstimatedCost",
                                RawValue = 1360,
                                NormalizedScore = 90,
                                Weight = 0.25m,
                                WeightedContribution = 22.5m
                            }
                        ]
                    }
                }
            ]
        };
    }

    private static DeliveryStrategyExplanationResponse CreateExplanationResponse(Guid strategyId)
    {
        return new DeliveryStrategyExplanationResponse
        {
            StrategyId = strategyId,
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            CompanyDecisionProfileId = Guid.NewGuid(),
            StrategySummary = new DeliveryStrategySummaryResponse
            {
                StrategyId = strategyId,
                StrategyName = "Internal Human",
                RankPosition = 1,
                TotalRankedStrategies = 1,
                FinalScore = 82.5m,
                CompanyDecisionProfileName = "Balanced Strategy"
            },
            EstimatedCost = 1360,
            EstimatedDurationHours = 16,
            ElapsedMilliseconds = 60
        };
    }

    private static EvaluatedDeliveryStrategyResponse CreateEvaluatedStrategy(
        Guid contractId,
        Guid missionId)
    {
        return new EvaluatedDeliveryStrategyResponse
        {
            Strategy = new DeliveryStrategyResponse
            {
                StrategyId = Guid.NewGuid(),
                ContractId = contractId,
                MissionId = missionId,
                StrategyName = "Internal Human",
                EstimatedHours = 16,
                EstimatedCost = 1360
            },
            EvaluationMetrics = new DeliveryStrategyEvaluationMetricsResponse
            {
                EstimatedCost = 1360,
                EstimatedDurationHours = 16,
                CapacityUtilizationPercentage = 25,
                ResourceUtilizationPercentage = 25,
                WorkloadImpactPercentage = 10,
                AvailabilityImpactPercentage = 20,
                OperationalRiskScore = 15,
                HumanUtilizationPercentage = 100
            }
        };
    }
}

public sealed class DeliveryStrategyApiFactory : WebApplicationFactory<Program>
{
    public Mock<IDeliveryStrategyEvaluatorService> DeliveryStrategyEvaluatorService { get; } = new();

    public Mock<IDeliveryStrategyRankingService> DeliveryStrategyRankingService { get; } = new();

    public Mock<IDeliveryStrategyExplanationService> DeliveryStrategyExplanationService { get; } = new();

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
            services.RemoveAll<IDeliveryStrategyEvaluatorService>();
            services.RemoveAll<IDeliveryStrategyRankingService>();
            services.RemoveAll<IDeliveryStrategyExplanationService>();
            services.AddSingleton(DeliveryStrategyEvaluatorService.Object);
            services.AddSingleton(DeliveryStrategyRankingService.Object);
            services.AddSingleton(DeliveryStrategyExplanationService.Object);
        });
    }
}

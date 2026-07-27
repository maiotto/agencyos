using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationServiceTests
{
    private readonly Mock<IRecommendationRepository> _repository = new();
    private readonly Mock<IRecommendationHistoryService> _historyService = new();
    private readonly Mock<ILogger<RecommendationService>> _logger = new();

    private RecommendationService CreateService() =>
        new(_repository.Object, _historyService.Object, new NoOpAuditService(), _logger.Object);

    [Fact]
    public async Task CreateAsync_PersistsImmutableRecommendation()
    {
        Recommendation? persisted = null;
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()))
            .Callback<Recommendation, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((Recommendation item, CancellationToken _) => item);

        var result = await CreateService().CreateAsync(CreateRequest());

        Assert.NotNull(persisted);
        Assert.Equal(RecommendationStatus.Active, persisted!.Status);
        Assert.Equal(result.Id, persisted.Id);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public async Task CreateNewVersionAsync_NeverOverwritesSource()
    {
        var source = CreateEntity();
        _repository
            .Setup(repository => repository.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recommendation item, CancellationToken _) => item);

        var result = await CreateService().CreateNewVersionAsync(source.Id, new CreateRecommendationVersionRequest
        {
            Title = "v2",
            RecommendationPayload = "{\"v\":2}",
            CapacitySnapshot = "{}",
            WorkloadSnapshot = "{}",
            GeneratedBy = "planner",
            Score = 90m,
            Rank = 1
        });

        Assert.Equal(2, result.Version);
        Assert.Equal(source.RecommendationNumber, result.RecommendationNumber);
        Assert.NotEqual(source.Id, result.Id);
    }

    [Fact]
    public async Task ArchiveAsync_Then_RestoreAsync()
    {
        var recommendation = CreateEntity();
        _repository
            .Setup(repository => repository.GetByIdAsync(recommendation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Recommendation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recommendation item, CancellationToken _) => item);

        var archived = await CreateService().ArchiveAsync(recommendation.Id);
        Assert.True(archived.Archived);

        var restored = await CreateService().RestoreAsync(recommendation.Id);
        Assert.False(restored.Archived);
        Assert.Equal(RecommendationStatus.Active, restored.Status);
    }

    [Fact]
    public async Task PersistRankedStrategiesAsync_CreatesRecommendations()
    {
        var request = new RankDeliveryStrategyRequest
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ContractId = Guid.NewGuid(),
            MissionId = Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 31),
            CompanyDecisionProfileId = Guid.NewGuid(),
            GeneratedBy = "decision-engine"
        };

        var strategyId = Guid.NewGuid();
        var ranking = new RankDeliveryStrategyResponse
        {
            ContractId = request.ContractId,
            MissionId = request.MissionId,
            CompanyDecisionProfileId = request.CompanyDecisionProfileId,
            CompanyDecisionProfileName = "Balanced",
            RankedStrategies =
            [
                new RankedDeliveryStrategyResponse
                {
                    RankPosition = 1,
                    FinalScore = 80m,
                    EvaluatedStrategy = new EvaluatedDeliveryStrategyResponse
                    {
                        Strategy = new DeliveryStrategyResponse
                        {
                            StrategyId = strategyId,
                            StrategyName = "Human + AI",
                            ContractId = request.ContractId,
                            MissionId = request.MissionId
                        }
                    },
                    DecisionFactors = new DeliveryStrategyDecisionFactorsResponse
                    {
                        Factors =
                        [
                            new DeliveryStrategyDecisionFactorResponse
                            {
                                Dimension = "EstimatedCost",
                                NormalizedScore = 90
                            }
                        ]
                    }
                }
            ]
        };

        _repository
            .Setup(repository => repository.GetLatestByDeliveryStrategyAsync(
                strategyId,
                request.ContractId,
                request.MissionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recommendation?)null);
        _repository
            .Setup(repository => repository.AddRangeAsync(
                It.IsAny<IReadOnlyList<Recommendation>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var persisted = await CreateService().PersistRankedStrategiesAsync(request, ranking);

        Assert.Single(persisted);
        Assert.Equal(strategyId, persisted[0].DeliveryStrategyId);
        Assert.Equal(1, persisted[0].Rank);
    }

    [Fact]
    public void Validators_RequirePayloadAndCompany()
    {
        var createValidator = new CreateRecommendationRequestValidator();
        Assert.False(createValidator.Validate(new CreateRecommendationRequest()).IsValid);

        var versionValidator = new CreateRecommendationVersionRequestValidator();
        Assert.False(versionValidator.Validate(new CreateRecommendationVersionRequest()).IsValid);
    }

    private static CreateRecommendationRequest CreateRequest() =>
        new()
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DeliveryStrategyId = Guid.NewGuid(),
            Title = "Human + AI",
            RecommendationPayload = "{\"ok\":true}",
            CapacitySnapshot = "{}",
            WorkloadSnapshot = "{}",
            GeneratedBy = "planner@agencyos.local",
            Score = 82.5m,
            Rank = 1
        };

    private static Recommendation CreateEntity() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-TEST-LINEAGE",
            "Human + AI",
            "Summary",
            "Reason",
            82.5m,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}

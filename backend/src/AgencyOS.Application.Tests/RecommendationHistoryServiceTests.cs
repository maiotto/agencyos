using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationHistoryServiceTests
{
    private readonly Mock<IRecommendationHistoryRepository> _historyRepository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<ILogger<RecommendationHistoryService>> _logger = new();

    private RecommendationHistoryService CreateService() =>
        new(_historyRepository.Object, _recommendationRepository.Object, _logger.Object);

    [Fact]
    public async Task PersistFromRecommendationAsync_AppendsHistory()
    {
        RecommendationHistory? persisted = null;
        _historyRepository
            .Setup(repository => repository.AddAsync(It.IsAny<RecommendationHistory>(), It.IsAny<CancellationToken>()))
            .Callback<RecommendationHistory, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((RecommendationHistory item, CancellationToken _) => item);

        var recommendation = CreateRecommendation();
        await CreateService().PersistFromRecommendationAsync(
            recommendation,
            RecommendationHistoryEventType.VersionCreated,
            "decision-engine");

        Assert.NotNull(persisted);
        Assert.Equal(recommendation.Id, persisted!.RecommendationId);
        Assert.Equal(RecommendationHistoryEventType.VersionCreated, persisted.EventType);
    }

    [Fact]
    public async Task GetVersionsAsync_ThrowsWhenRecommendationMissing()
    {
        var id = Guid.NewGuid();
        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recommendation?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetVersionsAsync(id));
    }

    [Fact]
    public async Task GetTimelineAsync_ReturnsOrderedEntries()
    {
        var recommendation = CreateRecommendation();
        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(recommendation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _historyRepository
            .Setup(repository => repository.GetTimelineByRecommendationIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                RecommendationHistory.FromRecommendation(
                    recommendation,
                    RecommendationHistoryEventType.VersionCreated,
                    "decision-engine",
                    DateTimeOffset.UtcNow.AddMinutes(-5)),
                RecommendationHistory.FromRecommendation(
                    recommendation,
                    RecommendationHistoryEventType.WorkflowTransition,
                    "planner",
                    DateTimeOffset.UtcNow,
                    RecommendationWorkflowStatus.Draft,
                    Guid.NewGuid())
            ]);

        var timeline = await CreateService().GetTimelineAsync(recommendation.Id);

        Assert.Equal(2, timeline.Count);
        Assert.Equal(RecommendationHistoryEventType.VersionCreated, timeline[0].EventType);
        Assert.Equal(RecommendationHistoryEventType.WorkflowTransition, timeline[1].EventType);
    }

    [Fact]
    public void Validator_AcceptsEmptyQuery()
    {
        var validator = new RecommendationHistoryQueryParametersValidator();
        Assert.True(validator.Validate(new RecommendationHistoryQueryParameters()).IsValid);
    }

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-HIST-SVC",
            "Human + AI",
            "Summary",
            "Reason",
            80m,
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

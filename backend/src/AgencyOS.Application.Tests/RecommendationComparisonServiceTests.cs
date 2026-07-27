using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationComparisonServiceTests
{
    private readonly Mock<IRecommendationHistoryRepository> _historyRepository = new();

    private RecommendationComparisonService CreateService() =>
        new(_historyRepository.Object, NullLogger<RecommendationComparisonService>.Instance);

    [Fact]
    public async Task CompareByIdsAsync_UsesHistorySnapshots_AndComputesDeltas()
    {
        var left = CreateHistory(1, 80m, 2, "{\"hours\":40}");
        var right = CreateHistory(2, 90m, 1, "{\"hours\":50}");
        _historyRepository.Setup(repository => repository.GetByIdAsync(left.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(left);
        _historyRepository.Setup(repository => repository.GetByIdAsync(right.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(right);

        var result = await CreateService().CompareByIdsAsync(left.Id, right.Id);

        Assert.Equal(10m, result.ScoreDelta);
        Assert.Equal(-1, result.RankDelta);
        Assert.True(result.HasDifferences);
        Assert.Contains(result.Differences, item => item.Path == "hours" && item.Changed);
    }

    [Fact]
    public async Task CompareByIdsAsync_ResolvesRecommendationIdToVersionSnapshot()
    {
        var recommendationId = Guid.NewGuid();
        var left = CreateHistory(1, 70m, 2, "{}");
        var right = CreateHistory(2, 75m, 1, "{}");
        _historyRepository.Setup(repository => repository.GetByIdAsync(recommendationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecommendationHistory?)null);
        _historyRepository.Setup(repository => repository.GetByIdAsync(right.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(right);
        _historyRepository.Setup(repository => repository.GetVersionSnapshotByRecommendationIdAsync(
                recommendationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(left);

        var result = await CreateService().CompareByIdsAsync(recommendationId, right.Id);

        Assert.Equal(left.Id, result.Left.Id);
        Assert.Equal(right.Id, result.Right.Id);
    }

    [Fact]
    public async Task CompareVersionsAsync_DefaultsToLatestTwoVersions()
    {
        var v1 = CreateHistory(1, 70m, 3, "{}");
        var v2 = CreateHistory(2, 80m, 2, "{}");
        var v3 = CreateHistory(3, 90m, 1, "{}");
        _historyRepository.Setup(repository => repository.GetVersionsByRecommendationNumberAsync(
                "REC-COMPARE-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { v1, v2, v3 });

        var result = await CreateService().CompareVersionsAsync(
            "REC-COMPARE-1",
            new RecommendationVersionComparisonQueryParameters());

        Assert.Equal(v2.Id, result.Left.Id);
        Assert.Equal(v3.Id, result.Right.Id);
        Assert.Equal(10m, result.ScoreDelta);
    }

    [Fact]
    public async Task CompareVersionsAsync_UsesExplicitVersions()
    {
        var v1 = CreateHistory(1, 70m, 3, "{}");
        var v2 = CreateHistory(2, 80m, 2, "{}");
        var v3 = CreateHistory(3, 90m, 1, "{}");
        _historyRepository.Setup(repository => repository.GetVersionsByRecommendationNumberAsync(
                "REC-COMPARE-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { v1, v2, v3 });

        var result = await CreateService().CompareVersionsAsync(
            "REC-COMPARE-1",
            new RecommendationVersionComparisonQueryParameters
            {
                LeftVersion = 1,
                RightVersion = 3
            });

        Assert.Equal(v1.Id, result.Left.Id);
        Assert.Equal(v3.Id, result.Right.Id);
        Assert.Equal(2, result.VersionDelta);
    }

    [Fact]
    public async Task CompareVersionsAsync_RequiresAtLeastTwoSnapshots()
    {
        _historyRepository.Setup(repository => repository.GetVersionsByRecommendationNumberAsync(
                "REC-COMPARE-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateHistory(1, 70m, 1, "{}") });

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().CompareVersionsAsync(
                "REC-COMPARE-1",
                new RecommendationVersionComparisonQueryParameters()));
    }

    [Fact]
    public void CompareValidator_RejectsSameIds()
    {
        var validator = new RecommendationComparisonQueryParametersValidator();
        var result = validator.Validate(new RecommendationComparisonQueryParameters
        {
            LeftId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            RightId = Guid.Parse("11111111-1111-4111-8111-111111111111")
        });

        Assert.False(result.IsValid);
    }

    private static RecommendationHistory CreateHistory(
        int version,
        decimal score,
        int rank,
        string capacity) =>
        RecommendationHistory.Create(
            Guid.NewGuid(),
            "REC-COMPARE-1",
            version,
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Human + AI",
            "Summary",
            RecommendationHistoryEventType.VersionCreated,
            RecommendationStatus.Active,
            null,
            null,
            null,
            null,
            null,
            score,
            rank,
            null,
            RecommendationVersions.CurrentDecisionEngineVersion,
            capacity,
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}

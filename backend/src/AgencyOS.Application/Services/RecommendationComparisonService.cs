using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class RecommendationComparisonService : IRecommendationComparisonService
{
    private readonly IRecommendationHistoryRepository _historyRepository;
    private readonly ILogger<RecommendationComparisonService> _logger;

    public RecommendationComparisonService(
        IRecommendationHistoryRepository historyRepository,
        ILogger<RecommendationComparisonService> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public Task<RecommendationComparisonResponse> CompareAsync(
        RecommendationComparisonQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        CompareByIdsAsync(parameters.LeftId, parameters.RightId, cancellationToken);

    public async Task<RecommendationComparisonResponse> CompareByIdsAsync(
        Guid leftId,
        Guid rightId,
        CancellationToken cancellationToken = default)
    {
        if (leftId == Guid.Empty || rightId == Guid.Empty)
        {
            throw new BusinessRuleException("LeftId and RightId are required.");
        }

        if (leftId == rightId)
        {
            throw new BusinessRuleException("LeftId and RightId must be different.");
        }

        var left = await ResolveSnapshotAsync(leftId, cancellationToken);
        var right = await ResolveSnapshotAsync(rightId, cancellationToken);
        return BuildComparison(left, right);
    }

    public async Task<RecommendationComparisonResponse> CompareVersionsAsync(
        string recommendationNumber,
        RecommendationVersionComparisonQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recommendationNumber))
        {
            throw new BusinessRuleException("RecommendationNumber is required.");
        }

        var versions = await _historyRepository.GetVersionsByRecommendationNumberAsync(
            recommendationNumber.Trim(),
            cancellationToken);

        if (versions.Count < 2)
        {
            throw new BusinessRuleException(
                $"Recommendation '{recommendationNumber}' requires at least two VersionCreated history snapshots to compare.");
        }

        RecommendationHistory left;
        RecommendationHistory right;

        if (parameters.LeftVersion.HasValue && parameters.RightVersion.HasValue)
        {
            left = versions.FirstOrDefault(item => item.RecommendationVersion == parameters.LeftVersion.Value)
                ?? throw new NotFoundException(
                    $"Version {parameters.LeftVersion} was not found for recommendation '{recommendationNumber}'.");
            right = versions.FirstOrDefault(item => item.RecommendationVersion == parameters.RightVersion.Value)
                ?? throw new NotFoundException(
                    $"Version {parameters.RightVersion} was not found for recommendation '{recommendationNumber}'.");
        }
        else
        {
            var ordered = versions
                .OrderByDescending(item => item.RecommendationVersion)
                .ThenByDescending(item => item.CreatedAt)
                .ToList();
            right = ordered[0];
            left = ordered[1];
        }

        return BuildComparison(left, right);
    }

    private async Task<RecommendationHistory> ResolveSnapshotAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var byHistoryId = await _historyRepository.GetByIdAsync(id, cancellationToken);
        if (byHistoryId is not null)
        {
            return byHistoryId;
        }

        var byRecommendationId = await _historyRepository.GetVersionSnapshotByRecommendationIdAsync(
            id,
            cancellationToken);

        if (byRecommendationId is not null)
        {
            return byRecommendationId;
        }

        throw new NotFoundException(
            $"Recommendation history or recommendation snapshot with id '{id}' was not found.");
    }

    private RecommendationComparisonResponse BuildComparison(
        RecommendationHistory left,
        RecommendationHistory right)
    {
        var comparison = RecommendationComparison.Compare(left, right);
        _logger.LogInformation(
            "Compared recommendation history {LeftId} vs {RightId}. Differences={HasDifferences}",
            left.Id,
            right.Id,
            comparison.HasDifferences);

        return RecommendationComparisonMappings.ToResponse(comparison);
    }
}

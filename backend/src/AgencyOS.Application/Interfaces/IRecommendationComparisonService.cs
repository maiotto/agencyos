using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationComparisonService
{
    Task<RecommendationComparisonResponse> CompareAsync(
        RecommendationComparisonQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationComparisonResponse> CompareByIdsAsync(
        Guid leftId,
        Guid rightId,
        CancellationToken cancellationToken = default);

    Task<RecommendationComparisonResponse> CompareVersionsAsync(
        string recommendationNumber,
        RecommendationVersionComparisonQueryParameters parameters,
        CancellationToken cancellationToken = default);
}

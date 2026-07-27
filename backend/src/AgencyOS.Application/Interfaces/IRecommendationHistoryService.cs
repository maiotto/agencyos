using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationHistoryService
{
    Task<IReadOnlyList<RecommendationHistoryResponse>> GetAllAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistoryResponse>> FilterAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationHistoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistoryResponse>> GetVersionsAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistoryTimelineEntryResponse>> GetTimelineAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task PersistFromRecommendationAsync(
        Recommendation recommendation,
        string eventType,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task PersistRangeFromRecommendationsAsync(
        IReadOnlyList<Recommendation> recommendations,
        string eventType,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task PersistWorkflowTransitionAsync(
        Recommendation recommendation,
        RecommendationWorkflow workflow,
        string createdBy,
        CancellationToken cancellationToken = default);
}

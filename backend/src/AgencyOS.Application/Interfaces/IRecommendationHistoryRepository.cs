using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationHistoryRepository
{
    Task<IReadOnlyList<RecommendationHistory>> QueryAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistory>> GetVersionsByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistory>> GetVersionsByRecommendationNumberAsync(
        string recommendationNumber,
        CancellationToken cancellationToken = default);

    Task<RecommendationHistory?> GetVersionSnapshotByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationHistory>> GetTimelineByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<RecommendationHistory> AddAsync(
        RecommendationHistory history,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyList<RecommendationHistory> histories,
        CancellationToken cancellationToken = default);
}

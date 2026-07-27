using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendationResponse>> GetAllAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> FilterAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetByCompanyIdAsync(
        Guid companyId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetByMissionIdAsync(
        Guid missionId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetByContractIdAsync(
        Guid contractId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationResponse>> GetVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponse> CreateAsync(
        CreateRecommendationRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponse> CreateNewVersionAsync(
        Guid id,
        CreateRecommendationVersionRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RecommendationResponse> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists ranked Decision Engine strategies as Recommendations (BR-1101, BR-1110).
    /// Failure aborts recommendation publication.
    /// </summary>
    Task<IReadOnlyList<Recommendation>> PersistRankedStrategiesAsync(
        RankDeliveryStrategyRequest request,
        RankDeliveryStrategyResponse ranking,
        CancellationToken cancellationToken = default);
}

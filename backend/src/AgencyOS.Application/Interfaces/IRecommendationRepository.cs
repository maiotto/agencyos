using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationRepository
{
    Task<IReadOnlyList<Recommendation>> QueryAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Recommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> GetByCompanyIdAsync(
        Guid companyId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> GetByMissionIdAsync(
        Guid missionId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> GetByContractIdAsync(
        Guid contractId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> GetVersionsByNumberAsync(
        string recommendationNumber,
        CancellationToken cancellationToken = default);

    Task<Recommendation?> GetLatestByDeliveryStrategyAsync(
        Guid deliveryStrategyId,
        Guid contractId,
        Guid missionId,
        CancellationToken cancellationToken = default);

    Task<Recommendation> AddAsync(
        Recommendation recommendation,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyList<Recommendation> recommendations,
        CancellationToken cancellationToken = default);

    Task<Recommendation> UpdateAsync(
        Recommendation recommendation,
        CancellationToken cancellationToken = default);
}

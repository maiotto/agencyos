using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IDecisionRepository
{
    Task<IReadOnlyList<Decision>> GetAllAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Decision?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Decision?> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<Decision> AddAsync(Decision decision, CancellationToken cancellationToken = default);

    Task<Decision> UpdateAsync(Decision decision, CancellationToken cancellationToken = default);
}

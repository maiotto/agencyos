using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationWorkflowRepository
{
    Task<IReadOnlyList<RecommendationWorkflow>> GetAllAsync(
        RecommendationWorkflowQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RecommendationWorkflow> AddAsync(
        RecommendationWorkflow workflow,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflow> UpdateAsync(
        RecommendationWorkflow workflow,
        CancellationToken cancellationToken = default);
}

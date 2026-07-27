using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IPortfolioService
{
    Task<IReadOnlyList<PortfolioResponse>> GetAllAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PortfolioResponse>> FilterAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PortfolioResponse> CreateAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> UpdateAsync(
        Guid id,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PortfolioResponse> AssociateMissionAsync(
        Guid id,
        PortfolioMissionRequest request,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> RemoveMissionAsync(
        Guid id,
        Guid missionId,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> AssignPlanningTemplateAsync(
        Guid id,
        AssignPortfolioPlanningTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<PortfolioSummaryResponse> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PortfolioHealthResponse> GetHealthAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> CalculateCapacityAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> CalculateWorkloadAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PortfolioResponse> CalculateHealthAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

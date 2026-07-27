using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IPlanningTemplateService
{
    Task<IReadOnlyList<PlanningTemplateResponse>> GetAllAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanningTemplateResponse>> FilterAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningTemplateResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PlanningTemplateResponse> CreateAsync(
        CreatePlanningTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<PlanningTemplateResponse> UpdateAsync(
        Guid id,
        UpdatePlanningTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PlanningTemplateResponse> CloneAsync(
        Guid id,
        ClonePlanningTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<AppliedPlanningConfigurationResponse> ApplyAsync(
        Guid id,
        ApplyPlanningTemplateRequest request,
        CancellationToken cancellationToken = default);
}

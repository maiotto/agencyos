using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Planning Workspace orchestration (US-502 / BR-2501..BR-2510). A read-only
/// orchestration facade over existing Planning Template, Portfolio, Capacity/Workload History,
/// and Cross-Portfolio Planning services (DEC-502-001) — resolves the active Company
/// (<c>parameters.CompanyId ?? ICompanyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId</c>),
/// aggregates existing data, and exposes navigation/drill-down targets. Never calls a
/// Portfolio/Template Update/Add/Delete, never recalculates an engine for storage, and never
/// persists a Scenario.
/// </summary>
public interface IPlanningWorkspaceService
{
    Task<PlanningWorkspaceResponse> GetWorkspaceAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningOverviewResponse> GetOverviewAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningTemplatesSectionResponse> GetTemplatesAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningCapacitySectionResponse> GetCapacityAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningWorkloadSectionResponse> GetWorkloadAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningPortfoliosSectionResponse> GetPortfoliosAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningHistoryResponse> GetHistoryAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningScenariosSectionResponse> GetScenariosAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}

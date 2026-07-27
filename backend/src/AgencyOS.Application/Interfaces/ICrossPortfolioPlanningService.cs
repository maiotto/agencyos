using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only, advisory Cross-Portfolio Planning orchestration (US-405 / BR-2301..BR-2310).
/// Resolves the active Company, never modifies a Portfolio, and treats every Scenario as
/// TEMPORARY in-memory state (DEC-405-001). Every simulation is audited and every response
/// requires human approval — Cross-Portfolio Planning never executes automatic reallocation,
/// AI/financial optimization, hiring recommendations, multi-company planning, or automatic
/// execution of any kind.
/// </summary>
public interface ICrossPortfolioPlanningService
{
    Task<CrossPortfolioOverviewResponse> GetOverviewAsync(
        CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CrossPortfolioScenarioListResponse> GetScenariosAsync(
        CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ConflictSummaryResponse> GetConflictsAsync(
        CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CrossPortfolioBalanceResponse> GetBalanceAsync(
        CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<SimulationSummaryResponse> SimulateAsync(
        SimulateCrossPortfolioPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<ScenarioComparisonResponse> CompareAsync(
        CompareCrossPortfolioScenariosRequest request,
        CancellationToken cancellationToken = default);
}

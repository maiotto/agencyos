using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Cross-Portfolio Planning (US-405 / BR-2301..BR-2310). Read-only advisory simulation across
/// multiple Portfolios: enterprise Capacity/Workload balance, Resource/Mission conflict detection,
/// and advisory rebalancing recommendations. Never modifies a Portfolio. Scenarios are TEMPORARY
/// in-memory records (DEC-405-001); every simulation is audited and every response requires human
/// approval — nothing here is executed automatically.
/// </summary>
[ApiController]
[Route("cross-portfolio-planning")]
[Produces("application/json")]
public class CrossPortfolioPlanningController : ControllerBase
{
    private readonly ICrossPortfolioPlanningService _crossPortfolioPlanningService;

    public CrossPortfolioPlanningController(ICrossPortfolioPlanningService crossPortfolioPlanningService)
    {
        _crossPortfolioPlanningService = crossPortfolioPlanningService;
    }

    /// <summary>
    /// Returns the Cross-Portfolio Planning overview: active Portfolios in the resolved Company,
    /// presented as participation candidates for a simulation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CrossPortfolioOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CrossPortfolioOverviewResponse>> GetOverview(
        [FromQuery] CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _crossPortfolioPlanningService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Lists temporary, in-memory Cross-Portfolio Plan scenarios previously simulated for the
    /// resolved Company (DEC-405-001).
    /// </summary>
    [HttpGet("scenarios")]
    [ProducesResponseType(typeof(CrossPortfolioScenarioListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CrossPortfolioScenarioListResponse>> GetScenarios(
        [FromQuery] CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var scenarios = await _crossPortfolioPlanningService.GetScenariosAsync(parameters, cancellationToken);
        return Ok(scenarios);
    }

    /// <summary>
    /// Detects Portfolio Mission overlap and Resource conflicts (delegated to
    /// <c>IAllocationConflictDetectionService</c>) across the selected Portfolios.
    /// </summary>
    [HttpGet("conflicts")]
    [ProducesResponseType(typeof(ConflictSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictSummaryResponse>> GetConflicts(
        [FromQuery] CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var conflicts = await _crossPortfolioPlanningService.GetConflictsAsync(parameters, cancellationToken);
        return Ok(conflicts);
    }

    /// <summary>
    /// Returns enterprise Capacity/Workload balance and advisory rebalancing recommendations for
    /// the selected Portfolios. Mission priorities are preserved and never reordered.
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(CrossPortfolioBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CrossPortfolioBalanceResponse>> GetBalance(
        [FromQuery] CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var balance = await _crossPortfolioPlanningService.GetBalanceAsync(parameters, cancellationToken);
        return Ok(balance);
    }

    /// <summary>
    /// Simulates a temporary Cross-Portfolio Plan across at least two Portfolios: builds enterprise
    /// Capacity/Workload views, detects conflicts, computes advisory balancing recommendations, and
    /// stores the resulting scenario in-memory (DEC-405-001). Every simulation is audited
    /// (BR-2309) and requires human approval — nothing is executed automatically.
    /// </summary>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(SimulationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationSummaryResponse>> Simulate(
        [FromBody] SimulateCrossPortfolioPlanRequest request,
        CancellationToken cancellationToken)
    {
        var summary = await _crossPortfolioPlanningService.SimulateAsync(request, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Compares two previously simulated, in-memory Cross-Portfolio Plan scenarios by id.
    /// </summary>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(ScenarioComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScenarioComparisonResponse>> Compare(
        [FromBody] CompareCrossPortfolioScenariosRequest request,
        CancellationToken cancellationToken)
    {
        var comparison = await _crossPortfolioPlanningService.CompareAsync(request, cancellationToken);
        return Ok(comparison);
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Enterprise Dashboard (US-403 / BR-2101..BR-2110). Aggregates existing
/// Recommendation, Decision, Portfolio, Capacity/Workload History, Planning Template,
/// AI Decision Support, and Audit data. Never recalculates history and never persists
/// new analytical storage.
/// </summary>
[ApiController]
[Route("enterprise-dashboard")]
[Produces("application/json")]
public class EnterpriseDashboardController : ControllerBase
{
    private readonly IEnterpriseDashboardService _enterpriseDashboardService;

    public EnterpriseDashboardController(IEnterpriseDashboardService enterpriseDashboardService)
    {
        _enterpriseDashboardService = enterpriseDashboardService;
    }

    /// <summary>
    /// Returns the full Enterprise Dashboard for a Company and reporting period.
    /// </summary>
    /// <response code="200">Full dashboard.</response>
    [HttpGet]
    [ProducesResponseType(typeof(EnterpriseDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardResponse>> GetDashboard(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var dashboard = await _enterpriseDashboardService.GetDashboardAsync(parameters, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Returns the Summary section (Portfolio/Recommendation/Decision headline counts and overall health).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(EnterpriseDashboardSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardSummaryResponse>> GetSummary(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _enterpriseDashboardService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Returns the Planning Template section (status breakdown).
    /// </summary>
    [HttpGet("planning")]
    [ProducesResponseType(typeof(EnterpriseDashboardPlanningResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardPlanningResponse>> GetPlanning(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var planning = await _enterpriseDashboardService.GetPlanningAsync(parameters, cancellationToken);
        return Ok(planning);
    }

    /// <summary>
    /// Returns the Portfolio section (status/health breakdown and average utilization/workload).
    /// </summary>
    [HttpGet("portfolio")]
    [ProducesResponseType(typeof(EnterpriseDashboardPortfolioResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardPortfolioResponse>> GetPortfolio(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var portfolio = await _enterpriseDashboardService.GetPortfolioAsync(parameters, cancellationToken);
        return Ok(portfolio);
    }

    /// <summary>
    /// Returns the Capacity section from immutable Capacity History (never recalculated).
    /// </summary>
    [HttpGet("capacity")]
    [ProducesResponseType(typeof(EnterpriseDashboardCapacityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardCapacityResponse>> GetCapacity(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacity = await _enterpriseDashboardService.GetCapacityAsync(parameters, cancellationToken);
        return Ok(capacity);
    }

    /// <summary>
    /// Returns the Workload section from immutable Workload History (never recalculated).
    /// </summary>
    [HttpGet("workload")]
    [ProducesResponseType(typeof(EnterpriseDashboardWorkloadResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardWorkloadResponse>> GetWorkload(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workload = await _enterpriseDashboardService.GetWorkloadAsync(parameters, cancellationToken);
        return Ok(workload);
    }

    /// <summary>
    /// Returns the Recommendations section (counts, status breakdown, average score, trend).
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(EnterpriseDashboardRecommendationsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardRecommendationsResponse>> GetRecommendations(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var recommendations = await _enterpriseDashboardService.GetRecommendationsAsync(parameters, cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>
    /// Returns the Decisions section (status/implementation breakdown, completed/cancelled, trend).
    /// </summary>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(EnterpriseDashboardDecisionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardDecisionsResponse>> GetDecisions(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var decisions = await _enterpriseDashboardService.GetDecisionsAsync(parameters, cancellationToken);
        return Ok(decisions);
    }

    /// <summary>
    /// Returns the AI Decision Support section (AI Recommendations, Explainability, Executive Summaries).
    /// </summary>
    [HttpGet("ai")]
    [ProducesResponseType(typeof(EnterpriseDashboardAiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardAiResponse>> GetAi(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var ai = await _enterpriseDashboardService.GetAiAsync(parameters, cancellationToken);
        return Ok(ai);
    }

    /// <summary>
    /// Returns the Audit section (event/entity type breakdown, last event, trend).
    /// </summary>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(EnterpriseDashboardAuditResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnterpriseDashboardAuditResponse>> GetAudit(
        [FromQuery] EnterpriseDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var audit = await _enterpriseDashboardService.GetAuditAsync(parameters, cancellationToken);
        return Ok(audit);
    }
}

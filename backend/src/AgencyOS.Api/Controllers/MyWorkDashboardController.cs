using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only My Work Dashboard (US-501 / BR-2401..BR-2410). Personalized operational workspace
/// consolidating a caller's assigned work, pending Recommendations/Decisions, Capacity/Workload,
/// Activity timeline, and personal KPIs from existing modules. Never modifies data — drill-down
/// links only navigate to existing read views.
/// </summary>
[ApiController]
[Route("my-work")]
[Produces("application/json")]
public class MyWorkDashboardController : ControllerBase
{
    private readonly IMyWorkDashboardService _myWorkDashboardService;

    public MyWorkDashboardController(IMyWorkDashboardService myWorkDashboardService)
    {
        _myWorkDashboardService = myWorkDashboardService;
    }

    /// <summary>
    /// Returns the full My Work Dashboard for the resolved caller identity (DEC-501-001).
    /// </summary>
    /// <response code="200">Full dashboard.</response>
    [HttpGet]
    [ProducesResponseType(typeof(MyWorkDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkDashboardResponse>> GetDashboard(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var dashboard = await _myWorkDashboardService.GetDashboardAsync(parameters, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Returns the condensed Summary section (headline counts, KPIs, Capacity/Workload).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(MyWorkSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkSummaryResponse>> GetSummary(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _myWorkDashboardService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Returns the caller's active Task cards plus overdue/upcoming deadline breakdowns.
    /// </summary>
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(MyWorkTasksResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkTasksResponse>> GetTasks(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var tasks = await _myWorkDashboardService.GetTasksAsync(parameters, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Returns the caller's active Mission cards.
    /// </summary>
    [HttpGet("missions")]
    [ProducesResponseType(typeof(MyWorkMissionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkMissionsResponse>> GetMissions(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var missions = await _myWorkDashboardService.GetMissionsAsync(parameters, cancellationToken);
        return Ok(missions);
    }

    /// <summary>
    /// Returns pending Recommendation Workflow cards (company attention queue).
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(MyWorkRecommendationsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkRecommendationsResponse>> GetRecommendations(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var recommendations = await _myWorkDashboardService.GetRecommendationsAsync(parameters, cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>
    /// Returns pending Decision cards (DecisionStatus Created or InProgress).
    /// </summary>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(MyWorkDecisionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkDecisionsResponse>> GetDecisions(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var decisions = await _myWorkDashboardService.GetDecisionsAsync(parameters, cancellationToken);
        return Ok(decisions);
    }

    /// <summary>
    /// Returns the caller's Activity Timeline, projected from Audit Events.
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(MyWorkActivityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkActivityResponse>> GetActivity(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var activity = await _myWorkDashboardService.GetActivityAsync(parameters, cancellationToken);
        return Ok(activity);
    }

    /// <summary>
    /// Returns the personal KPI rollup only.
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(MyWorkKpiSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkKpiSummaryResponse>> GetKpis(
        [FromQuery] MyWorkDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var kpis = await _myWorkDashboardService.GetKpisAsync(parameters, cancellationToken);
        return Ok(kpis);
    }
}

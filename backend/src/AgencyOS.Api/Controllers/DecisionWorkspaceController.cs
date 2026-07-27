using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Decision Workspace (US-504 / BR-2701..BR-2710). Orchestration façade over the
/// existing Decision lifecycle, Recommendation linkage, Decision Timeline, and Decision Audit
/// capabilities. Never mutates Decision data and never bypasses mandatory human approval
/// (DEC-504-001). Create Decision/Start Implementation/Complete/Cancel/Record Outcome are
/// exposed exclusively as navigation deep-links.
/// </summary>
[ApiController]
[Route("decision-workspace")]
[Produces("application/json")]
public class DecisionWorkspaceController : ControllerBase
{
    private readonly IDecisionWorkspaceService _decisionWorkspaceService;

    public DecisionWorkspaceController(IDecisionWorkspaceService decisionWorkspaceService)
    {
        _decisionWorkspaceService = decisionWorkspaceService;
    }

    /// <summary>Returns the full Decision Workspace for the resolved Company.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DecisionWorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionWorkspaceResponse>> GetWorkspace(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workspace = await _decisionWorkspaceService.GetWorkspaceAsync(parameters, cancellationToken);
        return Ok(workspace);
    }

    /// <summary>Returns Decision Workspace overview KPIs.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(DecisionOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionOverviewResponse>> GetOverview(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _decisionWorkspaceService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>Returns Decision cards grouped by DecisionStatus with navigation actions.</summary>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(DecisionsSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionsSectionResponse>> GetDecisions(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var decisions = await _decisionWorkspaceService.GetDecisionsAsync(parameters, cancellationToken);
        return Ok(decisions);
    }

    /// <summary>
    /// Returns the immutable Decision Timeline. When DecisionId is provided, returns the full
    /// Timeline for that Decision; otherwise returns a bounded, aggregated view of recent
    /// Timeline entries across recent Decisions for the Company (BR-2703).
    /// </summary>
    [HttpGet("timeline")]
    [ProducesResponseType(typeof(DecisionTimelineSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionTimelineSectionResponse>> GetTimeline(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var timeline = await _decisionWorkspaceService.GetTimelineAsync(parameters, cancellationToken);
        return Ok(timeline);
    }

    /// <summary>Returns Decisions with a recorded Outcome (BR-2705: navigation only — never records an outcome).</summary>
    [HttpGet("outcomes")]
    [ProducesResponseType(typeof(DecisionOutcomesSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionOutcomesSectionResponse>> GetOutcomes(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var outcomes = await _decisionWorkspaceService.GetOutcomesAsync(parameters, cancellationToken);
        return Ok(outcomes);
    }

    /// <summary>Returns the immutable Decision Audit trail for the Company and period (BR-2704).</summary>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(DecisionAuditSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionAuditSectionResponse>> GetAudit(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var audit = await _decisionWorkspaceService.GetAuditAsync(parameters, cancellationToken);
        return Ok(audit);
    }

    /// <summary>Returns Decision Workspace KPIs only.</summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(DecisionKpiSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionKpiSummaryResponse>> GetKpis(
        [FromQuery] DecisionWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var kpis = await _decisionWorkspaceService.GetKpisAsync(parameters, cancellationToken);
        return Ok(kpis);
    }
}

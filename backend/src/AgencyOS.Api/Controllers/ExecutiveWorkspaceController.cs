using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Executive Workspace (US-505 / BR-2801..BR-2810). Orchestration façade that primarily
/// reuses <see cref="IEnterpriseDashboardService"/> section methods and adds executive navigation
/// deep-links into every existing operational workspace and dashboard. Never mutates operational
/// data (DEC-505-001). This completes EPIC-05 — Operational Workspace (US-501–US-505).
/// </summary>
[ApiController]
[Route("executive-workspace")]
[Produces("application/json")]
public class ExecutiveWorkspaceController : ControllerBase
{
    private readonly IExecutiveWorkspaceService _executiveWorkspaceService;

    public ExecutiveWorkspaceController(IExecutiveWorkspaceService executiveWorkspaceService)
    {
        _executiveWorkspaceService = executiveWorkspaceService;
    }

    /// <summary>Returns the full Executive Workspace for the resolved Company.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ExecutiveWorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveWorkspaceResponse>> GetWorkspace(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workspace = await _executiveWorkspaceService.GetWorkspaceAsync(parameters, cancellationToken);
        return Ok(workspace);
    }

    /// <summary>Returns Executive Workspace overview KPIs, overall health, and narrative.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(ExecutiveOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveOverviewResponse>> GetOverview(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _executiveWorkspaceService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>Returns the Enterprise section (embeds the existing Enterprise Dashboard Summary).</summary>
    [HttpGet("enterprise")]
    [ProducesResponseType(typeof(ExecutiveEnterpriseSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveEnterpriseSectionResponse>> GetEnterprise(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var enterprise = await _executiveWorkspaceService.GetEnterpriseAsync(parameters, cancellationToken);
        return Ok(enterprise);
    }

    /// <summary>Returns the Portfolios section (embeds the existing Enterprise Dashboard Portfolio rollup).</summary>
    [HttpGet("portfolios")]
    [ProducesResponseType(typeof(ExecutivePortfoliosSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutivePortfoliosSectionResponse>> GetPortfolios(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var portfolios = await _executiveWorkspaceService.GetPortfoliosAsync(parameters, cancellationToken);
        return Ok(portfolios);
    }

    /// <summary>Returns the Recommendations section (embeds the existing Enterprise Dashboard Recommendations rollup).</summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(ExecutiveRecommendationsSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationsSectionResponse>> GetRecommendations(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var recommendations = await _executiveWorkspaceService.GetRecommendationsAsync(parameters, cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>Returns the Decisions section (embeds the existing Enterprise Dashboard Decisions rollup).</summary>
    [HttpGet("decisions")]
    [ProducesResponseType(typeof(ExecutiveDecisionsSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveDecisionsSectionResponse>> GetDecisions(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var decisions = await _executiveWorkspaceService.GetDecisionsAsync(parameters, cancellationToken);
        return Ok(decisions);
    }

    /// <summary>Returns the Capacity section from immutable Capacity History (never recalculated).</summary>
    [HttpGet("capacity")]
    [ProducesResponseType(typeof(ExecutiveCapacitySectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveCapacitySectionResponse>> GetCapacity(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacity = await _executiveWorkspaceService.GetCapacityAsync(parameters, cancellationToken);
        return Ok(capacity);
    }

    /// <summary>Returns the Workload section from immutable Workload History (never recalculated).</summary>
    [HttpGet("workload")]
    [ProducesResponseType(typeof(ExecutiveWorkloadSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveWorkloadSectionResponse>> GetWorkload(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workload = await _executiveWorkspaceService.GetWorkloadAsync(parameters, cancellationToken);
        return Ok(workload);
    }

    /// <summary>Returns the AI section (advisory AI Recommendations, informational Explainability and Executive Summaries).</summary>
    [HttpGet("ai")]
    [ProducesResponseType(typeof(ExecutiveAiSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveAiSectionResponse>> GetAi(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var ai = await _executiveWorkspaceService.GetAiAsync(parameters, cancellationToken);
        return Ok(ai);
    }

    /// <summary>Returns the immutable Audit section for the Company and period (BR-2810).</summary>
    [HttpGet("audit")]
    [ProducesResponseType(typeof(ExecutiveAuditSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveAuditSectionResponse>> GetAudit(
        [FromQuery] ExecutiveWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var audit = await _executiveWorkspaceService.GetAuditAsync(parameters, cancellationToken);
        return Ok(audit);
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Planning Workspace (US-502 / BR-2501..BR-2510). Orchestration façade over existing
/// Planning Template, Capacity/Workload History, Portfolio, and Cross-Portfolio Planning
/// capabilities. Never mutates planning data and never recalculates engines for storage
/// (DEC-502-001). Cross-Portfolio scenarios remain advisory only.
/// </summary>
[ApiController]
[Route("planning-workspace")]
[Produces("application/json")]
public class PlanningWorkspaceController : ControllerBase
{
    private readonly IPlanningWorkspaceService _planningWorkspaceService;

    public PlanningWorkspaceController(IPlanningWorkspaceService planningWorkspaceService)
    {
        _planningWorkspaceService = planningWorkspaceService;
    }

    /// <summary>Returns the full Planning Workspace for the resolved Company.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PlanningWorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningWorkspaceResponse>> GetWorkspace(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workspace = await _planningWorkspaceService.GetWorkspaceAsync(parameters, cancellationToken);
        return Ok(workspace);
    }

    /// <summary>Returns Planning Overview KPIs and section counts.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(PlanningOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningOverviewResponse>> GetOverview(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _planningWorkspaceService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>Returns Planning Template cards with manage/apply navigation actions.</summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(PlanningTemplatesSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningTemplatesSectionResponse>> GetTemplates(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var templates = await _planningWorkspaceService.GetTemplatesAsync(parameters, cancellationToken);
        return Ok(templates);
    }

    /// <summary>Returns Capacity History aggregate and recent history for the period.</summary>
    [HttpGet("capacity")]
    [ProducesResponseType(typeof(PlanningCapacitySectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningCapacitySectionResponse>> GetCapacity(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacity = await _planningWorkspaceService.GetCapacityAsync(parameters, cancellationToken);
        return Ok(capacity);
    }

    /// <summary>Returns Workload History aggregate and recent history for the period.</summary>
    [HttpGet("workload")]
    [ProducesResponseType(typeof(PlanningWorkloadSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningWorkloadSectionResponse>> GetWorkload(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workload = await _planningWorkspaceService.GetWorkloadAsync(parameters, cancellationToken);
        return Ok(workload);
    }

    /// <summary>Returns Portfolio Planning cards with drill-down into existing Portfolio routes.</summary>
    [HttpGet("portfolios")]
    [ProducesResponseType(typeof(PlanningPortfoliosSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningPortfoliosSectionResponse>> GetPortfolios(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var portfolios = await _planningWorkspaceService.GetPortfoliosAsync(parameters, cancellationToken);
        return Ok(portfolios);
    }

    /// <summary>Returns planning-related Audit History for the Company and period.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PlanningHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningHistoryResponse>> GetHistory(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _planningWorkspaceService.GetHistoryAsync(parameters, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Returns temporary Cross-Portfolio Planning scenarios (advisory only; BR-2507).
    /// </summary>
    [HttpGet("scenarios")]
    [ProducesResponseType(typeof(PlanningScenariosSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningScenariosSectionResponse>> GetScenarios(
        [FromQuery] PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var scenarios = await _planningWorkspaceService.GetScenariosAsync(parameters, cancellationToken);
        return Ok(scenarios);
    }
}

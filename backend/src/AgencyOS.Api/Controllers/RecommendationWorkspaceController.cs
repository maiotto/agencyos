using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Recommendation Workspace (US-503 / BR-2601..BR-2610). Orchestration façade over
/// existing Recommendation, Recommendation Workflow, AI Recommendation, Explainability,
/// Executive Summary, Recommendation History, and Recommendation Comparison capabilities. Never
/// mutates Recommendation data and never bypasses mandatory human approval (DEC-503-001).
/// Generate/Approve/Reject/Archive/Restore/Start Workflow/Generate AI/Generate
/// Explainability/Generate Executive Summary are exposed exclusively as navigation deep-links.
/// </summary>
[ApiController]
[Route("recommendation-workspace")]
[Produces("application/json")]
public class RecommendationWorkspaceController : ControllerBase
{
    private readonly IRecommendationWorkspaceService _recommendationWorkspaceService;

    public RecommendationWorkspaceController(IRecommendationWorkspaceService recommendationWorkspaceService)
    {
        _recommendationWorkspaceService = recommendationWorkspaceService;
    }

    /// <summary>Returns the full Recommendation Workspace for the resolved Company.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(RecommendationWorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkspaceResponse>> GetWorkspace(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workspace = await _recommendationWorkspaceService.GetWorkspaceAsync(parameters, cancellationToken);
        return Ok(workspace);
    }

    /// <summary>Returns Recommendation Workspace overview KPIs.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(RecommendationOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationOverviewResponse>> GetOverview(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var overview = await _recommendationWorkspaceService.GetOverviewAsync(parameters, cancellationToken);
        return Ok(overview);
    }

    /// <summary>Returns active Recommendation cards with navigation actions.</summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(RecommendationsSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationsSectionResponse>> GetRecommendations(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var recommendations = await _recommendationWorkspaceService.GetRecommendationsSectionAsync(
            parameters,
            cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>Returns PendingApproval Workflow cards. Approve/Reject require explicit human action (BR-2604).</summary>
    [HttpGet("approval")]
    [ProducesResponseType(typeof(ApprovalSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalSectionResponse>> GetApproval(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var approval = await _recommendationWorkspaceService.GetApprovalSectionAsync(parameters, cancellationToken);
        return Ok(approval);
    }

    /// <summary>Returns immutable Recommendation History for the Company and period (BR-2609).</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(HistorySectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HistorySectionResponse>> GetHistory(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _recommendationWorkspaceService.GetHistorySectionAsync(parameters, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Returns a side-by-side Recommendation comparison when both LeftRecommendationId and
    /// RightRecommendationId are provided and distinct; otherwise returns a navigation stub only.
    /// </summary>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(CompareSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompareSectionResponse>> GetCompare(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var compare = await _recommendationWorkspaceService.GetCompareSectionAsync(parameters, cancellationToken);
        return Ok(compare);
    }

    /// <summary>Returns advisory AI Recommendation cards plus informational Explainability cards.</summary>
    [HttpGet("ai")]
    [ProducesResponseType(typeof(AiSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiSectionResponse>> GetAi(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var ai = await _recommendationWorkspaceService.GetAiSectionAsync(parameters, cancellationToken);
        return Ok(ai);
    }

    /// <summary>Returns Executive Recommendation Summary cards.</summary>
    [HttpGet("executive-summary")]
    [ProducesResponseType(typeof(ExecutiveSummarySectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveSummarySectionResponse>> GetExecutiveSummary(
        [FromQuery] RecommendationWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var executiveSummary = await _recommendationWorkspaceService.GetExecutiveSummaryAsync(
            parameters,
            cancellationToken);
        return Ok(executiveSummary);
    }
}

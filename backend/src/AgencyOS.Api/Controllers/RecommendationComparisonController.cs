using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Recommendation Comparison APIs (US-204 / BR-1301..BR-1306).
/// Comparisons are computed from immutable Recommendation History snapshots and never mutate Recommendations.
/// </summary>
[ApiController]
[Route("recommendations/compare")]
[Produces("application/json")]
public class RecommendationComparisonController : ControllerBase
{
    private readonly IRecommendationComparisonService _comparisonService;

    public RecommendationComparisonController(IRecommendationComparisonService comparisonService)
    {
        _comparisonService = comparisonService;
    }

    /// <summary>
    /// Compares two recommendation history snapshots (or recommendation ids resolved to VersionCreated snapshots).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RecommendationComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationComparisonResponse>> Compare(
        [FromQuery] RecommendationComparisonQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _comparisonService.CompareAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Compares VersionCreated history snapshots for a recommendation number lineage.
    /// When versions are omitted, the two most recent versions are compared.
    /// </summary>
    [HttpGet("version/{recommendationNumber}")]
    [ProducesResponseType(typeof(RecommendationComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationComparisonResponse>> CompareVersions(
        string recommendationNumber,
        [FromQuery] RecommendationVersionComparisonQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _comparisonService.CompareVersionsAsync(
            recommendationNumber,
            parameters,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Compares two snapshots by history id or recommendation id (BR-1303).
    /// </summary>
    [HttpGet("{leftId:guid}/{rightId:guid}")]
    [ProducesResponseType(typeof(RecommendationComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationComparisonResponse>> CompareByIds(
        Guid leftId,
        Guid rightId,
        CancellationToken cancellationToken)
    {
        var result = await _comparisonService.CompareByIdsAsync(leftId, rightId, cancellationToken);
        return Ok(result);
    }
}

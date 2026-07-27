using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("recommendations/history")]
[Produces("application/json")]
public class RecommendationHistoryController : ControllerBase
{
    private readonly IRecommendationHistoryService _recommendationHistoryService;

    public RecommendationHistoryController(IRecommendationHistoryService recommendationHistoryService)
    {
        _recommendationHistoryService = recommendationHistoryService;
    }

    /// <summary>
    /// Lists immutable recommendation history entries (US-203).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RecommendationHistoryResponse>>> GetAll(
        [FromQuery] RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationHistoryService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Filters recommendation history (BR-1206).
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RecommendationHistoryResponse>>> Filter(
        [FromQuery] RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationHistoryService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns version-created history entries for a recommendation lineage.
    /// </summary>
    [HttpGet("{recommendationId:guid}/versions")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecommendationHistoryResponse>>> GetVersions(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationHistoryService.GetVersionsAsync(recommendationId, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns the complete recommendation + workflow timeline for a recommendation lineage (BR-1203).
    /// </summary>
    [HttpGet("{recommendationId:guid}/timeline")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationHistoryTimelineEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecommendationHistoryTimelineEntryResponse>>> GetTimeline(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationHistoryService.GetTimelineAsync(recommendationId, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a single immutable history snapshot by history id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecommendationHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationHistoryResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationHistoryService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Executive Recommendation Summary APIs (US-303 / BR-1801..BR-1810).
/// Informational executive briefings. Never changes Recommendations, AI Recommendations, or Decisions.
/// Completes EPIC-03 — AI Decision Support.
/// </summary>
[ApiController]
[Route("executive-summaries")]
[Produces("application/json")]
public class ExecutiveRecommendationSummariesController : ControllerBase
{
    private readonly IExecutiveRecommendationSummaryService _service;

    public ExecutiveRecommendationSummariesController(IExecutiveRecommendationSummaryService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExecutiveRecommendationSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ExecutiveRecommendationSummaryResponse>>> GetAll(
        [FromQuery] ExecutiveRecommendationSummaryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExecutiveRecommendationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationSummaryResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("{id:guid}/compare")]
    [ProducesResponseType(typeof(ExecutiveRecommendationSummaryComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationSummaryComparisonResponse>> Compare(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _service.CompareAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("recommendation/{recommendationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ExecutiveRecommendationSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ExecutiveRecommendationSummaryResponse>>> GetByRecommendation(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var items = await _service.GetByRecommendationIdAsync(recommendationId, cancellationToken);
        return Ok(items);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(ExecutiveRecommendationSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationSummaryResponse>> Generate(
        [FromBody] GenerateExecutiveRecommendationSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(ExecutiveRecommendationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationSummaryResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _service.ArchiveAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(typeof(ExecutiveRecommendationSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutiveRecommendationSummaryResponse>> CreateNewVersion(
        Guid id,
        [FromBody] CreateExecutiveRecommendationSummaryVersionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.CreateNewVersionAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }
}

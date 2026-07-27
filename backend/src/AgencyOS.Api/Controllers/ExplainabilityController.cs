using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// LLM Explainability APIs (US-302 / BR-1701..BR-1710).
/// Informational only. Never changes Recommendations or Decision Engine calculations.
/// </summary>
[ApiController]
[Route("explainability")]
[Produces("application/json")]
public class ExplainabilityController : ControllerBase
{
    private readonly IExplainabilityService _explainabilityService;

    public ExplainabilityController(IExplainabilityService explainabilityService)
    {
        _explainabilityService = explainabilityService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExplainabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ExplainabilityResponse>>> GetAll(
        [FromQuery] ExplainabilityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _explainabilityService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExplainabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExplainabilityResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _explainabilityService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("recommendation/{recommendationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ExplainabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ExplainabilityResponse>>> GetByRecommendation(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var items = await _explainabilityService.GetByRecommendationIdAsync(
            recommendationId,
            cancellationToken);
        return Ok(items);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(ExplainabilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExplainabilityResponse>> Generate(
        [FromBody] GenerateExplainabilityRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _explainabilityService.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(ExplainabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExplainabilityResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _explainabilityService.ArchiveAsync(id, cancellationToken);
        return Ok(item);
    }
}

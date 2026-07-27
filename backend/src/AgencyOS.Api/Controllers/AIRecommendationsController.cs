using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// AI-assisted Recommendation APIs (US-301 / BR-1601..BR-1610).
/// Advisory only. Never replaces Recommendations. Human approval remains mandatory.
/// </summary>
[ApiController]
[Route("ai-recommendations")]
[Produces("application/json")]
public class AIRecommendationsController : ControllerBase
{
    private readonly IAIRecommendationService _aiRecommendationService;

    public AIRecommendationsController(IAIRecommendationService aiRecommendationService)
    {
        _aiRecommendationService = aiRecommendationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AIRecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AIRecommendationResponse>>> GetAll(
        [FromQuery] AIRecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _aiRecommendationService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIRecommendationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _aiRecommendationService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("{id:guid}/compare")]
    [ProducesResponseType(typeof(AIRecommendationComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIRecommendationComparisonResponse>> Compare(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _aiRecommendationService.CompareAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("recommendation/{recommendationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AIRecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AIRecommendationResponse>>> GetByRecommendation(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var items = await _aiRecommendationService.GetByRecommendationIdAsync(
            recommendationId,
            cancellationToken);
        return Ok(items);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIRecommendationResponse>> Generate(
        [FromBody] GenerateAIRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _aiRecommendationService.GenerateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIRecommendationResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _aiRecommendationService.ArchiveAsync(id, cancellationToken);
        return Ok(item);
    }
}

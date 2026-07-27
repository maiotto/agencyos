using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Decision Tracking APIs (US-205 / BR-1401..BR-1407).
/// </summary>
[ApiController]
[Route("decisions")]
[Produces("application/json")]
public class DecisionsController : ControllerBase
{
    private readonly IDecisionService _decisionService;

    public DecisionsController(IDecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    /// <summary>
    /// Lists decisions with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DecisionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<DecisionResponse>>> GetAll(
        [FromQuery] DecisionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _decisionService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Filters decisions (search / status / company / mission / contract).
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<DecisionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<DecisionResponse>>> Filter(
        [FromQuery] DecisionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _decisionService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a decision including timeline.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Returns the append-only decision timeline (BR-1404).
    /// </summary>
    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(typeof(IReadOnlyList<DecisionTimelineEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DecisionTimelineEntryResponse>>> GetTimeline(
        Guid id,
        CancellationToken cancellationToken)
    {
        var timeline = await _decisionService.GetTimelineAsync(id, cancellationToken);
        return Ok(timeline);
    }

    /// <summary>
    /// Creates a Decision from an Approved Recommendation (BR-1401).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> Create(
        [FromBody] CreateDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Starts implementation for a Created Decision.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> Start(
        Guid id,
        [FromBody] DecisionActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.StartImplementationAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Completes an In Progress Decision (BR-1405).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> Complete(
        Guid id,
        [FromBody] DecisionActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.CompleteAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Cancels a Created or In Progress Decision.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> Cancel(
        Guid id,
        [FromBody] DecisionActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.CancelAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Records outcome after completion (BR-1406).
    /// </summary>
    [HttpPost("{id:guid}/outcome")]
    [ProducesResponseType(typeof(DecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DecisionResponse>> RecordOutcome(
        Guid id,
        [FromBody] RecordDecisionOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _decisionService.RecordOutcomeAsync(id, request, cancellationToken);
        return Ok(item);
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("recommendations/workflow")]
[Produces("application/json")]
public class RecommendationWorkflowController : ControllerBase
{
    private readonly IRecommendationWorkflowService _recommendationWorkflowService;

    public RecommendationWorkflowController(IRecommendationWorkflowService recommendationWorkflowService)
    {
        _recommendationWorkflowService = recommendationWorkflowService;
    }

    /// <summary>
    /// Lists recommendation approval workflows with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationWorkflowResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RecommendationWorkflowResponse>>> GetAll(
        [FromQuery] RecommendationWorkflowQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationWorkflowService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a recommendation workflow including transition history.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Returns the immutable approval timeline for a recommendation workflow.
    /// </summary>
    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationWorkflowTransitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecommendationWorkflowTransitionResponse>>> GetTimeline(
        Guid id,
        CancellationToken cancellationToken)
    {
        var timeline = await _recommendationWorkflowService.GetTimelineAsync(id, cancellationToken);
        return Ok(timeline);
    }

    /// <summary>
    /// Creates a recommendation workflow in Draft status (BR-1001).
    /// Does not generate Decision Engine recommendations.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Create(
        [FromBody] CreateRecommendationWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Submits a Draft or Reopened recommendation for approval (BR-1002).
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Submit(
        Guid id,
        [FromBody] RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.SubmitAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Approves a Pending Approval recommendation (BR-1003 / BR-1005 / BR-1008).
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Approve(
        Guid id,
        [FromBody] ApproveRecommendationWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.ApproveAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Rejects a Pending Approval recommendation (BR-1004).
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Reject(
        Guid id,
        [FromBody] RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.RejectAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Cancels a recommendation before approval (BR-1007).
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Cancel(
        Guid id,
        [FromBody] RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.CancelAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Reopens a Rejected recommendation (BR-1006). Cancelled recommendations cannot be reopened.
    /// </summary>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(typeof(RecommendationWorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationWorkflowResponse>> Reopen(
        Guid id,
        [FromBody] RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationWorkflowService.ReopenAsync(id, request, cancellationToken);
        return Ok(item);
    }
}

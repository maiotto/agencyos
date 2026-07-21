using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("capacity")]
[Produces("application/json")]
public class CapacityController : ControllerBase
{
    private readonly ICapacityCalculatorService _capacityCalculatorService;

    public CapacityController(ICapacityCalculatorService capacityCalculatorService)
    {
        _capacityCalculatorService = capacityCalculatorService;
    }

    /// <summary>
    /// Calculates operational capacity for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Capacity calculated for all active execution resources.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CapacityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CapacityResponse>>> GetAll(
        [FromQuery] CapacityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacities = await _capacityCalculatorService.GetAllAsync(parameters, cancellationToken);
        return Ok(capacities);
    }

    /// <summary>
    /// Calculates aggregated capacity summary for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Capacity summary calculated.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CapacitySummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CapacitySummaryResponse>> GetSummary(
        [FromQuery] CapacityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _capacityCalculatorService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Calculates operational capacity for a specific active execution resource in the planning period.
    /// </summary>
    /// <response code="200">Capacity calculated for the execution resource.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Execution resource not found or not active.</response>
    [HttpGet("{resourceId:guid}")]
    [ProducesResponseType(typeof(CapacityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CapacityResponse>> GetByResourceId(
        Guid resourceId,
        [FromQuery] CapacityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacity = await _capacityCalculatorService.GetByResourceIdAsync(
            resourceId,
            parameters,
            cancellationToken);

        return Ok(capacity);
    }
}

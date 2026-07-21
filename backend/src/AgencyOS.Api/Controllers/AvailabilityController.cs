using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("availability")]
[Produces("application/json")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityEngineService _availabilityEngineService;

    public AvailabilityController(IAvailabilityEngineService availabilityEngineService)
    {
        _availabilityEngineService = availabilityEngineService;
    }

    /// <summary>
    /// Calculates availability for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Availability calculated for all active execution resources.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AvailabilityResponse>>> GetAll(
        [FromQuery] AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var availabilities = await _availabilityEngineService.GetAllAsync(parameters, cancellationToken);
        return Ok(availabilities);
    }

    /// <summary>
    /// Calculates aggregated availability summary for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Availability summary calculated.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AvailabilitySummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AvailabilitySummaryResponse>> GetSummary(
        [FromQuery] AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _availabilityEngineService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Calculates availability for a specific active execution resource in the planning period.
    /// </summary>
    /// <response code="200">Availability calculated for the execution resource.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Execution resource not found or not active.</response>
    [HttpGet("{resourceId:guid}")]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityResponse>> GetByResourceId(
        Guid resourceId,
        [FromQuery] AvailabilityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var availability = await _availabilityEngineService.GetByResourceIdAsync(
            resourceId,
            parameters,
            cancellationToken);

        return Ok(availability);
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("allocation-conflicts")]
[Produces("application/json")]
public class AllocationConflictController : ControllerBase
{
    private readonly IAllocationConflictDetectionService _allocationConflictDetectionService;

    public AllocationConflictController(
        IAllocationConflictDetectionService allocationConflictDetectionService)
    {
        _allocationConflictDetectionService = allocationConflictDetectionService;
    }

    /// <summary>
    /// Detects allocation conflicts for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Allocation conflicts detected for all active execution resources.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AllocationConflictResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AllocationConflictResponse>>> GetAll(
        [FromQuery] AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var conflicts = await _allocationConflictDetectionService.GetAllAsync(parameters, cancellationToken);
        return Ok(conflicts);
    }

    /// <summary>
    /// Detects aggregated allocation conflict summary for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Allocation conflict summary detected.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AllocationConflictSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AllocationConflictSummaryResponse>> GetSummary(
        [FromQuery] AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _allocationConflictDetectionService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Detects allocation conflicts for a specific active execution resource in the planning period.
    /// </summary>
    /// <response code="200">Allocation conflicts detected for the execution resource.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Execution resource not found or not active.</response>
    [HttpGet("{resourceId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AllocationConflictResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AllocationConflictResponse>>> GetByResourceId(
        Guid resourceId,
        [FromQuery] AllocationConflictQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var conflicts = await _allocationConflictDetectionService.GetByResourceIdAsync(
            resourceId,
            parameters,
            cancellationToken);

        return Ok(conflicts);
    }
}

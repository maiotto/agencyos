using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("workload")]
[Produces("application/json")]
public class WorkloadController : ControllerBase
{
    private readonly IWorkloadCalculatorService _workloadCalculatorService;

    public WorkloadController(IWorkloadCalculatorService workloadCalculatorService)
    {
        _workloadCalculatorService = workloadCalculatorService;
    }

    /// <summary>
    /// Calculates operational workload for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Workload calculated for all active execution resources.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkloadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkloadResponse>>> GetAll(
        [FromQuery] WorkloadQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workloads = await _workloadCalculatorService.GetAllAsync(parameters, cancellationToken);
        return Ok(workloads);
    }

    /// <summary>
    /// Calculates aggregated workload summary for all active execution resources in the planning period.
    /// </summary>
    /// <response code="200">Workload summary calculated.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(WorkloadSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkloadSummaryResponse>> GetSummary(
        [FromQuery] WorkloadQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var summary = await _workloadCalculatorService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Calculates operational workload for a specific active execution resource in the planning period.
    /// </summary>
    /// <response code="200">Workload calculated for the execution resource.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Execution resource not found or not active.</response>
    [HttpGet("{resourceId:guid}")]
    [ProducesResponseType(typeof(WorkloadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkloadResponse>> GetByResourceId(
        Guid resourceId,
        [FromQuery] WorkloadQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var workload = await _workloadCalculatorService.GetByResourceIdAsync(
            resourceId,
            parameters,
            cancellationToken);

        return Ok(workload);
    }
}

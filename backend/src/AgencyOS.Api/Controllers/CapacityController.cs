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
    private readonly ICapacityHistoryService _capacityHistoryService;

    public CapacityController(
        ICapacityCalculatorService capacityCalculatorService,
        ICapacityHistoryService capacityHistoryService)
    {
        _capacityCalculatorService = capacityCalculatorService;
        _capacityHistoryService = capacityHistoryService;
    }

    /// <summary>
    /// Calculates operational capacity for all active execution resources in the planning period
    /// using Working Calendar, Holidays, Working Hours, and Resource Availability (US-105).
    /// Successful calculations automatically persist immutable Capacity History (US-106).
    /// </summary>
    /// <response code="200">Capacity calculated for all active execution resources.</response>
    /// <response code="400">Validation or business-rule error (including missing operational configuration).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CapacityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CapacityResponse>>> GetAll(
        [FromQuery] CapacityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var capacities = await _capacityCalculatorService.GetAllAsync(parameters, cancellationToken);
        return Ok(capacities);
    }

    /// <summary>
    /// Calculates aggregated capacity summary for all active execution resources in the planning period.
    /// Does not persist Capacity History (use GetAll / GetByResourceId for historical snapshots).
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
    /// Queries immutable Capacity History with optional filters (resource, company, period, version).
    /// </summary>
    /// <response code="200">Historical capacity records matching the filters.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<CapacityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CapacityHistoryResponse>>> GetHistory(
        [FromQuery] CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _capacityHistoryService.QueryAsync(parameters, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Aggregates historical capacity totals for the filtered set of records.
    /// </summary>
    /// <response code="200">Aggregated historical capacity.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("history/aggregate")]
    [ProducesResponseType(typeof(CapacityHistoryAggregateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CapacityHistoryAggregateResponse>> GetHistoryAggregate(
        [FromQuery] CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var aggregate = await _capacityHistoryService.AggregateAsync(parameters, cancellationToken);
        return Ok(aggregate);
    }

    /// <summary>
    /// Compares two immutable Capacity History records and returns field-level deltas.
    /// </summary>
    /// <response code="200">Comparison result.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">One or both history records were not found.</response>
    [HttpGet("history/compare")]
    [ProducesResponseType(typeof(CapacityHistoryCompareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CapacityHistoryCompareResponse>> CompareHistory(
        [FromQuery] CapacityHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var comparison = await _capacityHistoryService.CompareAsync(parameters, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Returns Capacity History for a specific execution resource.
    /// </summary>
    /// <response code="200">Historical capacity for the execution resource.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("history/resource/{executionResourceId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CapacityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CapacityHistoryResponse>>> GetHistoryByResource(
        Guid executionResourceId,
        [FromQuery] CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _capacityHistoryService.GetByExecutionResourceIdAsync(
            executionResourceId,
            parameters,
            cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Returns Capacity History for a specific company.
    /// </summary>
    /// <response code="200">Historical capacity for the company.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet("history/company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CapacityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CapacityHistoryResponse>>> GetHistoryByCompany(
        Guid companyId,
        [FromQuery] CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _capacityHistoryService.GetByCompanyIdAsync(
            companyId,
            parameters,
            cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Returns a single immutable Capacity History record including daily operational snapshot.
    /// </summary>
    /// <response code="200">Capacity history detail.</response>
    /// <response code="404">History record was not found.</response>
    [HttpGet("history/{id:guid}")]
    [ProducesResponseType(typeof(CapacityHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CapacityHistoryResponse>> GetHistoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await _capacityHistoryService.GetByIdAsync(id, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Calculates operational capacity for a specific active execution resource in the planning period.
    /// Successful calculations automatically persist immutable Capacity History (US-106).
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

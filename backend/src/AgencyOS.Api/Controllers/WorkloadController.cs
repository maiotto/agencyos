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
    private readonly IWorkloadHistoryService _workloadHistoryService;

    public WorkloadController(
        IWorkloadCalculatorService workloadCalculatorService,
        IWorkloadHistoryService workloadHistoryService)
    {
        _workloadCalculatorService = workloadCalculatorService;
        _workloadHistoryService = workloadHistoryService;
    }

    /// <summary>
    /// Calculates operational workload for all active execution resources in the planning period.
    /// Successful calculations automatically persist immutable Workload History (US-107).
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
    /// Does not persist Workload History.
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
    /// Queries immutable Workload History with optional filters (resource, company, period, version).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkloadHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkloadHistoryResponse>>> GetHistory(
        [FromQuery] WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _workloadHistoryService.QueryAsync(parameters, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Aggregates historical workload totals for the filtered set of records.
    /// </summary>
    [HttpGet("history/aggregate")]
    [ProducesResponseType(typeof(WorkloadHistoryAggregateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkloadHistoryAggregateResponse>> GetHistoryAggregate(
        [FromQuery] WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var aggregate = await _workloadHistoryService.AggregateAsync(parameters, cancellationToken);
        return Ok(aggregate);
    }

    /// <summary>
    /// Returns chronological workload trend points for the filtered history set.
    /// </summary>
    [HttpGet("history/trends")]
    [ProducesResponseType(typeof(WorkloadHistoryTrendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkloadHistoryTrendResponse>> GetHistoryTrends(
        [FromQuery] WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var trends = await _workloadHistoryService.TrendsAsync(parameters, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Compares two immutable Workload History records and returns field-level deltas.
    /// </summary>
    [HttpGet("history/compare")]
    [ProducesResponseType(typeof(WorkloadHistoryCompareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkloadHistoryCompareResponse>> CompareHistory(
        [FromQuery] WorkloadHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var comparison = await _workloadHistoryService.CompareAsync(parameters, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Returns Workload History for a specific execution resource.
    /// </summary>
    [HttpGet("history/resource/{executionResourceId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkloadHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkloadHistoryResponse>>> GetHistoryByResource(
        Guid executionResourceId,
        [FromQuery] WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _workloadHistoryService.GetByExecutionResourceIdAsync(
            executionResourceId,
            parameters,
            cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Returns Workload History for a specific company.
    /// </summary>
    [HttpGet("history/company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkloadHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<WorkloadHistoryResponse>>> GetHistoryByCompany(
        Guid companyId,
        [FromQuery] WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var history = await _workloadHistoryService.GetByCompanyIdAsync(
            companyId,
            parameters,
            cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Returns a single immutable Workload History record including assignment snapshot.
    /// </summary>
    [HttpGet("history/{id:guid}")]
    [ProducesResponseType(typeof(WorkloadHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkloadHistoryResponse>> GetHistoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await _workloadHistoryService.GetByIdAsync(id, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Calculates operational workload for a specific active execution resource in the planning period.
    /// Successful calculations automatically persist immutable Workload History (US-107).
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

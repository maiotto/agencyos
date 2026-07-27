using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Personal Productivity Dashboard (US-507 / BR-3001..BR-3010).
/// Analytical, user-focused projections over existing operational data. Never modifies business behavior.
/// </summary>
[ApiController]
[Route("personal-dashboard")]
[Produces("application/json")]
public class PersonalProductivityDashboardController : ControllerBase
{
    private readonly IPersonalProductivityDashboardService _service;

    public PersonalProductivityDashboardController(IPersonalProductivityDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PersonalProductivityDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonalProductivityDashboardResponse>> GetDashboard(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDashboardAsync(parameters, cancellationToken));
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(PersonalProductivitySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalProductivitySummaryResponse>> GetSummary(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSummaryAsync(parameters, cancellationToken));
    }

    [HttpGet("kpis")]
    [ProducesResponseType(typeof(PersonalProductivityKpiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalProductivityKpiResponse>> GetKpis(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetKpisAsync(parameters, cancellationToken));
    }

    [HttpGet("trends")]
    [ProducesResponseType(typeof(PersonalProductivityTrendsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalProductivityTrendsResponse>> GetTrends(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTrendsAsync(parameters, cancellationToken));
    }

    [HttpGet("capacity")]
    [ProducesResponseType(typeof(MyWorkCapacitySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkCapacitySummaryResponse>> GetCapacity(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetCapacityAsync(parameters, cancellationToken));
    }

    [HttpGet("workload")]
    [ProducesResponseType(typeof(MyWorkWorkloadSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyWorkWorkloadSummaryResponse>> GetWorkload(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetWorkloadAsync(parameters, cancellationToken));
    }

    [HttpGet("activity")]
    [ProducesResponseType(typeof(PersonalProductivityActivitySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalProductivityActivitySummaryResponse>> GetActivity(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetActivityAsync(parameters, cancellationToken));
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(PersonalProductivityStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonalProductivityStatisticsResponse>> GetStatistics(
        [FromQuery] PersonalProductivityDashboardQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetStatisticsAsync(parameters, cancellationToken));
    }
}

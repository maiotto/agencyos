using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("portfolios")]
[Produces("application/json")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    /// <summary>
    /// Lists portfolios with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PortfolioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PortfolioResponse>>> GetAll(
        [FromQuery] PortfolioQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _portfolioService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Filters portfolios (search/status/company/mission/period).
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<PortfolioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PortfolioResponse>>> Filter(
        [FromQuery] PortfolioQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _portfolioService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a portfolio by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Returns consolidated portfolio summary with live and historical capacity/workload (BR-908..BR-910).
    /// </summary>
    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(PortfolioSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioSummaryResponse>> GetSummary(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.GetSummaryAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Returns portfolio health calculated from capacity/workload engines and history.
    /// </summary>
    [HttpGet("{id:guid}/health")]
    [ProducesResponseType(typeof(PortfolioHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioHealthResponse>> GetHealth(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.GetHealthAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Creates a portfolio with at least one Active Mission (BR-901..BR-905).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PortfolioResponse>> Create(
        [FromBody] CreatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Updates portfolio identity and planning period. Inactive portfolios cannot be modified (BR-906).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PortfolioResponse>> Update(
        Guid id,
        [FromBody] UpdatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Deletes an inactive portfolio. Deleting Active Portfolios is prohibited (BR-907).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _portfolioService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Activates a portfolio.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _portfolioService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a portfolio. Inactive portfolios cannot be modified (BR-906).
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _portfolioService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Associates an Active Mission to the portfolio (BR-904 / BR-905).
    /// </summary>
    [HttpPost("{id:guid}/missions")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> AssociateMission(
        Guid id,
        [FromBody] PortfolioMissionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.AssociateMissionAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Removes a Mission from the portfolio. At least one Mission must remain (BR-903).
    /// </summary>
    [HttpDelete("{id:guid}/missions/{missionId:guid}")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> RemoveMission(
        Guid id,
        Guid missionId,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.RemoveMissionAsync(id, missionId, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Assigns or clears an optional Planning Template on the portfolio.
    /// </summary>
    [HttpPost("{id:guid}/planning-template")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> AssignPlanningTemplate(
        Guid id,
        [FromBody] AssignPortfolioPlanningTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.AssignPlanningTemplateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Persists Capacity Engine summary for the portfolio planning period (BR-908 / BR-911).
    /// </summary>
    [HttpPost("{id:guid}/calculate-capacity")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> CalculateCapacity(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.CalculateCapacityAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Persists Workload Engine summary for the portfolio planning period (BR-909 / BR-911).
    /// </summary>
    [HttpPost("{id:guid}/calculate-workload")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> CalculateWorkload(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.CalculateWorkloadAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Persists portfolio health from live and historical capacity/workload (BR-910 / BR-912).
    /// </summary>
    [HttpPost("{id:guid}/calculate-health")]
    [ProducesResponseType(typeof(PortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PortfolioResponse>> CalculateHealth(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _portfolioService.CalculateHealthAsync(id, cancellationToken);
        return Ok(item);
    }
}

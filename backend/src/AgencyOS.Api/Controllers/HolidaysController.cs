using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("holidays")]
[Produces("application/json")]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidaysController(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    /// <summary>
    /// Returns holidays with optional search and filter parameters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HolidayResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HolidayResponse>>> GetAll(
        [FromQuery] HolidayQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var holidays = await _holidayService.GetAllAsync(parameters, cancellationToken);
        return Ok(holidays);
    }

    /// <summary>
    /// Filters holidays by company, type, status, name, location, and date range.
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<HolidayResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HolidayResponse>>> Filter(
        [FromQuery] HolidayQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var holidays = await _holidayService.FilterAsync(parameters, cancellationToken);
        return Ok(holidays);
    }

    /// <summary>
    /// Returns a holiday by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HolidayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HolidayResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var holiday = await _holidayService.GetByIdAsync(id, cancellationToken);
        return Ok(holiday);
    }

    /// <summary>
    /// Creates a new holiday in Inactive status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HolidayResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HolidayResponse>> Create(
        [FromBody] CreateHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var holiday = await _holidayService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = holiday.Id }, holiday);
    }

    /// <summary>
    /// Updates an existing holiday configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HolidayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HolidayResponse>> Update(
        Guid id,
        [FromBody] UpdateHolidayRequest request,
        CancellationToken cancellationToken)
    {
        var holiday = await _holidayService.UpdateAsync(id, request, cancellationToken);
        return Ok(holiday);
    }

    /// <summary>
    /// Activates a holiday so it affects Planning (BR-207).
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _holidayService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a holiday. Historical information is preserved (BR-208).
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _holidayService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an inactive holiday that has not yet occurred.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _holidayService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

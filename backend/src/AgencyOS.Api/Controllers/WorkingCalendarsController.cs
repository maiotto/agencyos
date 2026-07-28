using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("working-calendars")]
[Produces("application/json")]
public class WorkingCalendarsController : ControllerBase
{
    private readonly IWorkingCalendarService _workingCalendarService;

    public WorkingCalendarsController(IWorkingCalendarService workingCalendarService)
    {
        _workingCalendarService = workingCalendarService;
    }

    /// <summary>
    /// Returns all working calendars with optional filters and ordering.
    /// </summary>
    /// <response code="200">List of working calendars.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkingCalendarResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkingCalendarResponse>>> GetAll(
        [FromQuery] WorkingCalendarQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var calendars = await _workingCalendarService.GetAllAsync(parameters, cancellationToken);
        return Ok(calendars);
    }

    /// <summary>
    /// Returns a working calendar by identifier.
    /// </summary>
    /// <response code="200">Working calendar found.</response>
    /// <response code="404">Working calendar not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkingCalendarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkingCalendarResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarService.GetByIdAsync(id, cancellationToken);
        return Ok(calendar);
    }

    /// <summary>
    /// Returns the active working calendar covering a date for a company.
    /// </summary>
    /// <response code="200">Active working calendar found.</response>
    /// <response code="204">No active working calendar covers the date.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(WorkingCalendarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<WorkingCalendarResponse>> GetActiveForCompany(
        [FromQuery] Guid companyId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var asOfDate = date ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var calendar = await _workingCalendarService.GetActiveForCompanyAsync(
            companyId,
            asOfDate,
            cancellationToken);

        if (calendar is null)
        {
            return NoContent();
        }

        return Ok(calendar);
    }

    /// <summary>
    /// Evaluates whether a date is an operational working day for a company,
    /// combining the active Working Calendar with active Holidays (US-102).
    /// </summary>
    [HttpGet("operational-day")]
    [ProducesResponseType(typeof(OperationalWorkingDayResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperationalWorkingDayResponse>> GetOperationalWorkingDay(
        [FromQuery] Guid companyId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var asOfDate = date ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var result = await _workingCalendarService.GetOperationalWorkingDayAsync(
            companyId,
            asOfDate,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns active holidays that fall within a working calendar validity period.
    /// </summary>
    [HttpGet("{id:guid}/holidays")]
    [ProducesResponseType(typeof(IReadOnlyList<HolidayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HolidayResponse>>> GetHolidaysForCalendar(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var holidays = await _workingCalendarService.GetHolidaysForCalendarAsync(
            id,
            from,
            to,
            cancellationToken);
        return Ok(holidays);
    }

    /// <summary>
    /// Returns Working Hours configurations associated with a Working Calendar.
    /// </summary>
    [HttpGet("{id:guid}/working-hours")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkingHoursResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkingHoursResponse>>> GetWorkingHoursForCalendar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var items = await _workingCalendarService.GetWorkingHoursForCalendarAsync(id, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Creates a new working calendar in Inactive status.
    /// </summary>
    /// <response code="201">Working calendar created.</response>
    /// <response code="400">Validation or business rule error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkingCalendarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkingCalendarResponse>> Create(
        [FromBody] CreateWorkingCalendarRequest request,
        CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = calendar.Id }, calendar);
    }

    /// <summary>
    /// Updates an existing working calendar configuration.
    /// </summary>
    /// <response code="200">Working calendar updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Working calendar not found.</response>
    /// <response code="409">Active period overlap conflict.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkingCalendarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkingCalendarResponse>> Update(
        Guid id,
        [FromBody] UpdateWorkingCalendarRequest request,
        CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarService.UpdateAsync(id, request, cancellationToken);
        return Ok(calendar);
    }

    /// <summary>
    /// Activates a working calendar. Only one active calendar may exist for the same period.
    /// </summary>
    /// <response code="204">Working calendar activated.</response>
    /// <response code="404">Working calendar not found.</response>
    /// <response code="409">Active period overlap conflict.</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _workingCalendarService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a working calendar. Historical information is preserved.
    /// </summary>
    /// <response code="204">Working calendar deactivated.</response>
    /// <response code="404">Working calendar not found.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _workingCalendarService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an inactive working calendar that has not yet started.
    /// </summary>
    /// <response code="204">Working calendar deleted.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Working calendar not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _workingCalendarService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

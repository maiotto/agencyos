using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("working-hours")]
[Produces("application/json")]
public class WorkingHoursController : ControllerBase
{
    private readonly IWorkingHoursService _workingHoursService;

    public WorkingHoursController(IWorkingHoursService workingHoursService)
    {
        _workingHoursService = workingHoursService;
    }

    /// <summary>
    /// Returns working hours configurations with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkingHoursResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkingHoursResponse>>> GetAll(
        [FromQuery] WorkingHoursQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _workingHoursService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a working hours configuration by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkingHoursResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _workingHoursService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Creates a new working hours configuration in Inactive status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkingHoursResponse>> Create(
        [FromBody] CreateWorkingHoursRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _workingHoursService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Updates an existing working hours configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkingHoursResponse>> Update(
        Guid id,
        [FromBody] UpdateWorkingHoursRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _workingHoursService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Activates a working hours configuration. Only one active configuration may overlap per calendar.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _workingHoursService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a working hours configuration. Inactive configurations are not used (BR-307).
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _workingHoursService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an inactive working hours configuration that has not yet started.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _workingHoursService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

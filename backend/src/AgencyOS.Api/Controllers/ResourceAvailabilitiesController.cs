using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("resource-availabilities")]
[Produces("application/json")]
public class ResourceAvailabilitiesController : ControllerBase
{
    private readonly IResourceAvailabilityService _resourceAvailabilityService;

    public ResourceAvailabilitiesController(IResourceAvailabilityService resourceAvailabilityService)
    {
        _resourceAvailabilityService = resourceAvailabilityService;
    }

    /// <summary>
    /// Returns resource availability configurations with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResourceAvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceAvailabilityResponse>>> GetAll(
        [FromQuery] ResourceAvailabilityQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _resourceAvailabilityService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns operational availability for a resource on a specific date (calendar + hours + RA).
    /// </summary>
    [HttpGet("operational")]
    [ProducesResponseType(typeof(OperationalResourceAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationalResourceAvailabilityResponse>> GetOperational(
        [FromQuery] Guid executionResourceId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        if (executionResourceId == Guid.Empty)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(executionResourceId)] = ["Execution Resource is mandatory."]
            }));
        }

        var item = await _resourceAvailabilityService.GetOperationalAvailabilityAsync(
            executionResourceId,
            date,
            cancellationToken);

        return Ok(item);
    }

    /// <summary>
    /// Returns a resource availability configuration by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceAvailabilityResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _resourceAvailabilityService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Creates a new resource availability configuration in Inactive status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceAvailabilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceAvailabilityResponse>> Create(
        [FromBody] CreateResourceAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _resourceAvailabilityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Updates an existing resource availability configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResourceAvailabilityResponse>> Update(
        Guid id,
        [FromBody] UpdateResourceAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _resourceAvailabilityService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Activates a resource availability configuration. Only one active configuration may overlap per resource.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _resourceAvailabilityService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a resource availability configuration. Inactive configurations do not participate (BR-410).
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _resourceAvailabilityService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an inactive resource availability configuration that has not yet started.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _resourceAvailabilityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

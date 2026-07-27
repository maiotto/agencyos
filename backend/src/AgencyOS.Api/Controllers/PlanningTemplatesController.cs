using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("planning-templates")]
[Produces("application/json")]
public class PlanningTemplatesController : ControllerBase
{
    private readonly IPlanningTemplateService _planningTemplateService;

    public PlanningTemplatesController(IPlanningTemplateService planningTemplateService)
    {
        _planningTemplateService = planningTemplateService;
    }

    /// <summary>
    /// Returns planning templates with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanningTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningTemplateResponse>>> GetAll(
        [FromQuery] PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _planningTemplateService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Filters planning templates (search/status/company/calendar/hours).
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanningTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningTemplateResponse>>> Filter(
        [FromQuery] PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _planningTemplateService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a planning template by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanningTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningTemplateResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _planningTemplateService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Creates a new planning template in Inactive status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlanningTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanningTemplateResponse>> Create(
        [FromBody] CreatePlanningTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _planningTemplateService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Updates an existing planning template configuration references.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PlanningTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanningTemplateResponse>> Update(
        Guid id,
        [FromBody] UpdatePlanningTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _planningTemplateService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Activates a planning template. Referenced Working Calendar and Working Hours must be Active.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _planningTemplateService.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a planning template. Inactive templates cannot be applied.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _planningTemplateService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an inactive planning template.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _planningTemplateService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Clones configuration into a new Inactive template (no operational history copied).
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(PlanningTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanningTemplateResponse>> Clone(
        Guid id,
        [FromBody] ClonePlanningTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _planningTemplateService.CloneAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Applies an Active template to produce a new planning configuration without mutating the template.
    /// Optionally calculates Capacity and/or Workload for the resolved period.
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(typeof(AppliedPlanningConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppliedPlanningConfigurationResponse>> Apply(
        Guid id,
        [FromBody] ApplyPlanningTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var applied = await _planningTemplateService.ApplyAsync(id, request, cancellationToken);
        return Ok(applied);
    }
}

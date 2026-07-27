using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("assignments")]
[Produces("application/json")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// Returns all assignments with optional filters and ordering.
    /// </summary>
    /// <response code="200">List of assignments.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssignmentResponse>>> GetAll(
        [FromQuery] AssignmentQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var assignments = await _assignmentService.GetAllAsync(parameters, cancellationToken);
        return Ok(assignments);
    }

    /// <summary>
    /// Returns an assignment by identifier.
    /// </summary>
    /// <response code="200">Assignment found.</response>
    /// <response code="404">Assignment not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.GetByIdAsync(id, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// Creates a new assignment linking a task to an execution resource.
    /// </summary>
    /// <response code="201">Assignment created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Task or execution resource not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResponse>> Create(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
    }

    /// <summary>
    /// Updates an existing assignment.
    /// </summary>
    /// <response code="200">Assignment updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Assignment not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResponse>> Update(
        Guid id,
        [FromBody] UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.UpdateAsync(id, request, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// Cancels an assignment. Historical information is preserved.
    /// </summary>
    /// <response code="204">Assignment cancelled.</response>
    /// <response code="404">Assignment not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _assignmentService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancels an assignment. Historical information is preserved.
    /// </summary>
    /// <response code="200">Assignment cancelled.</response>
    /// <response code="404">Assignment not found.</response>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResponse>> CancelAssignment(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.CancelAsync(id, cancellationToken);
        return Ok(assignment);
    }
}

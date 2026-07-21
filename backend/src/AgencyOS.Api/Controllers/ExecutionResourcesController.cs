using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("execution-resources")]
[Produces("application/json")]
public class ExecutionResourcesController : ControllerBase
{
    private readonly IExecutionResourceService _executionResourceService;

    public ExecutionResourcesController(IExecutionResourceService executionResourceService)
    {
        _executionResourceService = executionResourceService;
    }

    /// <summary>
    /// Returns all execution resources with optional filters and ordering.
    /// </summary>
    /// <response code="200">List of execution resources.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExecutionResourceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExecutionResourceResponse>>> GetAll(
        [FromQuery] ExecutionResourceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var resources = await _executionResourceService.GetAllAsync(parameters, cancellationToken);
        return Ok(resources);
    }

    /// <summary>
    /// Returns an execution resource by identifier.
    /// </summary>
    /// <response code="200">Execution resource found.</response>
    /// <response code="404">Execution resource not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExecutionResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionResourceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _executionResourceService.GetByIdAsync(id, cancellationToken);
        return Ok(resource);
    }

    /// <summary>
    /// Creates a new execution resource.
    /// </summary>
    /// <response code="201">Execution resource created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="409">Duplicate resource code.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExecutionResourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExecutionResourceResponse>> Create(
        [FromBody] CreateExecutionResourceRequest request,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
    }

    /// <summary>
    /// Updates an existing execution resource.
    /// </summary>
    /// <response code="200">Execution resource updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Execution resource not found.</response>
    /// <response code="409">Duplicate resource code.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExecutionResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ExecutionResourceResponse>> Update(
        Guid id,
        [FromBody] UpdateExecutionResourceRequest request,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceService.UpdateAsync(id, request, cancellationToken);
        return Ok(resource);
    }

    /// <summary>
    /// Deactivates an execution resource. Historical information is preserved.
    /// </summary>
    /// <response code="204">Execution resource deactivated.</response>
    /// <response code="404">Execution resource not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _executionResourceService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}

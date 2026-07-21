using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Returns all tasks with optional filters.
    /// </summary>
    /// <response code="200">List of tasks.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] TaskQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(parameters, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Returns a task by identifier.
    /// </summary>
    /// <response code="200">Task found.</response>
    /// <response code="404">Task not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Creates a new task for a mission.
    /// </summary>
    /// <response code="201">Task created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Mission, task type or task status not found.</response>
    /// <response code="409">Duplicate task code for the mission.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <response code="200">Task updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Task, task type or task status not found.</response>
    /// <response code="409">Duplicate task code for the mission.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <response code="204">Task deleted.</response>
    /// <response code="404">Task not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Completes a task. Historical information is preserved.
    /// </summary>
    /// <response code="200">Task completed.</response>
    /// <response code="404">Task or completed status not found.</response>
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.CompleteAsync(id, cancellationToken);
        return Ok(task);
    }
}

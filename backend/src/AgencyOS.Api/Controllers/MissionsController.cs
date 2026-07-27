using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("missions")]
[Produces("application/json")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    /// <summary>
    /// Returns all missions.
    /// </summary>
    /// <response code="200">List of missions.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MissionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MissionResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var missions = await _missionService.GetAllAsync(cancellationToken);
        return Ok(missions);
    }

    /// <summary>
    /// Returns a mission by identifier.
    /// </summary>
    /// <response code="200">Mission found.</response>
    /// <response code="404">Mission not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MissionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var mission = await _missionService.GetByIdAsync(id, cancellationToken);
        return Ok(mission);
    }

    /// <summary>
    /// Creates a new mission.
    /// </summary>
    /// <response code="201">Mission created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Contract not found.</response>
    /// <response code="409">Duplicate mission code.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MissionResponse>> Create(
        [FromBody] CreateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var mission = await _missionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = mission.Id }, mission);
    }

    /// <summary>
    /// Updates an existing mission.
    /// </summary>
    /// <response code="200">Mission updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Mission not found.</response>
    /// <response code="409">Duplicate mission code.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MissionResponse>> Update(
        Guid id,
        [FromBody] UpdateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var mission = await _missionService.UpdateAsync(id, request, cancellationToken);
        return Ok(mission);
    }

    /// <summary>
    /// Deletes a mission.
    /// </summary>
    /// <response code="204">Mission deleted.</response>
    /// <response code="404">Mission not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _missionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

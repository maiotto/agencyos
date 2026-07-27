using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Company Decision Profile endpoints (US-401 / BR-1901..BR-1910).
/// </summary>
[ApiController]
[Route("decision-profiles")]
[Produces("application/json")]
public class DecisionProfilesController : ControllerBase
{
    private readonly ICompanyDecisionProfileService _service;

    public DecisionProfilesController(ICompanyDecisionProfileService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns Company Decision Profiles (latest version per profile family) with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDecisionProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyDecisionProfileResponse>>> GetAll(
        [FromQuery] CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var profiles = await _service.GetAllAsync(parameters, cancellationToken);
        return Ok(profiles);
    }

    /// <summary>
    /// Filters Company Decision Profiles by company, status, name, code, and default flag.
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDecisionProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyDecisionProfileResponse>>> Filter(
        [FromQuery] CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var profiles = await _service.FilterAsync(parameters, cancellationToken);
        return Ok(profiles);
    }

    /// <summary>
    /// Returns a Company Decision Profile by identifier (specific version).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Returns the latest version of every Company Decision Profile for a company.
    /// </summary>
    [HttpGet("company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDecisionProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyDecisionProfileResponse>>> GetByCompanyId(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var profiles = await _service.GetByCompanyIdAsync(companyId, cancellationToken);
        return Ok(profiles);
    }

    /// <summary>
    /// Returns the default Active Company Decision Profile for a company (BR-1901).
    /// </summary>
    [HttpGet("company/{companyId:guid}/default")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> GetDefaultActive(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var profile = await _service.GetDefaultActiveAsync(companyId, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Creates a new Company Decision Profile (version 1, Active).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> Create(
        [FromBody] CreateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
    }

    /// <summary>
    /// Creates a new immutable version of the profile (BR-1905) and deactivates the previous version.
    /// Returns the newly created version, including its new Id.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> Update(
        Guid id,
        [FromBody] UpdateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Clones a profile into a new lineage with a new Name and Code (version 1, not Default).
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> Clone(
        Guid id,
        [FromBody] CloneCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken)
    {
        var clone = await _service.CloneAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = clone.Id }, clone);
    }

    /// <summary>
    /// Activates a Company Decision Profile so it becomes eligible for ranking (BR-1904).
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _service.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a Company Decision Profile. The current default profile cannot be deactivated.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Archives a Company Decision Profile. The current default profile cannot be archived.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await _service.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets this profile as the company's default Active profile (BR-1901).
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> SetDefault(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await _service.SetDefaultAsync(id, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Clears the default flag from this profile without assigning a replacement.
    /// </summary>
    [HttpPost("{id:guid}/clear-default")]
    [ProducesResponseType(typeof(CompanyDecisionProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDecisionProfileResponse>> ClearDefault(
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await _service.ClearDefaultAsync(id, cancellationToken);
        return Ok(profile);
    }
}

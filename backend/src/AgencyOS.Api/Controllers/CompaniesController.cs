using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Company (multi-company tenant configuration) endpoints (US-402 / BR-2001..BR-2010).
/// </summary>
[ApiController]
[Route("companies")]
[Produces("application/json")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;
    private readonly ICompanyContextService _contextService;

    public CompaniesController(ICompanyService service, ICompanyContextService contextService)
    {
        _service = service;
        _contextService = contextService;
    }

    /// <summary>
    /// Returns Companies with optional status/search filters. Archived Companies are excluded
    /// unless IncludeArchived is set (BR-2009).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> GetAll(
        [FromQuery] CompanyQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var companies = await _service.GetAllAsync(parameters, cancellationToken);
        return Ok(companies);
    }

    /// <summary>
    /// Returns the active Company for the current request context, or the seeded default Company.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetActive(CancellationToken cancellationToken)
    {
        var company = await _contextService.GetActiveAsync(cancellationToken);
        return Ok(company);
    }

    /// <summary>
    /// Returns a Company by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var company = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(company);
    }

    /// <summary>
    /// Creates a new Company (Active status).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyResponse>> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
    }

    /// <summary>
    /// Updates an existing Company's configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyResponse>> Update(
        Guid id,
        [FromBody] UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(company);
    }

    /// <summary>
    /// Activates a Company so it becomes eligible for selection (BR-2003).
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
    /// Deactivates a Company. Historical data is preserved.
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
    /// Archives a Company. Archived Companies cannot be modified or selected (BR-2003/BR-2009).
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
    /// Selects a Company as the active company for the current request context (BR-2003).
    /// </summary>
    [HttpPost("select")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Select(
        [FromBody] SelectCompanyRequest request,
        CancellationToken cancellationToken)
    {
        await _contextService.SelectAsync(request.CompanyId, cancellationToken);
        return NoContent();
    }
}

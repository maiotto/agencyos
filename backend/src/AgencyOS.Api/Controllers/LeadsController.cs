using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Lead Management API for the Commercial Domain.
/// </summary>
[ApiController]
[Route("leads")]
[Produces("application/json")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    /// <summary>
    /// Returns a paginated list of leads with optional filters.
    /// </summary>
    /// <remarks>
    /// Archived leads are excluded by default. Set includeArchived=true or filter status=Archived to retrieve archived leads.
    /// </remarks>
    /// <response code="200">Paginated list of leads.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LeadResponse>>> GetPaged(
        [FromQuery] LeadQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var leads = await _leadService.GetPagedAsync(parameters, cancellationToken);
        return Ok(leads);
    }

    /// <summary>
    /// Returns a lead by identifier.
    /// </summary>
    /// <remarks>
    /// Returns the lead regardless of archived or converted status.
    /// </remarks>
    /// <response code="200">Lead found.</response>
    /// <response code="404">Lead not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var lead = await _leadService.GetByIdAsync(id, cancellationToken);
        return Ok(lead);
    }

    /// <summary>
    /// Creates a new lead.
    /// </summary>
    /// <remarks>
    /// Initial status must be Prospect. Optional email and phone create a primary contact association.
    /// </remarks>
    /// <response code="201">Lead created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="409">Duplicate email among active leads.</response>
    [HttpPost]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadResponse>> Create(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, lead);
    }

    /// <summary>
    /// Updates an existing lead.
    /// </summary>
    /// <remarks>
    /// Converted, archived, and lost leads are read-only. Status updates must follow the approved lifecycle.
    /// </remarks>
    /// <response code="200">Lead updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Lead not found.</response>
    /// <response code="409">Duplicate email among active leads.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadResponse>> Update(
        Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var lead = await _leadService.UpdateAsync(id, request, cancellationToken);
        return Ok(lead);
    }

    /// <summary>
    /// Archives a lead. Historical information is preserved.
    /// </summary>
    /// <remarks>
    /// Converted leads cannot be archived. Already archived leads are treated as idempotent success.
    /// </remarks>
    /// <response code="204">Lead archived.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Lead not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await _leadService.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Converts a lead into a client.
    /// </summary>
    /// <remarks>
    /// Only leads in status Won can be converted. Requires legal name and tax identifier. Returns HTTP 200.
    /// </remarks>
    /// <response code="200">Lead converted.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Lead not found.</response>
    [HttpPost("{id:guid}/convert")]
    [ProducesResponseType(typeof(ConvertLeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConvertLeadResponse>> Convert(
        Guid id,
        [FromBody] ConvertLeadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leadService.ConvertAsync(id, request, cancellationToken);
        return Ok(result);
    }
}

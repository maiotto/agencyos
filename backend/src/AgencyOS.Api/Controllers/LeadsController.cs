using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

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
    /// <response code="204">Lead archived.</response>
    /// <response code="404">Lead not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await _leadService.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Converts a lead into a client.
    /// </summary>
    /// <response code="200">Lead converted.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Lead not found.</response>
    [HttpPost("{id:guid}/convert")]
    [ProducesResponseType(typeof(ConvertLeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConvertLeadResponse>> Convert(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leadService.ConvertAsync(id, cancellationToken);
        return Ok(result);
    }
}

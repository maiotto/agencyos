using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("contacts")]
[Produces("application/json")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>
    /// Returns a paginated list of client contacts with optional filters and ordering.
    /// </summary>
    /// <response code="200">Paginated list of contacts.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ContactResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ContactResponse>>> GetPaged(
        [FromQuery] ContactQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var contacts = await _contactService.GetPagedAsync(parameters, cancellationToken);
        return Ok(contacts);
    }

    /// <summary>
    /// Returns a client contact by identifier.
    /// </summary>
    /// <response code="200">Contact found.</response>
    /// <response code="404">Contact not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _contactService.GetByIdAsync(id, cancellationToken);
        return Ok(contact);
    }

    /// <summary>
    /// Creates a new client contact.
    /// </summary>
    /// <response code="201">Contact created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="409">Duplicate email for client.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactResponse>> Create(
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await _contactService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    /// <summary>
    /// Updates an existing client contact.
    /// </summary>
    /// <response code="200">Contact updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Contact not found.</response>
    /// <response code="409">Duplicate email for client.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactResponse>> Update(
        Guid id,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await _contactService.UpdateAsync(id, request, cancellationToken);
        return Ok(contact);
    }

    /// <summary>
    /// Deactivates a client contact. Historical information is preserved.
    /// </summary>
    /// <response code="204">Contact deactivated.</response>
    /// <response code="404">Contact not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _contactService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets a client contact as the primary contact for its client.
    /// </summary>
    /// <response code="200">Primary contact updated.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Contact not found.</response>
    [HttpPatch("{id:guid}/primary")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> SetPrimary(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _contactService.SetPrimaryAsync(id, cancellationToken);
        return Ok(contact);
    }
}

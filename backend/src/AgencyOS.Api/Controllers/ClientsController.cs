using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("clients")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Returns a paginated list of clients with optional filters and ordering.
    /// </summary>
    /// <response code="200">Paginated list of clients.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ClientResponse>>> GetPaged(
        [FromQuery] ClientQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetPagedAsync(parameters, cancellationToken);
        return Ok(clients);
    }

    /// <summary>
    /// Returns a client by identifier.
    /// </summary>
    /// <response code="200">Client found.</response>
    /// <response code="404">Client not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetByIdAsync(id, cancellationToken);
        return Ok(client);
    }

    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <response code="201">Client created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="409">Duplicate tax identifier.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <response code="200">Client updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="409">Duplicate tax identifier.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Update(
        Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.UpdateAsync(id, request, cancellationToken);
        return Ok(client);
    }

    /// <summary>
    /// Deactivates a client. Historical information is preserved.
    /// </summary>
    /// <response code="204">Client deactivated.</response>
    /// <response code="404">Client not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _clientService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}

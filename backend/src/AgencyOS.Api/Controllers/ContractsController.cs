using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("contracts")]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IClientContractService _contractService;

    public ContractsController(IClientContractService contractService)
    {
        _contractService = contractService;
    }

    /// <summary>
    /// Returns a paginated list of contracts with optional filters and ordering.
    /// </summary>
    /// <response code="200">Paginated list of contracts.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ContractResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ContractResponse>>> GetPaged(
        [FromQuery] ContractQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var contracts = await _contractService.GetPagedAsync(parameters, cancellationToken);
        return Ok(contracts);
    }

    /// <summary>
    /// Returns a contract by identifier.
    /// </summary>
    /// <response code="200">Contract found.</response>
    /// <response code="404">Contract not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        return Ok(contract);
    }

    /// <summary>
    /// Creates a new contract for an active client.
    /// </summary>
    /// <response code="201">Contract created.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="409">Duplicate contract code.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractResponse>> Create(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    /// <summary>
    /// Updates an existing contract.
    /// </summary>
    /// <response code="200">Contract updated.</response>
    /// <response code="400">Validation or business rule error.</response>
    /// <response code="404">Contract not found.</response>
    /// <response code="409">Duplicate contract code.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContractResponse>> Update(
        Guid id,
        [FromBody] UpdateContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await _contractService.UpdateAsync(id, request, cancellationToken);
        return Ok(contract);
    }

    /// <summary>
    /// Cancels a contract. Historical information is preserved.
    /// </summary>
    /// <response code="204">Contract cancelled.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Contract not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _contractService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Activates a draft contract.
    /// </summary>
    /// <response code="200">Contract activated.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Contract not found.</response>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.ActivateAsync(id, cancellationToken);
        return Ok(contract);
    }

    /// <summary>
    /// Closes a contract. Historical information is preserved.
    /// </summary>
    /// <response code="200">Contract closed.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Contract not found.</response>
    [HttpPatch("{id:guid}/close")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> Close(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.CloseAsync(id, cancellationToken);
        return Ok(contract);
    }

    /// <summary>
    /// Cancels a contract. Historical information is preserved.
    /// </summary>
    /// <response code="200">Contract cancelled.</response>
    /// <response code="400">Business rule error.</response>
    /// <response code="404">Contract not found.</response>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> CancelContract(Guid id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.CancelAsync(id, cancellationToken);
        return Ok(contract);
    }
}

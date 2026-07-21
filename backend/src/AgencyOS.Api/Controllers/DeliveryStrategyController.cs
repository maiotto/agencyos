using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("delivery-strategies")]
[Produces("application/json")]
public class DeliveryStrategyController : ControllerBase
{
    private readonly IDeliveryStrategyBuilderService _deliveryStrategyBuilderService;

    public DeliveryStrategyController(IDeliveryStrategyBuilderService deliveryStrategyBuilderService)
    {
        _deliveryStrategyBuilderService = deliveryStrategyBuilderService;
    }

    /// <summary>
    /// Generates candidate delivery strategies for a contract and mission.
    /// </summary>
    /// <response code="200">Candidate delivery strategies generated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Contract or mission not found.</response>
    /// <response code="409">Business rule violation.</response>
    [HttpPost("build")]
    [ProducesResponseType(typeof(BuildDeliveryStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BuildDeliveryStrategyResponse>> Build(
        [FromBody] BuildDeliveryStrategyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _deliveryStrategyBuilderService.BuildAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Generates candidate delivery strategies for a contract using mission and planning period parameters.
    /// </summary>
    /// <response code="200">Candidate delivery strategies generated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Contract or mission not found.</response>
    /// <response code="409">Business rule violation.</response>
    [HttpGet("{contractId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<DeliveryStrategyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<DeliveryStrategyResponse>>> GetByContractId(
        Guid contractId,
        [FromQuery] DeliveryStrategyQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var strategies = await _deliveryStrategyBuilderService.GetByContractIdAsync(
            contractId,
            parameters,
            cancellationToken);

        return Ok(strategies);
    }
}

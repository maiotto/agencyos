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
    private readonly IDeliveryStrategyEvaluatorService _deliveryStrategyEvaluatorService;
    private readonly IDeliveryStrategyRankingService _deliveryStrategyRankingService;
    private readonly IDeliveryStrategyExplanationService _deliveryStrategyExplanationService;

    public DeliveryStrategyController(
        IDeliveryStrategyBuilderService deliveryStrategyBuilderService,
        IDeliveryStrategyEvaluatorService deliveryStrategyEvaluatorService,
        IDeliveryStrategyRankingService deliveryStrategyRankingService,
        IDeliveryStrategyExplanationService deliveryStrategyExplanationService)
    {
        _deliveryStrategyBuilderService = deliveryStrategyBuilderService;
        _deliveryStrategyEvaluatorService = deliveryStrategyEvaluatorService;
        _deliveryStrategyRankingService = deliveryStrategyRankingService;
        _deliveryStrategyExplanationService = deliveryStrategyExplanationService;
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

    /// <summary>
    /// Evaluates generated delivery strategies using operational metrics.
    /// </summary>
    /// <response code="200">Delivery strategies evaluated with operational metrics.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Contract or mission not found.</response>
    /// <response code="409">Business rule violation.</response>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(EvaluateDeliveryStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EvaluateDeliveryStrategyResponse>> Evaluate(
        [FromBody] EvaluateDeliveryStrategyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _deliveryStrategyEvaluatorService.EvaluateAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Ranks evaluated delivery strategies using a configurable company decision profile.
    /// </summary>
    /// <response code="200">Delivery strategies ranked from best to worst.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Contract, mission, or decision profile not found.</response>
    /// <response code="409">Business rule violation.</response>
    [HttpPost("rank")]
    [ProducesResponseType(typeof(RankDeliveryStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RankDeliveryStrategyResponse>> Rank(
        [FromBody] RankDeliveryStrategyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _deliveryStrategyRankingService.RankAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Explains why a ranked delivery strategy received its ranking position and score.
    /// </summary>
    /// <response code="200">Structured explanation generated for the ranked strategy.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Strategy, contract, mission, or decision profile not found.</response>
    /// <response code="409">Business rule violation.</response>
    [HttpGet("{strategyId:guid}/explanation")]
    [ProducesResponseType(typeof(DeliveryStrategyExplanationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeliveryStrategyExplanationResponse>> GetExplanation(
        Guid strategyId,
        [FromQuery] DeliveryStrategyExplanationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var response = await _deliveryStrategyExplanationService.GetExplanationAsync(
            strategyId,
            parameters,
            cancellationToken);

        return Ok(response);
    }
}

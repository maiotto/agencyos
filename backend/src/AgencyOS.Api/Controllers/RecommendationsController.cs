using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

[ApiController]
[Route("recommendations")]
[Produces("application/json")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// Lists persisted recommendations with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetAll(
        [FromQuery] RecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Filters persisted recommendations (search/status/company/mission/contract/version).
    /// </summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> Filter(
        [FromQuery] RecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns recommendations for a company.
    /// </summary>
    [HttpGet("company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetByCompanyId(
        Guid companyId,
        [FromQuery] RecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.GetByCompanyIdAsync(companyId, parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns recommendations for a mission.
    /// </summary>
    [HttpGet("mission/{missionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetByMissionId(
        Guid missionId,
        [FromQuery] RecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.GetByMissionIdAsync(missionId, parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns recommendations for a contract.
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetByContractId(
        Guid contractId,
        [FromQuery] RecommendationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.GetByContractIdAsync(contractId, parameters, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Returns a recommendation by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Returns all versions for the recommendation number lineage.
    /// </summary>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(typeof(IReadOnlyList<RecommendationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecommendationResponse>>> GetVersions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var items = await _recommendationService.GetVersionsAsync(id, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Persists a recommendation snapshot. Does not generate Decision Engine strategies.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecommendationResponse>> Create(
        [FromBody] CreateRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Creates a new immutable version of an existing recommendation (BR-1106).
    /// </summary>
    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationResponse>> CreateNewVersion(
        Guid id,
        [FromBody] CreateRecommendationVersionRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationService.CreateNewVersionAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>
    /// Archives a recommendation (BR-1107). Archived recommendations remain queryable.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationService.ArchiveAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Restores an archived recommendation.
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecommendationResponse>> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _recommendationService.RestoreAsync(id, cancellationToken);
        return Ok(item);
    }
}

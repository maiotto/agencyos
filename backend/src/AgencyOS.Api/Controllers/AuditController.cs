using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Read-only Decision Audit Trail APIs (US-206 / BR-1501..BR-1510).
/// </summary>
[ApiController]
[Route("audit")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly IAuditQueryService _auditQueryService;

    public AuditController(IAuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> GetAll(
        [FromQuery] AuditEventQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> Filter(
        [FromQuery] AuditEventQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditEventResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _auditQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("entity/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> GetByEntity(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.GetByEntityIdAsync(entityId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("correlation/{correlationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> GetByCorrelation(
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.GetByCorrelationIdAsync(correlationId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> GetByUser(
        string userId,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("company/{companyId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> GetByCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var items = await _auditQueryService.GetByCompanyIdAsync(companyId, cancellationToken);
        return Ok(items);
    }
}

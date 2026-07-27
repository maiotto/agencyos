using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgencyOS.Api.Controllers;

/// <summary>
/// Notification Center APIs (US-506 / BR-2901..BR-2910).
/// Informational only. Never modifies business data.
/// </summary>
[ApiController]
[Route("notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _queryService;
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationQueryService queryService,
        INotificationService notificationService)
    {
        _queryService = queryService;
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetAll(
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _queryService.GetAllAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("filter")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> Filter(
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await _queryService.FilterAsync(parameters, cancellationToken);
        return Ok(items);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(NotificationUnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationUnreadCountResponse>> GetUnreadCount(
        [FromQuery] NotificationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetUnreadCountAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _queryService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _notificationService.MarkReadAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost("{id:guid}/unread")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> MarkUnread(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _notificationService.MarkUnreadAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _notificationService.ArchiveAsync(id, cancellationToken);
        return Ok(item);
    }
}

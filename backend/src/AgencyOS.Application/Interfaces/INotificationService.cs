using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> QueryAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<Notification> AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    Task<Notification> UpdateAsync(
        Notification notification,
        CancellationToken cancellationToken = default);
}

public interface INotificationQueryService
{
    Task<IReadOnlyList<NotificationResponse>> GetAllAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationResponse>> FilterAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<NotificationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<NotificationUnreadCountResponse> GetUnreadCountAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<NotificationResponse> MarkReadAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<NotificationResponse> MarkUnreadAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<NotificationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public interface INotificationGenerationService
{
    /// <summary>
    /// Generates a notification without throwing. Failures are logged only (BR-2909).
    /// </summary>
    Task GenerateSafeAsync(
        NotificationGenerationRequest request,
        CancellationToken cancellationToken = default);
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Audit;

/// <summary>
/// No-op notification generator for unit tests (BR-2909 safe path).
/// </summary>
public sealed class NoOpNotificationGenerationService : INotificationGenerationService
{
    public Task GenerateSafeAsync(
        NotificationGenerationRequest request,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

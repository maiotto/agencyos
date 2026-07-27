using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Builds the caller's Activity Timeline from existing Audit Events (US-501 / BR-2405).
/// Read-only: never writes Audit Events, never modifies history.
/// </summary>
public interface IPersonalTimelineService
{
    Task<IReadOnlyList<MyWorkActivityItemResponse>> GetTimelineAsync(
        string userId,
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}

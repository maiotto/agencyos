using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Audit;

/// <summary>
/// No-op audit recorder for tests and optional disabled scenarios.
/// </summary>
public sealed class NoOpAuditService : IAuditService
{
    public Task RecordSafeAsync(
        AuditEventWriteRequest request,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

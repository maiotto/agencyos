using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Audit;

/// <summary>
/// Default audit context when no HTTP request is present (tests / background).
/// </summary>
public sealed class NullAuditContext : IAuditContext
{
    public Guid? CorrelationId => null;

    public string? SessionId => null;

    public string? RequestId => null;

    public string? UserId => null;

    public string? UserName => null;
}

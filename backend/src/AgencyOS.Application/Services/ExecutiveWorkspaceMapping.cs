using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Services;

/// <summary>
/// Shared, pure mapping from <see cref="ExecutiveWorkspaceQueryParameters"/> onto the existing
/// <see cref="EnterpriseDashboardQueryParameters"/> (DEC-505-001). Centralizes the projection so
/// every Executive Workspace service maps the query window identically.
/// </summary>
internal static class ExecutiveWorkspaceMapping
{
    public static EnterpriseDashboardQueryParameters ToDashboardParameters(
        Guid companyId,
        ExecutiveWorkspaceQueryParameters parameters) =>
        new()
        {
            CompanyId = companyId,
            From = parameters.From,
            To = parameters.To,
            PeriodStart = parameters.PeriodStart,
            PeriodEnd = parameters.PeriodEnd
        };
}

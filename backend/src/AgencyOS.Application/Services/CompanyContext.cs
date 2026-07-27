using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Scoped mutable implementation of <see cref="ICompanyContext"/> (US-402). One instance per request/scope.
/// </summary>
public class CompanyContext : ICompanyContext
{
    public Guid? CompanyId { get; set; }

    public string? CompanyCode { get; set; }

    public string? CompanyName { get; set; }

    public bool IsSelected { get; set; }
}

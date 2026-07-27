namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Mutable per-request active Company context (US-402). Populated by <see cref="ICompanyContextService"/>
/// from the resolved `X-Company-Id` header. Services may fall back to the default Company when unset.
/// </summary>
public interface ICompanyContext
{
    Guid? CompanyId { get; set; }

    string? CompanyCode { get; set; }

    string? CompanyName { get; set; }

    bool IsSelected { get; set; }
}

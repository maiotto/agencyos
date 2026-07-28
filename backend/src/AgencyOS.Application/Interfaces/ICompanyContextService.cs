using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Resolves and mutates the active <see cref="ICompanyContext"/> for the current scope (US-402 / BR-2003).
/// </summary>
public interface ICompanyContextService
{
    /// <summary>Loads the Company, validates it may be selected (BR-2003), and populates the context.</summary>
    Task SelectAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Validates and binds the Company into the context without generating a notification.</summary>
    Task BindContextAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Returns the currently selected Company, or the seeded default Company when none is selected.</summary>
    Task<CompanyResponse> GetActiveAsync(CancellationToken cancellationToken = default);

    void Clear();
}

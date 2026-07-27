using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Persistence contract for the Company aggregate (US-402 / BR-2001..BR-2010).
/// </summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Company>> GetAllAsync(
        CompanyQueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>Whether a Company with the same CompanyCode (case-insensitive) already exists (BR-2002).</summary>
    Task<bool> ExistsCodeAsync(
        string companyCode,
        Guid? excludeCompanyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Whether a Company with the same CompanyName (case-insensitive) already exists (BR-2001).</summary>
    Task<bool> ExistsNameAsync(
        string companyName,
        Guid? excludeCompanyId = null,
        CancellationToken cancellationToken = default);

    Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default);

    Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default);
}

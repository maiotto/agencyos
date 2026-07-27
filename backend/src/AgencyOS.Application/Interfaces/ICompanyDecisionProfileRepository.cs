using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ICompanyDecisionProfileRepository
{
    Task<CompanyDecisionProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyDecisionProfile>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Latest version per profile family, filtered/ordered per parameters (US-401).</summary>
    Task<IReadOnlyList<CompanyDecisionProfile>> QueryAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>Latest version per profile family for a given company.</summary>
    Task<IReadOnlyList<CompanyDecisionProfile>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>The single Active + DefaultProfile record for a company (BR-1901), if any.</summary>
    Task<CompanyDecisionProfile?> GetDefaultActiveAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>The highest-version record for a profile lineage (BR-1905).</summary>
    Task<CompanyDecisionProfile?> GetLatestByFamilyAsync(
        Guid profileFamilyId,
        CancellationToken cancellationToken = default);

    /// <summary>Whether an Active profile with the same Name (case-insensitive) exists among latest versions (BR-1902).</summary>
    Task<bool> ExistsActiveNameAsync(
        Guid companyId,
        string name,
        Guid? excludeProfileFamilyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Whether another profile lineage already uses the same Code within the company.</summary>
    Task<bool> ExistsCodeAsync(
        Guid companyId,
        string code,
        Guid? excludeProfileFamilyId = null,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfile> AddAsync(
        CompanyDecisionProfile profile,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfile> UpdateAsync(
        CompanyDecisionProfile profile,
        CancellationToken cancellationToken = default);
}

using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Company Decision Profile CRUD, versioning, and lifecycle operations (US-401 / BR-1901..BR-1910).
/// </summary>
public interface ICompanyDecisionProfileService
{
    Task<IReadOnlyList<CompanyDecisionProfileResponse>> GetAllAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyDecisionProfileResponse>> FilterAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyDecisionProfileResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> GetDefaultActiveAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> CreateAsync(
        CreateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new immutable version (BR-1905) and deactivates the previous version.</summary>
    Task<CompanyDecisionProfileResponse> UpdateAsync(
        Guid id,
        UpdateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> CloneAsync(
        Guid id,
        CloneCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> SetDefaultAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CompanyDecisionProfileResponse> ClearDefaultAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

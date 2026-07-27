using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Company CRUD and lifecycle operations (US-402 / BR-2001..BR-2010).
/// </summary>
public interface ICompanyService
{
    Task<IReadOnlyList<CompanyResponse>> GetAllAsync(
        CompanyQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CompanyResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CompanyResponse> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default);

    Task<CompanyResponse> UpdateAsync(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}

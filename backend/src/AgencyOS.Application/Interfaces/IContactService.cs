using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IContactService
{
    Task<PagedResponse<ContactResponse>> GetPagedAsync(
        ContactQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ContactResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContactResponse> CreateAsync(
        CreateContactRequest request,
        CancellationToken cancellationToken = default);

    Task<ContactResponse> UpdateAsync(
        Guid id,
        UpdateContactRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContactResponse> SetPrimaryAsync(Guid id, CancellationToken cancellationToken = default);
}

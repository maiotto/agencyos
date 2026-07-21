using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IContactRepository
{
    Task<(IReadOnlyList<(Contact Contact, Guid ClientId)> Items, int TotalCount)> GetPagedAsync(
        ContactQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<(Contact Contact, Guid ClientId)?> GetClientContactByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsWithEmailForClientAsync(
        string email,
        Guid clientId,
        Guid? excludeContactId = null,
        CancellationToken cancellationToken = default);

    Task<Contact> AddAsync(
        Contact contact,
        ClientContact clientContact,
        CancellationToken cancellationToken = default);

    Task<Contact> UpdateAsync(Contact contact, CancellationToken cancellationToken = default);

    Task ClearPrimaryForClientAsync(
        Guid clientId,
        Guid? excludeContactId = null,
        CancellationToken cancellationToken = default);
}

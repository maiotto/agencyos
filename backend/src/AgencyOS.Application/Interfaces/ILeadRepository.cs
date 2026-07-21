using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ILeadRepository
{
    Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetPagedAsync(
        LeadQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid?> GetClientIdByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, Guid>> GetClientIdsByLeadIdsAsync(
        IReadOnlyCollection<Guid> leadIds,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveLeadWithEmailAsync(
        string email,
        Guid? excludeLeadId = null,
        CancellationToken cancellationToken = default);

    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);

    Task<Lead> UpdateAsync(Lead lead, CancellationToken cancellationToken = default);

    Task<Client> ConvertLeadAsync(
        Lead lead,
        Client client,
        CancellationToken cancellationToken = default);
}

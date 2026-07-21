using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface ILeadService
{
    Task<PagedResponse<LeadResponse>> GetPagedAsync(
        LeadQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<LeadResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LeadResponse> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken = default);

    Task<LeadResponse> UpdateAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken = default);

    Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ConvertLeadResponse> ConvertAsync(Guid id, CancellationToken cancellationToken = default);
}

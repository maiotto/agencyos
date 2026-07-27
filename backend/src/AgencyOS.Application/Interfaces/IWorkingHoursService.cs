using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IWorkingHoursService
{
    Task<IReadOnlyList<WorkingHoursResponse>> GetAllAsync(
        WorkingHoursQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkingHoursResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkingHoursResponse> CreateAsync(
        CreateWorkingHoursRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkingHoursResponse> UpdateAsync(
        Guid id,
        UpdateWorkingHoursRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

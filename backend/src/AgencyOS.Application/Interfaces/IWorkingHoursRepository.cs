using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IWorkingHoursRepository
{
    Task<IReadOnlyList<WorkingHours>> GetAllAsync(
        WorkingHoursQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkingHours?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHours>> GetActiveOverlappingAsync(
        Guid workingCalendarId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeWorkingHoursId = null,
        CancellationToken cancellationToken = default);

    Task<WorkingHours?> GetActiveCoveringDateAsync(
        Guid workingCalendarId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<WorkingHours> AddAsync(WorkingHours workingHours, CancellationToken cancellationToken = default);

    Task<WorkingHours> UpdateAsync(WorkingHours workingHours, CancellationToken cancellationToken = default);

    Task DeleteAsync(WorkingHours workingHours, CancellationToken cancellationToken = default);
}

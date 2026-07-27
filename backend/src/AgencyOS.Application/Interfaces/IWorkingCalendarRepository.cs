using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IWorkingCalendarRepository
{
    Task<IReadOnlyList<WorkingCalendar>> GetAllAsync(
        WorkingCalendarQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingCalendar>> GetActiveOverlappingAsync(
        Guid companyId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeCalendarId = null,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendar?> GetActiveCoveringDateAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendar> AddAsync(WorkingCalendar calendar, CancellationToken cancellationToken = default);

    Task<WorkingCalendar> UpdateAsync(WorkingCalendar calendar, CancellationToken cancellationToken = default);

    Task DeleteAsync(WorkingCalendar calendar, CancellationToken cancellationToken = default);
}

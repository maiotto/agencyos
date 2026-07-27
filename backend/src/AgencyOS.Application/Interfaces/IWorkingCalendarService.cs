using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IWorkingCalendarService
{
    Task<IReadOnlyList<WorkingCalendarResponse>> GetAllAsync(
        WorkingCalendarQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendarResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkingCalendarResponse?> GetActiveForCompanyAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendarResponse> CreateAsync(
        CreateWorkingCalendarRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkingCalendarResponse> UpdateAsync(
        Guid id,
        UpdateWorkingCalendarRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates whether a date is an operational working day using the active Working Calendar
    /// and active Holidays (US-102 integration). Does not change Capacity Engine behavior.
    /// </summary>
    Task<OperationalWorkingDayResponse> GetOperationalWorkingDayAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HolidayResponse>> GetHolidaysForCalendarAsync(
        Guid calendarId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHoursResponse>> GetWorkingHoursForCalendarAsync(
        Guid calendarId,
        CancellationToken cancellationToken = default);
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class WorkingCalendarService : IWorkingCalendarService
{
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IWorkingHoursRepository _workingHoursRepository;
    private readonly ILogger<WorkingCalendarService> _logger;

    public WorkingCalendarService(
        IWorkingCalendarRepository workingCalendarRepository,
        IHolidayRepository holidayRepository,
        IWorkingHoursRepository workingHoursRepository,
        ILogger<WorkingCalendarService> logger)
    {
        _workingCalendarRepository = workingCalendarRepository;
        _holidayRepository = holidayRepository;
        _workingHoursRepository = workingHoursRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkingCalendarResponse>> GetAllAsync(
        WorkingCalendarQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var calendars = await _workingCalendarRepository.GetAllAsync(parameters, cancellationToken);
        return calendars.Select(MapToResponse).ToList();
    }

    public async Task<WorkingCalendarResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(id, cancellationToken);
        return MapToResponse(calendar);
    }

    public async Task<WorkingCalendarResponse?> GetActiveForCompanyAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var calendar = await _workingCalendarRepository.GetActiveCoveringDateAsync(
            companyId,
            date,
            cancellationToken);

        return calendar is null ? null : MapToResponse(calendar);
    }

    public async Task<WorkingCalendarResponse> CreateAsync(
        CreateWorkingCalendarRequest request,
        CancellationToken cancellationToken = default)
    {
        WorkingCalendar calendar;

        try
        {
            calendar = WorkingCalendar.Create(
                request.CompanyId,
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.WorkingDays,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _workingCalendarRepository.AddAsync(calendar, cancellationToken);

        _logger.LogInformation(
            "Working Calendar Created: {CalendarId} ({CalendarName}) for Company {CompanyId}",
            created.Id,
            created.Name,
            created.CompanyId);

        return MapToResponse(created);
    }

    public async Task<WorkingCalendarResponse> UpdateAsync(
        Guid id,
        UpdateWorkingCalendarRequest request,
        CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        try
        {
            calendar.Reconfigure(
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.WorkingDays,
                today,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (calendar.IsActive)
        {
            await EnsureNoActiveOverlapAsync(
                calendar.CompanyId,
                calendar.EffectiveFrom,
                calendar.EffectiveTo,
                calendar.Id,
                cancellationToken);
        }

        var updated = await _workingCalendarRepository.UpdateAsync(calendar, cancellationToken);

        _logger.LogInformation(
            "Working Calendar Updated: {CalendarId} ({CalendarName})",
            updated.Id,
            updated.Name);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(id, cancellationToken);

        if (calendar.IsActive)
        {
            return;
        }

        await EnsureNoActiveOverlapAsync(
            calendar.CompanyId,
            calendar.EffectiveFrom,
            calendar.EffectiveTo,
            calendar.Id,
            cancellationToken);

        calendar.Activate(DateTimeOffset.UtcNow);
        await _workingCalendarRepository.UpdateAsync(calendar, cancellationToken);

        _logger.LogInformation(
            "Working Calendar Activated: {CalendarId} ({CalendarName})",
            calendar.Id,
            calendar.Name);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(id, cancellationToken);

        if (calendar.IsInactive)
        {
            return;
        }

        calendar.Deactivate(DateTimeOffset.UtcNow);
        await _workingCalendarRepository.UpdateAsync(calendar, cancellationToken);

        _logger.LogInformation(
            "Working Calendar Deactivated: {CalendarId} ({CalendarName})",
            calendar.Id,
            calendar.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        try
        {
            calendar.EnsureCanDelete(today);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _workingCalendarRepository.DeleteAsync(calendar, cancellationToken);

        _logger.LogInformation(
            "Working Calendar Deleted: {CalendarId} ({CalendarName})",
            calendar.Id,
            calendar.Name);
    }

    public async Task<OperationalWorkingDayResponse> GetOperationalWorkingDayAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var calendar = await _workingCalendarRepository.GetActiveCoveringDateAsync(
            companyId,
            date,
            cancellationToken);

        var holidays = await _holidayRepository.GetActiveForCompanyOnDateAsync(
            companyId,
            date,
            cancellationToken);

        var isCalendarWorkingWeekday = calendar?.IsWorkingDay(date) ?? false;
        var isHoliday = holidays.Count > 0;
        var isWorkingDay = calendar is not null && calendar.IsWorkingDay(date, holidays);

        WorkingHoursDayResponse? schedule = null;
        Guid? workingHoursId = null;
        decimal? plannedNetHours = null;

        if (calendar is not null)
        {
            var workingHours = await _workingHoursRepository.GetActiveCoveringDateAsync(
                calendar.Id,
                date,
                cancellationToken);

            if (workingHours is not null)
            {
                workingHoursId = workingHours.Id;
                var day = workingHours.GetDay(date);

                if (day is not null)
                {
                    schedule = new WorkingHoursDayResponse
                    {
                        DayOfWeek = day.DayOfWeek,
                        Enabled = day.Enabled,
                        StartTime = day.StartTime,
                        EndTime = day.EndTime,
                        BreakStart = day.BreakStart,
                        BreakEnd = day.BreakEnd,
                        NetHours = day.NetHours
                    };

                    if (isWorkingDay && day.Enabled)
                    {
                        plannedNetHours = day.NetHours;
                    }
                }
            }
        }

        return new OperationalWorkingDayResponse
        {
            CompanyId = companyId,
            Date = date,
            IsWorkingDay = isWorkingDay,
            IsCalendarWorkingWeekday = isCalendarWorkingWeekday,
            IsHoliday = isHoliday,
            WorkingCalendarId = calendar?.Id,
            WorkingHoursId = workingHoursId,
            Schedule = schedule,
            PlannedNetHours = plannedNetHours,
            Holidays = holidays.Select(HolidayService.MapToResponse).ToList()
        };
    }

    public async Task<IReadOnlyList<WorkingHoursResponse>> GetWorkingHoursForCalendarAsync(
        Guid calendarId,
        CancellationToken cancellationToken = default)
    {
        await GetCalendarOrThrowAsync(calendarId, cancellationToken);

        var items = await _workingHoursRepository.GetAllAsync(
            new WorkingHoursQueryParameters
            {
                WorkingCalendarId = calendarId,
                OrderBy = "effectiveFrom",
                OrderDirection = "desc"
            },
            cancellationToken);

        return items.Select(WorkingHoursService.MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<HolidayResponse>> GetHolidaysForCalendarAsync(
        Guid calendarId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var calendar = await GetCalendarOrThrowAsync(calendarId, cancellationToken);
        var rangeFrom = from ?? calendar.EffectiveFrom;
        var rangeTo = to ?? calendar.EffectiveTo ?? rangeFrom.AddYears(1);

        if (rangeTo < rangeFrom)
        {
            throw new BusinessRuleException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        var holidays = await _holidayRepository.GetActiveForCompanyInRangeAsync(
            calendar.CompanyId,
            rangeFrom,
            rangeTo,
            cancellationToken);

        return holidays.Select(HolidayService.MapToResponse).ToList();
    }

    private async Task EnsureNoActiveOverlapAsync(
        Guid companyId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeCalendarId,
        CancellationToken cancellationToken)
    {
        var overlapping = await _workingCalendarRepository.GetActiveOverlappingAsync(
            companyId,
            effectiveFrom,
            effectiveTo,
            excludeCalendarId,
            cancellationToken);

        if (overlapping.Count > 0)
        {
            throw new ConflictException(
                "Only one active calendar may exist for the same period.");
        }
    }

    private async Task<WorkingCalendar> GetCalendarOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarRepository.GetByIdAsync(id, cancellationToken);

        if (calendar is null)
        {
            throw new NotFoundException($"Working Calendar with id '{id}' was not found.");
        }

        return calendar;
    }

    private static WorkingCalendarResponse MapToResponse(WorkingCalendar calendar)
    {
        return new WorkingCalendarResponse
        {
            Id = calendar.Id,
            CompanyId = calendar.CompanyId,
            Name = calendar.Name,
            Status = calendar.Status,
            EffectiveFrom = calendar.EffectiveFrom,
            EffectiveTo = calendar.EffectiveTo,
            WorkingDays = calendar.WorkingDays,
            CreatedAt = calendar.CreatedAt,
            UpdatedAt = calendar.UpdatedAt
        };
    }
}

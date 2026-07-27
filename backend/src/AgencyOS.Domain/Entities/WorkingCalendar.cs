namespace AgencyOS.Domain.Entities;

/// <summary>
/// Company Working Calendar aggregate (US-101).
/// Encodes validity, working days, and lifecycle transitions for Capacity Planning.
/// </summary>
public class WorkingCalendar
{
    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateOnly EffectiveFrom { get; private set; }

    public DateOnly? EffectiveTo { get; private set; }

    public string[] WorkingDays { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => WorkingCalendarStatus.IsActive(Status);

    public bool IsInactive => WorkingCalendarStatus.IsInactive(Status);

    /// <summary>
    /// EF Core materialization constructor.
    /// </summary>
    private WorkingCalendar()
    {
    }

    public static WorkingCalendar Create(
        Guid companyId,
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<string> workingDays,
        DateTimeOffset createdAt)
    {
        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Calendar Name is mandatory.");
        }

        if (!WorkingCalendarRules.HasValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        if (!WorkingCalendarRules.HasAtLeastOneWorkingDay(workingDays))
        {
            throw new InvalidOperationException("Calendar must contain at least one working day.");
        }

        if (WorkingCalendarRules.HasDuplicateWorkingDays(workingDays))
        {
            throw new InvalidOperationException("Working days cannot be duplicated.");
        }

        var normalizedDays = WorkingCalendarRules.NormalizeWorkingDays(workingDays);

        if (normalizedDays.Count == 0)
        {
            throw new InvalidOperationException("Calendar must contain at least one working day.");
        }

        if (normalizedDays.Any(day => !WorkingDayNames.All.Contains(day)))
        {
            throw new InvalidOperationException("Working day must be a valid weekday name.");
        }

        return new WorkingCalendar
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = name.Trim(),
            Status = WorkingCalendarStatus.Inactive,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            WorkingDays = normalizedDays.ToArray(),
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public bool CoversDate(DateOnly date) =>
        WorkingCalendarRules.CoversDate(EffectiveFrom, EffectiveTo, date);

    public bool IsWorkingDay(DateOnly date) =>
        CoversDate(date) && WorkingDayNames.IsWorkingDay(date, WorkingDays);

    /// <summary>
    /// Returns true when the date is a calendar working weekday and not an active holiday (US-102 integration).
    /// </summary>
    public bool IsWorkingDay(DateOnly date, IEnumerable<Holiday> activeHolidays)
    {
        if (!IsWorkingDay(date))
        {
            return false;
        }

        return !activeHolidays.Any(holiday =>
            holiday.IsActive
            && holiday.AffectsCompany(CompanyId)
            && holiday.OccursOn(date));
    }

    public bool OverlapsPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo) =>
        WorkingCalendarRules.PeriodsOverlap(EffectiveFrom, EffectiveTo, effectiveFrom, effectiveTo);

    public bool IsHistorical(DateOnly asOfDate) =>
        WorkingCalendarRules.IsHistorical(EffectiveFrom, asOfDate);

    public void Reconfigure(
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<string> workingDays,
        DateOnly asOfDate,
        DateTimeOffset updatedAt)
    {
        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must never be modified. Calendars that have already started cannot be reconfigured.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Calendar Name is mandatory.");
        }

        if (!WorkingCalendarRules.HasValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        if (!WorkingCalendarRules.HasAtLeastOneWorkingDay(workingDays))
        {
            throw new InvalidOperationException("Calendar must contain at least one working day.");
        }

        if (WorkingCalendarRules.HasDuplicateWorkingDays(workingDays))
        {
            throw new InvalidOperationException("Working days cannot be duplicated.");
        }

        var normalizedDays = WorkingCalendarRules.NormalizeWorkingDays(workingDays);

        if (normalizedDays.Count == 0)
        {
            throw new InvalidOperationException("Calendar must contain at least one working day.");
        }

        if (normalizedDays.Any(day => !WorkingDayNames.All.Contains(day)))
        {
            throw new InvalidOperationException("Working day must be a valid weekday name.");
        }

        Name = name.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        WorkingDays = normalizedDays.ToArray();
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (IsActive)
        {
            return;
        }

        Status = WorkingCalendarStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = WorkingCalendarStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete(DateOnly asOfDate)
    {
        if (!WorkingCalendarStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Only inactive calendars can be deleted.");
        }

        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must never be modified. Calendars that have already started cannot be deleted.");
        }
    }
}

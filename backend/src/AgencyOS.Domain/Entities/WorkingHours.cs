namespace AgencyOS.Domain.Entities;

/// <summary>
/// Standard Working Hours aggregate (US-103).
/// Defines operational schedules associated with a Working Calendar.
/// </summary>
public class WorkingHours
{
    private readonly List<WorkingHoursDay> _days = [];

    public Guid Id { get; private set; }

    public Guid WorkingCalendarId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateOnly EffectiveFrom { get; private set; }

    public DateOnly? EffectiveTo { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<WorkingHoursDay> Days => _days.AsReadOnly();

    public bool IsActive => WorkingHoursStatus.IsActive(Status);

    public bool IsInactive => WorkingHoursStatus.IsInactive(Status);

    private WorkingHours()
    {
    }

    public static WorkingHours Create(
        Guid workingCalendarId,
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<WorkingHoursDayDefinition> days,
        DateTimeOffset createdAt)
    {
        if (workingCalendarId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Calendar is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Working Hours name is mandatory.");
        }

        if (!WorkingHoursRules.HasValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        var workingHours = new WorkingHours
        {
            Id = Guid.NewGuid(),
            WorkingCalendarId = workingCalendarId,
            Name = name.Trim(),
            Status = WorkingHoursStatus.Inactive,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        workingHours.ReplaceDays(days);
        return workingHours;
    }

    public void Reconfigure(
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<WorkingHoursDayDefinition> days,
        DateOnly asOfDate,
        DateTimeOffset updatedAt)
    {
        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must remain unchanged. Working Hours that have already started cannot be reconfigured.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Working Hours name is mandatory.");
        }

        if (!WorkingHoursRules.HasValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        Name = name.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ReplaceDays(days);
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (IsActive)
        {
            return;
        }

        Status = WorkingHoursStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = WorkingHoursStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete(DateOnly asOfDate)
    {
        if (!WorkingHoursStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Only inactive Working Hours can be deleted.");
        }

        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must remain unchanged. Working Hours that have already started cannot be deleted.");
        }
    }

    public bool IsHistorical(DateOnly asOfDate) =>
        WorkingHoursRules.IsHistorical(EffectiveFrom, asOfDate);

    public bool CoversDate(DateOnly date) =>
        WorkingHoursRules.CoversDate(EffectiveFrom, EffectiveTo, date);

    public bool OverlapsPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo) =>
        WorkingHoursRules.PeriodsOverlap(EffectiveFrom, EffectiveTo, effectiveFrom, effectiveTo);

    public WorkingHoursDay? GetDay(string dayOfWeek)
    {
        var canonical = WorkingDayNames.Canonicalize(dayOfWeek);
        return _days.FirstOrDefault(day =>
            string.Equals(day.DayOfWeek, canonical, StringComparison.OrdinalIgnoreCase));
    }

    public WorkingHoursDay? GetDay(DateOnly date) =>
        GetDay(WorkingDayNames.FromDayOfWeek(date.DayOfWeek));

    private void ReplaceDays(IReadOnlyCollection<WorkingHoursDayDefinition> days)
    {
        if (days is null || days.Count == 0)
        {
            throw new InvalidOperationException("At least one enabled weekday is required.");
        }

        var normalized = days
            .Select(day => new
            {
                Day = WorkingDayNames.Canonicalize(day.DayOfWeek),
                day.Enabled,
                day.StartTime,
                day.EndTime,
                day.BreakStart,
                day.BreakEnd
            })
            .ToList();

        if (normalized.Select(day => day.Day).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
        {
            throw new InvalidOperationException("Weekday schedules cannot be duplicated.");
        }

        if (!normalized.Any(day => day.Enabled))
        {
            throw new InvalidOperationException("At least one enabled weekday is required.");
        }

        _days.Clear();

        foreach (var dayName in WorkingDayNames.Ordered)
        {
            var definition = normalized.FirstOrDefault(day =>
                string.Equals(day.Day, dayName, StringComparison.OrdinalIgnoreCase));

            if (definition is null)
            {
                _days.Add(WorkingHoursDay.Create(Id, dayName, false, null, null, null, null));
                continue;
            }

            _days.Add(WorkingHoursDay.Create(
                Id,
                dayName,
                definition.Enabled,
                definition.StartTime,
                definition.EndTime,
                definition.BreakStart,
                definition.BreakEnd));
        }
    }
}

/// <summary>
/// Input definition used to configure weekday schedules on the Working Hours aggregate.
/// </summary>
public sealed class WorkingHoursDayDefinition
{
    public string DayOfWeek { get; init; } = string.Empty;

    public bool Enabled { get; init; }

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public TimeOnly? BreakStart { get; init; }

    public TimeOnly? BreakEnd { get; init; }
}

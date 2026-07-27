namespace AgencyOS.Domain.Entities;

/// <summary>
/// Resource Availability aggregate (US-104).
/// Planned individual availability linked to Execution Resource, Working Calendar, and Working Hours.
/// </summary>
public class ResourceAvailability
{
    private readonly List<ResourceAvailabilityWeekDay> _weeklyAvailability = [];
    private readonly List<ResourceAvailabilityDayOverride> _dailyOverrides = [];

    public Guid Id { get; private set; }

    public Guid ExecutionResourceId { get; private set; }

    public Guid WorkingCalendarId { get; private set; }

    public Guid WorkingHoursId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateOnly EffectiveFrom { get; private set; }

    public DateOnly? EffectiveTo { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<ResourceAvailabilityWeekDay> WeeklyAvailability =>
        _weeklyAvailability.AsReadOnly();

    public IReadOnlyCollection<ResourceAvailabilityDayOverride> DailyOverrides =>
        _dailyOverrides.AsReadOnly();

    public bool IsActive => ResourceAvailabilityStatus.IsActive(Status);

    public bool IsInactive => ResourceAvailabilityStatus.IsInactive(Status);

    private ResourceAvailability()
    {
    }

    public static ResourceAvailability Create(
        Guid executionResourceId,
        Guid workingCalendarId,
        Guid workingHoursId,
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<ResourceAvailabilityWeekDayDefinition> weeklyAvailability,
        IReadOnlyCollection<ResourceAvailabilityDayOverrideDefinition> dailyOverrides,
        DateTimeOffset createdAt)
    {
        ValidateAssociations(executionResourceId, workingCalendarId, workingHoursId, name, effectiveFrom, effectiveTo);

        var availability = new ResourceAvailability
        {
            Id = Guid.NewGuid(),
            ExecutionResourceId = executionResourceId,
            WorkingCalendarId = workingCalendarId,
            WorkingHoursId = workingHoursId,
            Name = name.Trim(),
            Status = ResourceAvailabilityStatus.Inactive,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        availability.ReplaceWeeklyAvailability(weeklyAvailability);
        availability.ReplaceDailyOverrides(dailyOverrides, effectiveFrom, effectiveTo);
        return availability;
    }

    public void Reconfigure(
        Guid workingCalendarId,
        Guid workingHoursId,
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        IReadOnlyCollection<ResourceAvailabilityWeekDayDefinition> weeklyAvailability,
        IReadOnlyCollection<ResourceAvailabilityDayOverrideDefinition> dailyOverrides,
        DateOnly asOfDate,
        DateTimeOffset updatedAt)
    {
        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical allocations must never be modified. Resource Availability that has already started cannot be reconfigured.");
        }

        ValidateAssociations(ExecutionResourceId, workingCalendarId, workingHoursId, name, effectiveFrom, effectiveTo);

        WorkingCalendarId = workingCalendarId;
        WorkingHoursId = workingHoursId;
        Name = name.Trim();
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ReplaceWeeklyAvailability(weeklyAvailability);
        ReplaceDailyOverrides(dailyOverrides, effectiveFrom, effectiveTo);
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (IsActive)
        {
            return;
        }

        Status = ResourceAvailabilityStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = ResourceAvailabilityStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete(DateOnly asOfDate)
    {
        if (!ResourceAvailabilityStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Only inactive Resource Availability records can be deleted.");
        }

        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical allocations must never be modified. Resource Availability that has already started cannot be deleted.");
        }
    }

    public bool IsHistorical(DateOnly asOfDate) =>
        ResourceAvailabilityRules.IsHistorical(EffectiveFrom, asOfDate);

    public bool CoversDate(DateOnly date) =>
        ResourceAvailabilityRules.CoversDate(EffectiveFrom, EffectiveTo, date);

    public bool OverlapsPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo) =>
        ResourceAvailabilityRules.PeriodsOverlap(EffectiveFrom, EffectiveTo, effectiveFrom, effectiveTo);

    public ResourceAvailabilityWeekDay? GetWeeklyDay(string dayOfWeek)
    {
        var canonical = WorkingDayNames.Canonicalize(dayOfWeek);
        return _weeklyAvailability.FirstOrDefault(day =>
            string.Equals(day.DayOfWeek, canonical, StringComparison.OrdinalIgnoreCase));
    }

    public ResourceAvailabilityDayOverride? GetOverride(DateOnly date) =>
        _dailyOverrides.FirstOrDefault(overrideDay => overrideDay.OverrideDate == date);

    /// <summary>
    /// Resolves whether the resource is available on a date according to weekly flags and daily overrides.
    /// Does not evaluate calendar holidays or working-hours schedules; callers combine those separately.
    /// </summary>
    public bool IsAvailableOn(DateOnly date)
    {
        if (!CoversDate(date) || !IsActive)
        {
            return false;
        }

        var dayOverride = GetOverride(date);
        if (dayOverride is not null)
        {
            return dayOverride.Available;
        }

        var weekDay = GetWeeklyDay(WorkingDayNames.FromDayOfWeek(date.DayOfWeek));
        return weekDay?.Enabled ?? false;
    }

    private static void ValidateAssociations(
        Guid executionResourceId,
        Guid workingCalendarId,
        Guid workingHoursId,
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        if (executionResourceId == Guid.Empty)
        {
            throw new InvalidOperationException("Execution Resource is mandatory.");
        }

        if (workingCalendarId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Calendar is mandatory.");
        }

        if (workingHoursId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Hours configuration is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Resource Availability name is mandatory.");
        }

        if (!ResourceAvailabilityRules.HasValidPeriod(effectiveFrom, effectiveTo))
        {
            throw new InvalidOperationException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }
    }

    private void ReplaceWeeklyAvailability(
        IReadOnlyCollection<ResourceAvailabilityWeekDayDefinition> weeklyAvailability)
    {
        if (weeklyAvailability is null || weeklyAvailability.Count == 0)
        {
            throw new InvalidOperationException("Weekly availability must include all weekdays.");
        }

        var normalized = weeklyAvailability
            .Select(day => new
            {
                Day = WorkingDayNames.Canonicalize(day.DayOfWeek),
                day.Enabled
            })
            .ToList();

        if (normalized.Select(day => day.Day).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
        {
            throw new InvalidOperationException("Weekly availability weekdays cannot be duplicated.");
        }

        if (!normalized.Any(day => day.Enabled))
        {
            throw new InvalidOperationException("At least one weekday must be enabled in weekly availability.");
        }

        _weeklyAvailability.Clear();

        foreach (var dayName in WorkingDayNames.Ordered)
        {
            var definition = normalized.FirstOrDefault(day =>
                string.Equals(day.Day, dayName, StringComparison.OrdinalIgnoreCase));

            _weeklyAvailability.Add(ResourceAvailabilityWeekDay.Create(
                Id,
                dayName,
                definition?.Enabled ?? false));
        }
    }

    private void ReplaceDailyOverrides(
        IReadOnlyCollection<ResourceAvailabilityDayOverrideDefinition> dailyOverrides,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        _dailyOverrides.Clear();

        if (dailyOverrides is null || dailyOverrides.Count == 0)
        {
            return;
        }

        if (dailyOverrides.Select(day => day.OverrideDate).Distinct().Count() != dailyOverrides.Count)
        {
            throw new InvalidOperationException("Daily overrides cannot duplicate the same date.");
        }

        foreach (var definition in dailyOverrides.OrderBy(day => day.OverrideDate))
        {
            if (!ResourceAvailabilityRules.CoversDate(effectiveFrom, effectiveTo, definition.OverrideDate))
            {
                throw new InvalidOperationException(
                    "Daily overrides must fall within the Resource Availability effective period.");
            }

            _dailyOverrides.Add(ResourceAvailabilityDayOverride.Create(
                Id,
                definition.OverrideDate,
                definition.Available,
                definition.StartTime,
                definition.EndTime,
                definition.Notes));
        }
    }
}

public sealed class ResourceAvailabilityWeekDayDefinition
{
    public string DayOfWeek { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}

public sealed class ResourceAvailabilityDayOverrideDefinition
{
    public DateOnly OverrideDate { get; init; }

    public bool Available { get; init; }

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    public string? Notes { get; init; }
}

namespace AgencyOS.Domain.Entities;

/// <summary>
/// Weekday schedule entry within a Working Hours configuration (US-103).
/// </summary>
public class WorkingHoursDay
{
    public Guid Id { get; private set; }

    public Guid WorkingHoursId { get; private set; }

    public string DayOfWeek { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    public TimeOnly? StartTime { get; private set; }

    public TimeOnly? EndTime { get; private set; }

    public TimeOnly? BreakStart { get; private set; }

    public TimeOnly? BreakEnd { get; private set; }

    private WorkingHoursDay()
    {
    }

    internal static WorkingHoursDay Create(
        Guid workingHoursId,
        string dayOfWeek,
        bool enabled,
        TimeOnly? startTime,
        TimeOnly? endTime,
        TimeOnly? breakStart,
        TimeOnly? breakEnd)
    {
        var canonicalDay = WorkingDayNames.Canonicalize(dayOfWeek);

        if (!WorkingDayNames.All.Contains(canonicalDay))
        {
            throw new InvalidOperationException("DayOfWeek must be a valid weekday name.");
        }

        WorkingHoursRules.ValidateDaySchedule(enabled, startTime, endTime, breakStart, breakEnd);

        return new WorkingHoursDay
        {
            Id = Guid.NewGuid(),
            WorkingHoursId = workingHoursId,
            DayOfWeek = canonicalDay,
            Enabled = enabled,
            StartTime = enabled ? startTime : null,
            EndTime = enabled ? endTime : null,
            BreakStart = enabled ? breakStart : null,
            BreakEnd = enabled ? breakEnd : null
        };
    }

    public decimal? NetHours
    {
        get
        {
            if (!Enabled || StartTime is null || EndTime is null)
            {
                return null;
            }

            return WorkingHoursRules.CalculateNetHours(
                StartTime.Value,
                EndTime.Value,
                BreakStart,
                BreakEnd);
        }
    }
}

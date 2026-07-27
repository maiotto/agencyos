namespace AgencyOS.Domain.Entities;

/// <summary>
/// Weekly availability flag for a weekday within a Resource Availability configuration.
/// </summary>
public class ResourceAvailabilityWeekDay
{
    public Guid Id { get; private set; }

    public Guid ResourceAvailabilityId { get; private set; }

    public string DayOfWeek { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    private ResourceAvailabilityWeekDay()
    {
    }

    internal static ResourceAvailabilityWeekDay Create(
        Guid resourceAvailabilityId,
        string dayOfWeek,
        bool enabled)
    {
        var canonical = WorkingDayNames.Canonicalize(dayOfWeek);

        if (!WorkingDayNames.All.Contains(canonical))
        {
            throw new InvalidOperationException("DayOfWeek must be a valid weekday name.");
        }

        return new ResourceAvailabilityWeekDay
        {
            Id = Guid.NewGuid(),
            ResourceAvailabilityId = resourceAvailabilityId,
            DayOfWeek = canonical,
            Enabled = enabled
        };
    }
}

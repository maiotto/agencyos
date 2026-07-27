namespace AgencyOS.Domain.Entities;

/// <summary>
/// Date-specific availability override within a Resource Availability configuration.
/// </summary>
public class ResourceAvailabilityDayOverride
{
    public Guid Id { get; private set; }

    public Guid ResourceAvailabilityId { get; private set; }

    public DateOnly OverrideDate { get; private set; }

    public bool Available { get; private set; }

    public TimeOnly? StartTime { get; private set; }

    public TimeOnly? EndTime { get; private set; }

    public string? Notes { get; private set; }

    private ResourceAvailabilityDayOverride()
    {
    }

    internal static ResourceAvailabilityDayOverride Create(
        Guid resourceAvailabilityId,
        DateOnly overrideDate,
        bool available,
        TimeOnly? startTime,
        TimeOnly? endTime,
        string? notes)
    {
        ResourceAvailabilityRules.ValidateOverrideTimes(available, startTime, endTime);

        return new ResourceAvailabilityDayOverride
        {
            Id = Guid.NewGuid(),
            ResourceAvailabilityId = resourceAvailabilityId,
            OverrideDate = overrideDate,
            Available = available,
            StartTime = available ? startTime : null,
            EndTime = available ? endTime : null,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }
}

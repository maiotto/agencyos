namespace AgencyOS.Application.DTOs;

public class ResourceAvailabilityWeekDayRequest
{
    public string DayOfWeek { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}

public class ResourceAvailabilityDayOverrideRequest
{
    public DateOnly OverrideDate { get; set; }

    public bool Available { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Notes { get; set; }
}

public class CreateResourceAvailabilityRequest
{
    public Guid ExecutionResourceId { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<ResourceAvailabilityWeekDayRequest> WeeklyAvailability { get; set; } = [];

    public IReadOnlyList<ResourceAvailabilityDayOverrideRequest> DailyOverrides { get; set; } = [];
}

public class UpdateResourceAvailabilityRequest
{
    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<ResourceAvailabilityWeekDayRequest> WeeklyAvailability { get; set; } = [];

    public IReadOnlyList<ResourceAvailabilityDayOverrideRequest> DailyOverrides { get; set; } = [];
}

public class ResourceAvailabilityWeekDayResponse
{
    public string DayOfWeek { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}

public class ResourceAvailabilityDayOverrideResponse
{
    public DateOnly OverrideDate { get; set; }

    public bool Available { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Notes { get; set; }
}

public class ResourceAvailabilityResponse
{
    public Guid Id { get; set; }

    public Guid ExecutionResourceId { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<ResourceAvailabilityWeekDayResponse> WeeklyAvailability { get; set; } = [];

    public IReadOnlyList<ResourceAvailabilityDayOverrideResponse> DailyOverrides { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public class ResourceAvailabilityQueryParameters
{
    public Guid? ExecutionResourceId { get; set; }

    public Guid? WorkingCalendarId { get; set; }

    public string? Status { get; set; }

    public string? Name { get; set; }

    public string OrderBy { get; set; } = "name";

    public string OrderDirection { get; set; } = "asc";
}

public class OperationalResourceAvailabilityResponse
{
    public Guid ExecutionResourceId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsResourceAvailable { get; set; }

    public bool IsOperationalWorkingDay { get; set; }

    public bool IsHoliday { get; set; }

    public Guid? ResourceAvailabilityId { get; set; }

    public Guid? WorkingCalendarId { get; set; }

    public Guid? WorkingHoursId { get; set; }

    public WorkingHoursDayResponse? Schedule { get; set; }

    public decimal? PlannedNetHours { get; set; }

    public ResourceAvailabilityDayOverrideResponse? DayOverride { get; set; }

    public IReadOnlyList<HolidayResponse> Holidays { get; set; } = [];
}

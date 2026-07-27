namespace AgencyOS.Application.DTOs;

public class CapacityDayBreakdownResponse
{
    public DateOnly Date { get; set; }

    public bool IsOperationalDay { get; set; }

    public bool IsCalendarWorkingWeekday { get; set; }

    public bool IsHoliday { get; set; }

    public bool IsResourceAvailable { get; set; }

    public decimal PlannedCapacityHours { get; set; }

    public Guid? ResourceAvailabilityId { get; set; }

    public Guid? WorkingCalendarId { get; set; }

    public Guid? WorkingHoursId { get; set; }

    public string? ExclusionReason { get; set; }
}

public class CapacityResponse
{
    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int OperationalDayCount { get; set; }

    public int HolidayImpactDayCount { get; set; }

    public int ResourceAvailabilityExcludedDayCount { get; set; }

    public decimal ConfiguredWorkingHoursTotal { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal AvailableHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public decimal RemainingCapacityHours { get; set; }

    public IReadOnlyList<CapacityDayBreakdownResponse> OperationalDays { get; set; } = [];
}

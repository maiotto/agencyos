namespace AgencyOS.Application.DTOs;

public class CapacityHistoryDaySnapshotResponse
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

public class CapacityHistoryResponse
{
    public Guid HistoryId { get; set; }

    public Guid ExecutionResourceId { get; set; }

    public Guid CompanyId { get; set; }

    public DateTimeOffset CalculationDate { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int WorkingDays { get; set; }

    public int HolidayDays { get; set; }

    public int AvailableDays { get; set; }

    public decimal ConfiguredHours { get; set; }

    public decimal AvailableHours { get; set; }

    public decimal CapacityHours { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public string CalculationVersion { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<CapacityHistoryDaySnapshotResponse> OperationalDays { get; set; } = [];
}

public class CapacityHistoryAggregateResponse
{
    public Guid? CompanyId { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public string? CalculationVersion { get; set; }

    public int RecordCount { get; set; }

    public decimal TotalConfiguredHours { get; set; }

    public decimal TotalAvailableHours { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal AverageUtilizationPercentage { get; set; }
}

public class CapacityHistoryCompareQueryParameters
{
    public Guid LeftHistoryId { get; set; }

    public Guid RightHistoryId { get; set; }
}

public class CapacityHistoryCompareResponse
{
    public CapacityHistoryResponse Left { get; set; } = null!;

    public CapacityHistoryResponse Right { get; set; } = null!;

    public decimal CapacityHoursDelta { get; set; }

    public decimal AvailableHoursDelta { get; set; }

    public decimal ConfiguredHoursDelta { get; set; }

    public decimal UtilizationPercentageDelta { get; set; }

    public int WorkingDaysDelta { get; set; }

    public int HolidayDaysDelta { get; set; }

    public int AvailableDaysDelta { get; set; }
}

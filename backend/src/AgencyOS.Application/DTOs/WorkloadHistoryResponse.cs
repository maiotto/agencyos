namespace AgencyOS.Application.DTOs;

public class WorkloadHistoryAssignmentSnapshotResponse
{
    public Guid AssignmentId { get; set; }

    public Guid TaskId { get; set; }

    public string AssignmentRole { get; set; } = string.Empty;

    public decimal PlannedHours { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class WorkloadHistoryResponse
{
    public Guid HistoryId { get; set; }

    public Guid ExecutionResourceId { get; set; }

    public Guid CompanyId { get; set; }

    public DateTimeOffset CalculationDate { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal CapacityHours { get; set; }

    public decimal WorkloadPercentage { get; set; }

    public int WorkingDays { get; set; }

    public int HolidayDays { get; set; }

    public int AvailableDays { get; set; }

    public string CalculationVersion { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public int AssignmentCount { get; set; }

    public decimal AverageHoursPerAssignment { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public IReadOnlyList<WorkloadHistoryAssignmentSnapshotResponse> AssignmentDistribution { get; set; } = [];
}

public class WorkloadHistoryAggregateResponse
{
    public Guid? CompanyId { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public string? CalculationVersion { get; set; }

    public int RecordCount { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }
}

public class WorkloadHistoryCompareQueryParameters
{
    public Guid LeftHistoryId { get; set; }

    public Guid RightHistoryId { get; set; }
}

public class WorkloadHistoryCompareResponse
{
    public WorkloadHistoryResponse Left { get; set; } = null!;

    public WorkloadHistoryResponse Right { get; set; } = null!;

    public decimal AllocatedHoursDelta { get; set; }

    public decimal CapacityHoursDelta { get; set; }

    public decimal WorkloadPercentageDelta { get; set; }

    public int WorkingDaysDelta { get; set; }

    public int HolidayDaysDelta { get; set; }

    public int AvailableDaysDelta { get; set; }
}

public class WorkloadHistoryTrendPointResponse
{
    public DateTimeOffset CalculationDate { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal CapacityHours { get; set; }

    public decimal WorkloadPercentage { get; set; }

    public string CalculationVersion { get; set; } = string.Empty;
}

public class WorkloadHistoryTrendResponse
{
    public Guid? CompanyId { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public int PointCount { get; set; }

    public IReadOnlyList<WorkloadHistoryTrendPointResponse> Points { get; set; } = [];
}

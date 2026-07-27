namespace AgencyOS.Application.DTOs;

public class PlanningTemplateQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? Name { get; set; }

    public string? Status { get; set; }

    public Guid? WorkingCalendarId { get; set; }

    public Guid? WorkingHoursId { get; set; }

    public string? ResourceAvailabilityStrategy { get; set; }

    public string? Search { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class CreatePlanningTemplateRequest
{
    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string ResourceAvailabilityStrategy { get; set; } = "RequireActiveConfiguration";

    public int DefaultPlanningWindowDays { get; set; } = 7;

    public int DefaultPeriodStartOffsetDays { get; set; }

    public decimal? UtilizationWarningPercentage { get; set; }

    public bool IncludeAssignmentDistribution { get; set; } = true;

    public string? PlanningParametersJson { get; set; }
}

public class UpdatePlanningTemplateRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string ResourceAvailabilityStrategy { get; set; } = "RequireActiveConfiguration";

    public int DefaultPlanningWindowDays { get; set; } = 7;

    public int DefaultPeriodStartOffsetDays { get; set; }

    public decimal? UtilizationWarningPercentage { get; set; }

    public bool IncludeAssignmentDistribution { get; set; } = true;

    public string? PlanningParametersJson { get; set; }
}

public class ClonePlanningTemplateRequest
{
    public string Name { get; set; } = string.Empty;
}

public class ApplyPlanningTemplateRequest
{
    public DateOnly? PeriodStartDate { get; set; }

    public DateOnly? PeriodEndDate { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public Guid? ExcludeMissionId { get; set; }

    /// <summary>
    /// When true, also executes Capacity calculation for the resolved period.
    /// </summary>
    public bool CalculateCapacity { get; set; }

    /// <summary>
    /// When true, also executes Workload calculation for the resolved period.
    /// </summary>
    public bool CalculateWorkload { get; set; }
}

public class PlanningTemplateResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string ResourceAvailabilityStrategy { get; set; } = string.Empty;

    public int DefaultPlanningWindowDays { get; set; }

    public int DefaultPeriodStartOffsetDays { get; set; }

    public decimal? UtilizationWarningPercentage { get; set; }

    public bool IncludeAssignmentDistribution { get; set; }

    public string? PlanningParametersJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// New planning configuration produced by Apply (BR-807/BR-808).
/// Does not mutate the template and does not alter historical Capacity/Workload.
/// </summary>
public class AppliedPlanningConfigurationResponse
{
    public Guid SourceTemplateId { get; set; }

    public string SourceTemplateName { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public Guid WorkingHoursId { get; set; }

    public string ResourceAvailabilityStrategy { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal? UtilizationWarningPercentage { get; set; }

    public bool IncludeAssignmentDistribution { get; set; }

    public string? PlanningParametersJson { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public Guid? ExcludeMissionId { get; set; }

    public IReadOnlyList<CapacityResponse>? CapacityResults { get; set; }

    public CapacitySummaryResponse? CapacitySummary { get; set; }

    public IReadOnlyList<WorkloadResponse>? WorkloadResults { get; set; }

    public WorkloadSummaryResponse? WorkloadSummary { get; set; }
}

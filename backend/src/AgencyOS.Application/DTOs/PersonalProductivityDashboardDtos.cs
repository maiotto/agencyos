namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Personal Productivity Dashboard (US-507 / BR-3001..BR-3010).
/// Identity resolution follows DEC-501-001 (same fallback chain as My Work).
/// </summary>
public class PersonalProductivityDashboardQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? UserId { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }
}

public class PersonalProductivitySummaryResponse
{
    public Guid CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid? ExecutionResourceId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int PendingWorkCount { get; set; }

    public int CompletedWorkCount { get; set; }

    public int ActivityEventCount { get; set; }

    public PersonalProductivityKpiResponse Kpis { get; set; } = new();

    public MyWorkCapacitySummaryResponse Capacity { get; set; } = new();

    public MyWorkWorkloadSummaryResponse Workload { get; set; } = new();
}

public class PersonalProductivityKpiResponse
{
    public int AssignedTaskCount { get; set; }

    public int AssignedMissionCount { get; set; }

    public int PendingRecommendationCount { get; set; }

    public int PendingDecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int UpcomingDeadlineCount { get; set; }

    public int ActivityEventCount { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public decimal? CompletionRatePercentage { get; set; }
}

public class PersonalProductivityTrendPointResponse
{
    public string Metric { get; set; } = string.Empty;

    public decimal? CurrentValue { get; set; }

    public decimal? PreviousValue { get; set; }

    public decimal? Delta { get; set; }

    public string Direction { get; set; } = "Stable";

    public string Unit { get; set; } = string.Empty;
}

public class PersonalProductivityTrendsResponse
{
    public DateOnly CurrentPeriodStart { get; set; }

    public DateOnly CurrentPeriodEnd { get; set; }

    public DateOnly PreviousPeriodStart { get; set; }

    public DateOnly PreviousPeriodEnd { get; set; }

    public IReadOnlyList<PersonalProductivityTrendPointResponse> Points { get; set; } = [];
}

public class PersonalProductivityActivityItemResponse
{
    public Guid Id { get; set; }

    public string Kind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? OccurredAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

public class PersonalProductivityActivitySummaryResponse
{
    public IReadOnlyList<PersonalProductivityActivityItemResponse> Pending { get; set; } = [];

    public IReadOnlyList<PersonalProductivityActivityItemResponse> Completed { get; set; } = [];

    public IReadOnlyList<MyWorkActivityItemResponse> Timeline { get; set; } = [];
}

public class PersonalProductivityPerformanceIndicatorsResponse
{
    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public decimal? CompletionRatePercentage { get; set; }

    public decimal? OnTimeTaskPercentage { get; set; }

    public int OverdueTaskCount { get; set; }

    public int ActivityIntensity { get; set; }

    /// <summary>Informational label only — never drives operational behavior (BR-3007).</summary>
    public string FocusHint { get; set; } = string.Empty;
}

public class PersonalProductivityStatisticsResponse
{
    public int MissionCount { get; set; }

    public int PendingTaskCount { get; set; }

    public int PendingRecommendationCount { get; set; }

    public int PendingDecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int UpcomingDeadlineCount { get; set; }

    public int ActivityEventCount { get; set; }

    public decimal TotalPendingPlannedHours { get; set; }

    public decimal? CapacityHours { get; set; }

    public decimal? WorkloadHours { get; set; }

    public IReadOnlyList<PersonalProductivityNavigationLinkResponse> NavigationLinks { get; set; } = [];
}

public class PersonalProductivityNavigationLinkResponse
{
    public string Label { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}

/// <summary>Full Personal Productivity Dashboard (US-507 / BR-3001..BR-3010). Read-only.</summary>
public class PersonalProductivityDashboardResponse
{
    public Guid CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid? ExecutionResourceId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public PersonalProductivitySummaryResponse Summary { get; set; } = new();

    public PersonalProductivityKpiResponse Kpis { get; set; } = new();

    public PersonalProductivityTrendsResponse Trends { get; set; } = new();

    public MyWorkCapacitySummaryResponse Capacity { get; set; } = new();

    public MyWorkWorkloadSummaryResponse Workload { get; set; } = new();

    public PersonalProductivityActivitySummaryResponse Activity { get; set; } = new();

    public PersonalProductivityPerformanceIndicatorsResponse Performance { get; set; } = new();

    public PersonalProductivityStatisticsResponse Statistics { get; set; } = new();
}

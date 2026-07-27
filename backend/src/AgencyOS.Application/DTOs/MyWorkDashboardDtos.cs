namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the My Work Dashboard (US-501 / BR-2401..BR-2410).
/// Read-only; identity resolution follows DEC-501-001 (UserId/CompanyId/ExecutionResourceId
/// fall back through parameters, ambient context, and a company default/heuristic lookup).
/// </summary>
public class MyWorkDashboardQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c>.</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Defaults to <c>IAuditContext.UserId</c>, then the literal <c>"system"</c> (DEC-501-001).</summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Defaults to the first active <see cref="AgencyOS.Domain.Entities.ExecutionResource"/> whose Code equals
    /// UserId (case-insensitive), when resolvable. Assignments/Tasks/Missions/Capacity/Workload are scoped by
    /// this value; when unresolved those sections are returned empty (BR-2401/BR-2402).
    /// </summary>
    public Guid? ExecutionResourceId { get; set; }

    /// <summary>Lower bound applied to the Activity Timeline (BR-2405).</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to the Activity Timeline (BR-2405).</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Maps to Capacity/Workload engine period filters (BR-2405).</summary>
    public DateOnly? PeriodStart { get; set; }

    /// <summary>Maps to Capacity/Workload engine period filters (BR-2405).</summary>
    public DateOnly? PeriodEnd { get; set; }
}

/// <summary>Mission card for the caller's active work (BR-2406 — only active work items shown).</summary>
public class MyWorkMissionCardResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>Count of the caller's own active Tasks under this Mission (not the Mission's total Task count).</summary>
    public int ActiveTaskCount { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Task card for the caller's active assignments (BR-2406).</summary>
public class MyWorkTaskCardResponse
{
    public Guid Id { get; set; }

    public Guid MissionId { get; set; }

    public string MissionName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string Priority { get; set; } = string.Empty;

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedEnd { get; set; }

    public decimal EstimatedHours { get; set; }

    public Guid AssignmentId { get; set; }

    public string AssignmentStatus { get; set; } = string.Empty;

    public decimal AssignmentPlannedHours { get; set; }

    public bool IsOverdue { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Pending Recommendation card. Sourced from RecommendationWorkflow (the governance queue has no
/// assignee field, so it is company-scoped rather than user-scoped) optionally unioned with
/// workflows for Recommendations the caller generated (DEC-501-001).
/// </summary>
public class MyWorkRecommendationCardResponse
{
    /// <summary>RecommendationWorkflow id (the approval queue item that drives the drill-down).</summary>
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid MissionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Pending Decision card (DecisionStatus Created or InProgress).</summary>
public class MyWorkDecisionCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid MissionId { get; set; }

    public string DecisionStatus { get; set; } = string.Empty;

    public string ImplementationStatus { get; set; } = string.Empty;

    public DateTimeOffset DecisionDate { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Capacity snapshot for the resolved Execution Resource. When no Execution Resource could be
/// resolved, <see cref="HasData"/> is false and the numeric fields remain zero (BR-2401).
/// </summary>
public class MyWorkCapacitySummaryResponse
{
    public bool HasData { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AllocatedHours { get; set; }

    public decimal AvailableHours { get; set; }

    public decimal UtilizationPercentage { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Workload snapshot for the resolved Execution Resource. When no Execution Resource could be
/// resolved, <see cref="HasData"/> is false and the numeric fields remain zero (BR-2401).
/// </summary>
public class MyWorkWorkloadSummaryResponse
{
    public bool HasData { get; set; }

    public Guid? ExecutionResourceId { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal TotalPlannedHours { get; set; }

    public int AssignmentCount { get; set; }

    public decimal WorkloadPercentage { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Single entry in the caller's Activity Timeline, projected from Audit Events.</summary>
public class MyWorkActivityItemResponse
{
    public Guid Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Overdue or upcoming Task deadline entry (BR-2401 personal KPIs).</summary>
public class MyWorkDeadlineItemResponse
{
    public Guid TaskId { get; set; }

    public Guid MissionId { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string MissionName { get; set; } = string.Empty;

    public DateOnly PlannedEnd { get; set; }

    public bool IsOverdue { get; set; }

    /// <summary>Negative when overdue (days late); non-negative when still upcoming.</summary>
    public int DaysRemaining { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Personal KPI rollup computed purely from already-aggregated counts (BR-2407).</summary>
public class MyWorkKpiSummaryResponse
{
    public int AssignedTaskCount { get; set; }

    public int AssignedMissionCount { get; set; }

    public int PendingRecommendationCount { get; set; }

    public int PendingDecisionCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int UpcomingDeadlineCount { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }
}

public class MyWorkTasksResponse
{
    public IReadOnlyList<MyWorkTaskCardResponse> Tasks { get; set; } = [];

    public IReadOnlyList<MyWorkDeadlineItemResponse> OverdueTasks { get; set; } = [];

    public IReadOnlyList<MyWorkDeadlineItemResponse> UpcomingDeadlines { get; set; } = [];
}

public class MyWorkMissionsResponse
{
    public IReadOnlyList<MyWorkMissionCardResponse> Missions { get; set; } = [];
}

public class MyWorkRecommendationsResponse
{
    public IReadOnlyList<MyWorkRecommendationCardResponse> Recommendations { get; set; } = [];
}

public class MyWorkDecisionsResponse
{
    public IReadOnlyList<MyWorkDecisionCardResponse> Decisions { get; set; } = [];
}

public class MyWorkActivityResponse
{
    public IReadOnlyList<MyWorkActivityItemResponse> Items { get; set; } = [];
}

/// <summary>Condensed headline section returned by <c>GET /my-work/summary</c>.</summary>
public class MyWorkSummaryResponse
{
    public Guid CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid? ExecutionResourceId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public int MissionCount { get; set; }

    public int TaskCount { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }

    public MyWorkKpiSummaryResponse Kpis { get; set; } = new();

    public MyWorkCapacitySummaryResponse Capacity { get; set; } = new();

    public MyWorkWorkloadSummaryResponse Workload { get; set; } = new();
}

/// <summary>
/// Full, read-only My Work Dashboard (US-501 / BR-2401..BR-2410). Every section is a projection
/// over existing Assignment/Task/Mission/RecommendationWorkflow/Recommendation/Decision/Capacity/
/// Workload/Audit data — nothing is recalculated or persisted.
/// </summary>
public class MyWorkDashboardResponse
{
    public Guid CompanyId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid? ExecutionResourceId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public MyWorkKpiSummaryResponse Kpis { get; set; } = new();

    public MyWorkCapacitySummaryResponse Capacity { get; set; } = new();

    public MyWorkWorkloadSummaryResponse Workload { get; set; } = new();

    public IReadOnlyList<MyWorkMissionCardResponse> Missions { get; set; } = [];

    public IReadOnlyList<MyWorkTaskCardResponse> Tasks { get; set; } = [];

    public IReadOnlyList<MyWorkRecommendationCardResponse> Recommendations { get; set; } = [];

    public IReadOnlyList<MyWorkDecisionCardResponse> Decisions { get; set; } = [];

    public IReadOnlyList<MyWorkDeadlineItemResponse> OverdueTasks { get; set; } = [];

    public IReadOnlyList<MyWorkDeadlineItemResponse> UpcomingDeadlines { get; set; } = [];

    public IReadOnlyList<MyWorkActivityItemResponse> Activity { get; set; } = [];
}

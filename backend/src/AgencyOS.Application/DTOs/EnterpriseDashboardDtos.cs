namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Enterprise Dashboard (US-403 / BR-2101..BR-2110).
/// Read-only; never triggers recalculation of Capacity/Workload history.
/// </summary>
public class EnterpriseDashboardQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (BR-2104).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound applied to Generated/Decision/Occurred date filters (BR-2105).</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to Generated/Decision/Occurred date filters (BR-2105).</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Maps to Capacity/Workload history period filters (BR-2105).</summary>
    public DateOnly? PeriodStart { get; set; }

    /// <summary>Maps to Capacity/Workload history period filters (BR-2105).</summary>
    public DateOnly? PeriodEnd { get; set; }
}

/// <summary>
/// Simple status/count breakdown item reused across dashboard sections.
/// </summary>
public class StatusCountItem
{
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }
}

/// <summary>
/// Deterministic health rollup (BR-2107). Status values follow <c>PortfolioHealth</c>
/// (Unknown / Underutilized / Healthy / AtRisk / Overloaded).
/// </summary>
public class HealthIndicator
{
    public string Status { get; set; } = "Unknown";

    public string Label { get; set; } = string.Empty;

    public string? Detail { get; set; }
}

/// <summary>
/// Period-over-period trend (BR-2106): current period value vs. an immediately
/// preceding period of equal length.
/// </summary>
public class TrendIndicator
{
    public string Direction { get; set; } = "Flat";

    public decimal DeltaPercent { get; set; }

    public int CurrentValue { get; set; }

    public int PreviousValue { get; set; }
}

public class EnterpriseDashboardSummaryResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public int PortfolioCount { get; set; }

    public int ActivePortfolioCount { get; set; }

    public int PlanningTemplateCount { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }

    public int PendingDecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    /// <summary>Name of the Company's default Active Decision Profile, if any.</summary>
    public string? DefaultDecisionProfileName { get; set; }

    public HealthIndicator OverallHealth { get; set; } = new();

    public TrendIndicator RecommendationTrend { get; set; } = new();

    public TrendIndicator DecisionTrend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardPlanningResponse
{
    public int TemplateCount { get; set; }

    public int ActiveTemplateCount { get; set; }

    public int InactiveTemplateCount { get; set; }

    public IReadOnlyList<StatusCountItem> StatusBreakdown { get; set; } = [];

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardPortfolioResponse
{
    public int PortfolioCount { get; set; }

    public IReadOnlyList<StatusCountItem> StatusBreakdown { get; set; } = [];

    public IReadOnlyList<StatusCountItem> HealthBreakdown { get; set; } = [];

    public decimal AverageUtilizationPercentage { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }

    public HealthIndicator OverallHealth { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardCapacityResponse
{
    public int RecordCount { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal AverageUtilizationPercentage { get; set; }

    public HealthIndicator UtilizationHealth { get; set; } = new();

    public TrendIndicator UtilizationTrend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardWorkloadResponse
{
    public int RecordCount { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }

    public HealthIndicator WorkloadHealth { get; set; } = new();

    public TrendIndicator WorkloadTrend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardRecommendationsResponse
{
    public int TotalCount { get; set; }

    public int ActiveCount { get; set; }

    public int ArchivedCount { get; set; }

    public IReadOnlyList<StatusCountItem> StatusBreakdown { get; set; } = [];

    public decimal? AverageScore { get; set; }

    public TrendIndicator Trend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardDecisionsResponse
{
    public int TotalCount { get; set; }

    public IReadOnlyList<StatusCountItem> DecisionStatusBreakdown { get; set; } = [];

    public IReadOnlyList<StatusCountItem> ImplementationStatusBreakdown { get; set; } = [];

    public int CompletedCount { get; set; }

    public int CancelledCount { get; set; }

    public TrendIndicator Trend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardAiResponse
{
    public int AIRecommendationCount { get; set; }

    public decimal AverageConfidenceScore { get; set; }

    public int ExplainabilityCount { get; set; }

    public int ExecutiveSummaryCount { get; set; }

    public decimal AverageExecutiveSummaryConfidenceLevel { get; set; }

    public TrendIndicator Trend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

public class EnterpriseDashboardAuditResponse
{
    public int EventCount { get; set; }

    public IReadOnlyList<StatusCountItem> EventTypeBreakdown { get; set; } = [];

    public IReadOnlyList<StatusCountItem> EntityTypeBreakdown { get; set; } = [];

    public DateTimeOffset? LastEventAt { get; set; }

    public TrendIndicator Trend { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Aggregated, read-only Enterprise Dashboard (US-403 / BR-2101..BR-2110).
/// Every section is derived from existing repositories; nothing is recalculated or duplicated.
/// </summary>
public class EnterpriseDashboardResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public EnterpriseDashboardSummaryResponse Summary { get; set; } = new();

    public EnterpriseDashboardPlanningResponse Planning { get; set; } = new();

    public EnterpriseDashboardPortfolioResponse Portfolio { get; set; } = new();

    public EnterpriseDashboardCapacityResponse Capacity { get; set; } = new();

    public EnterpriseDashboardWorkloadResponse Workload { get; set; } = new();

    public EnterpriseDashboardRecommendationsResponse Recommendations { get; set; } = new();

    public EnterpriseDashboardDecisionsResponse Decisions { get; set; } = new();

    public EnterpriseDashboardAiResponse Ai { get; set; } = new();

    public EnterpriseDashboardAuditResponse Audit { get; set; } = new();
}

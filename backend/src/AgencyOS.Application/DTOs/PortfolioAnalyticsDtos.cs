namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for Portfolio Analytics (US-404 / BR-2201..BR-2210).
/// Read-only; never triggers recalculation of Capacity/Workload history and never
/// mutates a Portfolio. <c>PortfolioId</c> optionally scopes the Trends endpoint to a
/// single Portfolio's Missions.
/// </summary>
public class PortfolioAnalyticsQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (BR-2207).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound applied to Recommendation/Decision date filters (BR-2205, BR-2206).</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to Recommendation/Decision date filters (BR-2205, BR-2206).</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Maps to Capacity/Workload history and Portfolio planning-period filters (BR-2204).</summary>
    public DateOnly? PeriodStart { get; set; }

    /// <summary>Maps to Capacity/Workload history and Portfolio planning-period filters (BR-2204).</summary>
    public DateOnly? PeriodEnd { get; set; }

    /// <summary>Optionally scopes Trends to a single Portfolio's Missions.</summary>
    public Guid? PortfolioId { get; set; }
}

/// <summary>
/// Query parameters for the side-by-side Portfolio comparison endpoint (BR-2202).
/// </summary>
public class PortfolioCompareQueryParameters
{
    public Guid? CompanyId { get; set; }

    public Guid LeftPortfolioId { get; set; }

    public Guid RightPortfolioId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }
}

/// <summary>
/// Internal read-only projection combining a Portfolio's stored health/snapshot fields with
/// mission-scoped Recommendation/Decision counts (BR-2205, BR-2206, BR-2208). Built once per
/// request by <see cref="AgencyOS.Application.Services.PortfolioAnalyticsService"/> and reused
/// by the pure, repository-free <c>PortfolioHealthAnalyticsService</c> and
/// <c>PortfolioRiskAnalyticsService</c> calculators — mirroring how
/// <c>IDashboardHealthCalculationService</c>/<c>IDashboardTrendService</c> never query a
/// repository directly.
/// </summary>
public class PortfolioAnalyticsSnapshot
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    /// <summary>The Portfolio's own stored <c>PortfolioHealth</c> value — never recalculated here (BR-2203).</summary>
    public string PortfolioHealth { get; set; } = string.Empty;

    public int MissionCount { get; set; }

    public IReadOnlyList<Guid> MissionIds { get; set; } = [];

    /// <summary>Parsed from <c>Portfolio.CapacitySummary</c> JSON when present (BR-2204).</summary>
    public decimal? UtilizationPercentage { get; set; }

    /// <summary>Parsed from <c>Portfolio.WorkloadSummary</c> JSON when present (BR-2204).</summary>
    public decimal? WorkloadPercentage { get; set; }

    public int RecommendationCount { get; set; }

    public decimal? AverageRecommendationScore { get; set; }

    public int DecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    public int CancelledDecisionCount { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

public class PortfolioAnalyticsCardResponse
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public HealthIndicator Health { get; set; } = new();

    public int MissionCount { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Read-only Portfolio Analytics overview (US-404 / BR-2201..BR-2210). One card per Portfolio
/// in the resolved Company, derived entirely from Portfolio snapshot fields and mission-scoped
/// Recommendation/Decision counts — nothing here is recalculated or persisted.
/// </summary>
public class PortfolioAnalyticsOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public int PortfolioCount { get; set; }

    public HealthIndicator OverallHealth { get; set; } = new();

    public IReadOnlyList<PortfolioAnalyticsCardResponse> Portfolios { get; set; } = [];

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Full read-only analysis for a single Portfolio (US-404 / BR-2201..BR-2210).
/// </summary>
public class PortfolioAnalyticsDetailResponse
{
    public Guid PortfolioId { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }

    public HealthIndicator Health { get; set; } = new();

    public int MissionCount { get; set; }

    public IReadOnlyList<Guid> MissionIds { get; set; } = [];

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public decimal? HistoricalAverageUtilizationPercentage { get; set; }

    public decimal? HistoricalAverageWorkloadPercentage { get; set; }

    public int HistoricalCapacityRecordCount { get; set; }

    public int HistoricalWorkloadRecordCount { get; set; }

    public int RecommendationCount { get; set; }

    public IReadOnlyList<StatusCountItem> RecommendationStatusBreakdown { get; set; } = [];

    public decimal? AverageRecommendationScore { get; set; }

    public int RecommendationHistoryCount { get; set; }

    public int DecisionCount { get; set; }

    public IReadOnlyList<StatusCountItem> DecisionStatusBreakdown { get; set; } = [];

    public int CompletedDecisionCount { get; set; }

    public int CancelledDecisionCount { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

public class PortfolioTrendPointResponse
{
    public string PeriodLabel { get; set; } = string.Empty;

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    /// <summary>Average Capacity History utilization for the bucket, when history exists (BR-2204, BR-2209).</summary>
    public decimal? CapacityUtilization { get; set; }

    /// <summary>Average Workload History utilization for the bucket, when history exists (BR-2204, BR-2209).</summary>
    public decimal? WorkloadUtilization { get; set; }

    /// <summary>Health band derived from the bucket's averages via <c>PortfolioHealth.Calculate</c> (BR-2203).</summary>
    public string? HealthStatus { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }
}

/// <summary>
/// Capacity/workload/health/recommendation/decision trend series bucketed by month
/// (US-404 / BR-2204..BR-2210). Optionally scoped to a single Portfolio's Missions.
/// </summary>
public class PortfolioTrendsResponse
{
    public Guid CompanyId { get; set; }

    public Guid? PortfolioId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public IReadOnlyList<PortfolioTrendPointResponse> Points { get; set; } = [];

    public string DrillDownPath { get; set; } = string.Empty;
}

public class PortfolioComparisonSideResponse
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public HealthIndicator Health { get; set; } = new();

    public int MissionCount { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public int RecommendationCount { get; set; }

    public decimal? AverageRecommendationScore { get; set; }

    public int DecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    public int CancelledDecisionCount { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

public class PortfolioComparisonFieldDiff
{
    public string Field { get; set; } = string.Empty;

    public string? LeftValue { get; set; }

    public string? RightValue { get; set; }

    public decimal? Delta { get; set; }
}

/// <summary>
/// Side-by-side comparison of two Portfolios using stored snapshot fields plus mission-scoped
/// Recommendation/Decision counts (US-404 / BR-2202).
/// </summary>
public class PortfolioComparisonResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public PortfolioComparisonSideResponse Left { get; set; } = new();

    public PortfolioComparisonSideResponse Right { get; set; } = new();

    public IReadOnlyList<PortfolioComparisonFieldDiff> FieldDiffs { get; set; } = [];
}

public class PortfolioRankingItemResponse
{
    public int Rank { get; set; }

    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public HealthIndicator Health { get; set; } = new();

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public decimal? RecommendationEffectivenessPercentage { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Portfolios ranked by health severity, snapshot utilization, and Recommendation/Decision
/// effectiveness (US-404 / BR-2210).
/// </summary>
public class PortfolioRankingResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public IReadOnlyList<PortfolioRankingItemResponse> Items { get; set; } = [];
}

public class PortfolioRiskIndicatorResponse
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public HealthIndicator Health { get; set; } = new();

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Health distribution and risk indicators across a Company's Portfolios (US-404 / BR-2203).
/// </summary>
public class PortfolioHealthAnalyticsResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public int PortfolioCount { get; set; }

    public IReadOnlyList<StatusCountItem> Distribution { get; set; } = [];

    public HealthIndicator OverallHealth { get; set; } = new();

    public int HealthyCount { get; set; }

    public int AtRiskCount { get; set; }

    public int OverloadedCount { get; set; }

    public int UnderutilizedCount { get; set; }

    public int UnknownCount { get; set; }

    public IReadOnlyList<PortfolioRiskIndicatorResponse> RiskIndicators { get; set; } = [];

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Effectiveness metrics across a Company's Portfolios (US-404 / BR-2205, BR-2206, BR-2210).
/// </summary>
public class PortfolioPerformanceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public int PortfolioCount { get; set; }

    public decimal AverageUtilizationPercentage { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }

    public decimal? AverageRecommendationScore { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }

    public decimal DecisionCompletionRatePercentage { get; set; }

    public decimal DecisionCancellationRatePercentage { get; set; }

    /// <summary>Completed decisions as a percentage of all decisions tied to Recommendations (approved/completed ratio, BR-2210).</summary>
    public decimal RecommendationEffectivenessPercentage { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

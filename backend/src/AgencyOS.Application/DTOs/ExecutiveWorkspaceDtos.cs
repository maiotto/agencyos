namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Executive Workspace (US-505 / BR-2801..BR-2810). Read-only; never
/// triggers recalculation of any existing engine and never writes operational data.
/// </summary>
public class ExecutiveWorkspaceQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (DEC-505-001).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound mapped onto <c>EnterpriseDashboardQueryParameters.From</c>. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound mapped onto <c>EnterpriseDashboardQueryParameters.To</c>. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Lower bound mapped onto <c>EnterpriseDashboardQueryParameters.PeriodStart</c>. Defaults to a trailing 30-day window.</summary>
    public DateOnly? PeriodStart { get; set; }

    /// <summary>Upper bound mapped onto <c>EnterpriseDashboardQueryParameters.PeriodEnd</c>. Defaults to a trailing 30-day window.</summary>
    public DateOnly? PeriodEnd { get; set; }
}

/// <summary>
/// Standing disclaimer surfaced across the Executive Workspace (BR-2801..BR-2810). The Workspace
/// never changes this guarantee — it only projects and links to existing, unchanged dashboards
/// and operational workspaces.
/// </summary>
public static class ExecutiveWorkspaceDisclaimers
{
    /// <summary>DEC-505-001: the Executive Workspace is a read-only orchestration façade.</summary>
    public const string ReadOnlyOrchestration =
        "Executive Workspace consolidates existing enterprise signals. Read-only. Drill-down opens "
        + "operational workspaces and dashboards without modifying data.";
}

/// <summary>
/// A single Executive Workspace navigation target (US-505 / BR-2807: the Workspace supports
/// drill-down navigation into every existing operational workspace and dashboard). Every action
/// is a deep-link to an existing, already-audited frontend route — the Workspace never introduces
/// a new write path (DEC-505-001).
/// </summary>
public class ExecutiveActionResponse
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    /// <summary>One of Overview|Planning|Recommendations|Decisions|Capacity|Workload|AI|Audit|Portfolios.</summary>
    public string Category { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>True for deep-links into advisory-only capabilities (e.g. Cross-Portfolio Planning).</summary>
    public bool IsAdvisory { get; set; }
}

/// <summary>Ordered Executive Actions/navigation nodes exposed by the Workspace (BR-2807).</summary>
public class ExecutiveNavigationResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<ExecutiveActionResponse> Actions { get; set; } = [];
}

/// <summary>
/// Company-wide Executive KPI rollup. Every figure is a pure projection over the existing
/// <see cref="EnterpriseDashboardSummaryResponse"/> (plus the Capacity/Workload/Audit sections
/// when supplied) — never a new calculation and never a repository call (BR-2802, BR-2809).
/// </summary>
public class ExecutiveKpiSummaryResponse
{
    public int PortfolioCount { get; set; }

    public int ActivePortfolioCount { get; set; }

    public int RecommendationCount { get; set; }

    public int DecisionCount { get; set; }

    public int PendingDecisionCount { get; set; }

    public int CompletedDecisionCount { get; set; }

    public decimal CapacityUtilizationPercentage { get; set; }

    public decimal WorkloadPercentage { get; set; }

    public int AuditEventCount { get; set; }

    public HealthIndicator OverallHealth { get; set; } = new();
}

/// <summary>
/// Read-only Executive Workspace overview (US-505 / DEC-505-001). Combines KPIs, overall health
/// (reused from <see cref="IDashboardHealthCalculationService"/> — BR-2805), a short narrative,
/// and a navigation tip — never mutates any operational data.
/// </summary>
public class ExecutiveOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public ExecutiveKpiSummaryResponse Kpis { get; set; } = new();

    public HealthIndicator OverallHealth { get; set; } = new();

    public string Narrative { get; set; } = string.Empty;

    public string NavigationTip { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = "/executive-workspace";
}

/// <summary>Enterprise section: embeds the existing Enterprise Dashboard Summary (BR-2809 — no duplicate analytical data).</summary>
public class ExecutiveEnterpriseSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardSummaryResponse Summary { get; set; } = new();

    public string DrillDownPath { get; set; } = "/enterprise-dashboard";
}

/// <summary>Portfolios section: embeds the existing Enterprise Dashboard Portfolio rollup, linking to Portfolio Analytics.</summary>
public class ExecutivePortfoliosSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardPortfolioResponse Portfolio { get; set; } = new();

    public string DrillDownPath { get; set; } = "/portfolio-analytics";
}

/// <summary>Recommendations section: embeds the existing Enterprise Dashboard Recommendations rollup, linking to the Recommendation Workspace.</summary>
public class ExecutiveRecommendationsSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardRecommendationsResponse Recommendations { get; set; } = new();

    public string DrillDownPath { get; set; } = "/recommendation-workspace";
}

/// <summary>Decisions section: embeds the existing Enterprise Dashboard Decisions rollup, linking to the Decision Workspace.</summary>
public class ExecutiveDecisionsSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardDecisionsResponse Decisions { get; set; } = new();

    public string DrillDownPath { get; set; } = "/decision-workspace";
}

/// <summary>Capacity section: embeds the existing, immutable Enterprise Dashboard Capacity rollup, linking to Capacity History.</summary>
public class ExecutiveCapacitySectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardCapacityResponse Capacity { get; set; } = new();

    public string DrillDownPath { get; set; } = "/capacity/history";
}

/// <summary>Workload section: embeds the existing, immutable Enterprise Dashboard Workload rollup, linking to Workload History.</summary>
public class ExecutiveWorkloadSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardWorkloadResponse Workload { get; set; } = new();

    public string DrillDownPath { get; set; } = "/workload/history";
}

/// <summary>AI section: embeds the existing Enterprise Dashboard AI rollup, linking to AI Recommendations (advisory).</summary>
public class ExecutiveAiSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardAiResponse Ai { get; set; } = new();

    public string DrillDownPath { get; set; } = "/ai-recommendations";
}

/// <summary>Audit section: embeds the existing, immutable Enterprise Dashboard Audit rollup, linking to the Audit Trail (BR-2810).</summary>
public class ExecutiveAuditSectionResponse
{
    public Guid CompanyId { get; set; }

    public EnterpriseDashboardAuditResponse Audit { get; set; } = new();

    public string DrillDownPath { get; set; } = "/audit";
}

/// <summary>
/// Full, read-only Executive Workspace (US-505 / BR-2801..BR-2810; DEC-505-001). A read-only
/// orchestration façade that reuses <see cref="IEnterpriseDashboardService"/> section methods and
/// exposes executive navigation deep-links into every existing operational workspace and
/// dashboard — introduces no new persistence and never modifies operational data.
/// </summary>
public class ExecutiveWorkspaceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public ExecutiveKpiSummaryResponse Kpis { get; set; } = new();

    public ExecutiveOverviewResponse Overview { get; set; } = new();

    public ExecutiveEnterpriseSectionResponse Enterprise { get; set; } = new();

    public ExecutivePortfoliosSectionResponse Portfolios { get; set; } = new();

    public ExecutiveRecommendationsSectionResponse Recommendations { get; set; } = new();

    public ExecutiveDecisionsSectionResponse Decisions { get; set; } = new();

    public ExecutiveCapacitySectionResponse Capacity { get; set; } = new();

    public ExecutiveWorkloadSectionResponse Workload { get; set; } = new();

    public ExecutiveAiSectionResponse Ai { get; set; } = new();

    public ExecutiveAuditSectionResponse Audit { get; set; } = new();

    public ExecutiveNavigationResponse Navigation { get; set; } = new();
}

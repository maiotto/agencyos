namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Planning Workspace (US-502 / BR-2501..BR-2510). Read-only;
/// never triggers recalculation or persistence of Capacity/Workload/Portfolio/Scenario data.
/// </summary>
public class PlanningWorkspaceQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (DEC-502-001).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound applied to Planning History (Audit) date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to Planning History (Audit) date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Maps to Capacity/Workload/Portfolio/Scenario period filters. Defaults to the same calendar window as From/To.</summary>
    public DateOnly? PeriodStart { get; set; }

    /// <summary>Maps to Capacity/Workload/Portfolio/Scenario period filters. Defaults to the same calendar window as From/To.</summary>
    public DateOnly? PeriodEnd { get; set; }
}

/// <summary>
/// Company-wide planning KPI rollup. Every figure is a projection over existing
/// Planning Template/Portfolio/Capacity/Workload/Cross-Portfolio data (BR-2501, BR-2505).
/// </summary>
public class PlanningKpiSummaryResponse
{
    public int TemplateCount { get; set; }

    public int ActiveTemplateCount { get; set; }

    public int PortfolioCount { get; set; }

    public int ActivePortfolioCount { get; set; }

    public int ScenarioCount { get; set; }

    public int PlanningHistoryEventCount { get; set; }

    public decimal AverageUtilizationPercentage { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }

    public HealthIndicator OverallHealth { get; set; } = new();
}

/// <summary>
/// Read-only Planning Workspace overview (US-502 / DEC-502-001). Aggregates counts from
/// Planning Templates, Portfolios, Capacity/Workload History, and Cross-Portfolio Planning
/// scenarios — never recalculates the underlying engines and never persists a Scenario.
/// </summary>
public class PlanningOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public PlanningKpiSummaryResponse Kpis { get; set; } = new();

    public int InactiveTemplateCount { get; set; }

    public IReadOnlyList<StatusCountItem> PortfolioHealthBreakdown { get; set; } = [];

    public int CapacityHistoryRecordCount { get; set; }

    public int WorkloadHistoryRecordCount { get; set; }

    /// <summary>BR-2507: Cross-Portfolio Planning remains advisory only — surfaced for every consumer of the Overview.</summary>
    public string CrossPortfolioAdvisoryDisclaimer { get; set; } =
        "Cross-Portfolio Planning is advisory only. Scenarios are temporary and require human approval; nothing is executed automatically.";

    public string DrillDownPath { get; set; } = "/planning-workspace";
}

/// <summary>Thin, read-only Planning Template projection for the Workspace (BR-2506: Templates remain source of configuration).</summary>
public class PlanningTemplateCardResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string ResourceAvailabilityStrategy { get; set; } = string.Empty;

    public int DefaultPlanningWindowDays { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Planning Templates section: thin Template cards plus the "Manage Templates" Planning Action.</summary>
public class PlanningTemplatesSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<PlanningTemplateCardResponse> Templates { get; set; } = [];

    public PlanningActionResponse ManageAction { get; set; } = new();

    public PlanningActionResponse ApplyAction { get; set; } = new();
}

/// <summary>
/// Capacity section built entirely from immutable Capacity History (never recalculated for storage, BR-2503).
/// </summary>
public class PlanningCapacitySectionResponse
{
    public Guid CompanyId { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int RecordCount { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal AverageUtilizationPercentage { get; set; }

    public HealthIndicator UtilizationHealth { get; set; } = new();

    /// <summary>Most recent Capacity History records for the period (read-only snippet, no recalculation).</summary>
    public IReadOnlyList<CapacityHistoryResponse> RecentHistory { get; set; } = [];

    public PlanningActionResponse Action { get; set; } = new();
}

/// <summary>
/// Workload section built entirely from immutable Workload History (never recalculated for storage, BR-2503).
/// </summary>
public class PlanningWorkloadSectionResponse
{
    public Guid CompanyId { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int RecordCount { get; set; }

    public decimal TotalAllocatedHours { get; set; }

    public decimal TotalCapacityHours { get; set; }

    public decimal AverageWorkloadPercentage { get; set; }

    public HealthIndicator WorkloadHealth { get; set; } = new();

    /// <summary>Most recent Workload History records for the period (read-only snippet, no recalculation).</summary>
    public IReadOnlyList<WorkloadHistoryResponse> RecentHistory { get; set; } = [];

    public PlanningActionResponse Action { get; set; } = new();
}

/// <summary>Portfolio Planning card: utilization/workload parsed from the already-persisted snapshot JSON (BR-2503).</summary>
public class PlanningPortfolioCardResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string PortfolioHealth { get; set; } = string.Empty;

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Portfolios section: Portfolio cards plus the "View Portfolios" Planning Action.</summary>
public class PlanningPortfoliosSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<PlanningPortfolioCardResponse> Portfolios { get; set; } = [];

    public PlanningActionResponse Action { get; set; } = new();
}

/// <summary>
/// Planning-related Audit Trail entry (BR-2510: the Workspace is fully auditable). Never a new write
/// path — this is a read-only projection over <see cref="IAuditQueryService"/>.
/// </summary>
public class PlanningHistoryItemResponse
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Planning History (Audit Trail) timeline scoped to planning-related entity types.</summary>
public class PlanningHistoryResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public IReadOnlyList<PlanningHistoryItemResponse> Items { get; set; } = [];

    public PlanningActionResponse Action { get; set; } = new();
}

/// <summary>
/// Temporary, advisory-only Cross-Portfolio Planning scenario summary (BR-2507; DEC-405-001).
/// Never persisted by the Planning Workspace — Scenarios remain the in-memory store owned by
/// <c>ICrossPortfolioScenarioStore</c>.
/// </summary>
public class PlanningScenarioSummaryResponse
{
    public Guid ScenarioId { get; set; }

    public string? ScenarioName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int PortfolioCount { get; set; }

    public decimal? AverageUtilizationPercentage { get; set; }

    public decimal? AverageWorkloadPercentage { get; set; }

    public int ConflictCount { get; set; }

    /// <summary>Always true — Cross-Portfolio Planning never executes automatically (BR-2507).</summary>
    public bool RequiresHumanApproval { get; set; } = true;

    /// <summary>Always true — surfaced on every Scenario summary for BR-2507.</summary>
    public bool AdvisoryOnly { get; set; } = true;
}

/// <summary>Scenarios section: temporary scenario list plus the Cross-Portfolio Planning comparison action.</summary>
public class PlanningScenariosSectionResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public IReadOnlyList<PlanningScenarioSummaryResponse> Scenarios { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public bool AdvisoryOnly { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } =
        "Cross-Portfolio Planning is advisory only. Scenarios are temporary and require human approval; nothing is executed automatically.";

    public PlanningActionResponse ComparisonAction { get; set; } = new();
}

/// <summary>
/// A single Planning Workspace navigation target (US-502 / BR-2508: the Workspace supports
/// drill-down navigation). Every action is a deep-link to an existing, already-audited
/// frontend route/API — the Workspace never introduces a new write path (DEC-502-001).
/// </summary>
public class PlanningActionResponse
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    /// <summary>One of Templates|Capacity|Workload|Portfolio|Scenarios|History|Overview.</summary>
    public string Category { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>True for Cross-Portfolio Planning — advisory only (BR-2507).</summary>
    public bool IsAdvisory { get; set; }
}

/// <summary>Ordered Planning Actions/navigation nodes exposed by the Workspace (BR-2508).</summary>
public class PlanningNavigationResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<PlanningActionResponse> Actions { get; set; } = [];
}

/// <summary>
/// Full, read-only Planning Workspace (US-502 / BR-2501..BR-2510; DEC-502-001). Orchestrates
/// existing Planning Template, Portfolio, Capacity/Workload History, and Cross-Portfolio
/// Planning capabilities from a single response — introduces no new persistence, never
/// recalculates an engine for storage, and never mutates a Portfolio/Template/Scenario.
/// </summary>
public class PlanningWorkspaceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public PlanningKpiSummaryResponse Kpis { get; set; } = new();

    public PlanningOverviewResponse Overview { get; set; } = new();

    public PlanningTemplatesSectionResponse Templates { get; set; } = new();

    public PlanningCapacitySectionResponse Capacity { get; set; } = new();

    public PlanningWorkloadSectionResponse Workload { get; set; } = new();

    public PlanningPortfoliosSectionResponse Portfolios { get; set; } = new();

    public PlanningHistoryResponse History { get; set; } = new();

    public PlanningScenariosSectionResponse Scenarios { get; set; } = new();

    public PlanningNavigationResponse Navigation { get; set; } = new();
}

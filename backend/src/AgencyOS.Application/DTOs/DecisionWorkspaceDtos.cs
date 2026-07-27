namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Decision Workspace (US-504 / BR-2701..BR-2710). Read-only; never
/// triggers Decision Engine recalculation, never calls a Decision lifecycle write API, and never
/// bypasses mandatory human approval for a Decision action.
/// </summary>
public class DecisionWorkspaceQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (DEC-504-001).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound applied to Decision/Outcome/Audit date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to Decision/Outcome/Audit date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Optional Decision to focus the Timeline section on. When unset, the Timeline section aggregates recent Decisions instead.</summary>
    public Guid? DecisionId { get; set; }
}

/// <summary>
/// Standing disclaimers surfaced across every Decision Workspace response (BR-2701..BR-2710).
/// The Workspace never changes these guarantees — it only projects and links to the existing,
/// unchanged Decision lifecycle, Recommendation linkage, Timeline, and Audit capabilities.
/// </summary>
public static class DecisionWorkspaceDisclaimers
{
    /// <summary>DEC-504-001: every Decision lifecycle action is a navigation deep-link — human approval is mandatory.</summary>
    public const string HumanApproval =
        "Create Decision, Start Implementation, Complete, Cancel, and Record Outcome always require explicit human "
        + "action on the existing Decision pages. The Decision Workspace never creates, starts, completes, cancels, "
        + "or records an outcome for a Decision automatically.";

    /// <summary>BR-2703/BR-2704: the Decision Timeline and Decision Audit trail are immutable.</summary>
    public const string TimelineImmutable =
        "The Decision Timeline and Decision Audit trail are immutable, append-only records. The Decision Workspace "
        + "never edits or deletes a Timeline entry or Audit event.";
}

/// <summary>
/// A single Decision Workspace navigation target (US-504 / BR-2707: the Workspace supports
/// drill-down navigation). Every action is a deep-link to an existing, already-audited frontend
/// route — the Workspace never introduces a new write path (DEC-504-001).
/// </summary>
public class DecisionActionResponse
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    /// <summary>One of Decisions|Lifecycle|Outcomes|Recommendations|Dashboard|Audit.</summary>
    public string Category { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>True for Create/Start/Complete/Cancel/Record Outcome — human approval is mandatory (DEC-504-001).</summary>
    public bool RequiresHumanApproval { get; set; }
}

/// <summary>Ordered Decision Actions/navigation nodes exposed by the Workspace (BR-2707).</summary>
public class DecisionNavigationResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<DecisionActionResponse> Actions { get; set; } = [];
}

/// <summary>
/// Company-wide Decision Workspace KPI rollup. Every figure is a projection over existing
/// Decision data (BR-2701, BR-2708). Independent DecisionStatus/ImplementationStatus counts
/// mirror BR-1403 (the two status dimensions are independent).
/// </summary>
public class DecisionKpiSummaryResponse
{
    public int TotalCount { get; set; }

    public int PendingCount { get; set; }

    public int InProgressCount { get; set; }

    public int CompletedCount { get; set; }

    public int CancelledCount { get; set; }

    public int WithOutcomeCount { get; set; }

    public int ImplementationNotStartedCount { get; set; }
}

/// <summary>
/// Read-only Decision Workspace overview (US-504 / DEC-504-001). Aggregates counts from existing
/// Decisions for the Company and period — never mutates Decision data and never bypasses
/// mandatory human approval for a Decision lifecycle action.
/// </summary>
public class DecisionOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public DecisionKpiSummaryResponse Kpis { get; set; } = new();

    /// <summary>Always true — surfaced on the Overview as a standing disclaimer (DEC-504-001).</summary>
    public bool RequiresHumanApproval { get; set; } = true;

    public string HumanApprovalDisclaimer { get; set; } = DecisionWorkspaceDisclaimers.HumanApproval;

    public string DrillDownPath { get; set; } = "/decision-workspace";
}

/// <summary>Thin, read-only Decision projection for the Workspace (BR-2701, BR-2707 drill-down).</summary>
public class DecisionCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid MissionId { get; set; }

    public Guid ContractId { get; set; }

    public string DecisionStatus { get; set; } = string.Empty;

    public string ImplementationStatus { get; set; } = string.Empty;

    public DateTimeOffset DecisionDate { get; set; }

    public DateTimeOffset? ImplementationDate { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    public string? Outcome { get; set; }

    public string? BusinessValue { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    public string RecommendationDrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Decisions section: Decision cards grouped by DecisionStatus, plus navigation actions (all
/// deep-links; DEC-504-001). Cancelled Decisions are included for completeness but are not a
/// primary queue.
/// </summary>
public class DecisionsSectionResponse
{
    public Guid CompanyId { get; set; }

    public int TotalCount { get; set; }

    public IReadOnlyList<DecisionCardResponse> Pending { get; set; } = [];

    public IReadOnlyList<DecisionCardResponse> InProgress { get; set; } = [];

    public IReadOnlyList<DecisionCardResponse> Completed { get; set; } = [];

    public IReadOnlyList<DecisionCardResponse> Cancelled { get; set; } = [];

    public DecisionActionResponse ListAction { get; set; } = new();

    public DecisionActionResponse CreateAction { get; set; } = new();
}

/// <summary>
/// Immutable Decision Timeline entry projection (BR-2703: the Decision Timeline is immutable —
/// the Workspace never edits or deletes it).
/// </summary>
public class DecisionTimelineItemResponse
{
    public Guid DecisionId { get; set; }

    public Guid RecommendationId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string FromDecisionStatus { get; set; } = string.Empty;

    public string ToDecisionStatus { get; set; } = string.Empty;

    public string FromImplementationStatus { get; set; } = string.Empty;

    public string ToImplementationStatus { get; set; } = string.Empty;

    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Timeline section: either the full, immutable Timeline for a single focused Decision
/// (<see cref="DecisionWorkspaceQueryParameters.DecisionId"/>), or a bounded, aggregated view of
/// the most recent Timeline entries across recent Decisions for the Company (BR-2703).
/// </summary>
public class DecisionTimelineSectionResponse
{
    public Guid CompanyId { get; set; }

    public Guid? DecisionId { get; set; }

    public IReadOnlyList<DecisionTimelineItemResponse> Items { get; set; } = [];

    public DecisionActionResponse Action { get; set; } = new();
}

/// <summary>Thin, read-only Decision Outcome projection (BR-2705: outcome recording follows existing rules).</summary>
public class DecisionOutcomeCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public string DecisionStatus { get; set; } = string.Empty;

    public string ImplementationStatus { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string? BusinessValue { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;

    public string RecommendationDrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Outcomes section: Decisions with a recorded, non-empty Outcome plus a navigation-only Record
/// Outcome action (BR-2705: the Workspace never calls RecordOutcomeAsync itself).
/// </summary>
public class DecisionOutcomesSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<DecisionOutcomeCardResponse> Outcomes { get; set; } = [];

    public DecisionActionResponse RecordOutcomeAction { get; set; } = new();
}

/// <summary>Thin, read-only Decision Audit projection (BR-2704: the Decision Audit trail is immutable).</summary>
public class DecisionAuditItemResponse
{
    public Guid Id { get; set; }

    public Guid EntityId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Audit section: immutable Decision Audit events for the Company and period (BR-2704).</summary>
public class DecisionAuditSectionResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public IReadOnlyList<DecisionAuditItemResponse> Items { get; set; } = [];

    public DecisionActionResponse Action { get; set; } = new();
}

/// <summary>
/// Full, read-only Decision Workspace (US-504 / BR-2701..BR-2710; DEC-504-001). Orchestrates the
/// existing Decision lifecycle, Recommendation linkage, Decision Timeline, Decision Outcome, and
/// Decision Audit capabilities from a single response — introduces no new persistence, never
/// calls CreateAsync/StartImplementationAsync/CompleteAsync/CancelAsync/RecordOutcomeAsync, and
/// never bypasses mandatory human approval.
/// </summary>
public class DecisionWorkspaceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public Guid? DecisionId { get; set; }

    public DecisionKpiSummaryResponse Kpis { get; set; } = new();

    public DecisionOverviewResponse Overview { get; set; } = new();

    public DecisionsSectionResponse Decisions { get; set; } = new();

    public DecisionTimelineSectionResponse Timeline { get; set; } = new();

    public DecisionOutcomesSectionResponse Outcomes { get; set; } = new();

    public DecisionAuditSectionResponse Audit { get; set; } = new();

    public DecisionNavigationResponse Navigation { get; set; } = new();
}

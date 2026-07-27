namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for the Recommendation Workspace (US-503 / BR-2601..BR-2610). Read-only;
/// never triggers Decision Engine recalculation, never persists a Recommendation/Workflow/
/// AI/Explainability/Executive Summary, and never approves or rejects anything on behalf of
/// a human.
/// </summary>
public class RecommendationWorkspaceQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c> (DEC-503-001).</summary>
    public Guid? CompanyId { get; set; }

    /// <summary>Lower bound applied to Recommendation History/AI/Explainability/Executive Summary date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Upper bound applied to Recommendation History/AI/Explainability/Executive Summary date filters. Defaults to a trailing 30-day window.</summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>Optional left side of a Recommendation comparison. Must be paired with <see cref="RightRecommendationId"/>.</summary>
    public Guid? LeftRecommendationId { get; set; }

    /// <summary>Optional right side of a Recommendation comparison. Must be paired with <see cref="LeftRecommendationId"/> and distinct from it.</summary>
    public Guid? RightRecommendationId { get; set; }
}

/// <summary>
/// Standing disclaimers surfaced across every Recommendation Workspace response (BR-2604,
/// BR-2605, BR-2606). The Workspace never changes these guarantees — it only projects and
/// links to the existing, unchanged Decision Engine, Recommendation lifecycle, AI, and
/// Explainability capabilities.
/// </summary>
public static class RecommendationWorkspaceDisclaimers
{
    /// <summary>BR-2604: human approval is mandatory — the Workspace never auto-approves or auto-rejects.</summary>
    public const string HumanApproval =
        "Approving or rejecting a Recommendation always requires explicit human action in the Approval Workflow. The Recommendation Workspace never approves, rejects, or auto-executes anything.";

    /// <summary>BR-2605: AI Recommendations remain advisory only.</summary>
    public const string AiAdvisory =
        "AI Recommendations are advisory only. They do not alter Decision Engine output, Recommendation ranking, or workflow status.";

    /// <summary>BR-2606: Explainability remains informational only.</summary>
    public const string ExplainabilityInformational =
        "Explainability is informational only. It does not influence Recommendation ranking, approval workflow status, or any automated action.";
}

/// <summary>
/// A single Recommendation Workspace navigation target (US-503 / BR-2607: the Workspace
/// supports drill-down navigation). Every action is a deep-link to an existing, already-audited
/// frontend route/API — the Workspace never introduces a new write path (DEC-503-001).
/// </summary>
public class RecommendationActionResponse
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    /// <summary>One of Recommendations|Approval|History|Compare|AI|Explainability|ExecutiveSummary|Decisions|Audit.</summary>
    public string Category { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>True for Approve/Reject — human approval is mandatory (BR-2604).</summary>
    public bool RequiresHumanApproval { get; set; }

    /// <summary>True for AI Recommendation actions — advisory only (BR-2605).</summary>
    public bool IsAdvisory { get; set; }

    /// <summary>True for Explainability actions — informational only (BR-2606).</summary>
    public bool IsInformational { get; set; }
}

/// <summary>Ordered Recommendation Actions/navigation nodes exposed by the Workspace (BR-2607).</summary>
public class RecommendationNavigationResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<RecommendationActionResponse> Actions { get; set; } = [];
}

/// <summary>
/// Company-wide Recommendation Workspace KPI rollup. Every figure is a projection over existing
/// Recommendation/Workflow/AI/Explainability/Executive Summary/History data (BR-2601, BR-2608).
/// </summary>
public class RecommendationKpiSummaryResponse
{
    public int ActiveRecommendationCount { get; set; }

    public int ArchivedRecommendationCount { get; set; }

    public int PendingApprovalCount { get; set; }

    public int AiRecommendationCount { get; set; }

    public int ExplainabilityCount { get; set; }

    public int ExecutiveSummaryCount { get; set; }

    public int HistoryEventCount { get; set; }
}

/// <summary>
/// Read-only Recommendation Workspace overview (US-503 / DEC-503-001). Aggregates counts from
/// Recommendations, Recommendation Workflows, AI Recommendations, Explainability, Executive
/// Summaries, and Recommendation History — never mutates any of them and never bypasses human
/// approval (BR-2604).
/// </summary>
public class RecommendationOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public RecommendationKpiSummaryResponse Kpis { get; set; } = new();

    /// <summary>Always true — surfaced on the Overview as a standing disclaimer (BR-2604).</summary>
    public bool RequiresHumanApproval { get; set; } = true;

    public string HumanApprovalDisclaimer { get; set; } = RecommendationWorkspaceDisclaimers.HumanApproval;

    public string DrillDownPath { get; set; } = "/recommendation-workspace";
}

/// <summary>Thin, read-only Recommendation projection for the Workspace (BR-2601, BR-2608 drill-down).</summary>
public class RecommendationCardResponse
{
    public Guid Id { get; set; }

    public string RecommendationNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal? Score { get; set; }

    public int? Rank { get; set; }

    public int Version { get; set; }

    public bool Archived { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Recommendations section: active Recommendation cards plus navigation actions (all deep-links; DEC-503-001).</summary>
public class RecommendationsSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<RecommendationCardResponse> Recommendations { get; set; } = [];

    public RecommendationActionResponse ListAction { get; set; } = new();

    public RecommendationActionResponse GenerateAction { get; set; } = new();

    public RecommendationActionResponse ArchiveRestoreAction { get; set; } = new();
}

/// <summary>
/// Approval Workflow card summarizing a PendingApproval <c>RecommendationWorkflow</c>
/// (BR-2604: human approval mandatory). Approve/Reject are navigation deep-links only.
/// </summary>
public class RecommendationApprovalCardResponse
{
    public Guid WorkflowId { get; set; }

    public Guid RecommendationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;

    public string ApprovePath { get; set; } = string.Empty;

    public string RejectPath { get; set; } = string.Empty;

    /// <summary>Always true — approval/rejection always requires explicit human action (BR-2604).</summary>
    public bool RequiresHumanApproval { get; set; } = true;
}

/// <summary>Approval section: PendingApproval Workflow cards plus navigation actions (BR-2604).</summary>
public class ApprovalSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<RecommendationApprovalCardResponse> PendingApprovals { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public string HumanApprovalDisclaimer { get; set; } = RecommendationWorkspaceDisclaimers.HumanApproval;

    public RecommendationActionResponse ApprovalQueueAction { get; set; } = new();

    public RecommendationActionResponse StartWorkflowAction { get; set; } = new();
}

/// <summary>
/// Recommendation History (immutable) entry projection (BR-2609: history is immutable — the
/// Workspace never edits or deletes it).
/// </summary>
public class RecommendationHistoryCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public string RecommendationNumber { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string RecommendationStatus { get; set; } = string.Empty;

    public string? WorkflowStatus { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>History section: recent, immutable Recommendation History items plus navigation actions (BR-2609).</summary>
public class HistorySectionResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public IReadOnlyList<RecommendationHistoryCardResponse> Items { get; set; } = [];

    public RecommendationActionResponse Action { get; set; } = new();
}

/// <summary>
/// Compare section: an optional side-by-side Recommendation comparison (BR-2608 drill-down).
/// When either id is missing, or both are equal, no comparison is computed and only the
/// navigation stub to the full comparison page is returned.
/// </summary>
public class CompareSectionResponse
{
    public Guid CompanyId { get; set; }

    public Guid? LeftRecommendationId { get; set; }

    public Guid? RightRecommendationId { get; set; }

    public bool HasComparison { get; set; }

    public RecommendationComparisonResponse? Comparison { get; set; }

    public bool RequiresHumanApproval { get; set; } = true;

    public RecommendationActionResponse Action { get; set; } = new();
}

/// <summary>Thin, read-only AI Recommendation projection (BR-2605: AI is advisory only).</summary>
public class AIRecommendationCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public decimal ConfidenceScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public string DrillDownPath { get; set; } = string.Empty;

    /// <summary>Always true — AI Recommendations are advisory only (BR-2605).</summary>
    public bool IsAdvisory { get; set; } = true;
}

/// <summary>Thin, read-only Explainability projection (BR-2606: Explainability is informational only).</summary>
public class ExplainabilityCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public string ExplanationType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;

    /// <summary>Always true — Explainability is informational only (BR-2606).</summary>
    public bool IsInformational { get; set; } = true;
}

/// <summary>AI/Explainability section: AI Recommendation cards (advisory) plus nested Explainability cards (informational).</summary>
public class AiSectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<AIRecommendationCardResponse> AiRecommendations { get; set; } = [];

    public IReadOnlyList<ExplainabilityCardResponse> Explainability { get; set; } = [];

    /// <summary>Always true — surfaced for the AI Recommendation list (BR-2605).</summary>
    public bool IsAdvisory { get; set; } = true;

    /// <summary>Always true — surfaced for the Explainability list (BR-2606).</summary>
    public bool IsInformational { get; set; } = true;

    public string AiAdvisoryDisclaimer { get; set; } = RecommendationWorkspaceDisclaimers.AiAdvisory;

    public string ExplainabilityInformationalDisclaimer { get; set; } =
        RecommendationWorkspaceDisclaimers.ExplainabilityInformational;

    public RecommendationActionResponse AiListAction { get; set; } = new();

    public RecommendationActionResponse GenerateAiAction { get; set; } = new();

    public RecommendationActionResponse ExplainabilityListAction { get; set; } = new();

    public RecommendationActionResponse GenerateExplainabilityAction { get; set; } = new();
}

/// <summary>Thin, read-only Executive Recommendation Summary projection.</summary>
public class ExecutiveSummaryCardResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public int SummaryVersion { get; set; }

    public decimal ConfidenceLevel { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>Executive Summary section: summary cards plus navigation actions.</summary>
public class ExecutiveSummarySectionResponse
{
    public Guid CompanyId { get; set; }

    public IReadOnlyList<ExecutiveSummaryCardResponse> Summaries { get; set; } = [];

    public RecommendationActionResponse ListAction { get; set; } = new();

    public RecommendationActionResponse GenerateAction { get; set; } = new();
}

/// <summary>
/// Full, read-only Recommendation Workspace (US-503 / BR-2601..BR-2610; DEC-503-001).
/// Orchestrates existing Recommendation, Recommendation Workflow, AI Recommendation,
/// Explainability, Executive Summary, Recommendation History, and Recommendation Comparison
/// capabilities from a single response — introduces no new persistence, never calls a
/// Create/Approve/Reject/Archive/Restore/Generate write API, and never bypasses mandatory
/// human approval.
/// </summary>
public class RecommendationWorkspaceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public RecommendationKpiSummaryResponse Kpis { get; set; } = new();

    public RecommendationOverviewResponse Overview { get; set; } = new();

    public RecommendationsSectionResponse Recommendations { get; set; } = new();

    public ApprovalSectionResponse Approval { get; set; } = new();

    public HistorySectionResponse History { get; set; } = new();

    public CompareSectionResponse Compare { get; set; } = new();

    public AiSectionResponse Ai { get; set; } = new();

    public ExecutiveSummarySectionResponse ExecutiveSummary { get; set; } = new();

    public RecommendationNavigationResponse Navigation { get; set; } = new();
}

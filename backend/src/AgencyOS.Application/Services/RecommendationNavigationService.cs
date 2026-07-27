using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure, read-only Recommendation Workspace navigation (US-503 / BR-2607). Every action is a
/// deep-link to an existing, already-audited frontend route — the Workspace itself only
/// aggregates and navigates (DEC-503-001); it never introduces a new write path.
/// </summary>
public class RecommendationNavigationService : IRecommendationNavigationService
{
    public RecommendationNavigationResponse GetNavigation(Guid companyId)
    {
        var actions = new List<RecommendationActionResponse>
        {
            new()
            {
                Key = "list-recommendations",
                Label = "List Recommendations",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Browse Recommendations for this Company."
            },
            new()
            {
                Key = "generate-recommendations",
                Label = "Generate Recommendations",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Recommendations are published by the Decision Engine ranking process, which runs outside the Workspace. Use the Recommendations list to review newly published Recommendations."
            },
            new()
            {
                Key = "approval-queue",
                Label = "Approval Queue",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow",
                Description = "Review Recommendation Workflows awaiting approval."
            },
            new()
            {
                Key = "start-workflow",
                Label = "Start Workflow",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow/new",
                Description = "Start a new Recommendation approval Workflow."
            },
            new()
            {
                Key = "approve",
                Label = "Approve",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow",
                Description = "Approving a Recommendation Workflow always requires explicit human action (BR-2604).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "reject",
                Label = "Reject",
                Category = "Approval",
                DrillDownPath = "/recommendations/workflow",
                Description = "Rejecting a Recommendation Workflow always requires explicit human action (BR-2604).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "history",
                Label = "History",
                Category = "History",
                DrillDownPath = "/recommendations/history",
                Description = "Browse the immutable Recommendation History timeline (BR-2609)."
            },
            new()
            {
                Key = "compare",
                Label = "Compare",
                Category = "Compare",
                DrillDownPath = "/recommendations/compare",
                Description = "Compare two Recommendation History snapshots side by side."
            },
            new()
            {
                Key = "archive-restore",
                Label = "Archive / Restore",
                Category = "Recommendations",
                DrillDownPath = $"/recommendations?companyId={companyId}",
                Description = "Select a Recommendation from the list, then archive or restore it via its own detail actions."
            },
            new()
            {
                Key = "ai-recommendations",
                Label = "AI Recommendations",
                Category = "AI",
                DrillDownPath = $"/ai-recommendations?companyId={companyId}",
                Description = "Browse advisory AI Recommendations (BR-2605).",
                IsAdvisory = true
            },
            new()
            {
                Key = "generate-ai",
                Label = "Generate AI",
                Category = "AI",
                DrillDownPath = "/ai-recommendations/generate",
                Description = "Generate an advisory AI Recommendation for a Recommendation. Advisory only — never modifies Decision Engine output (BR-2605).",
                IsAdvisory = true
            },
            new()
            {
                Key = "explainability",
                Label = "Explainability",
                Category = "Explainability",
                DrillDownPath = $"/explainability?companyId={companyId}",
                Description = "Browse informational Explainability records (BR-2606).",
                IsInformational = true
            },
            new()
            {
                Key = "generate-explainability",
                Label = "Generate Explainability",
                Category = "Explainability",
                DrillDownPath = "/explainability/generate",
                Description = "Generate an informational Explainability record for a Recommendation. Informational only — never influences ranking or approval (BR-2606).",
                IsInformational = true
            },
            new()
            {
                Key = "executive-summaries",
                Label = "Executive Summaries",
                Category = "ExecutiveSummary",
                DrillDownPath = $"/executive-summaries?companyId={companyId}",
                Description = "Browse Executive Recommendation Summaries."
            },
            new()
            {
                Key = "generate-executive-summary",
                Label = "Generate Executive Summary",
                Category = "ExecutiveSummary",
                DrillDownPath = "/executive-summaries/generate",
                Description = "Generate an Executive Recommendation Summary for a Recommendation."
            },
            new()
            {
                Key = "decisions",
                Label = "Decisions",
                Category = "Decisions",
                DrillDownPath = $"/decisions?companyId={companyId}",
                Description = "Browse Decisions recorded against Recommendations."
            },
            new()
            {
                Key = "audit",
                Label = "Audit",
                Category = "Audit",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Full Audit Trail — every Recommendation Workspace navigation action is auditable at its origin (BR-2610)."
            }
        };

        return new RecommendationNavigationResponse
        {
            CompanyId = companyId,
            Actions = actions
        };
    }
}

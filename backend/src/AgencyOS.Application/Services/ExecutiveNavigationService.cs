using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure, read-only Executive Workspace navigation (US-505 / BR-2807). Every action is a deep-link
/// to an existing, already-audited frontend route or dashboard — the Workspace itself only
/// aggregates and navigates (DEC-505-001); it never introduces a new write path.
/// </summary>
public class ExecutiveNavigationService : IExecutiveNavigationService
{
    public ExecutiveNavigationResponse GetNavigation(Guid companyId)
    {
        var actions = new List<ExecutiveActionResponse>
        {
            new()
            {
                Key = "enterprise-dashboard",
                Label = "Enterprise Dashboard",
                Category = "Overview",
                DrillDownPath = "/enterprise-dashboard",
                Description = "Open the cross-domain Enterprise Dashboard for full Company detail."
            },
            new()
            {
                Key = "my-work",
                Label = "My Work",
                Category = "Overview",
                DrillDownPath = "/my-work",
                Description = "Open the personal My Work Dashboard."
            },
            new()
            {
                Key = "planning-workspace",
                Label = "Planning Workspace",
                Category = "Planning",
                DrillDownPath = "/planning-workspace",
                Description = "Open the Planning Workspace for Templates, Capacity/Workload History, and Portfolios."
            },
            new()
            {
                Key = "recommendation-workspace",
                Label = "Recommendation Workspace",
                Category = "Recommendations",
                DrillDownPath = "/recommendation-workspace",
                Description = "Open the Recommendation Workspace to review and govern Recommendations."
            },
            new()
            {
                Key = "decision-workspace",
                Label = "Decision Workspace",
                Category = "Decisions",
                DrillDownPath = "/decision-workspace",
                Description = "Open the Decision Workspace to track the Decision lifecycle."
            },
            new()
            {
                Key = "portfolio-analytics",
                Label = "Portfolio Analytics",
                Category = "Portfolios",
                DrillDownPath = "/portfolio-analytics",
                Description = "Open per-Portfolio Analytics (trends, comparison, ranking, health/risk)."
            },
            new()
            {
                Key = "cross-portfolio-planning",
                Label = "Cross-Portfolio Planning",
                Category = "Portfolios",
                DrillDownPath = "/cross-portfolio-planning",
                Description = "Open advisory, read-only Cross-Portfolio Planning scenarios.",
                IsAdvisory = true
            },
            new()
            {
                Key = "capacity-history",
                Label = "Capacity History",
                Category = "Capacity",
                DrillDownPath = "/capacity/history",
                Description = "Open the immutable Capacity History."
            },
            new()
            {
                Key = "workload-history",
                Label = "Workload History",
                Category = "Workload",
                DrillDownPath = "/workload/history",
                Description = "Open the immutable Workload History."
            },
            new()
            {
                Key = "ai-recommendations",
                Label = "AI Recommendations",
                Category = "AI",
                DrillDownPath = "/ai-recommendations",
                Description = "Open advisory AI Recommendations.",
                IsAdvisory = true
            },
            new()
            {
                Key = "explainability",
                Label = "Explainability",
                Category = "AI",
                DrillDownPath = "/explainability",
                Description = "Open informational Explainability reports.",
                IsAdvisory = true
            },
            new()
            {
                Key = "executive-summaries",
                Label = "Executive Summaries",
                Category = "AI",
                DrillDownPath = "/executive-summaries",
                Description = "Open advisory Executive Recommendation Summaries.",
                IsAdvisory = true
            },
            new()
            {
                Key = "audit",
                Label = "Audit Trail",
                Category = "Audit",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Open the full, immutable Audit Trail — every Executive Workspace navigation action is auditable at its origin (BR-2810)."
            }
        };

        return new ExecutiveNavigationResponse
        {
            CompanyId = companyId,
            Actions = actions
        };
    }
}

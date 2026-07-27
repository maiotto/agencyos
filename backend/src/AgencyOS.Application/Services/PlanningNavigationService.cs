using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure, read-only Planning Workspace navigation (US-502 / BR-2508). Every action is a deep-link
/// to an existing, already-audited frontend route — the Workspace itself only aggregates and
/// navigates (DEC-502-001); it never introduces a new write path.
/// </summary>
public class PlanningNavigationService : IPlanningNavigationService
{
    public PlanningNavigationResponse GetNavigation(Guid companyId)
    {
        var actions = new List<PlanningActionResponse>
        {
            new()
            {
                Key = "manage-templates",
                Label = "Manage Templates",
                Category = "Templates",
                DrillDownPath = $"/planning-templates?companyId={companyId}",
                Description = "Create, edit, activate, and deactivate Planning Templates. Templates remain the source of planning configuration (BR-2506)."
            },
            new()
            {
                Key = "apply-template",
                Label = "Apply Template",
                Category = "Templates",
                DrillDownPath = $"/planning-templates?companyId={companyId}",
                Description = "Select a Template from the list, then apply it via its own /planning-templates/{id}/apply action."
            },
            new()
            {
                Key = "capacity-planning",
                Label = "Execute Capacity Planning",
                Category = "Capacity",
                DrillDownPath = "/capacity",
                Description = "Run Capacity calculations for an Execution Resource using the existing, unchanged Capacity Calculator engine."
            },
            new()
            {
                Key = "capacity-history",
                Label = "Capacity History",
                Category = "Capacity",
                DrillDownPath = $"/capacity/history?companyId={companyId}",
                Description = "Browse immutable Capacity History records — never recalculated by the Workspace."
            },
            new()
            {
                Key = "workload-history",
                Label = "Workload History",
                Category = "Workload",
                DrillDownPath = $"/workload/history?companyId={companyId}",
                Description = "Browse immutable Workload History records — never recalculated by the Workspace."
            },
            new()
            {
                Key = "portfolios",
                Label = "Portfolios",
                Category = "Portfolio",
                DrillDownPath = $"/portfolios?companyId={companyId}",
                Description = "Manage Portfolios and their Mission assignments."
            },
            new()
            {
                Key = "portfolio-analytics",
                Label = "Portfolio Analytics",
                Category = "Portfolio",
                DrillDownPath = $"/portfolio-analytics?companyId={companyId}",
                Description = "Read-only Portfolio trends, comparisons, ranking, and health/risk analytics."
            },
            new()
            {
                Key = "cross-portfolio-planning",
                Label = "Cross-Portfolio Planning",
                Category = "Scenarios",
                DrillDownPath = $"/cross-portfolio-planning?companyId={companyId}",
                Description = "Advisory-only, temporary multi-Portfolio simulation. Requires human approval; nothing executes automatically (BR-2507).",
                IsAdvisory = true
            },
            new()
            {
                Key = "enterprise-dashboard",
                Label = "Enterprise Dashboard",
                Category = "Overview",
                DrillDownPath = $"/enterprise-dashboard?companyId={companyId}",
                Description = "Company-wide, read-only rollup across Portfolios, Capacity/Workload, Recommendations, Decisions, and Audit activity."
            },
            new()
            {
                Key = "audit-trail",
                Label = "Audit Trail",
                Category = "History",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Full Audit Trail — the durable, tamper-evident record behind Planning History (BR-2510)."
            }
        };

        return new PlanningNavigationResponse
        {
            CompanyId = companyId,
            Actions = actions
        };
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Pure, read-only Decision Workspace navigation (US-504 / BR-2707). Every action is a deep-link
/// to an existing, already-audited frontend route — the Workspace itself only aggregates and
/// navigates (DEC-504-001); it never introduces a new write path.
/// </summary>
public class DecisionNavigationService : IDecisionNavigationService
{
    public DecisionNavigationResponse GetNavigation(Guid companyId)
    {
        var actions = new List<DecisionActionResponse>
        {
            new()
            {
                Key = "list-decisions",
                Label = "List Decisions",
                Category = "Decisions",
                DrillDownPath = $"/decisions?companyId={companyId}",
                Description = "Browse Decisions for this Company."
            },
            new()
            {
                Key = "create-decision",
                Label = "Create Decision",
                Category = "Decisions",
                DrillDownPath = "/decisions/new",
                Description = "Create a Decision from an Approved Recommendation. Always requires explicit human action (DEC-504-001).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "decision-detail",
                Label = "Decision Detail",
                Category = "Decisions",
                DrillDownPath = "/decisions",
                Description = "Select a Decision from the list to open its detail page."
            },
            new()
            {
                Key = "start-implementation",
                Label = "Start Implementation",
                Category = "Lifecycle",
                DrillDownPath = "/decisions",
                Description = "Starting implementation always requires explicit human action on the Decision detail page (DEC-504-001).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "complete",
                Label = "Complete",
                Category = "Lifecycle",
                DrillDownPath = "/decisions",
                Description = "Completing a Decision always requires explicit human action on the Decision detail page (DEC-504-001).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "cancel",
                Label = "Cancel",
                Category = "Lifecycle",
                DrillDownPath = "/decisions",
                Description = "Cancelling a Decision always requires explicit human action on the Decision detail page (DEC-504-001).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "record-outcome",
                Label = "Record Outcome",
                Category = "Outcomes",
                DrillDownPath = "/decisions",
                Description = "Recording an outcome always requires explicit human action on the Decision detail page (DEC-504-001).",
                RequiresHumanApproval = true
            },
            new()
            {
                Key = "recommendation-workspace",
                Label = "Recommendation Workspace",
                Category = "Recommendations",
                DrillDownPath = "/recommendation-workspace",
                Description = "Open the Recommendation Workspace to review the Recommendation behind a Decision."
            },
            new()
            {
                Key = "enterprise-dashboard",
                Label = "Enterprise Dashboard",
                Category = "Dashboard",
                DrillDownPath = "/enterprise-dashboard",
                Description = "Open the Enterprise Dashboard."
            },
            new()
            {
                Key = "audit",
                Label = "Audit Trail",
                Category = "Audit",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Full Audit Trail — every Decision Workspace navigation action is auditable at its origin (BR-2704)."
            }
        };

        return new DecisionNavigationResponse
        {
            CompanyId = companyId,
            Actions = actions
        };
    }
}

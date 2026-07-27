using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions for Decision Workspace endpoints (US-504).
/// </summary>
public sealed class DecisionWorkspaceOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(DecisionWorkspaceController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(DecisionWorkspaceController.GetWorkspace):
                operation.Summary ??= "Get full Decision Workspace";
                operation.Description ??=
                    "Read-only orchestration façade over the Decision lifecycle, Recommendation linkage, Decision "
                    + "Timeline, Decision Outcomes, Decision Audit, and navigation actions (DEC-504-001). Never "
                    + "mutates Decision data or bypasses mandatory human approval.";
                break;

            case nameof(DecisionWorkspaceController.GetOverview):
                operation.Summary ??= "Get Decision Workspace overview";
                operation.Description ??= "Company-scoped Decision Workspace KPIs for the period.";
                break;

            case nameof(DecisionWorkspaceController.GetDecisions):
                operation.Summary ??= "Get Decisions section";
                operation.Description ??=
                    "Decision cards grouped by DecisionStatus (Pending/InProgress/Completed/Cancelled) plus "
                    + "list/create navigation actions.";
                break;

            case nameof(DecisionWorkspaceController.GetTimeline):
                operation.Summary ??= "Get Decision Timeline section";
                operation.Description ??=
                    "Immutable Decision Timeline (BR-2703). When DecisionId is provided, returns the full Timeline "
                    + "for that Decision; otherwise a bounded, aggregated view of recent Timeline entries across "
                    + "recent Decisions.";
                break;

            case nameof(DecisionWorkspaceController.GetOutcomes):
                operation.Summary ??= "Get Decision Outcomes section";
                operation.Description ??=
                    "Decisions with a recorded Outcome. Record Outcome is a navigation deep-link only — the "
                    + "Workspace never calls RecordOutcomeAsync (BR-2705).";
                break;

            case nameof(DecisionWorkspaceController.GetAudit):
                operation.Summary ??= "Get Decision Audit section";
                operation.Description ??= "Immutable Decision Audit trail projection for the Company and period (BR-2704).";
                break;

            case nameof(DecisionWorkspaceController.GetKpis):
                operation.Summary ??= "Get Decision Workspace KPIs";
                operation.Description ??= "Company-scoped Decision Workspace KPI rollup only.";
                break;
        }
    }
}

using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions for Recommendation Workspace endpoints (US-503).
/// </summary>
public sealed class RecommendationWorkspaceOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(RecommendationWorkspaceController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(RecommendationWorkspaceController.GetWorkspace):
                operation.Summary ??= "Get full Recommendation Workspace";
                operation.Description ??=
                    "Read-only orchestration façade over Recommendations, Approval Workflows, AI Recommendations, "
                    + "Explainability, Executive Summaries, Recommendation History, Comparison, and navigation "
                    + "actions (DEC-503-001). Never mutates Recommendation data or bypasses mandatory human approval.";
                break;

            case nameof(RecommendationWorkspaceController.GetOverview):
                operation.Summary ??= "Get Recommendation Workspace overview";
                operation.Description ??= "Company-scoped Recommendation Workspace KPIs and section counts.";
                break;

            case nameof(RecommendationWorkspaceController.GetRecommendations):
                operation.Summary ??= "Get Recommendations section";
                operation.Description ??=
                    "Active Recommendation cards plus list/generate/archive-restore navigation actions.";
                break;

            case nameof(RecommendationWorkspaceController.GetApproval):
                operation.Summary ??= "Get Approval section";
                operation.Description ??=
                    "PendingApproval Workflow cards with Approve/Reject navigation deep-links. Approval and "
                    + "rejection always require explicit human action (BR-2604).";
                break;

            case nameof(RecommendationWorkspaceController.GetHistory):
                operation.Summary ??= "Get Recommendation History section";
                operation.Description ??=
                    "Immutable Recommendation History projection for the Company and period (BR-2609).";
                break;

            case nameof(RecommendationWorkspaceController.GetCompare):
                operation.Summary ??= "Get Recommendation comparison section";
                operation.Description ??=
                    "Side-by-side Recommendation comparison when LeftRecommendationId and RightRecommendationId "
                    + "are both provided and distinct; otherwise a navigation stub only.";
                break;

            case nameof(RecommendationWorkspaceController.GetAi):
                operation.Summary ??= "Get AI/Explainability section";
                operation.Description ??=
                    "Advisory AI Recommendation cards (BR-2605) plus informational Explainability cards (BR-2606).";
                break;

            case nameof(RecommendationWorkspaceController.GetExecutiveSummary):
                operation.Summary ??= "Get Executive Summary section";
                operation.Description ??= "Executive Recommendation Summary cards with navigation actions.";
                break;
        }
    }
}

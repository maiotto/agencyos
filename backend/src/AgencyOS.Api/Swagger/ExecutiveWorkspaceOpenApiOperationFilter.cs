using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions for Executive Workspace endpoints (US-505).
/// </summary>
public sealed class ExecutiveWorkspaceOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ExecutiveWorkspaceController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(ExecutiveWorkspaceController.GetWorkspace):
                operation.Summary ??= "Get full Executive Workspace";
                operation.Description ??=
                    "Read-only orchestration façade that primarily reuses Enterprise Dashboard section methods and "
                    + "adds executive navigation deep-links into every existing operational workspace and dashboard "
                    + "(DEC-505-001). Never mutates operational data.";
                break;

            case nameof(ExecutiveWorkspaceController.GetOverview):
                operation.Summary ??= "Get Executive Workspace overview";
                operation.Description ??=
                    "Company-scoped Executive KPIs, overall health (reused from IDashboardHealthCalculationService), "
                    + "and a short narrative for the period.";
                break;

            case nameof(ExecutiveWorkspaceController.GetEnterprise):
                operation.Summary ??= "Get Enterprise section";
                operation.Description ??= "Embeds the existing Enterprise Dashboard Summary plus a drill-down into the Enterprise Dashboard.";
                break;

            case nameof(ExecutiveWorkspaceController.GetPortfolios):
                operation.Summary ??= "Get Portfolios section";
                operation.Description ??= "Embeds the existing Enterprise Dashboard Portfolio rollup plus a drill-down into Portfolio Analytics.";
                break;

            case nameof(ExecutiveWorkspaceController.GetRecommendations):
                operation.Summary ??= "Get Recommendations section";
                operation.Description ??= "Embeds the existing Enterprise Dashboard Recommendations rollup plus a drill-down into the Recommendation Workspace.";
                break;

            case nameof(ExecutiveWorkspaceController.GetDecisions):
                operation.Summary ??= "Get Decisions section";
                operation.Description ??= "Embeds the existing Enterprise Dashboard Decisions rollup plus a drill-down into the Decision Workspace.";
                break;

            case nameof(ExecutiveWorkspaceController.GetCapacity):
                operation.Summary ??= "Get Capacity section";
                operation.Description ??= "Embeds the existing, immutable Enterprise Dashboard Capacity rollup plus a drill-down into Capacity History.";
                break;

            case nameof(ExecutiveWorkspaceController.GetWorkload):
                operation.Summary ??= "Get Workload section";
                operation.Description ??= "Embeds the existing, immutable Enterprise Dashboard Workload rollup plus a drill-down into Workload History.";
                break;

            case nameof(ExecutiveWorkspaceController.GetAi):
                operation.Summary ??= "Get AI section";
                operation.Description ??= "Embeds the existing Enterprise Dashboard AI rollup (advisory) plus a drill-down into AI Recommendations.";
                break;

            case nameof(ExecutiveWorkspaceController.GetAudit):
                operation.Summary ??= "Get Audit section";
                operation.Description ??= "Embeds the existing, immutable Enterprise Dashboard Audit rollup plus a drill-down into the Audit Trail (BR-2810).";
                break;
        }
    }
}

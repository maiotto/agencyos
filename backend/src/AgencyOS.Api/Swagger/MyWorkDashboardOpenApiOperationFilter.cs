using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions for My Work Dashboard endpoints (US-501).
/// </summary>
public sealed class MyWorkDashboardOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(MyWorkDashboardController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(MyWorkDashboardController.GetDashboard):
                operation.Summary ??= "Get full My Work Dashboard";
                operation.Description ??=
                    "Personalized, read-only operational workspace consolidating assigned Missions/Tasks, "
                    + "pending Recommendations/Decisions, Capacity/Workload, Activity timeline, and personal "
                    + "KPIs. Identity resolves UserId/CompanyId/ExecutionResourceId per DEC-501-001 and never "
                    + "writes to any repository (BR-2401/BR-2410).";
                break;

            case nameof(MyWorkDashboardController.GetSummary):
                operation.Summary ??= "Get My Work summary section";
                operation.Description ??= "Headline counts, personal KPIs, and Capacity/Workload snapshots.";
                break;

            case nameof(MyWorkDashboardController.GetTasks):
                operation.Summary ??= "Get My Work active Tasks";
                operation.Description ??=
                    "Active Task cards (BR-2406) plus overdue and upcoming-deadline breakdowns.";
                break;

            case nameof(MyWorkDashboardController.GetMissions):
                operation.Summary ??= "Get My Work active Missions";
                operation.Description ??= "Active Mission cards reachable through the caller's Assignments.";
                break;

            case nameof(MyWorkDashboardController.GetRecommendations):
                operation.Summary ??= "Get My Work pending Recommendations";
                operation.Description ??=
                    "Company-scoped pending RecommendationWorkflows (Status=PendingApproval), optionally "
                    + "unioned with workflows for Recommendations the caller generated.";
                break;

            case nameof(MyWorkDashboardController.GetDecisions):
                operation.Summary ??= "Get My Work pending Decisions";
                operation.Description ??=
                    "Pending Decisions (DecisionStatus Created or InProgress), preferring the caller's own "
                    + "CreatedBy entries while remaining Company-isolated.";
                break;

            case nameof(MyWorkDashboardController.GetActivity):
                operation.Summary ??= "Get My Work Activity timeline";
                operation.Description ??= "Caller's Activity Timeline projected from existing Audit Events.";
                break;

            case nameof(MyWorkDashboardController.GetKpis):
                operation.Summary ??= "Get My Work personal KPIs";
                operation.Description ??=
                    "Personal KPI rollup computed purely from already-aggregated counts (BR-2407).";
                break;
        }
    }
}

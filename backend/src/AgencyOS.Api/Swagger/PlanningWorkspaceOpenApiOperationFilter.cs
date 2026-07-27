using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions for Planning Workspace endpoints (US-502).
/// </summary>
public sealed class PlanningWorkspaceOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PlanningWorkspaceController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(PlanningWorkspaceController.GetWorkspace):
                operation.Summary ??= "Get full Planning Workspace";
                operation.Description ??=
                    "Read-only orchestration façade over Planning Templates, Capacity/Workload History, "
                    + "Portfolios, Cross-Portfolio scenarios, Planning History, and navigation actions "
                    + "(DEC-502-001). Never mutates planning data or recalculates engines for storage.";
                break;

            case nameof(PlanningWorkspaceController.GetOverview):
                operation.Summary ??= "Get Planning Workspace overview";
                operation.Description ??= "Company-scoped Planning KPIs and section counts.";
                break;

            case nameof(PlanningWorkspaceController.GetTemplates):
                operation.Summary ??= "Get Planning Templates section";
                operation.Description ??=
                    "Thin Template cards plus manage/apply navigation into existing Template routes (BR-2506).";
                break;

            case nameof(PlanningWorkspaceController.GetCapacity):
                operation.Summary ??= "Get Planning Capacity section";
                operation.Description ??=
                    "Capacity History aggregate and recent records for the period — immutable history only (BR-2503).";
                break;

            case nameof(PlanningWorkspaceController.GetWorkload):
                operation.Summary ??= "Get Planning Workload section";
                operation.Description ??=
                    "Workload History aggregate and recent records for the period — immutable history only (BR-2503).";
                break;

            case nameof(PlanningWorkspaceController.GetPortfolios):
                operation.Summary ??= "Get Planning Portfolios section";
                operation.Description ??= "Portfolio cards with snapshot utilization/workload and drill-down paths.";
                break;

            case nameof(PlanningWorkspaceController.GetHistory):
                operation.Summary ??= "Get Planning History";
                operation.Description ??=
                    "Planning-related Audit Trail projection (Templates, Portfolios, Capacity/Workload History, "
                    + "Cross-Portfolio Plans) — workspace is fully auditable (BR-2510).";
                break;

            case nameof(PlanningWorkspaceController.GetScenarios):
                operation.Summary ??= "Get Cross-Portfolio Planning scenarios";
                operation.Description ??=
                    "Temporary, advisory-only Cross-Portfolio scenarios from the in-memory store (BR-2507). "
                    + "Requires human approval; nothing executes automatically.";
                break;
        }
    }
}

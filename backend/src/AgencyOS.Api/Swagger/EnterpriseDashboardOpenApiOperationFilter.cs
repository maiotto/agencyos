using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions and response examples for Enterprise Dashboard endpoints (US-403).
/// </summary>
public sealed class EnterpriseDashboardOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(EnterpriseDashboardController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(EnterpriseDashboardController.GetDashboard):
                operation.Summary ??= "Get full Enterprise Dashboard";
                operation.Description ??=
                    "Aggregates Portfolio, Capacity/Workload History, Planning Templates, Recommendations, "
                    + "Decisions, AI Decision Support, and Audit data for a Company and reporting period. "
                    + "Read-only — never recalculates history or persists new analytical storage (BR-2101/BR-2102).";
                SetJsonResponseExample(operation, "200", CreateSummaryExample());
                break;

            case nameof(EnterpriseDashboardController.GetSummary):
                operation.Summary ??= "Get Dashboard Summary section";
                operation.Description ??=
                    "Headline Portfolio/Recommendation/Decision counts, overall health, and trends.";
                SetJsonResponseExample(operation, "200", CreateSummaryExample());
                break;

            case nameof(EnterpriseDashboardController.GetPlanning):
                operation.Summary ??= "Get Dashboard Planning section";
                operation.Description ??= "Planning Template counts and status breakdown.";
                break;

            case nameof(EnterpriseDashboardController.GetPortfolio):
                operation.Summary ??= "Get Dashboard Portfolio section";
                operation.Description ??=
                    "Portfolio status/health breakdown and average utilization/workload (BR-2107).";
                break;

            case nameof(EnterpriseDashboardController.GetCapacity):
                operation.Summary ??= "Get Dashboard Capacity section";
                operation.Description ??=
                    "Capacity totals and average utilization from immutable Capacity History (BR-2103).";
                break;

            case nameof(EnterpriseDashboardController.GetWorkload):
                operation.Summary ??= "Get Dashboard Workload section";
                operation.Description ??=
                    "Workload totals and average workload from immutable Workload History (BR-2103).";
                break;

            case nameof(EnterpriseDashboardController.GetRecommendations):
                operation.Summary ??= "Get Dashboard Recommendations section";
                operation.Description ??= "Recommendation counts, status breakdown, average score, and trend.";
                break;

            case nameof(EnterpriseDashboardController.GetDecisions):
                operation.Summary ??= "Get Dashboard Decisions section";
                operation.Description ??=
                    "Decision status/implementation breakdown, completed/cancelled counts, and trend.";
                break;

            case nameof(EnterpriseDashboardController.GetAi):
                operation.Summary ??= "Get Dashboard AI Decision Support section";
                operation.Description ??=
                    "AI Recommendation, Explainability, and Executive Recommendation Summary counts and averages.";
                break;

            case nameof(EnterpriseDashboardController.GetAudit):
                operation.Summary ??= "Get Dashboard Audit section";
                operation.Description ??= "Audit event/entity type breakdown, last event time, and trend.";
                break;
        }
    }

    private static void SetJsonResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            return;
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();

        if (!response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            response.Content["application/json"] = mediaType;
        }

        mediaType.Example = example;
    }

    private static OpenApiObject CreateSummaryExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["generatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["portfolioCount"] = new OpenApiInteger(4),
        ["activePortfolioCount"] = new OpenApiInteger(3),
        ["planningTemplateCount"] = new OpenApiInteger(2),
        ["recommendationCount"] = new OpenApiInteger(12),
        ["decisionCount"] = new OpenApiInteger(8),
        ["pendingDecisionCount"] = new OpenApiInteger(3),
        ["completedDecisionCount"] = new OpenApiInteger(5),
        ["defaultDecisionProfileName"] = new OpenApiString("Balanced Strategy"),
        ["overallHealth"] = new OpenApiObject
        {
            ["status"] = new OpenApiString("Healthy"),
            ["label"] = new OpenApiString("Healthy"),
            ["detail"] = new OpenApiString("Utilization and workload are within expected operating ranges.")
        },
        ["recommendationTrend"] = new OpenApiObject
        {
            ["direction"] = new OpenApiString("Up"),
            ["deltaPercent"] = new OpenApiDouble(20.0),
            ["currentValue"] = new OpenApiInteger(12),
            ["previousValue"] = new OpenApiInteger(10)
        },
        ["decisionTrend"] = new OpenApiObject
        {
            ["direction"] = new OpenApiString("Flat"),
            ["deltaPercent"] = new OpenApiDouble(0.0),
            ["currentValue"] = new OpenApiInteger(8),
            ["previousValue"] = new OpenApiInteger(8)
        },
        ["drillDownPath"] = new OpenApiString("/portfolios?companyId=aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
    };
}

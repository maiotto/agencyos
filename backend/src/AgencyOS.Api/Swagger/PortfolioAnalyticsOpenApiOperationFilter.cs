using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions and response examples for Portfolio Analytics endpoints (US-404).
/// </summary>
public sealed class PortfolioAnalyticsOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PortfolioAnalyticsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(PortfolioAnalyticsController.GetOverview):
                operation.Summary ??= "Get Portfolio Analytics overview";
                operation.Description ??=
                    "One card per Portfolio in the resolved Company with health, mission count, "
                    + "utilization/workload snapshot, and mission-scoped Recommendation/Decision counts. "
                    + "Read-only — never recalculates Capacity/Workload history or modifies a Portfolio (BR-2201/BR-2209).";
                SetJsonResponseExample(operation, "200", CreateOverviewExample());
                break;

            case nameof(PortfolioAnalyticsController.GetTrends):
                operation.Summary ??= "Get Portfolio Analytics trends";
                operation.Description ??=
                    "Capacity/Workload/Health/Recommendation/Decision trend series bucketed by month, from "
                    + "immutable Capacity/Workload History. Optionally scoped to a single Portfolio's Missions via PortfolioId (BR-2204/BR-2210).";
                break;

            case nameof(PortfolioAnalyticsController.GetComparison):
                operation.Summary ??= "Compare two Portfolios";
                operation.Description ??=
                    "Side-by-side comparison of two Portfolios using stored snapshot fields plus "
                    + "mission-scoped Recommendation/Decision counts (BR-2202).";
                break;

            case nameof(PortfolioAnalyticsController.GetRanking):
                operation.Summary ??= "Get Portfolio ranking";
                operation.Description ??=
                    "Portfolios ranked by health severity, snapshot utilization, and Recommendation/Decision "
                    + "effectiveness (approved/completed ratio) (BR-2210).";
                break;

            case nameof(PortfolioAnalyticsController.GetHealth):
                operation.Summary ??= "Get Portfolio health analytics";
                operation.Description ??=
                    "Health distribution and risk indicators across a Company's Portfolios, reusing "
                    + "PortfolioHealth.Calculate/Canonicalize (BR-2203).";
                break;

            case nameof(PortfolioAnalyticsController.GetPerformance):
                operation.Summary ??= "Get Portfolio performance metrics";
                operation.Description ??=
                    "Recommendation/Decision effectiveness metrics across a Company's Portfolios "
                    + "(average score, decision completion/cancellation rates) (BR-2205/BR-2206/BR-2210).";
                break;

            case nameof(PortfolioAnalyticsController.GetDetail):
                operation.Summary ??= "Get Portfolio Analytics detail";
                operation.Description ??=
                    "Full read-only analysis for a single Portfolio: health, snapshot utilization/workload, "
                    + "historical Capacity/Workload averages, and mission-scoped Recommendation/Decision breakdowns.";
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

    private static OpenApiObject CreateOverviewExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["generatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["portfolioCount"] = new OpenApiInteger(2),
        ["overallHealth"] = new OpenApiObject
        {
            ["status"] = new OpenApiString("Healthy"),
            ["label"] = new OpenApiString("Healthy"),
            ["detail"] = new OpenApiString("Utilization and workload are within expected operating ranges.")
        },
        ["portfolios"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["portfolioId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                ["name"] = new OpenApiString("Q3 Delivery Portfolio"),
                ["status"] = new OpenApiString("Active"),
                ["health"] = new OpenApiObject
                {
                    ["status"] = new OpenApiString("Healthy"),
                    ["label"] = new OpenApiString("Healthy")
                },
                ["missionCount"] = new OpenApiInteger(4),
                ["utilizationPercentage"] = new OpenApiDouble(72.5),
                ["workloadPercentage"] = new OpenApiDouble(68.0),
                ["recommendationCount"] = new OpenApiInteger(6),
                ["decisionCount"] = new OpenApiInteger(4),
                ["drillDownPath"] = new OpenApiString("/portfolios/bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb")
            }
        },
        ["drillDownPath"] = new OpenApiString("/portfolios?companyId=aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
    };
}

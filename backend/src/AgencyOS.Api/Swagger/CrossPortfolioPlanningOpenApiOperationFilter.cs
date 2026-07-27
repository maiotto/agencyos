using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds descriptions and response examples for Cross-Portfolio Planning endpoints (US-405).
/// </summary>
public sealed class CrossPortfolioPlanningOpenApiOperationFilter : IOperationFilter
{
    private const string Disclaimer =
        "Advisory only — human approval required. Simulations never modify Portfolios.";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(CrossPortfolioPlanningController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(CrossPortfolioPlanningController.GetOverview):
                operation.Summary ??= "Get Cross-Portfolio Planning overview";
                operation.Description ??=
                    "Active Portfolios in the resolved Company presented as participation candidates for a "
                    + $"simulation. {Disclaimer} (BR-2301/BR-2308).";
                SetJsonResponseExample(operation, "200", CreateOverviewExample());
                break;

            case nameof(CrossPortfolioPlanningController.GetScenarios):
                operation.Summary ??= "List temporary Cross-Portfolio Plan scenarios";
                operation.Description ??=
                    "Lists in-memory Cross-Portfolio Plan scenarios previously simulated for the resolved "
                    + "Company. Scenarios are TEMPORARY — held only for the process lifetime (DEC-405-001).";
                break;

            case nameof(CrossPortfolioPlanningController.GetConflicts):
                operation.Summary ??= "Detect Cross-Portfolio conflicts";
                operation.Description ??=
                    "Detects Portfolio Mission overlap and Resource conflicts (delegated to "
                    + "IAllocationConflictDetectionService) across the selected Portfolios (BR-2305). "
                    + "Purely informative — conflicts are never auto-resolved.";
                break;

            case nameof(CrossPortfolioPlanningController.GetBalance):
                operation.Summary ??= "Get Cross-Portfolio Capacity/Workload balance";
                operation.Description ??=
                    "Enterprise Capacity/Workload balance and advisory rebalancing recommendations for the "
                    + "selected Portfolios, reusing the Capacity and Workload Engines (BR-2303/BR-2304/BR-2306). "
                    + $"Mission priorities are preserved and never reordered. {Disclaimer}";
                break;

            case nameof(CrossPortfolioPlanningController.Simulate):
                operation.Summary ??= "Simulate a Cross-Portfolio Plan";
                operation.Description ??=
                    "Simulates a temporary Cross-Portfolio Plan across at least two Portfolios: builds "
                    + "enterprise Capacity/Workload views, detects conflicts, computes advisory balancing "
                    + "recommendations, and stores the resulting scenario in-memory (DEC-405-001). Every "
                    + $"simulation is audited via IAuditService.RecordSafeAsync (BR-2309). {Disclaimer}";
                SetJsonResponseExample(operation, "200", CreateSimulationExample());
                break;

            case nameof(CrossPortfolioPlanningController.Compare):
                operation.Summary ??= "Compare two Cross-Portfolio Plan scenarios";
                operation.Description ??=
                    $"Compares two previously simulated, in-memory scenarios by id. {Disclaimer}";
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
        ["portfolios"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["portfolioId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                ["name"] = new OpenApiString("Q3 Delivery Portfolio"),
                ["status"] = new OpenApiString("Active"),
                ["missionCount"] = new OpenApiInteger(3),
                ["utilizationPercentage"] = new OpenApiDouble(72.5),
                ["workloadPercentage"] = new OpenApiDouble(68.0)
            }
        },
        ["requiresHumanApproval"] = new OpenApiBoolean(true),
        ["advisoryDisclaimer"] = new OpenApiString(Disclaimer)
    };

    private static OpenApiObject CreateSimulationExample() => new()
    {
        ["scenarioId"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["simulatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["requiresHumanApproval"] = new OpenApiBoolean(true),
        ["advisoryOnly"] = new OpenApiBoolean(true),
        ["advisoryDisclaimer"] = new OpenApiString(Disclaimer)
    };
}

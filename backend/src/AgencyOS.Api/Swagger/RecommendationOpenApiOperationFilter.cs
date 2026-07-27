using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Recommendation Persistence endpoints (US-202).
/// </summary>
public sealed class RecommendationOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(RecommendationsController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(RecommendationsController.GetAll):
            case nameof(RecommendationsController.Filter):
                operation.Summary ??= "List or filter recommendations";
                operation.Description ??= "Returns persisted Decision Engine recommendations with search and filters.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(RecommendationsController.GetById):
                operation.Summary ??= "Get recommendation by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationsController.GetVersions):
                operation.Summary ??= "Get recommendation versions";
                operation.Description ??= "Returns all immutable versions for the recommendation number lineage (BR-1106).";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(RecommendationsController.Create):
                operation.Summary ??= "Persist recommendation";
                operation.Description ??= "Creates an immutable recommendation snapshot. Does not generate strategies.";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(RecommendationsController.CreateNewVersion):
                operation.Summary ??= "Create recommendation version";
                operation.Description ??= "Creates a new version without overwriting the source (BR-1106).";
                SetJsonRequestExample(operation, CreateVersionRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(RecommendationsController.Archive):
                operation.Summary ??= "Archive recommendation";
                operation.Description ??= "Archives a recommendation. Remains queryable (BR-1107). No delete (BR-1108).";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationsController.Restore):
                operation.Summary ??= "Restore recommendation";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationsController.GetByCompanyId):
                operation.Summary ??= "Recommendations by company";
                break;

            case nameof(RecommendationsController.GetByMissionId):
                operation.Summary ??= "Recommendations by mission";
                break;

            case nameof(RecommendationsController.GetByContractId):
                operation.Summary ??= "Recommendations by contract";
                break;
        }
    }

    private static void SetJsonRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        operation.RequestBody ??= new OpenApiRequestBody();
        operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!operation.RequestBody.Content.TryGetValue("application/json", out var media))
        {
            media = new OpenApiMediaType();
            operation.RequestBody.Content["application/json"] = media;
        }

        media.Example = example;
    }

    private static void SetJsonResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            return;
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!response.Content.TryGetValue("application/json", out var media))
        {
            media = new OpenApiMediaType();
            response.Content["application/json"] = media;
        }

        media.Example = example;
    }

    private static OpenApiObject CreateExample() =>
        new()
        {
            ["id"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ["missionId"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
            ["contractId"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
            ["deliveryStrategyId"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
            ["recommendationNumber"] = new OpenApiString("REC-20260726-DDDDDD-EEEEEE"),
            ["title"] = new OpenApiString("Human + AI"),
            ["summary"] = new OpenApiString("Ranked delivery strategy"),
            ["reason"] = new OpenApiString("Rank 1 with final score 82.5."),
            ["score"] = new OpenApiDouble(82.5),
            ["rank"] = new OpenApiInteger(1),
            ["status"] = new OpenApiString("Active"),
            ["version"] = new OpenApiInteger(1),
            ["decisionEngineVersion"] = new OpenApiString("1.1.0-decision-engine"),
            ["archived"] = new OpenApiBoolean(false),
            ["generatedBy"] = new OpenApiString("decision-engine")
        };

    private static OpenApiArray CreateListExample() =>
        new() { CreateExample() };

    private static OpenApiObject CreateRequestExample() =>
        new()
        {
            ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ["missionId"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
            ["contractId"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
            ["deliveryStrategyId"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
            ["title"] = new OpenApiString("Human + AI"),
            ["recommendationPayload"] = new OpenApiString("{\"strategyName\":\"Human + AI\"}"),
            ["capacitySnapshot"] = new OpenApiString("{}"),
            ["workloadSnapshot"] = new OpenApiString("{}"),
            ["generatedBy"] = new OpenApiString("planner@agencyos.local"),
            ["score"] = new OpenApiDouble(82.5),
            ["rank"] = new OpenApiInteger(1)
        };

    private static OpenApiObject CreateVersionRequestExample() =>
        new()
        {
            ["title"] = new OpenApiString("Human + AI (revised)"),
            ["recommendationPayload"] = new OpenApiString("{\"strategyName\":\"Human + AI\"}"),
            ["capacitySnapshot"] = new OpenApiString("{}"),
            ["workloadSnapshot"] = new OpenApiString("{}"),
            ["generatedBy"] = new OpenApiString("planner@agencyos.local"),
            ["score"] = new OpenApiDouble(85.0),
            ["rank"] = new OpenApiInteger(1)
        };
}

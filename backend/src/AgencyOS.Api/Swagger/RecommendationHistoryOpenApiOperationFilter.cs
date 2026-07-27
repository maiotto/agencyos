using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Recommendation History endpoints (US-203).
/// </summary>
public sealed class RecommendationHistoryOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(RecommendationHistoryController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(RecommendationHistoryController.GetAll):
            case nameof(RecommendationHistoryController.Filter):
                operation.Summary ??= "List or filter recommendation history";
                operation.Description ??= "Immutable history of recommendation versions and workflow events (BR-1201..BR-1206).";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(RecommendationHistoryController.GetById):
                operation.Summary ??= "Get recommendation history entry";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationHistoryController.GetVersions):
                operation.Summary ??= "Get recommendation version history";
                operation.Description ??= "Returns VersionCreated history rows for the recommendation number lineage (BR-1202).";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(RecommendationHistoryController.GetTimeline):
                operation.Summary ??= "Get recommendation timeline";
                operation.Description ??= "Chronological recommendation and workflow history for a lineage (BR-1203).";
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
            ["id"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
            ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            ["recommendationNumber"] = new OpenApiString("REC-20260726-AAAAAA-BBBBBB"),
            ["recommendationVersion"] = new OpenApiInteger(1),
            ["eventType"] = new OpenApiString("VersionCreated"),
            ["recommendationStatus"] = new OpenApiString("Active"),
            ["title"] = new OpenApiString("Human + AI"),
            ["decisionEngineVersion"] = new OpenApiString("1.1.0-decision-engine"),
            ["createdBy"] = new OpenApiString("decision-engine")
        };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };
}

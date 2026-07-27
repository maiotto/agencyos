using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Recommendation Comparison endpoints (US-204).
/// </summary>
public sealed class RecommendationComparisonOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(RecommendationComparisonController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(RecommendationComparisonController.Compare):
                operation.Summary ??= "Compare recommendation snapshots";
                operation.Description ??=
                    "Side-by-side comparison of two immutable Recommendation History snapshots (BR-1301..BR-1306). "
                    + "Ids may be history ids or recommendation ids resolved to VersionCreated snapshots.";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationComparisonController.CompareByIds):
                operation.Summary ??= "Compare recommendation snapshots by id";
                operation.Description ??= "Compares left and right history/recommendation ids without mutating Recommendations.";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationComparisonController.CompareVersions):
                operation.Summary ??= "Compare recommendation versions";
                operation.Description ??=
                    "Compares VersionCreated history rows for a recommendation number. "
                    + "Optional leftVersion/rightVersion; defaults to the two latest versions.";
                SetJsonResponseExample(operation, "200", CreateExample());
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
            ["hasDifferences"] = new OpenApiBoolean(true),
            ["scoreDelta"] = new OpenApiDouble(5),
            ["rankDelta"] = new OpenApiInteger(-1),
            ["versionDelta"] = new OpenApiInteger(1),
            ["left"] = new OpenApiObject
            {
                ["id"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                ["recommendationNumber"] = new OpenApiString("REC-20260726-AAAAAA-BBBBBB"),
                ["recommendationVersion"] = new OpenApiInteger(1),
                ["score"] = new OpenApiDouble(80),
                ["rank"] = new OpenApiInteger(2)
            },
            ["right"] = new OpenApiObject
            {
                ["id"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
                ["recommendationNumber"] = new OpenApiString("REC-20260726-AAAAAA-BBBBBB"),
                ["recommendationVersion"] = new OpenApiInteger(2),
                ["score"] = new OpenApiDouble(85),
                ["rank"] = new OpenApiInteger(1)
            },
            ["differences"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["section"] = new OpenApiString("Metadata"),
                    ["path"] = new OpenApiString("score"),
                    ["leftValue"] = new OpenApiString("80"),
                    ["rightValue"] = new OpenApiString("85"),
                    ["changed"] = new OpenApiBoolean(true)
                }
            }
        };
}

using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class AIRecommendationOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AIRecommendationsController))
        {
            return;
        }

        operation.Summary ??= "AI-assisted Recommendation";
        operation.Description ??=
            "Advisory AI Recommendation APIs (US-301). AI never replaces Recommendations; human approval remains mandatory.";

        if (operation.Responses.TryGetValue("200", out var ok) ||
            operation.Responses.TryGetValue("201", out ok))
        {
            ok.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!ok.Content.TryGetValue("application/json", out var media))
            {
                media = new OpenApiMediaType();
                ok.Content["application/json"] = media;
            }

            media.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("aaaaaaaa-1111-4111-8111-aaaaaaaaaaaa"),
                ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                ["confidenceScore"] = new OpenApiDouble(82.5),
                ["suggestedDeliveryStrategy"] = new OpenApiString("Balanced human + AI delivery mix"),
                ["modelVersion"] = new OpenApiString("1.1.0-ai-advisor"),
                ["promptVersion"] = new OpenApiString("1.0.0-deterministic-advisor"),
                ["status"] = new OpenApiString("Active")
            };
        }
    }
}

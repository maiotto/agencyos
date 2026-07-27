using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class ExplainabilityOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ExplainabilityController))
        {
            return;
        }

        operation.Summary ??= "LLM Explainability";
        operation.Description ??=
            "Informational explainability APIs (US-302). Never changes Recommendations or Decision Engine calculations.";

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
                ["id"] = new OpenApiString("cccccccc-1111-4111-8111-cccccccccccc"),
                ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                ["explanationType"] = new OpenApiString("Recommendation"),
                ["executiveSummary"] = new OpenApiString("Explanation of Recommendation Human + AI"),
                ["modelVersion"] = new OpenApiString("1.1.0-explainability"),
                ["promptVersion"] = new OpenApiString("1.0.0-deterministic-explainer"),
                ["status"] = new OpenApiString("Active")
            };
        }
    }
}

using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class ExecutiveRecommendationSummaryOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ExecutiveRecommendationSummariesController))
        {
            return;
        }

        operation.Summary ??= "Executive Recommendation Summary";
        operation.Description ??=
            "Informational executive briefing APIs (US-303). Never changes Recommendations, AI Recommendations, or Decisions. Completes EPIC-03.";

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
                ["id"] = new OpenApiString("dddddddd-1111-4111-8111-dddddddddddd"),
                ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                ["summaryVersion"] = new OpenApiInteger(1),
                ["executiveSummary"] = new OpenApiString("Executive briefing for Human + AI"),
                ["confidenceLevel"] = new OpenApiDouble(81.5),
                ["modelVersion"] = new OpenApiString("1.1.0-executive-briefing"),
                ["promptVersion"] = new OpenApiString("1.0.0-deterministic-executive-summary"),
                ["status"] = new OpenApiString("Active")
            };
        }
    }
}

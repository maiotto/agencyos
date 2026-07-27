using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class AuditOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AuditController))
        {
            return;
        }

        operation.Summary ??= "Decision Audit Trail query";
        operation.Description ??=
            "Read-only immutable audit events (US-206 / BR-1501..BR-1510). No create/update/delete via API.";

        if (operation.Responses.TryGetValue("200", out var response))
        {
            response.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!response.Content.TryGetValue("application/json", out var media))
            {
                media = new OpenApiMediaType();
                response.Content["application/json"] = media;
            }

            media.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("ffffffff-ffff-4fff-8fff-ffffffffffff"),
                ["entityType"] = new OpenApiString("Decision"),
                ["entityId"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
                ["eventType"] = new OpenApiString("Created"),
                ["action"] = new OpenApiString("Decision.Create"),
                ["userId"] = new OpenApiString("planner@agencyos.local"),
                ["userName"] = new OpenApiString("planner@agencyos.local"),
                ["source"] = new OpenApiString("Api")
            };
        }
    }
}

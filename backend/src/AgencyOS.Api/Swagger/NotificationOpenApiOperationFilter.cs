using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class NotificationOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(NotificationsController))
        {
            return;
        }

        operation.Summary ??= "Notification Center";
        operation.Description ??=
            "In-application Notification Center APIs (US-506 / BR-2901..BR-2910). "
            + "Informational only. Never modifies business data. "
            + "Requires X-User-Id for user-scoped queries and actions; company isolation via X-Company-Id.";

        if (operation.Responses.TryGetValue("200", out var ok))
        {
            ok.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!ok.Content.TryGetValue("application/json", out var media))
            {
                media = new OpenApiMediaType();
                ok.Content["application/json"] = media;
            }

            media.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
                ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                ["userId"] = new OpenApiString("planner"),
                ["title"] = new OpenApiString("Decision awaiting implementation"),
                ["message"] = new OpenApiString("Decision DEC-100 was created from an approved recommendation."),
                ["category"] = new OpenApiString("Decision"),
                ["priority"] = new OpenApiString("High"),
                ["status"] = new OpenApiString("Unread"),
                ["sourceEntity"] = new OpenApiString("Decision"),
                ["sourceEntityId"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
                ["archived"] = new OpenApiBoolean(false),
                ["navigationPath"] = new OpenApiString("/decisions/eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee")
            };
        }
    }
}

using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Mission Management endpoints.
/// </summary>
public sealed class MissionOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(MissionsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(MissionsController.GetAll):
                operation.Summary ??= "List missions";
                operation.Description ??=
                    "Returns all missions ordered by name, then code.";
                SetJsonResponseExample(operation, "200", CreateMissionListExample());
                break;

            case nameof(MissionsController.GetById):
                operation.Summary ??= "Get mission by id";
                operation.Description ??= "Returns mission details by identifier.";
                SetJsonResponseExample(operation, "200", CreateMissionExample());
                break;

            case nameof(MissionsController.Create):
                operation.Summary ??= "Create mission";
                operation.Description ??=
                    "Creates a new mission. The referenced Contract must exist and have Status Active. Mission Code must be unique (case-insensitive).";
                SetJsonRequestExample(operation, CreateMissionRequestExample());
                SetJsonResponseExample(operation, "201", CreateMissionExample());
                break;

            case nameof(MissionsController.Update):
                operation.Summary ??= "Update mission";
                operation.Description ??=
                    "Updates an existing mission. Mission Code must remain unique (case-insensitive).";
                SetJsonRequestExample(operation, UpdateMissionRequestExample());
                SetJsonResponseExample(operation, "200", CreateMissionExample());
                break;

            case nameof(MissionsController.Delete):
                operation.Summary ??= "Delete mission";
                operation.Description ??=
                    "Permanently deletes a mission. Existing DELETE semantics are preserved.";
                break;
        }
    }

    private static void SetJsonRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        operation.RequestBody ??= new OpenApiRequestBody();
        operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            operation.RequestBody.Content["application/json"] = mediaType;
        }

        mediaType.Example = example;
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

    private static OpenApiObject CreateMissionRequestExample() => new()
    {
        ["clientContractId"] = new OpenApiString("44444444-4444-4444-8444-444444444444"),
        ["code"] = new OpenApiString("MSN-2026-001"),
        ["name"] = new OpenApiString("Acme Delivery Mission"),
        ["description"] = new OpenApiString("Primary delivery mission"),
        ["missionTypeId"] = new OpenApiString("11111111-1111-4111-8111-000000000002"),
        ["missionStatusId"] = new OpenApiString("11111111-1111-4111-8121-000000000002"),
        ["priority"] = new OpenApiString("NORMAL"),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-06-30")
    };

    private static OpenApiObject UpdateMissionRequestExample() => new()
    {
        ["clientContractId"] = new OpenApiString("44444444-4444-4444-8444-444444444444"),
        ["code"] = new OpenApiString("MSN-2026-001"),
        ["name"] = new OpenApiString("Acme Delivery Mission Updated"),
        ["description"] = new OpenApiString("Primary delivery mission"),
        ["missionTypeId"] = new OpenApiString("11111111-1111-4111-8111-000000000002"),
        ["missionStatusId"] = new OpenApiString("11111111-1111-4111-8121-000000000002"),
        ["priority"] = new OpenApiString("HIGH"),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-06-30")
    };

    private static OpenApiObject CreateMissionExample() => new()
    {
        ["id"] = new OpenApiString("55555555-5555-4555-8555-555555555555"),
        ["clientContractId"] = new OpenApiString("44444444-4444-4444-8444-444444444444"),
        ["code"] = new OpenApiString("MSN-2026-001"),
        ["name"] = new OpenApiString("Acme Delivery Mission"),
        ["description"] = new OpenApiString("Primary delivery mission"),
        ["missionTypeId"] = new OpenApiString("11111111-1111-4111-8111-000000000002"),
        ["missionStatusId"] = new OpenApiString("11111111-1111-4111-8121-000000000002"),
        ["priority"] = new OpenApiString("NORMAL"),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-06-30"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiArray CreateMissionListExample() => new()
    {
        CreateMissionExample()
    };
}

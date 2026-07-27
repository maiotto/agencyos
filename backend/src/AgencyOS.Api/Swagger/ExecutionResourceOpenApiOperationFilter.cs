using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Execution Resource Management endpoints.
/// </summary>
public sealed class ExecutionResourceOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ExecutionResourcesController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(ExecutionResourcesController.GetAll):
                operation.Summary ??= "List execution resources";
                operation.Description ??=
                    "Returns execution resources with optional resource type, status, and skill filters. Ordered by name, then code, unless a different ordering is requested.";
                SetJsonResponseExample(operation, "200", CreateExecutionResourceListExample());
                break;

            case nameof(ExecutionResourcesController.GetById):
                operation.Summary ??= "Get execution resource by id";
                operation.Description ??= "Returns execution resource details by identifier.";
                SetJsonResponseExample(operation, "200", CreateExecutionResourceExample());
                break;

            case nameof(ExecutionResourcesController.Create):
                operation.Summary ??= "Create execution resource";
                operation.Description ??=
                    "Creates a new execution resource. Resource Code must be unique (case-insensitive). Resource Type and Status are persisted using their canonical spelling.";
                SetJsonRequestExample(operation, CreateExecutionResourceRequestExample());
                SetJsonResponseExample(operation, "201", CreateExecutionResourceExample());
                break;

            case nameof(ExecutionResourcesController.Update):
                operation.Summary ??= "Update execution resource";
                operation.Description ??=
                    "Updates an existing execution resource. Resource Code must remain unique (case-insensitive).";
                SetJsonRequestExample(operation, UpdateExecutionResourceRequestExample());
                SetJsonResponseExample(operation, "200", CreateExecutionResourceExample());
                break;

            case nameof(ExecutionResourcesController.Deactivate):
                operation.Summary ??= "Deactivate execution resource";
                operation.Description ??=
                    "Deactivates an execution resource by setting Status to Inactive. Historical information is preserved and the call is idempotent.";
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

    private static OpenApiObject CreateExecutionResourceRequestExample() => new()
    {
        ["code"] = new OpenApiString("RES-001"),
        ["name"] = new OpenApiString("Senior Delivery Consultant"),
        ["resourceType"] = new OpenApiString("Internal Human"),
        ["status"] = new OpenApiString("Active"),
        ["capacityHoursPerWeek"] = new OpenApiDouble(40),
        ["costRate"] = new OpenApiDouble(120),
        ["currency"] = new OpenApiString("BRL"),
        ["skills"] = new OpenApiArray
        {
            new OpenApiString("Discovery"),
            new OpenApiString("Solution Design")
        },
        ["availability"] = new OpenApiString("Monday to Friday, business hours"),
        ["notes"] = new OpenApiString("Allocated to strategic accounts")
    };

    private static OpenApiObject UpdateExecutionResourceRequestExample() => new()
    {
        ["code"] = new OpenApiString("RES-001"),
        ["name"] = new OpenApiString("Lead Delivery Consultant"),
        ["resourceType"] = new OpenApiString("Internal Human"),
        ["status"] = new OpenApiString("Active"),
        ["capacityHoursPerWeek"] = new OpenApiDouble(32),
        ["costRate"] = new OpenApiDouble(140),
        ["currency"] = new OpenApiString("BRL"),
        ["skills"] = new OpenApiArray
        {
            new OpenApiString("Discovery"),
            new OpenApiString("Delivery Management")
        },
        ["availability"] = new OpenApiString("Monday to Thursday, business hours"),
        ["notes"] = new OpenApiString("Allocated to strategic accounts")
    };

    private static OpenApiObject CreateExecutionResourceExample() => new()
    {
        ["id"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["code"] = new OpenApiString("RES-001"),
        ["name"] = new OpenApiString("Senior Delivery Consultant"),
        ["resourceType"] = new OpenApiString("Internal Human"),
        ["status"] = new OpenApiString("Active"),
        ["capacityHoursPerWeek"] = new OpenApiDouble(40),
        ["costRate"] = new OpenApiDouble(120),
        ["currency"] = new OpenApiString("BRL"),
        ["skills"] = new OpenApiArray
        {
            new OpenApiString("Discovery"),
            new OpenApiString("Solution Design")
        },
        ["availability"] = new OpenApiString("Monday to Friday, business hours"),
        ["notes"] = new OpenApiString("Allocated to strategic accounts"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiArray CreateExecutionResourceListExample() => new()
    {
        CreateExecutionResourceExample()
    };
}

using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Client Management endpoints.
/// </summary>
public sealed class ClientOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ClientsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(ClientsController.GetPaged):
                operation.Summary ??= "List clients";
                operation.Description ??=
                    "Returns a paginated list of clients with optional status, industry, and company name filters.";
                SetJsonResponseExample(operation, "200", CreatePagedClientExample());
                break;

            case nameof(ClientsController.GetById):
                operation.Summary ??= "Get client by id";
                operation.Description ??= "Returns client details by identifier.";
                SetJsonResponseExample(operation, "200", CreateClientExample());
                break;

            case nameof(ClientsController.Create):
                operation.Summary ??= "Create client";
                operation.Description ??=
                    "Creates a new client. Status must be Active or Inactive.";
                SetJsonRequestExample(operation, CreateClientRequestExample());
                SetJsonResponseExample(operation, "201", CreateClientExample());
                break;

            case nameof(ClientsController.Update):
                operation.Summary ??= "Update client";
                operation.Description ??=
                    "Updates an existing client. Status must be Active or Inactive.";
                SetJsonRequestExample(operation, UpdateClientRequestExample());
                SetJsonResponseExample(operation, "200", CreateClientExample());
                break;

            case nameof(ClientsController.Deactivate):
                operation.Summary ??= "Deactivate client";
                operation.Description ??=
                    "Deactivates a client by setting status to Inactive. Historical information is preserved.";
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

    private static OpenApiObject CreateClientRequestExample() => new()
    {
        ["legalName"] = new OpenApiString("Acme Corporation Ltd."),
        ["tradeName"] = new OpenApiString("Acme Corp"),
        ["taxIdentifier"] = new OpenApiString("12-3456789"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["industry"] = new OpenApiString("Technology"),
        ["status"] = new OpenApiString("Active"),
        ["accountOwner"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
    };

    private static OpenApiObject UpdateClientRequestExample() => new()
    {
        ["legalName"] = new OpenApiString("Acme Corporation Ltd."),
        ["tradeName"] = new OpenApiString("Acme Corporation"),
        ["taxIdentifier"] = new OpenApiString("12-3456789"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["industry"] = new OpenApiString("Technology"),
        ["status"] = new OpenApiString("Active"),
        ["accountOwner"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
    };

    private static OpenApiObject CreateClientExample() => new()
    {
        ["id"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["leadId"] = new OpenApiNull(),
        ["legalName"] = new OpenApiString("Acme Corporation Ltd."),
        ["tradeName"] = new OpenApiString("Acme Corp"),
        ["taxIdentifier"] = new OpenApiString("12-3456789"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["industry"] = new OpenApiString("Technology"),
        ["status"] = new OpenApiString("Active"),
        ["accountOwner"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreatePagedClientExample() => new()
    {
        ["items"] = new OpenApiArray { CreateClientExample() },
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalCount"] = new OpenApiInteger(1),
        ["totalPages"] = new OpenApiInteger(1)
    };
}

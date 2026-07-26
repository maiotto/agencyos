using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Lead Management endpoints.
/// </summary>
public sealed class LeadOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(LeadsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(LeadsController.GetPaged):
                operation.Summary ??= "List leads";
                operation.Description ??=
                    "Returns a paginated list of leads. Archived leads are excluded by default. " +
                    "Use includeArchived=true or status=Archived to include archived records.";
                SetJsonResponseExample(operation, "200", CreatePagedLeadExample());
                break;

            case nameof(LeadsController.GetById):
                operation.Summary ??= "Get lead by id";
                operation.Description ??=
                    "Returns lead details by identifier, including archived and converted leads.";
                SetJsonResponseExample(operation, "200", CreateLeadExample());
                break;

            case nameof(LeadsController.Create):
                operation.Summary ??= "Create lead";
                operation.Description ??=
                    "Registers a new lead. Initial status must be Prospect.";
                SetJsonRequestExample(operation, CreateLeadRequestExample());
                SetJsonResponseExample(operation, "201", CreateLeadExample());
                break;

            case nameof(LeadsController.Update):
                operation.Summary ??= "Update lead";
                operation.Description ??=
                    "Updates an editable lead. Status changes must follow the approved lifecycle transitions.";
                SetJsonRequestExample(operation, UpdateLeadRequestExample());
                SetJsonResponseExample(operation, "200", CreateLeadExample());
                break;

            case nameof(LeadsController.Archive):
                operation.Summary ??= "Archive lead";
                operation.Description ??=
                    "Archives a lead. Converted leads cannot be archived. Historical information is preserved.";
                break;

            case nameof(LeadsController.Convert):
                operation.Summary ??= "Convert lead to client";
                operation.Description ??=
                    "Converts a Won lead into a client using the supplied legal name and tax identifier. Returns HTTP 200.";
                SetJsonRequestExample(operation, ConvertLeadRequestExample());
                SetJsonResponseExample(operation, "200", ConvertLeadResponseExample());
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

    private static OpenApiObject CreateLeadRequestExample() => new()
    {
        ["companyName"] = new OpenApiString("Acme Corporation"),
        ["leadName"] = new OpenApiString("Alex Rivera"),
        ["email"] = new OpenApiString("alex.rivera@acme.example"),
        ["phone"] = new OpenApiString("+1-555-1000"),
        ["source"] = new OpenApiString("Referral"),
        ["status"] = new OpenApiString("Prospect"),
        ["estimatedContractValue"] = new OpenApiDouble(85000),
        ["assignedUserId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["segment"] = new OpenApiString("Technology"),
        ["notes"] = new OpenApiString("Inbound commercial opportunity")
    };

    private static OpenApiObject UpdateLeadRequestExample() => new()
    {
        ["companyName"] = new OpenApiString("Acme Corporation"),
        ["leadName"] = new OpenApiString("Alex Rivera"),
        ["email"] = new OpenApiString("alex.rivera@acme.example"),
        ["phone"] = new OpenApiString("+1-555-1000"),
        ["source"] = new OpenApiString("Referral"),
        ["status"] = new OpenApiString("Qualified"),
        ["estimatedContractValue"] = new OpenApiDouble(85000),
        ["assignedUserId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["segment"] = new OpenApiString("Technology"),
        ["notes"] = new OpenApiString("Qualified after discovery call")
    };

    private static OpenApiObject ConvertLeadRequestExample() => new()
    {
        ["legalName"] = new OpenApiString("Acme Corporation Ltd"),
        ["taxIdentifier"] = new OpenApiString("12.345.678/0001-90"),
        ["tradeName"] = new OpenApiString("Acme"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["accountOwner"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
    };

    private static OpenApiObject CreateLeadExample() => new()
    {
        ["id"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["code"] = new OpenApiString("LED-A1B2C3D4"),
        ["companyName"] = new OpenApiString("Acme Corporation"),
        ["leadName"] = new OpenApiString("Alex Rivera"),
        ["email"] = new OpenApiString("alex.rivera@acme.example"),
        ["phone"] = new OpenApiString("+1-555-1000"),
        ["source"] = new OpenApiString("Referral"),
        ["status"] = new OpenApiString("Prospect"),
        ["estimatedContractValue"] = new OpenApiDouble(85000),
        ["assignedUserId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        ["website"] = new OpenApiString("https://acme.example"),
        ["segment"] = new OpenApiString("Technology"),
        ["notes"] = new OpenApiString("Inbound commercial opportunity"),
        ["clientId"] = new OpenApiNull(),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreatePagedLeadExample() => new()
    {
        ["items"] = new OpenApiArray { CreateLeadExample() },
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalCount"] = new OpenApiInteger(1),
        ["totalPages"] = new OpenApiInteger(1)
    };

    private static OpenApiObject ConvertLeadResponseExample() => new()
    {
        ["lead"] = CreateLeadExample(),
        ["clientId"] = new OpenApiString("22222222-2222-4222-8222-222222222222")
    };
}

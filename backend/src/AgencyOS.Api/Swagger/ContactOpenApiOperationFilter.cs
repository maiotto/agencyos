using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Client Contact Management endpoints.
/// </summary>
public sealed class ContactOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ContactsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(ContactsController.GetPaged):
                operation.Summary ??= "List contacts";
                operation.Description ??=
                    "Returns a paginated list of client contacts with optional client, status, and primary filters.";
                SetJsonResponseExample(operation, "200", CreatePagedContactExample());
                break;

            case nameof(ContactsController.GetById):
                operation.Summary ??= "Get contact by id";
                operation.Description ??= "Returns client contact details by identifier.";
                SetJsonResponseExample(operation, "200", CreateContactExample());
                break;

            case nameof(ContactsController.Create):
                operation.Summary ??= "Create contact";
                operation.Description ??=
                    "Creates a new client contact. Status must be Active or Inactive. Only one Primary Contact is allowed per Client.";
                SetJsonRequestExample(operation, CreateContactRequestExample());
                SetJsonResponseExample(operation, "201", CreateContactExample());
                break;

            case nameof(ContactsController.Update):
                operation.Summary ??= "Update contact";
                operation.Description ??=
                    "Updates an existing client contact. Status must be Active or Inactive.";
                SetJsonRequestExample(operation, UpdateContactRequestExample());
                SetJsonResponseExample(operation, "200", CreateContactExample());
                break;

            case nameof(ContactsController.Deactivate):
                operation.Summary ??= "Deactivate contact";
                operation.Description ??=
                    "Deactivates a client contact by setting status to Inactive. Mobile and historical information are preserved.";
                break;

            case nameof(ContactsController.SetPrimary):
                operation.Summary ??= "Set primary contact";
                operation.Description ??=
                    "Designates the contact as Primary for its Client. Clears any previous Primary Contact for the same Client. Inactive Contacts cannot become Primary.";
                SetJsonResponseExample(operation, "200", CreateContactExample());
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

    private static OpenApiObject CreateContactRequestExample() => new()
    {
        ["clientId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["firstName"] = new OpenApiString("Alex"),
        ["lastName"] = new OpenApiString("Morgan"),
        ["jobTitle"] = new OpenApiString("Account Manager"),
        ["email"] = new OpenApiString("alex.morgan@acme.example"),
        ["phone"] = new OpenApiString("+1-555-0100"),
        ["mobile"] = new OpenApiString("+1-555-0199"),
        ["isPrimaryContact"] = new OpenApiBoolean(true),
        ["status"] = new OpenApiString("Active")
    };

    private static OpenApiObject UpdateContactRequestExample() => new()
    {
        ["firstName"] = new OpenApiString("Alex"),
        ["lastName"] = new OpenApiString("Morgan"),
        ["jobTitle"] = new OpenApiString("Senior Account Manager"),
        ["email"] = new OpenApiString("alex.morgan@acme.example"),
        ["phone"] = new OpenApiString("+1-555-0100"),
        ["mobile"] = new OpenApiString("+1-555-0199"),
        ["isPrimaryContact"] = new OpenApiBoolean(true),
        ["status"] = new OpenApiString("Active")
    };

    private static OpenApiObject CreateContactExample() => new()
    {
        ["id"] = new OpenApiString("33333333-3333-4333-8333-333333333333"),
        ["clientId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["firstName"] = new OpenApiString("Alex"),
        ["lastName"] = new OpenApiString("Morgan"),
        ["jobTitle"] = new OpenApiString("Account Manager"),
        ["email"] = new OpenApiString("alex.morgan@acme.example"),
        ["phone"] = new OpenApiString("+1-555-0100"),
        ["mobile"] = new OpenApiString("+1-555-0199"),
        ["isPrimaryContact"] = new OpenApiBoolean(true),
        ["status"] = new OpenApiString("Active"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreatePagedContactExample() => new()
    {
        ["items"] = new OpenApiArray { CreateContactExample() },
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalCount"] = new OpenApiInteger(1),
        ["totalPages"] = new OpenApiInteger(1)
    };
}

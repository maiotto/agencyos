using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Company (Multi-Company Configuration) endpoints (US-402).
/// </summary>
public sealed class CompanyOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(CompaniesController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(CompaniesController.GetAll):
                operation.Summary ??= "List companies";
                operation.Description ??=
                    "Returns Companies with optional status/search filters. Archived Companies are " +
                    "excluded unless IncludeArchived is set (BR-2009).";
                SetJsonResponseExample(operation, "200", CreateCompanyListExample());
                break;

            case nameof(CompaniesController.GetActive):
                operation.Summary ??= "Get active company";
                operation.Description ??=
                    "Returns the Company selected for the current request context (X-Company-Id), " +
                    "or the seeded default Company when none is selected.";
                SetJsonResponseExample(operation, "200", CreateCompanyExample());
                break;

            case nameof(CompaniesController.GetById):
                operation.Summary ??= "Get company by id";
                SetJsonResponseExample(operation, "200", CreateCompanyExample());
                break;

            case nameof(CompaniesController.Create):
                operation.Summary ??= "Create company";
                operation.Description ??= "Creates a new Company in Active status.";
                SetJsonRequestExample(operation, CreateCompanyRequestExample());
                SetJsonResponseExample(operation, "201", CreateCompanyExample());
                break;

            case nameof(CompaniesController.Update):
                operation.Summary ??= "Update company";
                operation.Description ??=
                    "Updates a Company's configuration. DecisionProfileId must reference the company's " +
                    "Active default Company Decision Profile (BR-2007); DefaultPlanningTemplateId must " +
                    "reference a Planning Template belonging to the company (BR-2008).";
                SetJsonRequestExample(operation, UpdateCompanyRequestExample());
                SetJsonResponseExample(operation, "200", CreateCompanyExample());
                break;

            case nameof(CompaniesController.Activate):
                operation.Summary ??= "Activate company";
                operation.Description ??= "Activates a Company so it becomes eligible for selection (BR-2003).";
                break;

            case nameof(CompaniesController.Deactivate):
                operation.Summary ??= "Deactivate company";
                operation.Description ??= "Deactivates a Company while preserving history.";
                break;

            case nameof(CompaniesController.Archive):
                operation.Summary ??= "Archive company";
                operation.Description ??=
                    "Archives a Company. Archived Companies cannot be modified or selected (BR-2003/BR-2009).";
                break;

            case nameof(CompaniesController.Select):
                operation.Summary ??= "Select active company";
                operation.Description ??=
                    "Selects a Company as the active company for the current request context. " +
                    "Only Active Companies may be selected (BR-2003).";
                SetJsonRequestExample(operation, SelectCompanyRequestExample());
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

    private static OpenApiObject CreateCompanyRequestExample() => new()
    {
        ["companyCode"] = new OpenApiString("ACME"),
        ["companyName"] = new OpenApiString("Acme Agency"),
        ["legalName"] = new OpenApiString("Acme Agency, LLC"),
        ["timezone"] = new OpenApiString("America/Sao_Paulo"),
        ["country"] = new OpenApiString("BR"),
        ["language"] = new OpenApiString("pt-BR"),
        ["currency"] = new OpenApiString("BRL"),
        ["planningConfiguration"] = new OpenApiString("{}"),
        ["decisionProfileId"] = new OpenApiNull(),
        ["defaultCalendarId"] = new OpenApiNull(),
        ["defaultPlanningTemplateId"] = new OpenApiNull()
    };

    private static OpenApiObject UpdateCompanyRequestExample() => new()
    {
        ["companyName"] = new OpenApiString("Acme Agency"),
        ["legalName"] = new OpenApiString("Acme Agency, LLC"),
        ["timezone"] = new OpenApiString("America/Sao_Paulo"),
        ["country"] = new OpenApiString("BR"),
        ["language"] = new OpenApiString("pt-BR"),
        ["currency"] = new OpenApiString("BRL"),
        ["planningConfiguration"] = new OpenApiString("{}"),
        ["decisionProfileId"] = new OpenApiNull(),
        ["defaultCalendarId"] = new OpenApiNull(),
        ["defaultPlanningTemplateId"] = new OpenApiNull()
    };

    private static OpenApiObject SelectCompanyRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa")
    };

    private static OpenApiObject CreateCompanyExample() => new()
    {
        ["id"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["companyCode"] = new OpenApiString("DEFAULT"),
        ["companyName"] = new OpenApiString("Default Agency"),
        ["legalName"] = new OpenApiNull(),
        ["status"] = new OpenApiString("Active"),
        ["timezone"] = new OpenApiString("UTC"),
        ["country"] = new OpenApiNull(),
        ["language"] = new OpenApiNull(),
        ["currency"] = new OpenApiNull(),
        ["planningConfiguration"] = new OpenApiString("{}"),
        ["decisionProfileId"] = new OpenApiNull(),
        ["defaultCalendarId"] = new OpenApiNull(),
        ["defaultPlanningTemplateId"] = new OpenApiNull(),
        ["createdAt"] = new OpenApiString("2026-07-26T15:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T15:00:00+00:00"),
        ["archivedAt"] = new OpenApiNull()
    };

    private static OpenApiArray CreateCompanyListExample() => new()
    {
        CreateCompanyExample()
    };
}

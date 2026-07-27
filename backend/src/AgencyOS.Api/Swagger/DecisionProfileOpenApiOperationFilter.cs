using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Company Decision Profile endpoints (US-401).
/// </summary>
public sealed class DecisionProfileOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(DecisionProfilesController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(DecisionProfilesController.GetAll):
                operation.Summary ??= "List Company Decision Profiles";
                operation.Description ??=
                    "Returns the latest version per profile family, with optional filters.";
                SetJsonResponseExample(operation, "200", CreateProfileListExample());
                break;

            case nameof(DecisionProfilesController.Filter):
                operation.Summary ??= "Filter Company Decision Profiles";
                operation.Description ??=
                    "Filters latest-version Company Decision Profiles by company, status, name, code, and default flag.";
                SetJsonResponseExample(operation, "200", CreateProfileListExample());
                break;

            case nameof(DecisionProfilesController.GetById):
                operation.Summary ??= "Get Company Decision Profile by id";
                SetJsonResponseExample(operation, "200", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.GetByCompanyId):
                operation.Summary ??= "List Company Decision Profiles by company";
                operation.Description ??= "Returns the latest version of every profile family for a company.";
                SetJsonResponseExample(operation, "200", CreateProfileListExample());
                break;

            case nameof(DecisionProfilesController.GetDefaultActive):
                operation.Summary ??= "Get default Active Company Decision Profile";
                operation.Description ??=
                    "Returns the single default Active profile for a company (BR-1901).";
                SetJsonResponseExample(operation, "200", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.Create):
                operation.Summary ??= "Create Company Decision Profile";
                operation.Description ??= "Creates a new Company Decision Profile (version 1, Active).";
                SetJsonRequestExample(operation, CreateProfileRequestExample());
                SetJsonResponseExample(operation, "201", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.Update):
                operation.Summary ??= "Update Company Decision Profile";
                operation.Description ??=
                    "Creates a new immutable version (BR-1905) and deactivates the previous version. " +
                    "Returns the newly created version, including its new Id.";
                SetJsonRequestExample(operation, UpdateProfileRequestExample());
                SetJsonResponseExample(operation, "200", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.Clone):
                operation.Summary ??= "Clone Company Decision Profile";
                operation.Description ??=
                    "Clones a profile into a new lineage with a new Name and Code (version 1, not Default).";
                SetJsonRequestExample(operation, CloneProfileRequestExample());
                SetJsonResponseExample(operation, "201", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.Activate):
                operation.Summary ??= "Activate Company Decision Profile";
                operation.Description ??=
                    "Activates a profile so it becomes eligible for ranking and AI Decision Support (BR-1904).";
                break;

            case nameof(DecisionProfilesController.Deactivate):
                operation.Summary ??= "Deactivate Company Decision Profile";
                operation.Description ??= "Deactivates a profile. The current default profile cannot be deactivated.";
                break;

            case nameof(DecisionProfilesController.Archive):
                operation.Summary ??= "Archive Company Decision Profile";
                operation.Description ??= "Archives a profile. The current default profile cannot be archived.";
                break;

            case nameof(DecisionProfilesController.SetDefault):
                operation.Summary ??= "Set default Company Decision Profile";
                operation.Description ??=
                    "Sets this profile as the company's default Active profile (BR-1901). Only one default is allowed per company.";
                SetJsonResponseExample(operation, "200", CreateProfileExample());
                break;

            case nameof(DecisionProfilesController.ClearDefault):
                operation.Summary ??= "Clear default flag";
                operation.Description ??= "Clears the default flag from this profile without assigning a replacement.";
                SetJsonResponseExample(operation, "200", CreateProfileExample());
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

    private static OpenApiArray CreateDimensionsExample() => new()
    {
        new OpenApiObject
        {
            ["dimension"] = new OpenApiString("EstimatedCost"),
            ["weight"] = new OpenApiDouble(0.125),
            ["preferHigherValues"] = new OpenApiBoolean(false)
        },
        new OpenApiObject
        {
            ["dimension"] = new OpenApiString("OperationalRisk"),
            ["weight"] = new OpenApiDouble(0.125),
            ["preferHigherValues"] = new OpenApiBoolean(false)
        }
    };

    private static OpenApiObject CreateProfileRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["code"] = new OpenApiString("CustomBalanced"),
        ["name"] = new OpenApiString("Custom Balanced"),
        ["description"] = new OpenApiString("Custom weighting for this company."),
        ["priorityWeights"] = CreateDimensionsExample(),
        ["capacityWeight"] = new OpenApiDouble(0.2),
        ["workloadWeight"] = new OpenApiDouble(0.2),
        ["costWeight"] = new OpenApiDouble(0.2),
        ["riskWeight"] = new OpenApiDouble(0.2),
        ["qualityWeight"] = new OpenApiDouble(0.2),
        ["preferredStrategy"] = new OpenApiNull(),
        ["preferredCapacityThreshold"] = new OpenApiNull(),
        ["preferredWorkloadThreshold"] = new OpenApiNull(),
        ["defaultProfile"] = new OpenApiBoolean(false)
    };

    private static OpenApiObject UpdateProfileRequestExample() => new()
    {
        ["name"] = new OpenApiString("Custom Balanced (updated)"),
        ["description"] = new OpenApiString("Updated weighting."),
        ["priorityWeights"] = CreateDimensionsExample(),
        ["capacityWeight"] = new OpenApiDouble(0.2),
        ["workloadWeight"] = new OpenApiDouble(0.2),
        ["costWeight"] = new OpenApiDouble(0.2),
        ["riskWeight"] = new OpenApiDouble(0.2),
        ["qualityWeight"] = new OpenApiDouble(0.2),
        ["preferredStrategy"] = new OpenApiNull(),
        ["preferredCapacityThreshold"] = new OpenApiNull(),
        ["preferredWorkloadThreshold"] = new OpenApiNull()
    };

    private static OpenApiObject CloneProfileRequestExample() => new()
    {
        ["name"] = new OpenApiString("Custom Balanced (copy)"),
        ["code"] = new OpenApiString("CustomBalancedCopy")
    };

    private static OpenApiObject CreateProfileExample() => new()
    {
        ["id"] = new OpenApiString("11111111-1111-1111-1111-111111111106"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["profileFamilyId"] = new OpenApiString("11111111-1111-1111-1111-111111111106"),
        ["code"] = new OpenApiString("BalancedStrategy"),
        ["name"] = new OpenApiString("Balanced Strategy"),
        ["description"] = new OpenApiString("Balanced weighting across all ranking dimensions."),
        ["status"] = new OpenApiString("Active"),
        ["dimensions"] = CreateDimensionsExample(),
        ["capacityWeight"] = new OpenApiDouble(0.2),
        ["workloadWeight"] = new OpenApiDouble(0.2),
        ["costWeight"] = new OpenApiDouble(0.2),
        ["riskWeight"] = new OpenApiDouble(0.2),
        ["qualityWeight"] = new OpenApiDouble(0.2),
        ["preferredStrategy"] = new OpenApiNull(),
        ["preferredCapacityThreshold"] = new OpenApiNull(),
        ["preferredWorkloadThreshold"] = new OpenApiNull(),
        ["defaultProfile"] = new OpenApiBoolean(true),
        ["version"] = new OpenApiInteger(1),
        ["createdAt"] = new OpenApiString("2026-07-26T15:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T15:00:00+00:00"),
        ["archivedAt"] = new OpenApiNull()
    };

    private static OpenApiArray CreateProfileListExample() => new()
    {
        CreateProfileExample()
    };
}

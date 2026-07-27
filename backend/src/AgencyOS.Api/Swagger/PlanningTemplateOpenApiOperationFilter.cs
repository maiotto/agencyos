using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Planning Template endpoints (US-108).
/// </summary>
public sealed class PlanningTemplateOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PlanningTemplatesController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(PlanningTemplatesController.GetAll):
                operation.Summary ??= "List planning templates";
                operation.Description ??= "Returns planning templates with optional company, status, name, and reference filters.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(PlanningTemplatesController.Filter):
                operation.Summary ??= "Filter planning templates";
                operation.Description ??= "Search and filter planning templates by company, status, calendar, hours, and free-text search.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(PlanningTemplatesController.GetById):
                operation.Summary ??= "Get planning template by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(PlanningTemplatesController.Create):
                operation.Summary ??= "Create planning template";
                operation.Description ??= "Creates an Inactive planning template that references Working Calendar and Working Hours (configuration only).";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(PlanningTemplatesController.Update):
                operation.Summary ??= "Update planning template";
                SetJsonRequestExample(operation, UpdateRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(PlanningTemplatesController.Activate):
                operation.Summary ??= "Activate planning template";
                operation.Description ??= "Activates a template. Referenced Working Calendar and Working Hours must be Active (BR-803/BR-804).";
                break;

            case nameof(PlanningTemplatesController.Deactivate):
                operation.Summary ??= "Deactivate planning template";
                operation.Description ??= "Deactivates a template. Inactive templates cannot be applied (BR-805).";
                break;

            case nameof(PlanningTemplatesController.Delete):
                operation.Summary ??= "Delete inactive planning template";
                operation.Description ??= "Deletes an inactive template. Active templates cannot be deleted (BR-806).";
                break;

            case nameof(PlanningTemplatesController.Clone):
                operation.Summary ??= "Clone planning template";
                operation.Description ??= "Copies configuration into a new Inactive template. No operational history is copied (BR-809).";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["name"] = new OpenApiString("Standard Weekly Planning (Copy)")
                });
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(PlanningTemplatesController.Apply):
                operation.Summary ??= "Apply planning template";
                operation.Description ??=
                    "Produces a new planning configuration from an Active template without mutating the template (BR-807/BR-808). Optionally calculates Capacity and/or Workload. Historical Capacity/Workload records remain immutable (BR-810).";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["periodStartDate"] = new OpenApiString("2026-07-01"),
                    ["periodEndDate"] = new OpenApiString("2026-07-07"),
                    ["calculateCapacity"] = new OpenApiBoolean(true),
                    ["calculateWorkload"] = new OpenApiBoolean(false)
                });
                SetJsonResponseExample(operation, "200", CreateAppliedExample());
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

    private static OpenApiObject CreateExample() => new()
    {
        ["id"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Standard Weekly Planning"),
        ["description"] = new OpenApiString("Default 7-day planning window"),
        ["status"] = new OpenApiString("Inactive"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["resourceAvailabilityStrategy"] = new OpenApiString("RequireActiveConfiguration"),
        ["defaultPlanningWindowDays"] = new OpenApiInteger(7),
        ["defaultPeriodStartOffsetDays"] = new OpenApiInteger(0),
        ["utilizationWarningPercentage"] = new OpenApiDouble(85),
        ["includeAssignmentDistribution"] = new OpenApiBoolean(true),
        ["planningParametersJson"] = new OpenApiNull()
    };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };

    private static OpenApiObject CreateRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Standard Weekly Planning"),
        ["description"] = new OpenApiString("Default 7-day planning window"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["resourceAvailabilityStrategy"] = new OpenApiString("RequireActiveConfiguration"),
        ["defaultPlanningWindowDays"] = new OpenApiInteger(7),
        ["defaultPeriodStartOffsetDays"] = new OpenApiInteger(0),
        ["utilizationWarningPercentage"] = new OpenApiDouble(85),
        ["includeAssignmentDistribution"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject UpdateRequestExample() => new()
    {
        ["name"] = new OpenApiString("Standard Weekly Planning"),
        ["description"] = new OpenApiString("Updated description"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["resourceAvailabilityStrategy"] = new OpenApiString("RequireActiveConfiguration"),
        ["defaultPlanningWindowDays"] = new OpenApiInteger(14),
        ["defaultPeriodStartOffsetDays"] = new OpenApiInteger(0),
        ["includeAssignmentDistribution"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject CreateAppliedExample() => new()
    {
        ["sourceTemplateId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["sourceTemplateName"] = new OpenApiString("Standard Weekly Planning"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["resourceAvailabilityStrategy"] = new OpenApiString("RequireActiveConfiguration"),
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["utilizationWarningPercentage"] = new OpenApiDouble(85),
        ["includeAssignmentDistribution"] = new OpenApiBoolean(true)
    };
}

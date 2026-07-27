using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Working Hours endpoints (US-103).
/// </summary>
public sealed class WorkingHoursOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(WorkingHoursController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(WorkingHoursController.GetAll):
                operation.Summary ??= "List working hours";
                operation.Description ??= "Returns working hours configurations with optional calendar, status, and name filters.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(WorkingHoursController.GetById):
                operation.Summary ??= "Get working hours by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(WorkingHoursController.Create):
                operation.Summary ??= "Create working hours";
                operation.Description ??= "Creates a Working Hours configuration associated with a Working Calendar.";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(WorkingHoursController.Update):
                operation.Summary ??= "Update working hours";
                operation.Description ??= "Updates weekday schedules and validity. Historical configurations cannot be reconfigured (BR-309).";
                SetJsonRequestExample(operation, UpdateRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(WorkingHoursController.Activate):
                operation.Summary ??= "Activate working hours";
                operation.Description ??= "Activates a configuration. Only one active configuration may exist for the same calendar period (BR-308).";
                break;

            case nameof(WorkingHoursController.Deactivate):
                operation.Summary ??= "Deactivate working hours";
                operation.Description ??= "Deactivates a configuration so it is not used by Planning (BR-307).";
                break;

            case nameof(WorkingHoursController.Delete):
                operation.Summary ??= "Delete inactive working hours";
                operation.Description ??= "Deletes an inactive configuration that has not yet started.";
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

    private static OpenApiObject CreateRequestExample() => new()
    {
        ["workingCalendarId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["name"] = new OpenApiString("Standard Office Hours"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2026-12-31"),
        ["days"] = CreateDaysExample()
    };

    private static OpenApiObject UpdateRequestExample() => new()
    {
        ["name"] = new OpenApiString("Standard Office Hours"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2027-06-30"),
        ["days"] = CreateDaysExample()
    };

    private static OpenApiArray CreateDaysExample() => new()
    {
        CreateDayExample("Monday", true),
        CreateDayExample("Tuesday", true),
        CreateDayExample("Wednesday", true),
        CreateDayExample("Thursday", true),
        CreateDayExample("Friday", true),
        CreateDayExample("Saturday", false),
        CreateDayExample("Sunday", false)
    };

    private static OpenApiObject CreateDayExample(string day, bool enabled) => new()
    {
        ["dayOfWeek"] = new OpenApiString(day),
        ["enabled"] = new OpenApiBoolean(enabled),
        ["startTime"] = enabled ? new OpenApiString("09:00:00") : new OpenApiNull(),
        ["endTime"] = enabled ? new OpenApiString("18:00:00") : new OpenApiNull(),
        ["breakStart"] = enabled ? new OpenApiString("12:00:00") : new OpenApiNull(),
        ["breakEnd"] = enabled ? new OpenApiString("13:00:00") : new OpenApiNull()
    };

    private static OpenApiObject CreateExample() => new()
    {
        ["id"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
        ["workingCalendarId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["name"] = new OpenApiString("Standard Office Hours"),
        ["status"] = new OpenApiString("Inactive"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2026-12-31"),
        ["days"] = CreateDaysExample(),
        ["createdAt"] = new OpenApiString("2026-07-26T16:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T16:00:00+00:00")
    };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };
}

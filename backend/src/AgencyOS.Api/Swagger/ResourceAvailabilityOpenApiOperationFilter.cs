using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Resource Availability endpoints (US-104).
/// </summary>
public sealed class ResourceAvailabilityOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ResourceAvailabilitiesController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(ResourceAvailabilitiesController.GetAll):
                operation.Summary ??= "List resource availabilities";
                operation.Description ??=
                    "Returns Resource Availability configurations with optional resource, calendar, status, and name filters.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(ResourceAvailabilitiesController.GetOperational):
                operation.Summary ??= "Resolve operational resource availability";
                operation.Description ??=
                    "Resolves planned availability for a resource on a date using Active Resource Availability, Working Calendar, Holidays, and Working Hours. Returns a business-rule error when operational configuration is missing (no Monday–Friday fallback).";
                SetJsonResponseExample(operation, "200", CreateOperationalExample());
                break;

            case nameof(ResourceAvailabilitiesController.GetById):
                operation.Summary ??= "Get resource availability by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(ResourceAvailabilitiesController.Create):
                operation.Summary ??= "Create resource availability";
                operation.Description ??=
                    "Creates a Resource Availability configuration linked to an Execution Resource, Working Calendar, and Working Hours.";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(ResourceAvailabilitiesController.Update):
                operation.Summary ??= "Update resource availability";
                operation.Description ??=
                    "Updates associations, weekly flags, and daily overrides. Historical configurations cannot be reconfigured (BR-408).";
                SetJsonRequestExample(operation, UpdateRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(ResourceAvailabilitiesController.Activate):
                operation.Summary ??= "Activate resource availability";
                operation.Description ??=
                    "Activates a configuration. Only one active configuration may exist for the same resource period (BR-407 / BR-409).";
                break;

            case nameof(ResourceAvailabilitiesController.Deactivate):
                operation.Summary ??= "Deactivate resource availability";
                operation.Description ??=
                    "Deactivates a configuration so it does not participate in operational availability (BR-410).";
                break;

            case nameof(ResourceAvailabilitiesController.Delete):
                operation.Summary ??= "Delete inactive resource availability";
                operation.Description ??=
                    "Deletes an inactive configuration that has not yet started.";
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
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["name"] = new OpenApiString("Consultant Standard Availability"),
        ["status"] = new OpenApiString("Active"),
        ["effectiveFrom"] = new OpenApiString("2026-01-01"),
        ["effectiveTo"] = new OpenApiNull(),
        ["weeklyAvailability"] = new OpenApiArray
        {
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Monday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Tuesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Wednesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Thursday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Friday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Saturday"), ["enabled"] = new OpenApiBoolean(false) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Sunday"), ["enabled"] = new OpenApiBoolean(false) }
        },
        ["dailyOverrides"] = new OpenApiArray(),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00Z"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00Z")
    };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };

    private static OpenApiObject CreateOperationalExample() => new()
    {
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["date"] = new OpenApiString("2026-07-01"),
        ["isResourceAvailable"] = new OpenApiBoolean(true),
        ["isOperationalWorkingDay"] = new OpenApiBoolean(true),
        ["isHoliday"] = new OpenApiBoolean(false),
        ["resourceAvailabilityId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["plannedNetHours"] = new OpenApiDouble(8),
        ["holidays"] = new OpenApiArray()
    };

    private static OpenApiObject CreateRequestExample() => new()
    {
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["name"] = new OpenApiString("Consultant Standard Availability"),
        ["effectiveFrom"] = new OpenApiString("2026-01-01"),
        ["effectiveTo"] = new OpenApiNull(),
        ["weeklyAvailability"] = new OpenApiArray
        {
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Monday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Tuesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Wednesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Thursday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Friday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Saturday"), ["enabled"] = new OpenApiBoolean(false) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Sunday"), ["enabled"] = new OpenApiBoolean(false) }
        },
        ["dailyOverrides"] = new OpenApiArray()
    };

    private static OpenApiObject UpdateRequestExample() => new()
    {
        ["workingCalendarId"] = new OpenApiString("11111111-1111-4111-8111-111111111111"),
        ["workingHoursId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["name"] = new OpenApiString("Consultant Standard Availability"),
        ["effectiveFrom"] = new OpenApiString("2026-01-01"),
        ["effectiveTo"] = new OpenApiNull(),
        ["weeklyAvailability"] = new OpenApiArray
        {
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Monday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Tuesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Wednesday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Thursday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Friday"), ["enabled"] = new OpenApiBoolean(true) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Saturday"), ["enabled"] = new OpenApiBoolean(false) },
            new OpenApiObject { ["dayOfWeek"] = new OpenApiString("Sunday"), ["enabled"] = new OpenApiBoolean(false) }
        },
        ["dailyOverrides"] = new OpenApiArray()
    };
}

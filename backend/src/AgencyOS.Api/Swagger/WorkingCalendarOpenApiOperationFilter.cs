using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Working Calendar endpoints (US-101).
/// </summary>
public sealed class WorkingCalendarOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(WorkingCalendarsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(WorkingCalendarsController.GetAll):
                operation.Summary ??= "List working calendars";
                operation.Description ??=
                    "Returns working calendars with optional company, status, and name filters.";
                SetJsonResponseExample(operation, "200", CreateWorkingCalendarListExample());
                break;

            case nameof(WorkingCalendarsController.GetById):
                operation.Summary ??= "Get working calendar by id";
                operation.Description ??= "Returns working calendar details by identifier.";
                SetJsonResponseExample(operation, "200", CreateWorkingCalendarExample());
                break;

            case nameof(WorkingCalendarsController.GetActiveForCompany):
                operation.Summary ??= "Get active working calendar for company";
                operation.Description ??=
                    "Returns the active working calendar covering the given date for a company. When date is omitted, UTC today is used.";
                SetJsonResponseExample(operation, "200", CreateWorkingCalendarExample());
                break;

            case nameof(WorkingCalendarsController.GetOperationalWorkingDay):
                operation.Summary ??= "Evaluate operational working day";
                operation.Description ??=
                    "Combines the active Working Calendar with active Holidays to determine whether a date is an operational working day (US-102 integration).";
                break;

            case nameof(WorkingCalendarsController.GetHolidaysForCalendar):
                operation.Summary ??= "List holidays for working calendar";
                operation.Description ??=
                    "Returns active holidays that apply to the calendar company within the requested or calendar validity range.";
                break;

            case nameof(WorkingCalendarsController.GetWorkingHoursForCalendar):
                operation.Summary ??= "List working hours for working calendar";
                operation.Description ??=
                    "Returns Working Hours configurations associated with the Working Calendar (US-103).";
                break;

            case nameof(WorkingCalendarsController.Create):
                operation.Summary ??= "Create working calendar";
                operation.Description ??=
                    "Creates a new working calendar in Inactive status with the configured working days and validity period.";
                SetJsonRequestExample(operation, CreateWorkingCalendarRequestExample());
                SetJsonResponseExample(operation, "201", CreateWorkingCalendarExample());
                break;

            case nameof(WorkingCalendarsController.Update):
                operation.Summary ??= "Update working calendar";
                operation.Description ??=
                    "Updates an existing working calendar. Calendars that have already started cannot be structurally modified (BR-109).";
                SetJsonRequestExample(operation, UpdateWorkingCalendarRequestExample());
                SetJsonResponseExample(operation, "200", CreateWorkingCalendarExample());
                break;

            case nameof(WorkingCalendarsController.Activate):
                operation.Summary ??= "Activate working calendar";
                operation.Description ??=
                    "Activates a working calendar. Only one active calendar may exist for the same company and overlapping period (BR-107).";
                break;

            case nameof(WorkingCalendarsController.Deactivate):
                operation.Summary ??= "Deactivate working calendar";
                operation.Description ??=
                    "Deactivates a working calendar. Historical information is preserved and the call is idempotent.";
                break;

            case nameof(WorkingCalendarsController.Delete):
                operation.Summary ??= "Delete inactive working calendar";
                operation.Description ??=
                    "Physically deletes an inactive working calendar that has not yet started. Active and historical calendars cannot be deleted.";
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

    private static OpenApiObject CreateWorkingCalendarRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Standard Agency Week"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2026-12-31"),
        ["workingDays"] = new OpenApiArray
        {
            new OpenApiString("Monday"),
            new OpenApiString("Tuesday"),
            new OpenApiString("Wednesday"),
            new OpenApiString("Thursday"),
            new OpenApiString("Friday")
        }
    };

    private static OpenApiObject UpdateWorkingCalendarRequestExample() => new()
    {
        ["name"] = new OpenApiString("Standard Agency Week (Updated)"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2027-06-30"),
        ["workingDays"] = new OpenApiArray
        {
            new OpenApiString("Monday"),
            new OpenApiString("Tuesday"),
            new OpenApiString("Wednesday"),
            new OpenApiString("Thursday"),
            new OpenApiString("Friday")
        }
    };

    private static OpenApiObject CreateWorkingCalendarExample() => new()
    {
        ["id"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Standard Agency Week"),
        ["status"] = new OpenApiString("Inactive"),
        ["effectiveFrom"] = new OpenApiString("2026-08-01"),
        ["effectiveTo"] = new OpenApiString("2026-12-31"),
        ["workingDays"] = new OpenApiArray
        {
            new OpenApiString("Monday"),
            new OpenApiString("Tuesday"),
            new OpenApiString("Wednesday"),
            new OpenApiString("Thursday"),
            new OpenApiString("Friday")
        },
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiArray CreateWorkingCalendarListExample() => new()
    {
        CreateWorkingCalendarExample()
    };
}

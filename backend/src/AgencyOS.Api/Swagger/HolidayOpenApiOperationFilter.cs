using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Holiday Management endpoints (US-102).
/// </summary>
public sealed class HolidayOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(HolidaysController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(HolidaysController.GetAll):
                operation.Summary ??= "List holidays";
                operation.Description ??= "Returns holidays with optional search and filter parameters.";
                SetJsonResponseExample(operation, "200", CreateHolidayListExample());
                break;

            case nameof(HolidaysController.Filter):
                operation.Summary ??= "Filter holidays";
                operation.Description ??=
                    "Filters holidays by company, type, status, name, location, recurring flag, and date range.";
                SetJsonResponseExample(operation, "200", CreateHolidayListExample());
                break;

            case nameof(HolidaysController.GetById):
                operation.Summary ??= "Get holiday by id";
                SetJsonResponseExample(operation, "200", CreateHolidayExample());
                break;

            case nameof(HolidaysController.Create):
                operation.Summary ??= "Create holiday";
                operation.Description ??= "Creates a new holiday in Inactive status.";
                SetJsonRequestExample(operation, CreateHolidayRequestExample());
                SetJsonResponseExample(operation, "201", CreateHolidayExample());
                break;

            case nameof(HolidaysController.Update):
                operation.Summary ??= "Update holiday";
                operation.Description ??=
                    "Updates a holiday. Holidays that have already occurred cannot be structurally modified (BR-213).";
                SetJsonRequestExample(operation, UpdateHolidayRequestExample());
                SetJsonResponseExample(operation, "200", CreateHolidayExample());
                break;

            case nameof(HolidaysController.Activate):
                operation.Summary ??= "Activate holiday";
                operation.Description ??= "Activates a holiday so only Active holidays affect Planning (BR-207).";
                break;

            case nameof(HolidaysController.Deactivate):
                operation.Summary ??= "Deactivate holiday";
                operation.Description ??= "Deactivates a holiday while preserving history (BR-208).";
                break;

            case nameof(HolidaysController.Delete):
                operation.Summary ??= "Delete inactive holiday";
                operation.Description ??=
                    "Deletes an inactive holiday. Active holidays must be deactivated first (BR-212).";
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

    private static OpenApiObject CreateHolidayRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Independence Day"),
        ["description"] = new OpenApiString("National holiday"),
        ["holidayType"] = new OpenApiString("National"),
        ["holidayDate"] = new OpenApiString("2026-09-07"),
        ["stateCode"] = new OpenApiNull(),
        ["city"] = new OpenApiNull(),
        ["recurring"] = new OpenApiBoolean(true)
    };

    private static OpenApiObject UpdateHolidayRequestExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Company Foundation Day"),
        ["description"] = new OpenApiString("Company holiday"),
        ["holidayType"] = new OpenApiString("Company"),
        ["holidayDate"] = new OpenApiString("2026-11-15"),
        ["stateCode"] = new OpenApiNull(),
        ["city"] = new OpenApiNull(),
        ["recurring"] = new OpenApiBoolean(false)
    };

    private static OpenApiObject CreateHolidayExample() => new()
    {
        ["id"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["name"] = new OpenApiString("Independence Day"),
        ["description"] = new OpenApiString("National holiday"),
        ["holidayType"] = new OpenApiString("National"),
        ["holidayDate"] = new OpenApiString("2026-09-07"),
        ["stateCode"] = new OpenApiNull(),
        ["city"] = new OpenApiNull(),
        ["recurring"] = new OpenApiBoolean(true),
        ["status"] = new OpenApiString("Inactive"),
        ["createdAt"] = new OpenApiString("2026-07-26T15:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T15:00:00+00:00")
    };

    private static OpenApiArray CreateHolidayListExample() => new()
    {
        CreateHolidayExample()
    };
}

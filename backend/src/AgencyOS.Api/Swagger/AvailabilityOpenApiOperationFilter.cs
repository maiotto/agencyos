using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Availability Analysis endpoints.
/// </summary>
public sealed class AvailabilityOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AvailabilityController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(AvailabilityController.GetAll):
                operation.Summary ??= "Calculate availability for all active resources";
                operation.Description ??=
                    "Calculates operational availability for every Active Execution Resource in the planning period. Availability is derived from certified Capacity and Workload results, respects the Monday-to-Friday working calendar, and never modifies operational data.";
                SetJsonResponseExample(operation, "200", CreateAvailabilityListExample());
                break;

            case nameof(AvailabilityController.GetSummary):
                operation.Summary ??= "Calculate availability summary";
                operation.Description ??=
                    "Calculates an aggregated availability summary across all Active Execution Resources for the planning period.";
                SetJsonResponseExample(operation, "200", CreateAvailabilitySummaryExample());
                break;

            case nameof(AvailabilityController.GetByResourceId):
                operation.Summary ??= "Calculate availability for one resource";
                operation.Description ??=
                    "Calculates operational availability for a specific Active Execution Resource in the planning period. Returns 404 when the resource is missing or not Active.";
                SetJsonResponseExample(operation, "200", CreateAvailabilityExample());
                break;
        }
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

    private static OpenApiObject CreateAvailabilityExample() => new()
    {
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["executionResourceCode"] = new OpenApiString("RES-001"),
        ["executionResourceName"] = new OpenApiString("Senior Delivery Consultant"),
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["nextAvailableDate"] = new OpenApiString("2026-07-01"),
        ["availableHours"] = new OpenApiDouble(30),
        ["occupiedHours"] = new OpenApiDouble(10),
        ["availabilityPercentage"] = new OpenApiDouble(75),
        ["availableTimeSlots"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["startDate"] = new OpenApiString("2026-07-01"),
                ["endDate"] = new OpenApiString("2026-07-03"),
                ["availableHours"] = new OpenApiDouble(18)
            },
            new OpenApiObject
            {
                ["startDate"] = new OpenApiString("2026-07-07"),
                ["endDate"] = new OpenApiString("2026-07-07"),
                ["availableHours"] = new OpenApiDouble(8)
            }
        }
    };

    private static OpenApiObject CreateAvailabilitySummaryExample() => new()
    {
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["activeResourceCount"] = new OpenApiInteger(1),
        ["totalAvailableHours"] = new OpenApiDouble(30),
        ["totalOccupiedHours"] = new OpenApiDouble(10),
        ["overallAvailabilityPercentage"] = new OpenApiDouble(75),
        ["resourcesWithAvailability"] = new OpenApiInteger(1)
    };

    private static OpenApiArray CreateAvailabilityListExample() => new()
    {
        CreateAvailabilityExample()
    };
}

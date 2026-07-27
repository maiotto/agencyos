using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Capacity Planning and Capacity History endpoints.
/// </summary>
public sealed class CapacityOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(CapacityController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(CapacityController.GetAll):
                operation.Summary ??= "Calculate capacity for all active resources";
                operation.Description ??=
                    "Calculates planned capacity for every Active Execution Resource using Active Working Calendars, Holidays, Working Hours, and Resource Availability (BR-501..BR-509). Successful calculations automatically persist immutable Capacity History (US-106 / BR-602). Does not fall back to Monday–Friday defaults when operational configuration is missing.";
                SetJsonResponseExample(operation, "200", CreateCapacityListExample());
                break;

            case nameof(CapacityController.GetSummary):
                operation.Summary ??= "Calculate capacity summary";
                operation.Description ??=
                    "Calculates an aggregated capacity summary across all Active Execution Resources for the planning period. Does not persist Capacity History.";
                SetJsonResponseExample(operation, "200", CreateCapacitySummaryExample());
                break;

            case nameof(CapacityController.GetByResourceId):
                operation.Summary ??= "Calculate capacity for one resource";
                operation.Description ??=
                    "Calculates planned capacity for a specific Active Execution Resource. Successful calculations automatically persist immutable Capacity History (US-106).";
                SetJsonResponseExample(operation, "200", CreateCapacityExample());
                break;

            case nameof(CapacityController.GetHistory):
                operation.Summary ??= "Query capacity history";
                operation.Description ??=
                    "Returns immutable Capacity History records filtered by resource, company, period, and/or calculation version (BR-606). Records are never updated or deleted.";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(CapacityController.GetHistoryById):
                operation.Summary ??= "Get capacity history by id";
                operation.Description ??=
                    "Returns a single immutable Capacity History record including the preserved operational day snapshot (BR-609).";
                SetJsonResponseExample(operation, "200", CreateHistoryExample());
                break;

            case nameof(CapacityController.GetHistoryByResource):
                operation.Summary ??= "Get capacity history by execution resource";
                operation.Description ??=
                    "Returns Capacity History for the specified execution resource, with optional period and version filters.";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(CapacityController.GetHistoryByCompany):
                operation.Summary ??= "Get capacity history by company";
                operation.Description ??=
                    "Returns Capacity History for the specified company, with optional period and version filters.";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(CapacityController.CompareHistory):
                operation.Summary ??= "Compare capacity history records";
                operation.Description ??=
                    "Compares two immutable Capacity History records and returns absolute values plus field-level deltas.";
                SetJsonResponseExample(operation, "200", CreateCompareExample());
                break;

            case nameof(CapacityController.GetHistoryAggregate):
                operation.Summary ??= "Aggregate capacity history";
                operation.Description ??=
                    "Aggregates historical capacity totals across the filtered immutable history set.";
                SetJsonResponseExample(operation, "200", CreateAggregateExample());
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

    private static OpenApiObject CreateCapacityExample() => new()
    {
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["executionResourceCode"] = new OpenApiString("RES-001"),
        ["executionResourceName"] = new OpenApiString("Senior Delivery Consultant"),
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["operationalDayCount"] = new OpenApiInteger(5),
        ["holidayImpactDayCount"] = new OpenApiInteger(0),
        ["resourceAvailabilityExcludedDayCount"] = new OpenApiInteger(0),
        ["configuredWorkingHoursTotal"] = new OpenApiDouble(40),
        ["totalCapacityHours"] = new OpenApiDouble(40),
        ["allocatedHours"] = new OpenApiDouble(10),
        ["availableHours"] = new OpenApiDouble(30),
        ["utilizationPercentage"] = new OpenApiDouble(25),
        ["remainingCapacityHours"] = new OpenApiDouble(30),
        ["operationalDays"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["date"] = new OpenApiString("2026-07-01"),
                ["isOperationalDay"] = new OpenApiBoolean(true),
                ["isCalendarWorkingWeekday"] = new OpenApiBoolean(true),
                ["isHoliday"] = new OpenApiBoolean(false),
                ["isResourceAvailable"] = new OpenApiBoolean(true),
                ["plannedCapacityHours"] = new OpenApiDouble(8),
                ["exclusionReason"] = new OpenApiNull()
            }
        }
    };

    private static OpenApiObject CreateCapacitySummaryExample() => new()
    {
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["activeResourceCount"] = new OpenApiInteger(1),
        ["totalCapacityHours"] = new OpenApiDouble(40),
        ["totalAllocatedHours"] = new OpenApiDouble(10),
        ["totalAvailableHours"] = new OpenApiDouble(30),
        ["overallUtilizationPercentage"] = new OpenApiDouble(25),
        ["totalRemainingCapacityHours"] = new OpenApiDouble(30)
    };

    private static OpenApiArray CreateCapacityListExample() => new()
    {
        CreateCapacityExample()
    };

    private static OpenApiObject CreateHistoryExample() => new()
    {
        ["historyId"] = new OpenApiString("99999999-9999-4999-8999-999999999999"),
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["calculationDate"] = new OpenApiString("2026-07-26T12:00:00Z"),
        ["periodStart"] = new OpenApiString("2026-07-01"),
        ["periodEnd"] = new OpenApiString("2026-07-07"),
        ["workingDays"] = new OpenApiInteger(5),
        ["holidayDays"] = new OpenApiInteger(0),
        ["availableDays"] = new OpenApiInteger(5),
        ["configuredHours"] = new OpenApiDouble(40),
        ["availableHours"] = new OpenApiDouble(30),
        ["capacityHours"] = new OpenApiDouble(40),
        ["allocatedHours"] = new OpenApiDouble(10),
        ["utilizationPercentage"] = new OpenApiDouble(25),
        ["calculationVersion"] = new OpenApiString("1.1.0"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00Z"),
        ["operationalDays"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["date"] = new OpenApiString("2026-07-01"),
                ["isOperationalDay"] = new OpenApiBoolean(true),
                ["plannedCapacityHours"] = new OpenApiDouble(8)
            }
        }
    };

    private static OpenApiArray CreateHistoryListExample() => new()
    {
        CreateHistoryExample()
    };

    private static OpenApiObject CreateCompareExample() => new()
    {
        ["left"] = CreateHistoryExample(),
        ["right"] = CreateHistoryExample(),
        ["capacityHoursDelta"] = new OpenApiDouble(0),
        ["availableHoursDelta"] = new OpenApiDouble(0),
        ["configuredHoursDelta"] = new OpenApiDouble(0),
        ["utilizationPercentageDelta"] = new OpenApiDouble(0),
        ["workingDaysDelta"] = new OpenApiInteger(0),
        ["holidayDaysDelta"] = new OpenApiInteger(0),
        ["availableDaysDelta"] = new OpenApiInteger(0)
    };

    private static OpenApiObject CreateAggregateExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["recordCount"] = new OpenApiInteger(2),
        ["totalConfiguredHours"] = new OpenApiDouble(80),
        ["totalAvailableHours"] = new OpenApiDouble(60),
        ["totalCapacityHours"] = new OpenApiDouble(80),
        ["totalAllocatedHours"] = new OpenApiDouble(20),
        ["averageUtilizationPercentage"] = new OpenApiDouble(25)
    };
}

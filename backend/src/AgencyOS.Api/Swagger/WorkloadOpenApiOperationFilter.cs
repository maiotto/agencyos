using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Workload Analysis and Workload History endpoints.
/// </summary>
public sealed class WorkloadOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(WorkloadController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(WorkloadController.GetAll):
                operation.Summary ??= "Calculate workload for all active resources";
                operation.Description ??=
                    "Calculates planned operational workload for every Active Execution Resource. Successful calculations automatically persist immutable Workload History (US-107 / BR-702).";
                SetJsonResponseExample(operation, "200", CreateWorkloadListExample());
                break;

            case nameof(WorkloadController.GetSummary):
                operation.Summary ??= "Calculate workload summary";
                operation.Description ??=
                    "Calculates an aggregated workload summary. Does not persist Workload History.";
                SetJsonResponseExample(operation, "200", CreateWorkloadSummaryExample());
                break;

            case nameof(WorkloadController.GetByResourceId):
                operation.Summary ??= "Calculate workload for one resource";
                operation.Description ??=
                    "Calculates planned operational workload for a specific Active Execution Resource. Successful calculations automatically persist immutable Workload History (US-107).";
                SetJsonResponseExample(operation, "200", CreateWorkloadExample());
                break;

            case nameof(WorkloadController.GetHistory):
                operation.Summary ??= "Query workload history";
                operation.Description ??=
                    "Returns immutable Workload History filtered by resource, company, period, and/or calculation version (BR-707).";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(WorkloadController.GetHistoryById):
                operation.Summary ??= "Get workload history by id";
                operation.Description ??=
                    "Returns a single immutable Workload History record including the preserved assignment snapshot (BR-709).";
                SetJsonResponseExample(operation, "200", CreateHistoryExample());
                break;

            case nameof(WorkloadController.GetHistoryByResource):
                operation.Summary ??= "Get workload history by execution resource";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(WorkloadController.GetHistoryByCompany):
                operation.Summary ??= "Get workload history by company";
                SetJsonResponseExample(operation, "200", CreateHistoryListExample());
                break;

            case nameof(WorkloadController.CompareHistory):
                operation.Summary ??= "Compare workload history records";
                operation.Description ??=
                    "Compares two immutable Workload History records and returns absolute values plus field-level deltas.";
                SetJsonResponseExample(operation, "200", CreateCompareExample());
                break;

            case nameof(WorkloadController.GetHistoryAggregate):
                operation.Summary ??= "Aggregate workload history";
                operation.Description ??=
                    "Aggregates historical workload totals across the filtered immutable history set.";
                SetJsonResponseExample(operation, "200", CreateAggregateExample());
                break;

            case nameof(WorkloadController.GetHistoryTrends):
                operation.Summary ??= "Workload history trends";
                operation.Description ??=
                    "Returns chronological workload trend points for the filtered history set.";
                SetJsonResponseExample(operation, "200", CreateTrendExample());
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

    private static OpenApiObject CreateWorkloadExample() => new()
    {
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["executionResourceCode"] = new OpenApiString("RES-001"),
        ["executionResourceName"] = new OpenApiString("Senior Delivery Consultant"),
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["totalPlannedHours"] = new OpenApiDouble(24),
        ["assignmentCount"] = new OpenApiInteger(2),
        ["averageHoursPerAssignment"] = new OpenApiDouble(12),
        ["workloadPercentage"] = new OpenApiDouble(60),
        ["assignmentDistribution"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["assignmentId"] = new OpenApiString("88888888-8888-4888-8888-888888888888"),
                ["taskId"] = new OpenApiString("66666666-6666-4666-8666-666666666666"),
                ["assignmentRole"] = new OpenApiString("Responsible"),
                ["plannedHours"] = new OpenApiDouble(16),
                ["plannedStartDate"] = new OpenApiString("2026-07-01"),
                ["plannedEndDate"] = new OpenApiString("2026-07-04"),
                ["status"] = new OpenApiString("Planned")
            }
        }
    };

    private static OpenApiObject CreateWorkloadSummaryExample() => new()
    {
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["activeResourceCount"] = new OpenApiInteger(1),
        ["totalPlannedHours"] = new OpenApiDouble(24),
        ["totalAssignmentCount"] = new OpenApiInteger(2),
        ["averageHoursPerAssignment"] = new OpenApiDouble(12),
        ["overallWorkloadPercentage"] = new OpenApiDouble(60)
    };

    private static OpenApiArray CreateWorkloadListExample() => new()
    {
        CreateWorkloadExample()
    };

    private static OpenApiObject CreateHistoryExample() => new()
    {
        ["historyId"] = new OpenApiString("99999999-9999-4999-8999-999999999999"),
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["calculationDate"] = new OpenApiString("2026-07-26T12:00:00Z"),
        ["periodStart"] = new OpenApiString("2026-07-01"),
        ["periodEnd"] = new OpenApiString("2026-07-07"),
        ["allocatedHours"] = new OpenApiDouble(24),
        ["capacityHours"] = new OpenApiDouble(40),
        ["workloadPercentage"] = new OpenApiDouble(60),
        ["workingDays"] = new OpenApiInteger(5),
        ["holidayDays"] = new OpenApiInteger(0),
        ["availableDays"] = new OpenApiInteger(5),
        ["calculationVersion"] = new OpenApiString("1.1.0"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00Z"),
        ["assignmentCount"] = new OpenApiInteger(2),
        ["averageHoursPerAssignment"] = new OpenApiDouble(12),
        ["executionResourceCode"] = new OpenApiString("RES-001"),
        ["executionResourceName"] = new OpenApiString("Senior Delivery Consultant")
    };

    private static OpenApiArray CreateHistoryListExample() => new()
    {
        CreateHistoryExample()
    };

    private static OpenApiObject CreateCompareExample() => new()
    {
        ["left"] = CreateHistoryExample(),
        ["right"] = CreateHistoryExample(),
        ["allocatedHoursDelta"] = new OpenApiDouble(0),
        ["capacityHoursDelta"] = new OpenApiDouble(0),
        ["workloadPercentageDelta"] = new OpenApiDouble(0),
        ["workingDaysDelta"] = new OpenApiInteger(0),
        ["holidayDaysDelta"] = new OpenApiInteger(0),
        ["availableDaysDelta"] = new OpenApiInteger(0)
    };

    private static OpenApiObject CreateAggregateExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["recordCount"] = new OpenApiInteger(2),
        ["totalAllocatedHours"] = new OpenApiDouble(48),
        ["totalCapacityHours"] = new OpenApiDouble(80),
        ["averageWorkloadPercentage"] = new OpenApiDouble(60)
    };

    private static OpenApiObject CreateTrendExample() => new()
    {
        ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["pointCount"] = new OpenApiInteger(1),
        ["points"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["calculationDate"] = new OpenApiString("2026-07-26T12:00:00Z"),
                ["periodStart"] = new OpenApiString("2026-07-01"),
                ["periodEnd"] = new OpenApiString("2026-07-07"),
                ["allocatedHours"] = new OpenApiDouble(24),
                ["capacityHours"] = new OpenApiDouble(40),
                ["workloadPercentage"] = new OpenApiDouble(60),
                ["calculationVersion"] = new OpenApiString("1.1.0")
            }
        }
    };
}

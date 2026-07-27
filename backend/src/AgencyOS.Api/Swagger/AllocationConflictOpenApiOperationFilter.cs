using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Allocation Conflict Detection endpoints.
/// </summary>
public sealed class AllocationConflictOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AllocationConflictController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(AllocationConflictController.GetAll):
                operation.Summary ??= "Detect allocation conflicts for all active resources";
                operation.Description ??=
                    "Detects operational allocation conflicts for every Active Execution Resource in the planning period. Conflicts are derived from certified Capacity, Workload, and Availability results. Detection reports inconsistencies only and never modifies operational data.";
                SetJsonResponseExample(operation, "200", CreateConflictListExample());
                break;

            case nameof(AllocationConflictController.GetSummary):
                operation.Summary ??= "Detect allocation conflict summary";
                operation.Description ??=
                    "Detects an aggregated allocation conflict summary across all Active Execution Resources for the planning period.";
                SetJsonResponseExample(operation, "200", CreateConflictSummaryExample());
                break;

            case nameof(AllocationConflictController.GetByResourceId):
                operation.Summary ??= "Detect allocation conflicts for one resource";
                operation.Description ??=
                    "Detects operational allocation conflicts for a specific Active Execution Resource in the planning period. Returns 404 when the resource is missing or not Active.";
                SetJsonResponseExample(operation, "200", CreateConflictListExample());
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

    private static OpenApiObject CreateConflictExample() => new()
    {
        ["conflictId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
        ["conflictType"] = new OpenApiString("Capacity Exceeded"),
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["executionResourceCode"] = new OpenApiString("RES-001"),
        ["executionResourceName"] = new OpenApiString("Senior Delivery Consultant"),
        ["relatedAssignments"] = new OpenApiArray
        {
            new OpenApiObject
            {
                ["assignmentId"] = new OpenApiString("88888888-8888-4888-8888-888888888888"),
                ["taskId"] = new OpenApiString("66666666-6666-4666-8666-666666666666"),
                ["assignmentRole"] = new OpenApiString("Responsible"),
                ["plannedHours"] = new OpenApiDouble(30),
                ["plannedStartDate"] = new OpenApiString("2026-07-01"),
                ["plannedEndDate"] = new OpenApiString("2026-07-07")
            }
        },
        ["severity"] = new OpenApiString("High"),
        ["description"] = new OpenApiString(
            "Planned workload of 30 hours exceeds available capacity of 20 hours."),
        ["suggestedResolution"] = new OpenApiString(
            "Reduce planned hours, extend the assignment period, or reassign work to another resource.")
    };

    private static OpenApiObject CreateConflictSummaryExample() => new()
    {
        ["periodStartDate"] = new OpenApiString("2026-07-01"),
        ["periodEndDate"] = new OpenApiString("2026-07-07"),
        ["activeResourceCount"] = new OpenApiInteger(1),
        ["totalConflictCount"] = new OpenApiInteger(1),
        ["criticalConflictCount"] = new OpenApiInteger(0),
        ["highConflictCount"] = new OpenApiInteger(1),
        ["mediumConflictCount"] = new OpenApiInteger(0),
        ["lowConflictCount"] = new OpenApiInteger(0),
        ["resourcesWithConflicts"] = new OpenApiInteger(1)
    };

    private static OpenApiArray CreateConflictListExample() => new()
    {
        CreateConflictExample()
    };
}

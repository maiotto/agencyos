using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Assignment Management endpoints.
/// </summary>
public sealed class AssignmentOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AssignmentsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(AssignmentsController.GetAll):
                operation.Summary ??= "List assignments";
                operation.Description ??=
                    "Returns assignments with optional task, mission, execution resource, resource type, and status filters. Ordered by planned start date unless a different ordering is requested.";
                SetJsonResponseExample(operation, "200", CreateAssignmentListExample());
                break;

            case nameof(AssignmentsController.GetById):
                operation.Summary ??= "Get assignment by id";
                operation.Description ??= "Returns assignment details by identifier.";
                SetJsonResponseExample(operation, "200", CreateAssignmentExample());
                break;

            case nameof(AssignmentsController.Create):
                operation.Summary ??= "Create assignment";
                operation.Description ??=
                    "Creates a new assignment linking a Task to an Active Execution Resource. Assignment Role and Status are persisted using their canonical spelling. Business rule violations return HTTP 400.";
                SetJsonRequestExample(operation, CreateAssignmentRequestExample());
                SetJsonResponseExample(operation, "201", CreateAssignmentExample());
                break;

            case nameof(AssignmentsController.Update):
                operation.Summary ??= "Update assignment";
                operation.Description ??=
                    "Updates an existing assignment. Cancelled Assignments cannot be edited. Status changes to Cancelled must use the dedicated cancel endpoints. Business rule violations return HTTP 400.";
                SetJsonRequestExample(operation, UpdateAssignmentRequestExample());
                SetJsonResponseExample(operation, "200", CreateAssignmentExample());
                break;

            case nameof(AssignmentsController.Cancel):
                operation.Summary ??= "Cancel assignment";
                operation.Description ??=
                    "Cancels an assignment by setting Status to Cancelled. Historical information is preserved. Equivalent soft-cancel behavior via DELETE.";
                break;

            case nameof(AssignmentsController.CancelAssignment):
                operation.Summary ??= "Cancel assignment";
                operation.Description ??=
                    "Cancels an assignment by setting Status to Cancelled. Historical information is preserved and the call is idempotent.";
                SetJsonResponseExample(operation, "200", CreateCancelledAssignmentExample());
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

    private static OpenApiObject CreateAssignmentRequestExample() => new()
    {
        ["taskId"] = new OpenApiString("66666666-6666-4666-8666-666666666666"),
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["assignmentRole"] = new OpenApiString("Responsible"),
        ["plannedHours"] = new OpenApiDouble(24),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-09"),
        ["allocationPercentage"] = new OpenApiInteger(50),
        ["status"] = new OpenApiString("Planned"),
        ["notes"] = new OpenApiString("Primary delivery allocation")
    };

    private static OpenApiObject UpdateAssignmentRequestExample() => new()
    {
        ["assignmentRole"] = new OpenApiString("Responsible"),
        ["plannedHours"] = new OpenApiDouble(32),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-12"),
        ["allocationPercentage"] = new OpenApiInteger(60),
        ["status"] = new OpenApiString("Confirmed"),
        ["notes"] = new OpenApiString("Primary delivery allocation")
    };

    private static OpenApiObject CreateAssignmentExample() => new()
    {
        ["id"] = new OpenApiString("88888888-8888-4888-8888-888888888888"),
        ["taskId"] = new OpenApiString("66666666-6666-4666-8666-666666666666"),
        ["executionResourceId"] = new OpenApiString("77777777-7777-4777-8777-777777777777"),
        ["assignmentRole"] = new OpenApiString("Responsible"),
        ["plannedHours"] = new OpenApiDouble(24),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-09"),
        ["allocationPercentage"] = new OpenApiInteger(50),
        ["status"] = new OpenApiString("Planned"),
        ["notes"] = new OpenApiString("Primary delivery allocation"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreateCancelledAssignmentExample()
    {
        var example = CreateAssignmentExample();
        example["status"] = new OpenApiString("Cancelled");
        return example;
    }

    private static OpenApiArray CreateAssignmentListExample() => new()
    {
        CreateAssignmentExample()
    };
}

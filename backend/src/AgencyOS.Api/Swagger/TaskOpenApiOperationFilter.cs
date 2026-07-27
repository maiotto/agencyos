using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Task Management endpoints.
/// </summary>
public sealed class TaskOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(TasksController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(TasksController.GetAll):
                operation.Summary ??= "List tasks";
                operation.Description ??=
                    "Returns tasks with optional mission, status, priority, and task type filters. Ordered by name, then code.";
                SetJsonResponseExample(operation, "200", CreateTaskListExample());
                break;

            case nameof(TasksController.GetById):
                operation.Summary ??= "Get task by id";
                operation.Description ??= "Returns task details by identifier.";
                SetJsonResponseExample(operation, "200", CreateTaskExample());
                break;

            case nameof(TasksController.Create):
                operation.Summary ??= "Create task";
                operation.Description ??=
                    "Creates a new task for an existing Mission. Task Code must be unique per Mission (case-insensitive).";
                SetJsonRequestExample(operation, CreateTaskRequestExample());
                SetJsonResponseExample(operation, "201", CreateTaskExample());
                break;

            case nameof(TasksController.Update):
                operation.Summary ??= "Update task";
                operation.Description ??=
                    "Updates an existing task. Completed Tasks cannot be edited. Status changes to Completed must use the dedicated complete endpoint. Task Code must remain unique per Mission (case-insensitive).";
                SetJsonRequestExample(operation, UpdateTaskRequestExample());
                SetJsonResponseExample(operation, "200", CreateTaskExample());
                break;

            case nameof(TasksController.Delete):
                operation.Summary ??= "Delete task";
                operation.Description ??=
                    "Permanently deletes a task. Existing DELETE semantics are preserved.";
                break;

            case nameof(TasksController.Complete):
                operation.Summary ??= "Complete task";
                operation.Description ??=
                    "Marks a task as Completed and sets Actual End Date when not already set. Idempotent if the task is already Completed.";
                SetJsonResponseExample(operation, "200", CreateCompletedTaskExample());
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

    private static OpenApiObject CreateTaskRequestExample() => new()
    {
        ["missionId"] = new OpenApiString("55555555-5555-4555-8555-555555555555"),
        ["code"] = new OpenApiString("TSK-001"),
        ["name"] = new OpenApiString("Discovery Workshop"),
        ["description"] = new OpenApiString("Kickoff discovery with stakeholders"),
        ["taskTypeId"] = new OpenApiString("11111111-1111-4111-8211-000000000001"),
        ["taskStatusId"] = new OpenApiString("11111111-1111-4111-8221-000000000002"),
        ["priority"] = new OpenApiString("High"),
        ["estimatedHours"] = new OpenApiDouble(16),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-09")
    };

    private static OpenApiObject UpdateTaskRequestExample() => new()
    {
        ["code"] = new OpenApiString("TSK-001"),
        ["name"] = new OpenApiString("Discovery Workshop Updated"),
        ["description"] = new OpenApiString("Kickoff discovery with stakeholders"),
        ["taskTypeId"] = new OpenApiString("11111111-1111-4111-8211-000000000001"),
        ["taskStatusId"] = new OpenApiString("11111111-1111-4111-8221-000000000002"),
        ["priority"] = new OpenApiString("High"),
        ["estimatedHours"] = new OpenApiDouble(20),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-10")
    };

    private static OpenApiObject CreateTaskExample() => new()
    {
        ["id"] = new OpenApiString("66666666-6666-4666-8666-666666666666"),
        ["missionId"] = new OpenApiString("55555555-5555-4555-8555-555555555555"),
        ["code"] = new OpenApiString("TSK-001"),
        ["name"] = new OpenApiString("Discovery Workshop"),
        ["description"] = new OpenApiString("Kickoff discovery with stakeholders"),
        ["taskTypeId"] = new OpenApiString("11111111-1111-4111-8211-000000000001"),
        ["taskStatusId"] = new OpenApiString("11111111-1111-4111-8221-000000000002"),
        ["status"] = new OpenApiString("Planned"),
        ["priority"] = new OpenApiString("High"),
        ["estimatedHours"] = new OpenApiDouble(16),
        ["plannedStartDate"] = new OpenApiString("2026-01-05"),
        ["plannedEndDate"] = new OpenApiString("2026-01-09"),
        ["actualStartDate"] = new OpenApiNull(),
        ["actualEndDate"] = new OpenApiNull(),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreateCompletedTaskExample()
    {
        var example = CreateTaskExample();
        example["status"] = new OpenApiString("Completed");
        example["taskStatusId"] = new OpenApiString("11111111-1111-4111-8221-000000000004");
        example["actualEndDate"] = new OpenApiString("2026-01-09");
        return example;
    }

    private static OpenApiArray CreateTaskListExample() => new()
    {
        CreateTaskExample()
    };
}

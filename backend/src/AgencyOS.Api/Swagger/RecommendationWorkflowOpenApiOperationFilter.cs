using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Recommendation Approval Workflow endpoints (US-201).
/// </summary>
public sealed class RecommendationWorkflowOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(RecommendationWorkflowController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(RecommendationWorkflowController.GetAll):
                operation.Summary ??= "List recommendation workflows";
                operation.Description ??= "Returns recommendation approval workflows with optional status, contract, mission, and search filters.";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(RecommendationWorkflowController.GetById):
                operation.Summary ??= "Get recommendation workflow by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.GetTimeline):
                operation.Summary ??= "Get recommendation approval timeline";
                operation.Description ??= "Returns immutable transition history for the workflow (BR-1010).";
                SetJsonResponseExample(operation, "200", CreateTimelineExample());
                break;

            case nameof(RecommendationWorkflowController.Create):
                operation.Summary ??= "Create recommendation workflow";
                operation.Description ??= "Creates a Draft workflow for an existing Decision Engine recommendation (BR-1001). Does not generate strategies.";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.Submit):
                operation.Summary ??= "Submit recommendation for approval";
                operation.Description ??= "Transitions Draft or Reopened → PendingApproval (BR-1002).";
                SetJsonRequestExample(operation, ActionRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.Approve):
                operation.Summary ??= "Approve recommendation";
                operation.Description ??= "Transitions PendingApproval → Approved. Requires Approver (BR-1003/BR-1005/BR-1008).";
                SetJsonRequestExample(operation, ApproveRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.Reject):
                operation.Summary ??= "Reject recommendation";
                operation.Description ??= "Transitions PendingApproval → Rejected (BR-1004).";
                SetJsonRequestExample(operation, ActionRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.Cancel):
                operation.Summary ??= "Cancel recommendation";
                operation.Description ??= "Cancels before approval. Cancelled workflows cannot be reopened (BR-1007).";
                SetJsonRequestExample(operation, ActionRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(RecommendationWorkflowController.Reopen):
                operation.Summary ??= "Reopen rejected recommendation";
                operation.Description ??= "Transitions Rejected → Reopened (BR-1006). Submit again to return to PendingApproval.";
                SetJsonRequestExample(operation, ActionRequestExample());
                SetJsonResponseExample(operation, "200", CreateExample());
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
        ["id"] = new OpenApiString("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
        ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["deliveryStrategyId"] = new OpenApiString("dddddddd-dddd-4ddd-8ddd-dddddddddddd"),
        ["contractId"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
        ["missionId"] = new OpenApiString("ffffffff-ffff-4fff-8fff-ffffffffffff"),
        ["title"] = new OpenApiString("Balanced delivery mix"),
        ["summary"] = new OpenApiString("Ranked strategy ready for human approval"),
        ["status"] = new OpenApiString("Draft"),
        ["createdBy"] = new OpenApiString("planner@agencyos.local"),
        ["rankPosition"] = new OpenApiInteger(1),
        ["finalScore"] = new OpenApiDouble(0.91)
    };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };

    private static OpenApiArray CreateTimelineExample() => new()
    {
        new OpenApiObject
        {
            ["fromStatus"] = new OpenApiString("Draft"),
            ["toStatus"] = new OpenApiString("Draft"),
            ["actor"] = new OpenApiString("planner@agencyos.local"),
            ["comment"] = new OpenApiString("Recommendation workflow created in Draft."),
            ["occurredAt"] = new OpenApiString("2026-07-26T12:00:00Z")
        }
    };

    private static OpenApiObject CreateRequestExample() => new()
    {
        ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
        ["createdBy"] = new OpenApiString("planner@agencyos.local")
    };

    private static OpenApiObject ActionRequestExample() => new()
    {
        ["actor"] = new OpenApiString("approver@agencyos.local"),
        ["comment"] = new OpenApiString("Ready for review")
    };

    private static OpenApiObject ApproveRequestExample() => new()
    {
        ["approver"] = new OpenApiString("approver@agencyos.local"),
        ["approvalDate"] = new OpenApiString("2026-07-26T15:00:00Z"),
        ["comment"] = new OpenApiString("Approved for execution")
    };
}

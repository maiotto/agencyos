using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Decision Tracking endpoints (US-205).
/// </summary>
public sealed class DecisionOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(DecisionsController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(DecisionsController.GetAll):
            case nameof(DecisionsController.Filter):
                operation.Summary ??= "List or filter decisions";
                operation.Description ??= "Decision Tracking list/search (US-205).";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(DecisionsController.GetById):
                operation.Summary ??= "Get decision by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(DecisionsController.GetTimeline):
                operation.Summary ??= "Get decision timeline";
                operation.Description ??= "Append-only decision lifecycle timeline (BR-1404).";
                break;

            case nameof(DecisionsController.Create):
                operation.Summary ??= "Create decision from approved recommendation";
                operation.Description ??= "BR-1401: one Decision per Approved Recommendation.";
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(DecisionsController.Start):
                operation.Summary ??= "Start decision implementation";
                break;

            case nameof(DecisionsController.Complete):
                operation.Summary ??= "Complete decision";
                operation.Description ??= "BR-1405: Completed Decisions cannot return to In Progress.";
                break;

            case nameof(DecisionsController.Cancel):
                operation.Summary ??= "Cancel decision";
                break;

            case nameof(DecisionsController.RecordOutcome):
                operation.Summary ??= "Record decision outcome";
                operation.Description ??= "BR-1406: Outcome is recorded only after completion.";
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
        if (!response.Content.TryGetValue("application/json", out var media))
        {
            media = new OpenApiMediaType();
            response.Content["application/json"] = media;
        }

        media.Example = example;
    }

    private static OpenApiObject CreateExample() =>
        new()
        {
            ["id"] = new OpenApiString("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"),
            ["recommendationId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ["decisionStatus"] = new OpenApiString("Created"),
            ["implementationStatus"] = new OpenApiString("NotStarted"),
            ["createdBy"] = new OpenApiString("planner@agencyos.local")
        };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };
}

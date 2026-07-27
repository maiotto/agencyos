using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Swagger examples for Portfolio Planning endpoints (US-109).
/// </summary>
public sealed class PortfolioOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PortfoliosController))
        {
            return;
        }

        switch (context.MethodInfo.Name)
        {
            case nameof(PortfoliosController.GetAll):
            case nameof(PortfoliosController.Filter):
                operation.Summary ??= "List or filter portfolios";
                SetJsonResponseExample(operation, "200", CreateListExample());
                break;

            case nameof(PortfoliosController.GetById):
                operation.Summary ??= "Get portfolio by id";
                SetJsonResponseExample(operation, "200", CreateExample());
                break;

            case nameof(PortfoliosController.GetSummary):
                operation.Summary ??= "Get portfolio summary";
                operation.Description ??= "Live Capacity/Workload engine summaries plus historical aggregates (BR-908..BR-910).";
                break;

            case nameof(PortfoliosController.GetHealth):
                operation.Summary ??= "Get portfolio health";
                break;

            case nameof(PortfoliosController.Create):
                operation.Summary ??= "Create portfolio";
                operation.Description ??= "Creates an Active portfolio with at least one Active Mission (BR-901..BR-905).";
                SetJsonRequestExample(operation, CreateRequestExample());
                SetJsonResponseExample(operation, "201", CreateExample());
                break;

            case nameof(PortfoliosController.Update):
                operation.Summary ??= "Update portfolio";
                operation.Description ??= "Inactive portfolios cannot be modified (BR-906).";
                break;

            case nameof(PortfoliosController.Delete):
                operation.Summary ??= "Delete inactive portfolio";
                operation.Description ??= "Deleting Active Portfolios is prohibited (BR-907).";
                break;

            case nameof(PortfoliosController.Activate):
                operation.Summary ??= "Activate portfolio";
                break;

            case nameof(PortfoliosController.Deactivate):
                operation.Summary ??= "Deactivate portfolio";
                break;

            case nameof(PortfoliosController.AssociateMission):
                operation.Summary ??= "Associate mission";
                break;

            case nameof(PortfoliosController.RemoveMission):
                operation.Summary ??= "Remove mission";
                break;

            case nameof(PortfoliosController.AssignPlanningTemplate):
                operation.Summary ??= "Assign planning template";
                break;

            case nameof(PortfoliosController.CalculateCapacity):
                operation.Summary ??= "Calculate portfolio capacity";
                break;

            case nameof(PortfoliosController.CalculateWorkload):
                operation.Summary ??= "Calculate portfolio workload";
                break;

            case nameof(PortfoliosController.CalculateHealth):
                operation.Summary ??= "Calculate portfolio health";
                break;
        }
    }

    private static void SetJsonRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        operation.RequestBody ??= new OpenApiRequestBody();
        operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!operation.RequestBody.Content.TryGetValue("application/json", out var media))
        {
            media = new OpenApiMediaType();
            operation.RequestBody.Content["application/json"] = media;
        }

        media.Example = example;
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
            ["id"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1"),
            ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ["name"] = new OpenApiString("Q3 Delivery Portfolio"),
            ["status"] = new OpenApiString("Active"),
            ["planningPeriodStart"] = new OpenApiString("2026-07-01"),
            ["planningPeriodEnd"] = new OpenApiString("2026-09-30"),
            ["portfolioHealth"] = new OpenApiString("Healthy"),
            ["missions"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["missionId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                    ["priority"] = new OpenApiInteger(1)
                }
            }
        };

    private static OpenApiArray CreateListExample() => new() { CreateExample() };

    private static OpenApiObject CreateRequestExample() =>
        new()
        {
            ["companyId"] = new OpenApiString("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            ["name"] = new OpenApiString("Q3 Delivery Portfolio"),
            ["description"] = new OpenApiString("Consolidated planning for priority missions"),
            ["planningPeriodStart"] = new OpenApiString("2026-07-01"),
            ["planningPeriodEnd"] = new OpenApiString("2026-09-30"),
            ["missions"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["missionId"] = new OpenApiString("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                    ["priority"] = new OpenApiInteger(1)
                }
            }
        };
}

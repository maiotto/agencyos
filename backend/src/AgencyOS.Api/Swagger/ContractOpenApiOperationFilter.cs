using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

/// <summary>
/// Adds request and response examples for Client Contract Management endpoints.
/// </summary>
public sealed class ContractOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ContractsController))
        {
            return;
        }

        var actionName = context.MethodInfo.Name;

        switch (actionName)
        {
            case nameof(ContractsController.GetPaged):
                operation.Summary ??= "List contracts";
                operation.Description ??=
                    "Returns a paginated list of contracts with optional client, status, type, and date filters.";
                SetJsonResponseExample(operation, "200", CreatePagedContractExample());
                break;

            case nameof(ContractsController.GetById):
                operation.Summary ??= "Get contract by id";
                operation.Description ??= "Returns contract details by identifier.";
                SetJsonResponseExample(operation, "200", CreateContractExample());
                break;

            case nameof(ContractsController.Create):
                operation.Summary ??= "Create contract";
                operation.Description ??=
                    "Creates a new contract for an Active Client. Status must be Draft. ContractCode must be unique (case-insensitive).";
                SetJsonRequestExample(operation, CreateContractRequestExample());
                SetJsonResponseExample(operation, "201", CreateContractExample());
                break;

            case nameof(ContractsController.Update):
                operation.Summary ??= "Update contract";
                operation.Description ??=
                    "Updates an existing contract. Closed and Cancelled contracts cannot be edited. Status changes to Active, Closed, or Cancelled must use dedicated action endpoints.";
                SetJsonRequestExample(operation, UpdateContractRequestExample());
                SetJsonResponseExample(operation, "200", CreateContractExample());
                break;

            case nameof(ContractsController.Cancel):
                operation.Summary ??= "Cancel contract";
                operation.Description ??=
                    "Cancels a contract by setting status to Cancelled. Historical information is preserved. Equivalent soft-cancel behavior via DELETE.";
                break;

            case nameof(ContractsController.Activate):
                operation.Summary ??= "Activate contract";
                operation.Description ??=
                    "Activates a Draft contract by setting status to Active.";
                SetJsonResponseExample(operation, "200", CreateActiveContractExample());
                break;

            case nameof(ContractsController.Close):
                operation.Summary ??= "Close contract";
                operation.Description ??=
                    "Closes an Active or Suspended contract. Historical information is preserved.";
                SetJsonResponseExample(operation, "200", CreateClosedContractExample());
                break;

            case nameof(ContractsController.CancelContract):
                operation.Summary ??= "Cancel contract";
                operation.Description ??=
                    "Cancels a contract by setting status to Cancelled. Historical information is preserved.";
                SetJsonResponseExample(operation, "200", CreateCancelledContractExample());
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

    private static OpenApiObject CreateContractRequestExample() => new()
    {
        ["clientId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["contractCode"] = new OpenApiString("CTR-2026-001"),
        ["contractName"] = new OpenApiString("Acme Retainer 2026"),
        ["contractType"] = new OpenApiString("Monthly Retainer"),
        ["status"] = new OpenApiString("Draft"),
        ["estimatedValue"] = new OpenApiDouble(120000),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-12-31"),
        ["renewalDate"] = new OpenApiString("2026-11-01")
    };

    private static OpenApiObject UpdateContractRequestExample() => new()
    {
        ["contractCode"] = new OpenApiString("CTR-2026-001"),
        ["contractName"] = new OpenApiString("Acme Retainer 2026"),
        ["contractType"] = new OpenApiString("Monthly Retainer"),
        ["status"] = new OpenApiString("Draft"),
        ["estimatedValue"] = new OpenApiDouble(125000),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-12-31"),
        ["renewalDate"] = new OpenApiString("2026-11-01")
    };

    private static OpenApiObject CreateContractExample() => new()
    {
        ["id"] = new OpenApiString("44444444-4444-4444-8444-444444444444"),
        ["clientId"] = new OpenApiString("22222222-2222-4222-8222-222222222222"),
        ["contractCode"] = new OpenApiString("CTR-2026-001"),
        ["contractName"] = new OpenApiString("Acme Retainer 2026"),
        ["contractType"] = new OpenApiString("Monthly Retainer"),
        ["status"] = new OpenApiString("Draft"),
        ["estimatedValue"] = new OpenApiDouble(120000),
        ["startDate"] = new OpenApiString("2026-01-01"),
        ["endDate"] = new OpenApiString("2026-12-31"),
        ["renewalDate"] = new OpenApiString("2026-11-01"),
        ["createdAt"] = new OpenApiString("2026-07-26T12:00:00+00:00"),
        ["updatedAt"] = new OpenApiString("2026-07-26T12:00:00+00:00")
    };

    private static OpenApiObject CreateActiveContractExample()
    {
        var example = CreateContractExample();
        example["status"] = new OpenApiString("Active");
        return example;
    }

    private static OpenApiObject CreateClosedContractExample()
    {
        var example = CreateContractExample();
        example["status"] = new OpenApiString("Closed");
        return example;
    }

    private static OpenApiObject CreateCancelledContractExample()
    {
        var example = CreateContractExample();
        example["status"] = new OpenApiString("Cancelled");
        return example;
    }

    private static OpenApiObject CreatePagedContractExample() => new()
    {
        ["items"] = new OpenApiArray { CreateContractExample() },
        ["page"] = new OpenApiInteger(1),
        ["pageSize"] = new OpenApiInteger(20),
        ["totalCount"] = new OpenApiInteger(1),
        ["totalPages"] = new OpenApiInteger(1)
    };
}

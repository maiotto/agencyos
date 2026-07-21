using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMissionService, MissionService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IClientContractService, ClientContractService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IExecutionResourceService, ExecutionResourceService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ICapacityCalculatorService, CapacityCalculatorService>();
        services.AddScoped<IWorkloadCalculatorService, WorkloadCalculatorService>();
        services.AddScoped<IAvailabilityEngineService, AvailabilityEngineService>();
        services.AddScoped<IAllocationConflictDetectionService, AllocationConflictDetectionService>();
        services.AddScoped<IDeliveryStrategyBuilderService, DeliveryStrategyBuilderService>();
        services.AddScoped<IDeliveryStrategyEvaluatorService, DeliveryStrategyEvaluatorService>();
        services.AddScoped<IDeliveryStrategyRankingService, DeliveryStrategyRankingService>();
        services.AddScoped<IDeliveryStrategyExplanationService, DeliveryStrategyExplanationService>();
        services.AddValidatorsFromAssemblyContaining<CreateMissionRequestValidator>();

        return services;
    }
}

using AgencyOS.Application.Interfaces;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IClientContractRepository, ClientContractRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IExecutionResourceRepository, ExecutionResourceRepository>();
        services.AddScoped<IWorkingCalendarRepository, WorkingCalendarRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IWorkingHoursRepository, WorkingHoursRepository>();
        services.AddScoped<IResourceAvailabilityRepository, ResourceAvailabilityRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ICapacityHistoryRepository, CapacityHistoryRepository>();
        services.AddScoped<IWorkloadHistoryRepository, WorkloadHistoryRepository>();
        services.AddScoped<IPlanningTemplateRepository, PlanningTemplateRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IRecommendationWorkflowRepository, RecommendationWorkflowRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();
        services.AddScoped<IRecommendationHistoryRepository, RecommendationHistoryRepository>();
        services.AddScoped<IDecisionRepository, DecisionRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();
        services.AddScoped<IAIRecommendationRepository, AIRecommendationRepository>();
        services.AddScoped<IExplainabilityRepository, ExplainabilityRepository>();
        services.AddScoped<IExecutiveRecommendationSummaryRepository, ExecutiveRecommendationSummaryRepository>();
        services.AddScoped<ICompanyDecisionProfileRepository, CompanyDecisionProfileRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}

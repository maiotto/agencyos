using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mission> Missions => Set<Mission>();

    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<LeadContact> LeadContacts => Set<LeadContact>();

    public DbSet<ClientContact> ClientContacts => Set<ClientContact>();

    public DbSet<ClientContract> ClientContracts => Set<ClientContract>();

    public DbSet<MissionTask> MissionTasks => Set<MissionTask>();

    public DbSet<TaskStatusLookup> TaskStatuses => Set<TaskStatusLookup>();

    public DbSet<TaskTypeLookup> TaskTypes => Set<TaskTypeLookup>();

    public DbSet<ExecutionResource> ExecutionResources => Set<ExecutionResource>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<WorkingCalendar> WorkingCalendars => Set<WorkingCalendar>();

    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();

    public DbSet<WorkingHoursDay> WorkingHoursDays => Set<WorkingHoursDay>();

    public DbSet<ResourceAvailability> ResourceAvailabilities => Set<ResourceAvailability>();

    public DbSet<ResourceAvailabilityWeekDay> ResourceAvailabilityWeekDays => Set<ResourceAvailabilityWeekDay>();

    public DbSet<ResourceAvailabilityDayOverride> ResourceAvailabilityDayOverrides => Set<ResourceAvailabilityDayOverride>();

    public DbSet<CapacityHistory> CapacityHistories => Set<CapacityHistory>();

    public DbSet<WorkloadHistory> WorkloadHistories => Set<WorkloadHistory>();

    public DbSet<PlanningTemplate> PlanningTemplates => Set<PlanningTemplate>();

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public DbSet<PortfolioMission> PortfolioMissions => Set<PortfolioMission>();

    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    public DbSet<RecommendationHistory> RecommendationHistories => Set<RecommendationHistory>();

    public DbSet<RecommendationWorkflow> RecommendationWorkflows => Set<RecommendationWorkflow>();

    public DbSet<RecommendationWorkflowTransition> RecommendationWorkflowTransitions => Set<RecommendationWorkflowTransition>();

    public DbSet<Decision> Decisions => Set<Decision>();

    public DbSet<DecisionTimelineEntry> DecisionTimelineEntries => Set<DecisionTimelineEntry>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<AIRecommendation> AIRecommendations => Set<AIRecommendation>();

    public DbSet<Explainability> Explainabilities => Set<Explainability>();

    public DbSet<ExecutiveRecommendationSummary> ExecutiveRecommendationSummaries =>
        Set<ExecutiveRecommendationSummary>();

    public DbSet<CompanyDecisionProfile> CompanyDecisionProfiles => Set<CompanyDecisionProfile>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

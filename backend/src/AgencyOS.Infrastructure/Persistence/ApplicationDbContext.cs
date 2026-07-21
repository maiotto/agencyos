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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

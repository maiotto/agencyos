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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

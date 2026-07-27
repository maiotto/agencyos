using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolio");

        builder.HasKey(portfolio => portfolio.Id);

        builder.Property(portfolio => portfolio.Id).HasColumnName("id");
        builder.Property(portfolio => portfolio.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(portfolio => portfolio.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(portfolio => portfolio.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(portfolio => portfolio.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(portfolio => portfolio.PlanningTemplateId).HasColumnName("planning_template_id");
        builder.Property(portfolio => portfolio.PlanningPeriodStart)
            .HasColumnName("planning_period_start")
            .HasColumnType("date")
            .IsRequired();
        builder.Property(portfolio => portfolio.PlanningPeriodEnd)
            .HasColumnName("planning_period_end")
            .HasColumnType("date")
            .IsRequired();
        builder.Property(portfolio => portfolio.CapacitySummary)
            .HasColumnName("capacity_summary")
            .HasColumnType("jsonb");
        builder.Property(portfolio => portfolio.WorkloadSummary)
            .HasColumnName("workload_summary")
            .HasColumnType("jsonb");
        builder.Property(portfolio => portfolio.PortfolioHealth)
            .HasColumnName("portfolio_health")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(portfolio => portfolio.HealthDetails)
            .HasColumnName("health_details")
            .HasColumnType("jsonb");
        builder.Property(portfolio => portfolio.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(portfolio => portfolio.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(portfolio => portfolio.IsActive);
        builder.Ignore(portfolio => portfolio.IsInactive);

        builder.HasMany(portfolio => portfolio.Missions)
            .WithOne()
            .HasForeignKey(mission => mission.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(portfolio => portfolio.Missions)
            .HasField("_missions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(Portfolio.IsActive) and not nameof(Portfolio.IsInactive))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(portfolio => portfolio.CompanyId)
            .HasDatabaseName("idx_portfolio_company_id");
        builder.HasIndex(portfolio => portfolio.Status)
            .HasDatabaseName("idx_portfolio_status");
        builder.HasIndex(portfolio => new { portfolio.CompanyId, portfolio.Name })
            .IsUnique()
            .HasDatabaseName("uq_portfolio_company_name");
    }
}

public class PortfolioMissionConfiguration : IEntityTypeConfiguration<PortfolioMission>
{
    public void Configure(EntityTypeBuilder<PortfolioMission> builder)
    {
        builder.ToTable("portfolio_mission");

        builder.HasKey(mission => mission.Id);

        builder.Property(mission => mission.Id).HasColumnName("id");
        builder.Property(mission => mission.PortfolioId).HasColumnName("portfolio_id").IsRequired();
        builder.Property(mission => mission.MissionId).HasColumnName("mission_id").IsRequired();
        builder.Property(mission => mission.Priority).HasColumnName("priority").IsRequired();
        builder.Property(mission => mission.IncludedAt).HasColumnName("included_at").IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(mission => mission.PortfolioId)
            .HasDatabaseName("idx_portfolio_mission_portfolio_id");
        builder.HasIndex(mission => mission.MissionId)
            .HasDatabaseName("idx_portfolio_mission_mission_id");
        builder.HasIndex(mission => new { mission.PortfolioId, mission.MissionId })
            .IsUnique()
            .HasDatabaseName("uq_portfolio_mission");
    }
}

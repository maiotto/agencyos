using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class CompanyDecisionProfileConfiguration : IEntityTypeConfiguration<CompanyDecisionProfile>
{
    public void Configure(EntityTypeBuilder<CompanyDecisionProfile> builder)
    {
        builder.ToTable("company_decision_profile");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(item => item.ProfileFamilyId).HasColumnName("profile_family_id").IsRequired();
        builder.Property(item => item.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(item => item.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(item => item.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(item => item.PriorityWeights).HasColumnName("priority_weights").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.CapacityWeight).HasColumnName("capacity_weight").HasPrecision(8, 4).IsRequired();
        builder.Property(item => item.WorkloadWeight).HasColumnName("workload_weight").HasPrecision(8, 4).IsRequired();
        builder.Property(item => item.CostWeight).HasColumnName("cost_weight").HasPrecision(8, 4).IsRequired();
        builder.Property(item => item.RiskWeight).HasColumnName("risk_weight").HasPrecision(8, 4).IsRequired();
        builder.Property(item => item.QualityWeight).HasColumnName("quality_weight").HasPrecision(8, 4).IsRequired();
        builder.Property(item => item.PreferredStrategy).HasColumnName("preferred_strategy").HasMaxLength(200);
        builder.Property(item => item.PreferredCapacityThreshold)
            .HasColumnName("preferred_capacity_threshold")
            .HasPrecision(5, 2);
        builder.Property(item => item.PreferredWorkloadThreshold)
            .HasColumnName("preferred_workload_threshold")
            .HasPrecision(5, 2);
        builder.Property(item => item.DefaultProfile).HasColumnName("default_profile").IsRequired();
        builder.Property(item => item.Version).HasColumnName("version").IsRequired();
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(item => item.ArchivedAt).HasColumnName("archived_at");

        builder.Ignore(item => item.Archived);
        builder.Ignore(item => item.IsActive);
        builder.Ignore(item => item.IsInactive);
        builder.Ignore(item => item.Dimensions);

        builder.HasIndex(item => new { item.CompanyId, item.ProfileFamilyId, item.Version })
            .IsUnique()
            .HasDatabaseName("uq_company_decision_profile_family_version");

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not (
                nameof(CompanyDecisionProfile.Archived)
                or nameof(CompanyDecisionProfile.IsActive)
                or nameof(CompanyDecisionProfile.IsInactive)
                or nameof(CompanyDecisionProfile.Dimensions)))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => item.CompanyId).HasDatabaseName("idx_company_decision_profile_company_id");
        builder.HasIndex(item => item.Status).HasDatabaseName("idx_company_decision_profile_status");
        builder.HasIndex(item => item.ProfileFamilyId).HasDatabaseName("idx_company_decision_profile_family");
        builder.HasIndex(item => new { item.CompanyId, item.Name })
            .HasDatabaseName("idx_company_decision_profile_name");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.CompanyCode).HasColumnName("company_code").HasMaxLength(50).IsRequired();
        builder.Property(item => item.CompanyName).HasColumnName("company_name").HasMaxLength(200).IsRequired();
        builder.Property(item => item.LegalName).HasColumnName("legal_name").HasMaxLength(300);
        builder.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(item => item.Timezone).HasColumnName("timezone").HasMaxLength(100).IsRequired();
        builder.Property(item => item.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(item => item.Language).HasColumnName("language").HasMaxLength(20);
        builder.Property(item => item.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(item => item.PlanningConfiguration).HasColumnName("planning_configuration").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.DecisionProfileId).HasColumnName("decision_profile_id");
        builder.Property(item => item.DefaultCalendarId).HasColumnName("default_calendar_id");
        builder.Property(item => item.DefaultPlanningTemplateId).HasColumnName("default_planning_template_id");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(item => item.ArchivedAt).HasColumnName("archived_at");

        builder.Ignore(item => item.Archived);
        builder.Ignore(item => item.IsActive);
        builder.Ignore(item => item.IsInactive);

        builder.HasIndex(item => item.CompanyCode).IsUnique().HasDatabaseName("uq_company_code");
        builder.HasIndex(item => item.CompanyName).IsUnique().HasDatabaseName("uq_company_name");

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not (
                nameof(Company.Archived)
                or nameof(Company.IsActive)
                or nameof(Company.IsInactive)))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => item.Status).HasDatabaseName("idx_company_status");
        builder.HasIndex(item => item.DecisionProfileId).HasDatabaseName("idx_company_decision_profile_id");
    }
}

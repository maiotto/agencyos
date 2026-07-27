using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class PlanningTemplateConfiguration : IEntityTypeConfiguration<PlanningTemplate>
{
    public void Configure(EntityTypeBuilder<PlanningTemplate> builder)
    {
        builder.ToTable("planning_template");

        builder.HasKey(template => template.Id);

        builder.Property(template => template.Id).HasColumnName("id");
        builder.Property(template => template.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(template => template.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(template => template.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(template => template.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(template => template.WorkingCalendarId).HasColumnName("working_calendar_id").IsRequired();
        builder.Property(template => template.WorkingHoursId).HasColumnName("working_hours_id").IsRequired();
        builder.Property(template => template.ResourceAvailabilityStrategy)
            .HasColumnName("resource_availability_strategy")
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(template => template.DefaultPlanningWindowDays)
            .HasColumnName("default_planning_window_days")
            .IsRequired();
        builder.Property(template => template.DefaultPeriodStartOffsetDays)
            .HasColumnName("default_period_start_offset_days")
            .IsRequired();
        builder.Property(template => template.UtilizationWarningPercentage)
            .HasColumnName("utilization_warning_percentage")
            .HasPrecision(8, 2);
        builder.Property(template => template.IncludeAssignmentDistribution)
            .HasColumnName("include_assignment_distribution")
            .IsRequired();
        builder.Property(template => template.PlanningParametersJson)
            .HasColumnName("planning_parameters_json")
            .HasColumnType("jsonb");
        builder.Property(template => template.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(template => template.UpdatedAt).HasColumnName("updated_at").IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(template => new { template.CompanyId, template.Name })
            .IsUnique()
            .HasDatabaseName("uq_planning_template_company_name");
        builder.HasIndex(template => template.CompanyId)
            .HasDatabaseName("idx_planning_template_company_id");
        builder.HasIndex(template => template.Status)
            .HasDatabaseName("idx_planning_template_status");
        builder.HasIndex(template => template.WorkingCalendarId)
            .HasDatabaseName("idx_planning_template_working_calendar_id");
        builder.HasIndex(template => template.WorkingHoursId)
            .HasDatabaseName("idx_planning_template_working_hours_id");
    }
}

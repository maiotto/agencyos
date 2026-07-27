using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class WorkloadHistoryConfiguration : IEntityTypeConfiguration<WorkloadHistory>
{
    public void Configure(EntityTypeBuilder<WorkloadHistory> builder)
    {
        builder.ToTable("workload_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id).HasColumnName("id");
        builder.Property(history => history.ExecutionResourceId).HasColumnName("execution_resource_id").IsRequired();
        builder.Property(history => history.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(history => history.CalculationDate).HasColumnName("calculation_date").IsRequired();
        builder.Property(history => history.PeriodStart).HasColumnName("period_start").HasColumnType("date").IsRequired();
        builder.Property(history => history.PeriodEnd).HasColumnName("period_end").HasColumnType("date").IsRequired();
        builder.Property(history => history.AllocatedHours).HasColumnName("allocated_hours").HasPrecision(12, 2).IsRequired();
        builder.Property(history => history.CapacityHours).HasColumnName("capacity_hours").HasPrecision(12, 2).IsRequired();
        builder.Property(history => history.WorkloadPercentage).HasColumnName("workload_percentage").HasPrecision(8, 2).IsRequired();
        builder.Property(history => history.WorkingDays).HasColumnName("working_days").IsRequired();
        builder.Property(history => history.HolidayDays).HasColumnName("holiday_days").IsRequired();
        builder.Property(history => history.AvailableDays).HasColumnName("available_days").IsRequired();
        builder.Property(history => history.CalculationVersion).HasColumnName("calculation_version").HasMaxLength(50).IsRequired();
        builder.Property(history => history.OperationalInputsJson)
            .HasColumnName("operational_inputs_json")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(history => history.CreatedAt).HasColumnName("created_at").IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(history => history.ExecutionResourceId)
            .HasDatabaseName("idx_workload_history_execution_resource_id");
        builder.HasIndex(history => history.CompanyId)
            .HasDatabaseName("idx_workload_history_company_id");
        builder.HasIndex(history => new { history.PeriodStart, history.PeriodEnd })
            .HasDatabaseName("idx_workload_history_period");
        builder.HasIndex(history => history.CalculationVersion)
            .HasDatabaseName("idx_workload_history_calculation_version");
    }
}

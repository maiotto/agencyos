using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(a => a.ExecutionResourceId)
            .HasColumnName("execution_resource_id")
            .IsRequired();

        builder.Property(a => a.AssignmentRole)
            .HasColumnName("assignment_role")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.PlannedHours)
            .HasColumnName("planned_hours")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(a => a.PlannedStartDate)
            .HasColumnName("planned_start_date")
            .IsRequired();

        builder.Property(a => a.PlannedEndDate)
            .HasColumnName("planned_end_date")
            .IsRequired();

        builder.Property(a => a.AllocationPercentage)
            .HasColumnName("allocation_percentage")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasColumnName("notes");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(a => a.Task)
            .WithMany()
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ExecutionResource)
            .WithMany()
            .HasForeignKey(a => a.ExecutionResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TaskId)
            .HasDatabaseName("idx_assignment_task");

        builder.HasIndex(a => a.ExecutionResourceId)
            .HasDatabaseName("idx_assignment_execution_resource");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("idx_assignment_status");

        builder.HasIndex(a => a.PlannedStartDate)
            .HasDatabaseName("idx_assignment_planned_start_date");
    }
}

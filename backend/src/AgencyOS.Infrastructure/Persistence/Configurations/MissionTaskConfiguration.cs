using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class MissionTaskConfiguration : IEntityTypeConfiguration<MissionTask>
{
    public void Configure(EntityTypeBuilder<MissionTask> builder)
    {
        builder.ToTable("task");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.MissionId)
            .HasColumnName("mission_id")
            .IsRequired();

        builder.Property(t => t.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("uq_task_code");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.TaskTypeId)
            .HasColumnName("task_type_id")
            .IsRequired();

        builder.Property(t => t.TaskStatusId)
            .HasColumnName("task_status_id")
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasDefaultValue("Medium")
            .IsRequired();

        builder.Property(t => t.EstimatedHours)
            .HasColumnName("estimated_hours")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(t => t.PlannedStart)
            .HasColumnName("planned_start");

        builder.Property(t => t.PlannedEnd)
            .HasColumnName("planned_end");

        builder.Property(t => t.ActualStart)
            .HasColumnName("actual_start");

        builder.Property(t => t.ActualEnd)
            .HasColumnName("actual_end");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.TaskStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Type)
            .WithMany()
            .HasForeignKey(t => t.TaskTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

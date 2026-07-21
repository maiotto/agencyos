using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ExecutionResourceConfiguration : IEntityTypeConfiguration<ExecutionResource>
{
    public void Configure(EntityTypeBuilder<ExecutionResource> builder)
    {
        builder.ToTable("execution_resource");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.ResourceType)
            .HasColumnName("resource_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.CapacityHoursPerWeek)
            .HasColumnName("capacity_hours_per_week")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(r => r.CostRate)
            .HasColumnName("cost_rate")
            .HasPrecision(15, 2);

        builder.Property(r => r.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3);

        builder.Property(r => r.Skills)
            .HasColumnName("skills");

        builder.Property(r => r.Availability)
            .HasColumnName("availability");

        builder.Property(r => r.Notes)
            .HasColumnName("notes");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(r => r.Code)
            .IsUnique()
            .HasDatabaseName("uq_execution_resource_code");
    }
}

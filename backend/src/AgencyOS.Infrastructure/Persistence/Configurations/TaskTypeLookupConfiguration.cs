using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class TaskTypeLookupConfiguration : IEntityTypeConfiguration<TaskTypeLookup>
{
    public void Configure(EntityTypeBuilder<TaskTypeLookup> builder)
    {
        builder.ToTable("task_type");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("uq_task_type_code");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}

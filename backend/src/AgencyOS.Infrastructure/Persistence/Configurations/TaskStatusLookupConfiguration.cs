using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class TaskStatusLookupConfiguration : IEntityTypeConfiguration<TaskStatusLookup>
{
    public void Configure(EntityTypeBuilder<TaskStatusLookup> builder)
    {
        builder.ToTable("task_status");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("uq_task_status_code");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}

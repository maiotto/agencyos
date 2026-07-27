using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("working_hours");

        builder.HasKey(hours => hours.Id);

        builder.Property(hours => hours.Id)
            .HasColumnName("id");

        builder.Property(hours => hours.WorkingCalendarId)
            .HasColumnName("working_calendar_id")
            .IsRequired();

        builder.Property(hours => hours.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(hours => hours.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(hours => hours.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(hours => hours.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("date");

        builder.Property(hours => hours.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(hours => hours.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(hours => hours.IsActive);
        builder.Ignore(hours => hours.IsInactive);

        builder.Property(hours => hours.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.WorkingCalendarId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.Status).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.EffectiveFrom).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.EffectiveTo).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.CreatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(hours => hours.UpdatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasMany(hours => hours.Days)
            .WithOne()
            .HasForeignKey(day => day.WorkingHoursId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(hours => hours.Days)
            .HasField("_days")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(hours => hours.WorkingCalendarId)
            .HasDatabaseName("idx_working_hours_calendar_id");

        builder.HasIndex(hours => hours.Status)
            .HasDatabaseName("idx_working_hours_status");

        builder.HasIndex(hours => new { hours.WorkingCalendarId, hours.Status })
            .HasDatabaseName("idx_working_hours_calendar_status");

        builder.HasIndex(hours => hours.EffectiveFrom)
            .HasDatabaseName("idx_working_hours_effective_from");

        builder.HasIndex(hours => new { hours.WorkingCalendarId, hours.EffectiveFrom, hours.EffectiveTo })
            .HasDatabaseName("idx_working_hours_period");

        builder.HasIndex(hours => hours.Name)
            .HasDatabaseName("idx_working_hours_name");
    }
}

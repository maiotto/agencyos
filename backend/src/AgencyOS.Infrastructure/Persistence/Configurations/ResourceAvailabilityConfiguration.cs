using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ResourceAvailabilityConfiguration : IEntityTypeConfiguration<ResourceAvailability>
{
    public void Configure(EntityTypeBuilder<ResourceAvailability> builder)
    {
        builder.ToTable("resource_availability");

        builder.HasKey(availability => availability.Id);

        builder.Property(availability => availability.Id).HasColumnName("id");
        builder.Property(availability => availability.ExecutionResourceId).HasColumnName("execution_resource_id").IsRequired();
        builder.Property(availability => availability.WorkingCalendarId).HasColumnName("working_calendar_id").IsRequired();
        builder.Property(availability => availability.WorkingHoursId).HasColumnName("working_hours_id").IsRequired();
        builder.Property(availability => availability.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(availability => availability.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(availability => availability.EffectiveFrom).HasColumnName("effective_from").HasColumnType("date").IsRequired();
        builder.Property(availability => availability.EffectiveTo).HasColumnName("effective_to").HasColumnType("date");
        builder.Property(availability => availability.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(availability => availability.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(availability => availability.IsActive);
        builder.Ignore(availability => availability.IsInactive);

        foreach (var property in new[]
                 {
                     nameof(ResourceAvailability.Id),
                     nameof(ResourceAvailability.ExecutionResourceId),
                     nameof(ResourceAvailability.WorkingCalendarId),
                     nameof(ResourceAvailability.WorkingHoursId),
                     nameof(ResourceAvailability.Name),
                     nameof(ResourceAvailability.Status),
                     nameof(ResourceAvailability.EffectiveFrom),
                     nameof(ResourceAvailability.EffectiveTo),
                     nameof(ResourceAvailability.CreatedAt),
                     nameof(ResourceAvailability.UpdatedAt)
                 })
        {
            builder.Property(property).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasMany(availability => availability.WeeklyAvailability)
            .WithOne()
            .HasForeignKey(day => day.ResourceAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(availability => availability.WeeklyAvailability)
            .HasField("_weeklyAvailability")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(availability => availability.DailyOverrides)
            .WithOne()
            .HasForeignKey(day => day.ResourceAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(availability => availability.DailyOverrides)
            .HasField("_dailyOverrides")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(availability => availability.ExecutionResourceId)
            .HasDatabaseName("idx_resource_availability_resource_id");
        builder.HasIndex(availability => availability.WorkingCalendarId)
            .HasDatabaseName("idx_resource_availability_calendar_id");
        builder.HasIndex(availability => availability.WorkingHoursId)
            .HasDatabaseName("idx_resource_availability_hours_id");
        builder.HasIndex(availability => availability.Status)
            .HasDatabaseName("idx_resource_availability_status");
        builder.HasIndex(availability => availability.Name)
            .HasDatabaseName("idx_resource_availability_name");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ResourceAvailabilityWeekDayConfiguration : IEntityTypeConfiguration<ResourceAvailabilityWeekDay>
{
    public void Configure(EntityTypeBuilder<ResourceAvailabilityWeekDay> builder)
    {
        builder.ToTable("resource_availability_week_day");

        builder.HasKey(day => day.Id);
        builder.Property(day => day.Id).HasColumnName("id");
        builder.Property(day => day.ResourceAvailabilityId).HasColumnName("resource_availability_id").IsRequired();
        builder.Property(day => day.DayOfWeek).HasColumnName("day_of_week").HasMaxLength(20).IsRequired();
        builder.Property(day => day.Enabled).HasColumnName("enabled").IsRequired();

        builder.Property(day => day.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.ResourceAvailabilityId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.DayOfWeek).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.Enabled).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasIndex(day => new { day.ResourceAvailabilityId, day.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("uq_resource_availability_week_day");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ResourceAvailabilityDayOverrideConfiguration : IEntityTypeConfiguration<ResourceAvailabilityDayOverride>
{
    public void Configure(EntityTypeBuilder<ResourceAvailabilityDayOverride> builder)
    {
        builder.ToTable("resource_availability_day_override");

        builder.HasKey(day => day.Id);
        builder.Property(day => day.Id).HasColumnName("id");
        builder.Property(day => day.ResourceAvailabilityId).HasColumnName("resource_availability_id").IsRequired();
        builder.Property(day => day.OverrideDate).HasColumnName("override_date").HasColumnType("date").IsRequired();
        builder.Property(day => day.Available).HasColumnName("available").IsRequired();
        builder.Property(day => day.StartTime).HasColumnName("start_time").HasColumnType("time");
        builder.Property(day => day.EndTime).HasColumnName("end_time").HasColumnType("time");
        builder.Property(day => day.Notes).HasColumnName("notes");

        builder.Property(day => day.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.ResourceAvailabilityId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.OverrideDate).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.Available).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.StartTime).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.EndTime).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.Notes).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasIndex(day => new { day.ResourceAvailabilityId, day.OverrideDate })
            .IsUnique()
            .HasDatabaseName("uq_resource_availability_day_override");
    }
}

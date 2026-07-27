using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class WorkingHoursDayConfiguration : IEntityTypeConfiguration<WorkingHoursDay>
{
    public void Configure(EntityTypeBuilder<WorkingHoursDay> builder)
    {
        builder.ToTable("working_hours_day");

        builder.HasKey(day => day.Id);

        builder.Property(day => day.Id)
            .HasColumnName("id");

        builder.Property(day => day.WorkingHoursId)
            .HasColumnName("working_hours_id")
            .IsRequired();

        builder.Property(day => day.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(day => day.Enabled)
            .HasColumnName("enabled")
            .IsRequired();

        builder.Property(day => day.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time");

        builder.Property(day => day.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time");

        builder.Property(day => day.BreakStart)
            .HasColumnName("break_start")
            .HasColumnType("time");

        builder.Property(day => day.BreakEnd)
            .HasColumnName("break_end")
            .HasColumnType("time");

        builder.Ignore(day => day.NetHours);

        builder.Property(day => day.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.WorkingHoursId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.DayOfWeek).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.Enabled).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.StartTime).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.EndTime).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.BreakStart).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(day => day.BreakEnd).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasIndex(day => day.WorkingHoursId)
            .HasDatabaseName("idx_working_hours_day_working_hours_id");

        builder.HasIndex(day => new { day.WorkingHoursId, day.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("uq_working_hours_day");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class WorkingCalendarConfiguration : IEntityTypeConfiguration<WorkingCalendar>
{
    public void Configure(EntityTypeBuilder<WorkingCalendar> builder)
    {
        builder.ToTable("working_calendar");

        builder.HasKey(calendar => calendar.Id);

        builder.Property(calendar => calendar.Id)
            .HasColumnName("id");

        builder.Property(calendar => calendar.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(calendar => calendar.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(calendar => calendar.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(calendar => calendar.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(calendar => calendar.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("date");

        builder.Property(calendar => calendar.WorkingDays)
            .HasColumnName("working_days")
            .IsRequired();

        builder.Property(calendar => calendar.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(calendar => calendar.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(calendar => calendar.IsActive);
        builder.Ignore(calendar => calendar.IsInactive);

        builder.Property(calendar => calendar.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.CompanyId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.Status).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.EffectiveFrom).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.EffectiveTo).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.WorkingDays).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.CreatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(calendar => calendar.UpdatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasIndex(calendar => calendar.CompanyId)
            .HasDatabaseName("idx_working_calendar_company_id");

        builder.HasIndex(calendar => calendar.Status)
            .HasDatabaseName("idx_working_calendar_status");

        builder.HasIndex(calendar => new { calendar.CompanyId, calendar.Status })
            .HasDatabaseName("idx_working_calendar_company_status");

        builder.HasIndex(calendar => calendar.EffectiveFrom)
            .HasDatabaseName("idx_working_calendar_effective_from");

        builder.HasIndex(calendar => new { calendar.CompanyId, calendar.EffectiveFrom, calendar.EffectiveTo })
            .HasDatabaseName("idx_working_calendar_period");

        builder.HasIndex(calendar => calendar.Name)
            .HasDatabaseName("idx_working_calendar_name");
    }
}

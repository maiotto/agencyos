using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holiday");

        builder.HasKey(holiday => holiday.Id);

        builder.Property(holiday => holiday.Id)
            .HasColumnName("id");

        builder.Property(holiday => holiday.CompanyId)
            .HasColumnName("company_id");

        builder.Property(holiday => holiday.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(holiday => holiday.Description)
            .HasColumnName("description");

        builder.Property(holiday => holiday.HolidayType)
            .HasColumnName("holiday_type")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(holiday => holiday.HolidayDate)
            .HasColumnName("holiday_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(holiday => holiday.StateCode)
            .HasColumnName("state_code")
            .HasMaxLength(10);

        builder.Property(holiday => holiday.City)
            .HasColumnName("city")
            .HasMaxLength(120);

        builder.Property(holiday => holiday.Recurring)
            .HasColumnName("recurring")
            .IsRequired();

        builder.Property(holiday => holiday.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(holiday => holiday.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(holiday => holiday.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(holiday => holiday.IsActive);
        builder.Ignore(holiday => holiday.IsInactive);

        builder.Property(holiday => holiday.Id).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.CompanyId).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.Description).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.HolidayType).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.HolidayDate).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.StateCode).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.City).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.Recurring).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.Status).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.CreatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(holiday => holiday.UpdatedAt).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasIndex(holiday => holiday.CompanyId)
            .HasDatabaseName("idx_holiday_company_id");

        builder.HasIndex(holiday => holiday.Status)
            .HasDatabaseName("idx_holiday_status");

        builder.HasIndex(holiday => holiday.HolidayType)
            .HasDatabaseName("idx_holiday_type");

        builder.HasIndex(holiday => holiday.HolidayDate)
            .HasDatabaseName("idx_holiday_date");

        builder.HasIndex(holiday => holiday.Name)
            .HasDatabaseName("idx_holiday_name");
    }
}

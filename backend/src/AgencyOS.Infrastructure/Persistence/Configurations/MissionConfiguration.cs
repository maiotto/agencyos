using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("mission");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.ClientContractId)
            .HasColumnName("client_contract_id")
            .IsRequired();

        builder.Property(m => m.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(m => m.Code)
            .IsUnique()
            .HasDatabaseName("uq_mission_code");

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description");

        builder.Property(m => m.MissionTypeId)
            .HasColumnName("mission_type_id")
            .IsRequired();

        builder.Property(m => m.MissionStatusId)
            .HasColumnName("mission_status_id")
            .IsRequired();

        builder.Property(m => m.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasDefaultValue("NORMAL")
            .IsRequired();

        builder.Property(m => m.StartDate)
            .HasColumnName("start_date");

        builder.Property(m => m.EndDate)
            .HasColumnName("end_date");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}

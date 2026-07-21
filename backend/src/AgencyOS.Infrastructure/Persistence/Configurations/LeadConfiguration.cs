using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("lead");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(l => l.Code)
            .IsUnique()
            .HasDatabaseName("uq_lead_code");

        builder.Property(l => l.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.TradeName)
            .HasColumnName("trade_name")
            .HasMaxLength(200);

        builder.Property(l => l.Website)
            .HasColumnName("website")
            .HasMaxLength(255);

        builder.Property(l => l.Segment)
            .HasColumnName("segment")
            .HasMaxLength(100);

        builder.Property(l => l.Source)
            .HasColumnName("source")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(l => l.EstimatedRevenue)
            .HasColumnName("estimated_revenue")
            .HasColumnType("numeric(15,2)");

        builder.Property(l => l.OwnerId)
            .HasColumnName("owner_id");

        builder.Property(l => l.Notes)
            .HasColumnName("notes");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(l => l.LeadContacts)
            .WithOne(lc => lc.Lead)
            .HasForeignKey(lc => lc.LeadId);
    }
}

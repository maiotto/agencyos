using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("client");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.LeadId)
            .HasColumnName("lead_id");

        builder.HasIndex(c => c.LeadId)
            .IsUnique()
            .HasFilter("lead_id IS NOT NULL")
            .HasDatabaseName("uq_client_lead_id");

        builder.HasIndex(c => c.TaxId)
            .IsUnique()
            .HasDatabaseName("uq_client_tax_id");

        builder.Property(c => c.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.TradeName)
            .HasColumnName("trade_name")
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(30);

        builder.Property(c => c.Website)
            .HasColumnName("website")
            .HasMaxLength(255);

        builder.Property(c => c.Segment)
            .HasColumnName("segment")
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.AccountOwner)
            .HasColumnName("account_owner");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(c => c.Lead)
            .WithMany()
            .HasForeignKey(c => c.LeadId)
            .HasConstraintName("fk_client_lead");
    }
}

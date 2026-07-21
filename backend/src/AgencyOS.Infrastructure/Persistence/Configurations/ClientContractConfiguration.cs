using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ClientContractConfiguration : IEntityTypeConfiguration<ClientContract>
{
    public void Configure(EntityTypeBuilder<ClientContract> builder)
    {
        builder.ToTable("client_contract");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(c => c.ContractNumber)
            .HasColumnName("contract_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.BillingModel)
            .HasColumnName("billing_model")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Value)
            .HasColumnName("value")
            .HasColumnType("numeric(15,2)");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date");

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date");

        builder.Property(c => c.RenewalDate)
            .HasColumnName("renewal_date");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .HasConstraintName("fk_contract_client");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ClientContactConfiguration : IEntityTypeConfiguration<ClientContact>
{
    public void Configure(EntityTypeBuilder<ClientContact> builder)
    {
        builder.ToTable("client_contact");

        builder.HasKey(cc => new { cc.ClientId, cc.ContactId });

        builder.Property(cc => cc.ClientId)
            .HasColumnName("client_id");

        builder.Property(cc => cc.ContactId)
            .HasColumnName("contact_id");

        builder.Property(cc => cc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(cc => cc.Client)
            .WithMany(c => c.ClientContacts)
            .HasForeignKey(cc => cc.ClientId)
            .HasConstraintName("fk_client_contact_client");

        builder.HasOne(cc => cc.Contact)
            .WithMany(c => c.ClientContacts)
            .HasForeignKey(cc => cc.ContactId)
            .HasConstraintName("fk_client_contact_contact");
    }
}

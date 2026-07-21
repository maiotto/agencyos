using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class LeadContactConfiguration : IEntityTypeConfiguration<LeadContact>
{
    public void Configure(EntityTypeBuilder<LeadContact> builder)
    {
        builder.ToTable("lead_contact");

        builder.HasKey(lc => new { lc.LeadId, lc.ContactId });

        builder.Property(lc => lc.LeadId)
            .HasColumnName("lead_id");

        builder.Property(lc => lc.ContactId)
            .HasColumnName("contact_id");

        builder.Property(lc => lc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}

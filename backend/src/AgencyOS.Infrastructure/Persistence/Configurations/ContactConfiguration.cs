using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contact");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(120);

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(40);

        builder.Property(c => c.Mobile)
            .HasColumnName("mobile")
            .HasMaxLength(40);

        builder.Property(c => c.JobTitle)
            .HasColumnName("job_title")
            .HasMaxLength(120);

        builder.Property(c => c.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(c => c.LeadContacts)
            .WithOne(lc => lc.Contact)
            .HasForeignKey(lc => lc.ContactId);
    }
}

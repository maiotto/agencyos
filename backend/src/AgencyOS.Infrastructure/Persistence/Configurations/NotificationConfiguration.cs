using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(item => item.UserId).HasColumnName("user_id").HasMaxLength(200).IsRequired();
        builder.Property(item => item.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(item => item.Message).HasColumnName("message").HasMaxLength(2000).IsRequired();
        builder.Property(item => item.Category).HasColumnName("category").HasMaxLength(50).IsRequired();
        builder.Property(item => item.Priority).HasColumnName("priority").HasMaxLength(30).IsRequired();
        builder.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(item => item.SourceEntity).HasColumnName("source_entity").HasMaxLength(80).IsRequired();
        builder.Property(item => item.SourceEntityId).HasColumnName("source_entity_id");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(item => item.ReadAt).HasColumnName("read_at");
        builder.Property(item => item.Archived).HasColumnName("archived").IsRequired();

        builder.Ignore(item => item.IsUnread);
        builder.Ignore(item => item.IsRead);

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not (nameof(Notification.IsUnread) or nameof(Notification.IsRead)))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => new { item.CompanyId, item.UserId })
            .HasDatabaseName("idx_notification_company_user");
        builder.HasIndex(item => new { item.CompanyId, item.UserId, item.Status })
            .HasDatabaseName("idx_notification_company_user_status");
        builder.HasIndex(item => new { item.CompanyId, item.UserId, item.Category })
            .HasDatabaseName("idx_notification_company_user_category");
        builder.HasIndex(item => new { item.CompanyId, item.UserId, item.Priority })
            .HasDatabaseName("idx_notification_company_user_priority");
        builder.HasIndex(item => new { item.CompanyId, item.UserId, item.Archived })
            .HasDatabaseName("idx_notification_company_user_archived");
        builder.HasIndex(item => item.CreatedAt).HasDatabaseName("idx_notification_created_at");
        builder.HasIndex(item => new { item.SourceEntity, item.SourceEntityId })
            .HasDatabaseName("idx_notification_source_entity");
    }
}

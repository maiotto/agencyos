using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_event");

        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.Id).HasColumnName("id");
        builder.Property(audit => audit.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        builder.Property(audit => audit.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(audit => audit.EntityVersion).HasColumnName("entity_version").HasMaxLength(50);
        builder.Property(audit => audit.EventType).HasColumnName("event_type").HasMaxLength(80).IsRequired();
        builder.Property(audit => audit.Action).HasColumnName("action").HasMaxLength(120).IsRequired();
        builder.Property(audit => audit.CompanyId).HasColumnName("company_id");
        builder.Property(audit => audit.UserId).HasColumnName("user_id").HasMaxLength(200).IsRequired();
        builder.Property(audit => audit.UserName).HasColumnName("user_name").HasMaxLength(200).IsRequired();
        builder.Property(audit => audit.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(audit => audit.Source).HasColumnName("source").HasMaxLength(80).IsRequired();
        builder.Property(audit => audit.CorrelationId).HasColumnName("correlation_id");
        builder.Property(audit => audit.SessionId).HasColumnName("session_id").HasMaxLength(100);
        builder.Property(audit => audit.RequestId).HasColumnName("request_id").HasMaxLength(100);
        builder.Property(audit => audit.PreviousState).HasColumnName("previous_state").HasColumnType("jsonb");
        builder.Property(audit => audit.CurrentState).HasColumnName("current_state").HasColumnType("jsonb");
        builder.Property(audit => audit.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(audit => new { audit.EntityType, audit.EntityId })
            .HasDatabaseName("idx_audit_event_entity");
        builder.HasIndex(audit => audit.EntityId).HasDatabaseName("idx_audit_event_entity_id");
        builder.HasIndex(audit => audit.CompanyId).HasDatabaseName("idx_audit_event_company_id");
        builder.HasIndex(audit => audit.UserId).HasDatabaseName("idx_audit_event_user_id");
        builder.HasIndex(audit => audit.CorrelationId).HasDatabaseName("idx_audit_event_correlation_id");
        builder.HasIndex(audit => audit.EventType).HasDatabaseName("idx_audit_event_event_type");
        builder.HasIndex(audit => audit.OccurredAt).HasDatabaseName("idx_audit_event_occurred_at");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class DecisionConfiguration : IEntityTypeConfiguration<Decision>
{
    public void Configure(EntityTypeBuilder<Decision> builder)
    {
        builder.ToTable("decision");

        builder.HasKey(decision => decision.Id);

        builder.Property(decision => decision.Id).HasColumnName("id");
        builder.Property(decision => decision.RecommendationId).HasColumnName("recommendation_id").IsRequired();
        builder.Property(decision => decision.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(decision => decision.MissionId).HasColumnName("mission_id").IsRequired();
        builder.Property(decision => decision.ContractId).HasColumnName("contract_id").IsRequired();
        builder.Property(decision => decision.DecisionStatus).HasColumnName("decision_status").HasMaxLength(30).IsRequired();
        builder.Property(decision => decision.ImplementationStatus).HasColumnName("implementation_status").HasMaxLength(30).IsRequired();
        builder.Property(decision => decision.DecisionDate).HasColumnName("decision_date").IsRequired();
        builder.Property(decision => decision.ImplementationDate).HasColumnName("implementation_date");
        builder.Property(decision => decision.CompletedDate).HasColumnName("completed_date");
        builder.Property(decision => decision.Outcome).HasColumnName("outcome").HasMaxLength(2000);
        builder.Property(decision => decision.BusinessValue).HasColumnName("business_value").HasMaxLength(2000);
        builder.Property(decision => decision.CreatedBy).HasColumnName("created_by").HasMaxLength(200).IsRequired();
        builder.Property(decision => decision.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(decision => decision.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(decision => decision.IsCompleted);
        builder.Ignore(decision => decision.IsCancelled);

        builder.HasIndex(decision => decision.RecommendationId)
            .IsUnique()
            .HasDatabaseName("uq_decision_recommendation_id");

        builder.HasMany(decision => decision.Timeline)
            .WithOne()
            .HasForeignKey(entry => entry.DecisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(decision => decision.Timeline)
            .HasField("_timeline")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(Decision.IsCompleted)
                and not nameof(Decision.IsCancelled))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(decision => decision.CompanyId).HasDatabaseName("idx_decision_company_id");
        builder.HasIndex(decision => decision.MissionId).HasDatabaseName("idx_decision_mission_id");
        builder.HasIndex(decision => decision.ContractId).HasDatabaseName("idx_decision_contract_id");
        builder.HasIndex(decision => decision.DecisionStatus).HasDatabaseName("idx_decision_decision_status");
        builder.HasIndex(decision => decision.ImplementationStatus).HasDatabaseName("idx_decision_implementation_status");
        builder.HasIndex(decision => decision.DecisionDate).HasDatabaseName("idx_decision_decision_date");
        builder.HasIndex(decision => decision.CreatedAt).HasDatabaseName("idx_decision_created_at");
    }
}

public class DecisionTimelineEntryConfiguration : IEntityTypeConfiguration<DecisionTimelineEntry>
{
    public void Configure(EntityTypeBuilder<DecisionTimelineEntry> builder)
    {
        builder.ToTable("decision_timeline");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).HasColumnName("id");
        builder.Property(entry => entry.DecisionId).HasColumnName("decision_id").IsRequired();
        builder.Property(entry => entry.EventType).HasColumnName("event_type").HasMaxLength(40).IsRequired();
        builder.Property(entry => entry.FromDecisionStatus).HasColumnName("from_decision_status").HasMaxLength(30).IsRequired();
        builder.Property(entry => entry.ToDecisionStatus).HasColumnName("to_decision_status").HasMaxLength(30).IsRequired();
        builder.Property(entry => entry.FromImplementationStatus).HasColumnName("from_implementation_status").HasMaxLength(30).IsRequired();
        builder.Property(entry => entry.ToImplementationStatus).HasColumnName("to_implementation_status").HasMaxLength(30).IsRequired();
        builder.Property(entry => entry.Actor).HasColumnName("actor").HasMaxLength(200).IsRequired();
        builder.Property(entry => entry.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(entry => entry.OccurredAt).HasColumnName("occurred_at").IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(entry => entry.DecisionId).HasDatabaseName("idx_decision_timeline_decision_id");
        builder.HasIndex(entry => entry.OccurredAt).HasDatabaseName("idx_decision_timeline_occurred_at");
    }
}

using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class RecommendationHistoryConfiguration : IEntityTypeConfiguration<RecommendationHistory>
{
    public void Configure(EntityTypeBuilder<RecommendationHistory> builder)
    {
        builder.ToTable("recommendation_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Id).HasColumnName("id");
        builder.Property(history => history.RecommendationId).HasColumnName("recommendation_id").IsRequired();
        builder.Property(history => history.RecommendationNumber)
            .HasColumnName("recommendation_number")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(history => history.RecommendationVersion)
            .HasColumnName("recommendation_version")
            .IsRequired();
        builder.Property(history => history.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(history => history.MissionId).HasColumnName("mission_id").IsRequired();
        builder.Property(history => history.ContractId).HasColumnName("contract_id").IsRequired();
        builder.Property(history => history.DeliveryStrategyId)
            .HasColumnName("delivery_strategy_id")
            .IsRequired();
        builder.Property(history => history.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(history => history.Summary).HasColumnName("summary").HasMaxLength(2000);
        builder.Property(history => history.EventType).HasColumnName("event_type").HasMaxLength(40).IsRequired();
        builder.Property(history => history.RecommendationStatus)
            .HasColumnName("recommendation_status")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(history => history.WorkflowStatus).HasColumnName("workflow_status").HasMaxLength(30);
        builder.Property(history => history.WorkflowId).HasColumnName("workflow_id");
        builder.Property(history => history.Approver).HasColumnName("approver").HasMaxLength(200);
        builder.Property(history => history.ApprovalDate).HasColumnName("approval_date");
        builder.Property(history => history.ApprovalComment).HasColumnName("approval_comment").HasMaxLength(2000);
        builder.Property(history => history.Score).HasColumnName("score").HasPrecision(12, 4);
        builder.Property(history => history.Rank).HasColumnName("rank");
        builder.Property(history => history.PlanningTemplateId).HasColumnName("planning_template_id");
        builder.Property(history => history.DecisionEngineVersion)
            .HasColumnName("decision_engine_version")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(history => history.CapacitySnapshot)
            .HasColumnName("capacity_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(history => history.WorkloadSnapshot)
            .HasColumnName("workload_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(history => history.RecommendationPayload)
            .HasColumnName("recommendation_payload")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(history => history.CreatedBy).HasColumnName("created_by").HasMaxLength(200).IsRequired();
        builder.Property(history => history.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(history => history.DecisionProfileId).HasColumnName("decision_profile_id");
        builder.Property(history => history.DecisionProfileVersion)
            .HasColumnName("decision_profile_version")
            .IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(history => history.RecommendationId)
            .HasDatabaseName("idx_recommendation_history_recommendation_id");
        builder.HasIndex(history => history.RecommendationNumber)
            .HasDatabaseName("idx_recommendation_history_recommendation_number");
        builder.HasIndex(history => history.CompanyId)
            .HasDatabaseName("idx_recommendation_history_company_id");
        builder.HasIndex(history => history.MissionId)
            .HasDatabaseName("idx_recommendation_history_mission_id");
        builder.HasIndex(history => history.ContractId)
            .HasDatabaseName("idx_recommendation_history_contract_id");
        builder.HasIndex(history => history.CreatedAt)
            .HasDatabaseName("idx_recommendation_history_created_at");
        builder.HasIndex(history => history.WorkflowStatus)
            .HasDatabaseName("idx_recommendation_history_workflow_status");
    }
}

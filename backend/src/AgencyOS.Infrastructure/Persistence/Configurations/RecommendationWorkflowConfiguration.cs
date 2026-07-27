using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class RecommendationWorkflowConfiguration : IEntityTypeConfiguration<RecommendationWorkflow>
{
    public void Configure(EntityTypeBuilder<RecommendationWorkflow> builder)
    {
        builder.ToTable("recommendation_workflow");

        builder.HasKey(workflow => workflow.Id);

        builder.Property(workflow => workflow.Id).HasColumnName("id");
        builder.Property(workflow => workflow.RecommendationId).HasColumnName("recommendation_id");
        builder.Property(workflow => workflow.DeliveryStrategyId).HasColumnName("delivery_strategy_id").IsRequired();
        builder.Property(workflow => workflow.ContractId).HasColumnName("contract_id").IsRequired();
        builder.Property(workflow => workflow.MissionId).HasColumnName("mission_id").IsRequired();
        builder.Property(workflow => workflow.CompanyDecisionProfileId).HasColumnName("company_decision_profile_id");
        builder.Property(workflow => workflow.CompanyId).HasColumnName("company_id");
        builder.Property(workflow => workflow.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(workflow => workflow.Summary).HasColumnName("summary").HasMaxLength(2000);
        builder.Property(workflow => workflow.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(workflow => workflow.CreatedBy).HasColumnName("created_by").HasMaxLength(200).IsRequired();
        builder.Property(workflow => workflow.Approver).HasColumnName("approver").HasMaxLength(200);
        builder.Property(workflow => workflow.ApprovalDate).HasColumnName("approval_date");
        builder.Property(workflow => workflow.ApprovalComment).HasColumnName("approval_comment").HasMaxLength(2000);
        builder.Property(workflow => workflow.RankPosition).HasColumnName("rank_position");
        builder.Property(workflow => workflow.FinalScore).HasColumnName("final_score").HasPrecision(12, 4);
        builder.Property(workflow => workflow.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(workflow => workflow.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(workflow => workflow.IsApproved);
        builder.Ignore(workflow => workflow.IsImmutable);

        builder.HasMany(workflow => workflow.Transitions)
            .WithOne()
            .HasForeignKey(transition => transition.RecommendationWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(workflow => workflow.Transitions)
            .HasField("_transitions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(RecommendationWorkflow.IsApproved)
                and not nameof(RecommendationWorkflow.IsImmutable))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(workflow => workflow.Status)
            .HasDatabaseName("idx_recommendation_workflow_status");
        builder.HasIndex(workflow => workflow.RecommendationId)
            .HasDatabaseName("idx_recommendation_workflow_recommendation_id");
        builder.HasIndex(workflow => workflow.DeliveryStrategyId)
            .HasDatabaseName("idx_recommendation_workflow_delivery_strategy_id");
        builder.HasIndex(workflow => workflow.ContractId)
            .HasDatabaseName("idx_recommendation_workflow_contract_id");
        builder.HasIndex(workflow => workflow.MissionId)
            .HasDatabaseName("idx_recommendation_workflow_mission_id");
        builder.HasIndex(workflow => workflow.CompanyId)
            .HasDatabaseName("idx_recommendation_workflow_company_id");
    }
}

public class RecommendationWorkflowTransitionConfiguration
    : IEntityTypeConfiguration<RecommendationWorkflowTransition>
{
    public void Configure(EntityTypeBuilder<RecommendationWorkflowTransition> builder)
    {
        builder.ToTable("recommendation_workflow_transition");

        builder.HasKey(transition => transition.Id);

        builder.Property(transition => transition.Id).HasColumnName("id");
        builder.Property(transition => transition.RecommendationWorkflowId)
            .HasColumnName("recommendation_workflow_id")
            .IsRequired();
        builder.Property(transition => transition.FromStatus).HasColumnName("from_status").HasMaxLength(30).IsRequired();
        builder.Property(transition => transition.ToStatus).HasColumnName("to_status").HasMaxLength(30).IsRequired();
        builder.Property(transition => transition.Actor).HasColumnName("actor").HasMaxLength(200).IsRequired();
        builder.Property(transition => transition.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(transition => transition.OccurredAt).HasColumnName("occurred_at").IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(transition => transition.RecommendationWorkflowId)
            .HasDatabaseName("idx_recommendation_workflow_transition_workflow_id");
        builder.HasIndex(transition => transition.OccurredAt)
            .HasDatabaseName("idx_recommendation_workflow_transition_occurred_at");
    }
}

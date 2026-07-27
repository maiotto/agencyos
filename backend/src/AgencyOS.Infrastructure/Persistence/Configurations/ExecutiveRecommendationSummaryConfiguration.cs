using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ExecutiveRecommendationSummaryConfiguration
    : IEntityTypeConfiguration<ExecutiveRecommendationSummary>
{
    public void Configure(EntityTypeBuilder<ExecutiveRecommendationSummary> builder)
    {
        builder.ToTable("executive_recommendation_summary");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.RecommendationId).HasColumnName("recommendation_id").IsRequired();
        builder.Property(item => item.AIRecommendationId).HasColumnName("ai_recommendation_id");
        builder.Property(item => item.ExplainabilityId).HasColumnName("explainability_id");
        builder.Property(item => item.SummaryVersion).HasColumnName("summary_version").IsRequired();
        builder.Property(item => item.ExecutiveSummary).HasColumnName("executive_summary").HasMaxLength(4000).IsRequired();
        builder.Property(item => item.KeyDecisionFactors).HasColumnName("key_decision_factors").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.BusinessImpact).HasColumnName("business_impact").IsRequired();
        builder.Property(item => item.CapacityImpact).HasColumnName("capacity_impact").IsRequired();
        builder.Property(item => item.WorkloadImpact).HasColumnName("workload_impact").IsRequired();
        builder.Property(item => item.Risks).HasColumnName("risks").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.Assumptions).HasColumnName("assumptions").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.ConfidenceLevel).HasColumnName("confidence_level").HasPrecision(5, 2).IsRequired();
        builder.Property(item => item.RecommendedActions).HasColumnName("recommended_actions").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.GeneratedAt).HasColumnName("generated_at").IsRequired();
        builder.Property(item => item.GeneratedBy).HasColumnName("generated_by").HasMaxLength(200).IsRequired();
        builder.Property(item => item.ModelVersion).HasColumnName("model_version").HasMaxLength(50).IsRequired();
        builder.Property(item => item.PromptVersion).HasColumnName("prompt_version").HasMaxLength(50).IsRequired();
        builder.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(item => item.ArchivedAt).HasColumnName("archived_at");
        builder.Property(item => item.DecisionProfileId).HasColumnName("decision_profile_id");
        builder.Property(item => item.DecisionProfileVersion)
            .HasColumnName("decision_profile_version")
            .IsRequired();
        builder.Property(item => item.CompanyId).HasColumnName("company_id");

        builder.Ignore(item => item.Archived);

        builder.HasIndex(item => new { item.RecommendationId, item.SummaryVersion })
            .IsUnique()
            .HasDatabaseName("uq_executive_summary_recommendation_version");

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(ExecutiveRecommendationSummary.Archived))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => item.RecommendationId)
            .HasDatabaseName("idx_executive_summary_recommendation_id");
        builder.HasIndex(item => item.AIRecommendationId)
            .HasDatabaseName("idx_executive_summary_ai_recommendation_id");
        builder.HasIndex(item => item.ExplainabilityId)
            .HasDatabaseName("idx_executive_summary_explainability_id");
        builder.HasIndex(item => item.Status)
            .HasDatabaseName("idx_executive_summary_status");
        builder.HasIndex(item => item.GeneratedAt)
            .HasDatabaseName("idx_executive_summary_generated_at");
        builder.HasIndex(item => item.ConfidenceLevel)
            .HasDatabaseName("idx_executive_summary_confidence_level");
        builder.HasIndex(item => item.ModelVersion)
            .HasDatabaseName("idx_executive_summary_model_version");
        builder.HasIndex(item => item.CompanyId)
            .HasDatabaseName("idx_executive_summary_company_id");
    }
}

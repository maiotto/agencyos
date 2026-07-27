using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class ExplainabilityConfiguration : IEntityTypeConfiguration<Explainability>
{
    public void Configure(EntityTypeBuilder<Explainability> builder)
    {
        builder.ToTable("recommendation_explainability");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.RecommendationId).HasColumnName("recommendation_id").IsRequired();
        builder.Property(item => item.AIRecommendationId).HasColumnName("ai_recommendation_id");
        builder.Property(item => item.ExplanationType).HasColumnName("explanation_type").HasMaxLength(40).IsRequired();
        builder.Property(item => item.GenerationVersion).HasColumnName("generation_version").IsRequired();
        builder.Property(item => item.ExecutiveSummary).HasColumnName("executive_summary").HasMaxLength(2000).IsRequired();
        builder.Property(item => item.DetailedExplanation).HasColumnName("detailed_explanation").IsRequired();
        builder.Property(item => item.DecisionFactors).HasColumnName("decision_factors").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.Assumptions).HasColumnName("assumptions").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.Risks).HasColumnName("risks").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.ConfidenceExplanation).HasColumnName("confidence_explanation").IsRequired();
        builder.Property(item => item.CapacityExplanation).HasColumnName("capacity_explanation").IsRequired();
        builder.Property(item => item.WorkloadExplanation).HasColumnName("workload_explanation").IsRequired();
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

        builder.HasIndex(item => new { item.RecommendationId, item.GenerationVersion })
            .IsUnique()
            .HasDatabaseName("uq_recommendation_explainability_recommendation_generation");

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(Explainability.Archived))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => item.RecommendationId)
            .HasDatabaseName("idx_recommendation_explainability_recommendation_id");
        builder.HasIndex(item => item.AIRecommendationId)
            .HasDatabaseName("idx_recommendation_explainability_ai_recommendation_id");
        builder.HasIndex(item => item.ExplanationType)
            .HasDatabaseName("idx_recommendation_explainability_type");
        builder.HasIndex(item => item.Status)
            .HasDatabaseName("idx_recommendation_explainability_status");
        builder.HasIndex(item => item.GeneratedAt)
            .HasDatabaseName("idx_recommendation_explainability_generated_at");
        builder.HasIndex(item => item.ModelVersion)
            .HasDatabaseName("idx_recommendation_explainability_model_version");
        builder.HasIndex(item => item.CompanyId)
            .HasDatabaseName("idx_recommendation_explainability_company_id");
    }
}

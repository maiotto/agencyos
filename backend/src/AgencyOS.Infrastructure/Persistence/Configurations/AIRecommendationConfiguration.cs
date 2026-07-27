using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class AIRecommendationConfiguration : IEntityTypeConfiguration<AIRecommendation>
{
    public void Configure(EntityTypeBuilder<AIRecommendation> builder)
    {
        builder.ToTable("ai_recommendation");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.RecommendationId).HasColumnName("recommendation_id").IsRequired();
        builder.Property(item => item.RecommendationVersion).HasColumnName("recommendation_version").IsRequired();
        builder.Property(item => item.GenerationVersion).HasColumnName("generation_version").IsRequired();
        builder.Property(item => item.GeneratedAt).HasColumnName("generated_at").IsRequired();
        builder.Property(item => item.GeneratedBy).HasColumnName("generated_by").HasMaxLength(200).IsRequired();
        builder.Property(item => item.ConfidenceScore).HasColumnName("confidence_score").HasPrecision(5, 2).IsRequired();
        builder.Property(item => item.ExecutiveSummary).HasColumnName("executive_summary").HasMaxLength(2000).IsRequired();
        builder.Property(item => item.Reasoning).HasColumnName("reasoning").IsRequired();
        builder.Property(item => item.Assumptions).HasColumnName("assumptions").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.Risks).HasColumnName("risks").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.Alternatives).HasColumnName("alternatives").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.SuggestedDeliveryStrategy).HasColumnName("suggested_delivery_strategy").HasMaxLength(300).IsRequired();
        builder.Property(item => item.SuggestedCapacityImpact).HasColumnName("suggested_capacity_impact").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.SuggestedWorkloadImpact).HasColumnName("suggested_workload_impact").HasColumnType("jsonb").IsRequired();
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
            .HasDatabaseName("uq_ai_recommendation_recommendation_generation");

        foreach (var property in builder.Metadata.GetProperties())
        {
            if (property.Name is not nameof(AIRecommendation.Archived))
            {
                builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        }

        builder.HasIndex(item => item.RecommendationId).HasDatabaseName("idx_ai_recommendation_recommendation_id");
        builder.HasIndex(item => item.Status).HasDatabaseName("idx_ai_recommendation_status");
        builder.HasIndex(item => item.GeneratedAt).HasDatabaseName("idx_ai_recommendation_generated_at");
        builder.HasIndex(item => item.ConfidenceScore).HasDatabaseName("idx_ai_recommendation_confidence_score");
        builder.HasIndex(item => item.ModelVersion).HasDatabaseName("idx_ai_recommendation_model_version");
        builder.HasIndex(item => item.CompanyId).HasDatabaseName("idx_ai_recommendation_company_id");
    }
}

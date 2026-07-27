using AgencyOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgencyOS.Infrastructure.Persistence.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("recommendation");

        builder.HasKey(recommendation => recommendation.Id);

        builder.Property(recommendation => recommendation.Id).HasColumnName("id");
        builder.Property(recommendation => recommendation.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(recommendation => recommendation.MissionId).HasColumnName("mission_id").IsRequired();
        builder.Property(recommendation => recommendation.ContractId).HasColumnName("contract_id").IsRequired();
        builder.Property(recommendation => recommendation.DeliveryStrategyId)
            .HasColumnName("delivery_strategy_id")
            .IsRequired();
        builder.Property(recommendation => recommendation.RecommendationNumber)
            .HasColumnName("recommendation_number")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(recommendation => recommendation.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(recommendation => recommendation.Summary).HasColumnName("summary").HasMaxLength(2000);
        builder.Property(recommendation => recommendation.Reason).HasColumnName("reason").HasMaxLength(4000);
        builder.Property(recommendation => recommendation.Score).HasColumnName("score").HasPrecision(12, 4);
        builder.Property(recommendation => recommendation.Rank).HasColumnName("rank");
        builder.Property(recommendation => recommendation.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(recommendation => recommendation.Version).HasColumnName("version").IsRequired();
        builder.Property(recommendation => recommendation.DecisionEngineVersion)
            .HasColumnName("decision_engine_version")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(recommendation => recommendation.PlanningTemplateId)
            .HasColumnName("planning_template_id");
        builder.Property(recommendation => recommendation.CapacitySnapshot)
            .HasColumnName("capacity_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(recommendation => recommendation.WorkloadSnapshot)
            .HasColumnName("workload_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(recommendation => recommendation.RecommendationPayload)
            .HasColumnName("recommendation_payload")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(recommendation => recommendation.GeneratedAt).HasColumnName("generated_at").IsRequired();
        builder.Property(recommendation => recommendation.GeneratedBy)
            .HasColumnName("generated_by")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(recommendation => recommendation.Archived).HasColumnName("archived").IsRequired();
        builder.Property(recommendation => recommendation.ArchivedAt).HasColumnName("archived_at");
        builder.Property(recommendation => recommendation.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(recommendation => recommendation.DecisionProfileId)
            .HasColumnName("decision_profile_id");
        builder.Property(recommendation => recommendation.DecisionProfileVersion)
            .HasColumnName("decision_profile_version")
            .IsRequired();

        foreach (var property in builder.Metadata.GetProperties())
        {
            builder.Property(property.Name).UsePropertyAccessMode(PropertyAccessMode.Property);
        }

        builder.HasIndex(recommendation => recommendation.CompanyId)
            .HasDatabaseName("idx_recommendation_company_id");
        builder.HasIndex(recommendation => recommendation.MissionId)
            .HasDatabaseName("idx_recommendation_mission_id");
        builder.HasIndex(recommendation => recommendation.ContractId)
            .HasDatabaseName("idx_recommendation_contract_id");
        builder.HasIndex(recommendation => recommendation.Status)
            .HasDatabaseName("idx_recommendation_status");
        builder.HasIndex(recommendation => recommendation.RecommendationNumber)
            .HasDatabaseName("idx_recommendation_number");
        builder.HasIndex(recommendation => recommendation.Version)
            .HasDatabaseName("idx_recommendation_version");
        builder.HasIndex(recommendation => recommendation.GeneratedAt)
            .HasDatabaseName("idx_recommendation_generated_at");
        builder.HasIndex(recommendation => new { recommendation.RecommendationNumber, recommendation.Version })
            .IsUnique()
            .HasDatabaseName("uq_recommendation_number_version");
    }
}

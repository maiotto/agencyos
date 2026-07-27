using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Mappings;

public static class RecommendationMappings
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static RecommendationResponse ToResponse(Recommendation recommendation) =>
        new()
        {
            Id = recommendation.Id,
            CompanyId = recommendation.CompanyId,
            MissionId = recommendation.MissionId,
            ContractId = recommendation.ContractId,
            DeliveryStrategyId = recommendation.DeliveryStrategyId,
            RecommendationNumber = recommendation.RecommendationNumber,
            Title = recommendation.Title,
            Summary = recommendation.Summary,
            Reason = recommendation.Reason,
            Score = recommendation.Score,
            Rank = recommendation.Rank,
            Status = recommendation.Status,
            Version = recommendation.Version,
            DecisionEngineVersion = recommendation.DecisionEngineVersion,
            PlanningTemplateId = recommendation.PlanningTemplateId,
            CapacitySnapshot = recommendation.CapacitySnapshot,
            WorkloadSnapshot = recommendation.WorkloadSnapshot,
            RecommendationPayload = recommendation.RecommendationPayload,
            GeneratedAt = recommendation.GeneratedAt,
            GeneratedBy = recommendation.GeneratedBy,
            Archived = recommendation.Archived,
            ArchivedAt = recommendation.ArchivedAt,
            CreatedAt = recommendation.CreatedAt,
            DecisionProfileId = recommendation.DecisionProfileId,
            DecisionProfileVersion = recommendation.DecisionProfileVersion
        };

    public static string SerializePayload(object payload) =>
        JsonSerializer.Serialize(payload, SerializerOptions);

    public static string BuildRecommendationNumber(Guid contractId, Guid deliveryStrategyId, DateTimeOffset generatedAt) =>
        $"REC-{generatedAt:yyyyMMdd}-{contractId.ToString("N")[..6].ToUpperInvariant()}-{deliveryStrategyId.ToString("N")[..6].ToUpperInvariant()}";

    public static string BuildReason(RankedDeliveryStrategyResponse ranked)
    {
        var factorSummary = string.Join(
            ", ",
            ranked.DecisionFactors.Factors.Select(factor =>
                $"{factor.Dimension}={factor.NormalizedScore:0.##}"));

        return string.IsNullOrWhiteSpace(factorSummary)
            ? $"Rank {ranked.RankPosition} with final score {ranked.FinalScore:0.####}."
            : $"Rank {ranked.RankPosition} with final score {ranked.FinalScore:0.####}. Factors: {factorSummary}.";
    }
}

using System.Text.Json;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic advisory generator (US-301). No external LLM providers.
/// Analyzes Recommendation score/rank/snapshots and proposes alternatives with rationale.
/// </summary>
public class AIRecommendationGenerationService : IAIRecommendationGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AIRecommendation Generate(
        Recommendation recommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt)
    {
        var score = recommendation.Score ?? 0m;
        var rank = recommendation.Rank ?? 99;
        var confidence = ComputeConfidence(score, rank);
        var strategy = SuggestStrategy(recommendation.Title, score, rank);
        var assumptions = BuildAssumptions(recommendation);
        var risks = BuildRisks(score, rank);
        var alternatives = BuildAlternatives(recommendation, strategy);
        var capacityImpact = BuildCapacityImpact(recommendation);
        var workloadImpact = BuildWorkloadImpact(recommendation);
        var reasoning = BuildReasoning(recommendation, strategy, confidence);
        var summary =
            $"Advisory alternative '{strategy}' for recommendation '{recommendation.Title}' " +
            $"(v{recommendation.Version}) with confidence {confidence:0.##}%.";

        return AIRecommendation.Generate(
            recommendation.Id,
            recommendation.Version,
            generationVersion,
            generatedAt,
            generatedBy,
            confidence,
            summary,
            reasoning,
            assumptions,
            risks,
            alternatives,
            strategy,
            capacityImpact,
            workloadImpact,
            AIRecommendationVersions.CurrentModelVersion,
            AIRecommendationVersions.CurrentPromptVersion,
            recommendation.DecisionProfileId,
            recommendation.DecisionProfileVersion,
            recommendation.CompanyId);
    }

    private static decimal ComputeConfidence(decimal score, int rank)
    {
        var rankBoost = Math.Max(0, 15 - (rank * 3));
        var confidence = Math.Clamp(score * 0.75m + rankBoost, 35m, 95m);
        return decimal.Round(confidence, 2, MidpointRounding.AwayFromZero);
    }

    private static string SuggestStrategy(string title, decimal score, int rank)
    {
        if (rank <= 1 && score >= 80)
        {
            return $"{title} (AI-reinforced primary)";
        }

        if (score < 60)
        {
            return "Hybrid delivery with staged outsourcing";
        }

        if (rank >= 3)
        {
            return "Human-led delivery with AI acceleration";
        }

        return "Balanced human + AI delivery mix";
    }

    private static string BuildAssumptions(Recommendation recommendation) =>
        JsonSerializer.Serialize(new[]
        {
            "Capacity and workload snapshots remain representative of the planning period.",
            "Decision Engine ranking inputs are complete and current.",
            $"Planning template reference: {(recommendation.PlanningTemplateId?.ToString() ?? "none")}.",
            "Human approval remains mandatory before any execution (BR-1603)."
        }, JsonOptions);

    private static string BuildRisks(decimal score, int rank) =>
        JsonSerializer.Serialize(new[]
        {
            score < 70
                ? "Low engine score increases delivery uncertainty."
                : "Score appears adequate but should be validated with planners.",
            rank > 1
                ? "Non-primary rank may indicate competing strategies."
                : "Primary rank may create over-confidence bias.",
            "AI suggestion is advisory and may omit operational constraints not present in snapshots."
        }, JsonOptions);

    private static string BuildAlternatives(Recommendation recommendation, string suggested) =>
        JsonSerializer.Serialize(new object[]
        {
            new
            {
                name = suggested,
                rationale = "Primary advisory alternative derived from score/rank heuristics.",
                relativePriority = 1
            },
            new
            {
                name = "Keep current recommendation unchanged",
                rationale = "Preserve Decision Engine output when operational constraints dominate (BR-1601).",
                relativePriority = 2
            },
            new
            {
                name = "Increase outsourcing share",
                rationale = "Useful when capacity snapshot indicates pressure.",
                relativePriority = 3,
                relatedTitle = recommendation.Title
            }
        }, JsonOptions);

    private static string BuildCapacityImpact(Recommendation recommendation) =>
        JsonSerializer.Serialize(new
        {
            basis = "recommendation.capacitySnapshot",
            snapshot = TryParseOrRaw(recommendation.CapacitySnapshot),
            advisoryDeltaPercent = -5,
            note = "AI suggests a modest capacity buffer to absorb variance."
        }, JsonOptions);

    private static string BuildWorkloadImpact(Recommendation recommendation) =>
        JsonSerializer.Serialize(new
        {
            basis = "recommendation.workloadSnapshot",
            snapshot = TryParseOrRaw(recommendation.WorkloadSnapshot),
            advisoryDeltaPercent = -8,
            note = "AI suggests smoothing peak workload through staged delivery."
        }, JsonOptions);

    private static string BuildReasoning(
        Recommendation recommendation,
        string strategy,
        decimal confidence) =>
        "Deterministic advisor evaluated Recommendation " +
        $"'{recommendation.RecommendationNumber}' v{recommendation.Version} " +
        $"(score={recommendation.Score?.ToString() ?? "n/a"}, rank={recommendation.Rank?.ToString() ?? "n/a"}). " +
        $"Suggested strategy '{strategy}' with confidence {confidence:0.##}%. " +
        "This output does not replace the Recommendation and requires human approval before workflow/decision actions.";

    private static object TryParseOrRaw(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return JsonSerializer.Deserialize<object>(document.RootElement.GetRawText(), JsonOptions) ?? new { };
        }
        catch (JsonException)
        {
            return new { raw = json };
        }
    }
}

using System.Text.Json;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic explainability generator (US-302). No external LLM providers.
/// Produces natural-language explanations for Recommendations and AI Recommendations.
/// </summary>
public class ExplainabilityGenerationService : IExplainabilityGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Explainability GenerateForRecommendation(
        Recommendation recommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt)
    {
        var score = recommendation.Score ?? 0m;
        var rank = recommendation.Rank ?? 99;
        var confidenceText = BuildConfidenceExplanation(score, rank, source: "Decision Engine");
        var factors = BuildDecisionFactors(recommendation, null);
        var assumptions = BuildAssumptions(recommendation, null);
        var risks = BuildRisks(score, rank, aiAdvisory: false);
        var capacity = BuildCapacityExplanation(recommendation, null);
        var workload = BuildWorkloadExplanation(recommendation, null);
        var detailed = BuildDetailedExplanation(recommendation, null, confidenceText);
        var summary =
            $"Explanation of Recommendation '{recommendation.Title}' " +
            $"(#{recommendation.RecommendationNumber} v{recommendation.Version}): " +
            $"ranked #{rank} with score {score:0.##}.";

        return Explainability.Generate(
            recommendation.Id,
            null,
            ExplainabilityTypes.Recommendation,
            generationVersion,
            generatedAt,
            generatedBy,
            summary,
            detailed,
            factors,
            assumptions,
            risks,
            confidenceText,
            capacity,
            workload,
            ExplainabilityVersions.CurrentModelVersion,
            ExplainabilityVersions.CurrentPromptVersion,
            recommendation.DecisionProfileId,
            recommendation.DecisionProfileVersion,
            recommendation.CompanyId);
    }

    public Explainability GenerateForAIRecommendation(
        Recommendation recommendation,
        AIRecommendation aiRecommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt)
    {
        if (aiRecommendation.RecommendationId != recommendation.Id)
        {
            throw new InvalidOperationException(
                "AI Recommendation does not belong to the specified Recommendation.");
        }

        var score = recommendation.Score ?? 0m;
        var rank = recommendation.Rank ?? 99;
        var confidenceText = BuildConfidenceExplanation(
            aiRecommendation.ConfidenceScore,
            rank,
            source: "AI advisor");
        var factors = BuildDecisionFactors(recommendation, aiRecommendation);
        var assumptions = BuildAssumptions(recommendation, aiRecommendation);
        var risks = BuildRisks(score, rank, aiAdvisory: true);
        var capacity = BuildCapacityExplanation(recommendation, aiRecommendation);
        var workload = BuildWorkloadExplanation(recommendation, aiRecommendation);
        var detailed = BuildDetailedExplanation(recommendation, aiRecommendation, confidenceText);
        var summary =
            $"Explanation of AI advisory '{aiRecommendation.SuggestedDeliveryStrategy}' " +
            $"for Recommendation '{recommendation.Title}' " +
            $"(generation g{aiRecommendation.GenerationVersion}, confidence {aiRecommendation.ConfidenceScore:0.##}%).";

        return Explainability.Generate(
            recommendation.Id,
            aiRecommendation.Id,
            ExplainabilityTypes.AIRecommendation,
            generationVersion,
            generatedAt,
            generatedBy,
            summary,
            detailed,
            factors,
            assumptions,
            risks,
            confidenceText,
            capacity,
            workload,
            ExplainabilityVersions.CurrentModelVersion,
            ExplainabilityVersions.CurrentPromptVersion,
            recommendation.DecisionProfileId,
            recommendation.DecisionProfileVersion,
            recommendation.CompanyId);
    }

    private static string BuildConfidenceExplanation(decimal scoreOrConfidence, int rank, string source)
    {
        var band = scoreOrConfidence switch
        {
            >= 85 => "high",
            >= 70 => "moderate-high",
            >= 55 => "moderate",
            _ => "cautious"
        };

        return
            $"{source} confidence is {scoreOrConfidence:0.##}% ({band}). " +
            $"Rank position #{rank} influences trust in the primary strategy. " +
            "This explanation is informational and does not alter Recommendation logic (BR-1701).";
    }

    private static string BuildDecisionFactors(Recommendation recommendation, AIRecommendation? ai) =>
        JsonSerializer.Serialize(new object[]
        {
            new { factor = "title", value = recommendation.Title, weight = "primary" },
            new { factor = "score", value = recommendation.Score, weight = "high" },
            new { factor = "rank", value = recommendation.Rank, weight = "high" },
            new { factor = "decisionEngineVersion", value = recommendation.DecisionEngineVersion, weight = "medium" },
            new
            {
                factor = "planningTemplateId",
                value = recommendation.PlanningTemplateId,
                weight = "medium"
            },
            new
            {
                factor = "aiSuggestedStrategy",
                value = ai?.SuggestedDeliveryStrategy,
                weight = ai is null ? "none" : "advisory"
            },
            new
            {
                factor = "aiConfidenceScore",
                value = ai?.ConfidenceScore,
                weight = ai is null ? "none" : "advisory"
            }
        }, JsonOptions);

    private static string BuildAssumptions(Recommendation recommendation, AIRecommendation? ai) =>
        JsonSerializer.Serialize(new[]
        {
            "Decision Engine inputs and ranking remain unchanged by this explanation.",
            "Capacity and workload snapshots are representative of the planning window.",
            $"Planning template: {(recommendation.PlanningTemplateId?.ToString() ?? "none")}.",
            ai is null
                ? "Explanation covers the persisted Recommendation only."
                : "Explanation covers both the Recommendation and its AI advisory alternative.",
            "Human approval remains mandatory before execution."
        }, JsonOptions);

    private static string BuildRisks(decimal score, int rank, bool aiAdvisory) =>
        JsonSerializer.Serialize(new[]
        {
            score < 70
                ? "Lower engine score increases delivery uncertainty."
                : "Score appears adequate but should be validated operationally.",
            rank > 1
                ? "Non-primary rank may indicate competing strategies."
                : "Primary rank may create over-confidence bias.",
            aiAdvisory
                ? "AI advisory content is informational and may omit constraints not present in snapshots."
                : "Explainability does not validate commercial or contractual constraints beyond Recommendation payload."
        }, JsonOptions);

    private static string BuildCapacityExplanation(Recommendation recommendation, AIRecommendation? ai)
    {
        var advisory = ai is null
            ? "No AI capacity delta applied."
            : $"AI advisory capacity impact: {ai.SuggestedCapacityImpact}";

        return
            "Capacity explanation is derived from the Recommendation capacity snapshot. " +
            $"Snapshot length={recommendation.CapacitySnapshot?.Length ?? 0} chars. {advisory} " +
            "Explainability never recalculates Capacity Engine results.";
    }

    private static string BuildWorkloadExplanation(Recommendation recommendation, AIRecommendation? ai)
    {
        var advisory = ai is null
            ? "No AI workload delta applied."
            : $"AI advisory workload impact: {ai.SuggestedWorkloadImpact}";

        return
            "Workload explanation is derived from the Recommendation workload snapshot. " +
            $"Snapshot length={recommendation.WorkloadSnapshot?.Length ?? 0} chars. {advisory} " +
            "Explainability never recalculates Workload Engine results.";
    }

    private static string BuildDetailedExplanation(
        Recommendation recommendation,
        AIRecommendation? ai,
        string confidenceText)
    {
        var aiBlock = ai is null
            ? "No AI Recommendation was attached to this explanation."
            : $"AI advisory strategy '{ai.SuggestedDeliveryStrategy}' " +
              $"(model {ai.ModelVersion}, prompt {ai.PromptVersion}) was explained alongside the Recommendation.";

        return
            $"Detailed explanation for Recommendation '{recommendation.RecommendationNumber}' " +
            $"v{recommendation.Version} titled '{recommendation.Title}'. " +
            $"Reason recorded by Decision Engine: {recommendation.Reason ?? "n/a"}. " +
            $"Summary: {recommendation.Summary ?? "n/a"}. " +
            $"{confidenceText} {aiBlock} " +
            "This record is immutable and does not modify Recommendation, workflow, or decision state.";
    }
}

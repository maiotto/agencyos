using System.Text.Json;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic executive briefing generator (US-303). No external LLM providers.
/// Consolidates Recommendation, AI Recommendation, Explainability, and Decision context.
/// </summary>
public class ExecutiveRecommendationSummaryGenerationService
    : IExecutiveRecommendationSummaryGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ExecutiveRecommendationSummary Generate(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability,
        Decision? decision,
        int summaryVersion,
        string generatedBy,
        DateTimeOffset generatedAt)
    {
        if (aiRecommendation is not null && aiRecommendation.RecommendationId != recommendation.Id)
        {
            throw new InvalidOperationException(
                "AI Recommendation does not belong to the specified Recommendation.");
        }

        if (explainability is not null && explainability.RecommendationId != recommendation.Id)
        {
            throw new InvalidOperationException(
                "Explainability does not belong to the specified Recommendation.");
        }

        if (decision is not null && decision.RecommendationId != recommendation.Id)
        {
            throw new InvalidOperationException(
                "Decision does not belong to the specified Recommendation.");
        }

        var confidence = ComputeConfidence(recommendation, aiRecommendation);
        var factors = BuildKeyDecisionFactors(recommendation, aiRecommendation, explainability, decision);
        var risks = BuildRisks(recommendation, aiRecommendation, explainability);
        var assumptions = BuildAssumptions(recommendation, aiRecommendation, explainability);
        var actions = BuildRecommendedActions(recommendation, aiRecommendation, decision);
        var businessImpact = BuildBusinessImpact(recommendation, decision);
        var capacityImpact = BuildCapacityImpact(recommendation, aiRecommendation, explainability);
        var workloadImpact = BuildWorkloadImpact(recommendation, aiRecommendation, explainability);
        var summary =
            $"Executive briefing for '{recommendation.Title}' " +
            $"(#{recommendation.RecommendationNumber} v{recommendation.Version}). " +
            $"Score {recommendation.Score?.ToString("0.##") ?? "n/a"}, rank #{recommendation.Rank?.ToString() ?? "n/a"}, " +
            $"confidence {confidence:0.##}%. " +
            (aiRecommendation is null
                ? "No AI advisory attached."
                : $"AI advisory: {aiRecommendation.SuggestedDeliveryStrategy}.") +
            (decision is null
                ? " No Decision tracked yet."
                : $" Decision status: {decision.DecisionStatus} / {decision.ImplementationStatus}.");

        return ExecutiveRecommendationSummary.Generate(
            recommendation.Id,
            aiRecommendation?.Id,
            explainability?.Id,
            summaryVersion,
            generatedAt,
            generatedBy,
            summary,
            factors,
            businessImpact,
            capacityImpact,
            workloadImpact,
            risks,
            assumptions,
            confidence,
            actions,
            ExecutiveRecommendationSummaryVersions.CurrentModelVersion,
            ExecutiveRecommendationSummaryVersions.CurrentPromptVersion,
            recommendation.DecisionProfileId,
            recommendation.DecisionProfileVersion,
            recommendation.CompanyId);
    }

    private static decimal ComputeConfidence(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation)
    {
        var score = recommendation.Score ?? 50m;
        var aiBoost = aiRecommendation?.ConfidenceScore is { } aiConfidence
            ? aiConfidence * 0.15m
            : 0m;
        var confidence = Math.Clamp(score * 0.85m + aiBoost, 30m, 98m);
        return decimal.Round(confidence, 2, MidpointRounding.AwayFromZero);
    }

    private static string BuildKeyDecisionFactors(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability,
        Decision? decision) =>
        JsonSerializer.Serialize(new object[]
        {
            new { factor = "recommendationTitle", value = recommendation.Title },
            new { factor = "score", value = recommendation.Score },
            new { factor = "rank", value = recommendation.Rank },
            new { factor = "decisionEngineVersion", value = recommendation.DecisionEngineVersion },
            new { factor = "planningTemplateId", value = recommendation.PlanningTemplateId },
            new
            {
                factor = "aiSuggestedStrategy",
                value = aiRecommendation?.SuggestedDeliveryStrategy
            },
            new { factor = "explainabilityType", value = explainability?.ExplanationType },
            new
            {
                factor = "decisionStatus",
                value = decision is null
                    ? null
                    : $"{decision.DecisionStatus}/{decision.ImplementationStatus}"
            }
        }, JsonOptions);

    private static string BuildRisks(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability) =>
        JsonSerializer.Serialize(new[]
        {
            (recommendation.Score ?? 0m) < 70
                ? "Engine score below 70 increases delivery uncertainty."
                : "Engine score appears adequate; validate with operational owners.",
            (recommendation.Rank ?? 99) > 1
                ? "Non-primary rank suggests competing strategies warrant review."
                : "Primary rank may create over-confidence bias.",
            aiRecommendation is null
                ? "No AI advisory available for risk triangulation."
                : "AI advisory is informational and may omit unmodeled constraints.",
            explainability is null
                ? "No explainability record attached to this briefing."
                : "Explainability attached; still requires human interpretation."
        }, JsonOptions);

    private static string BuildAssumptions(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability) =>
        JsonSerializer.Serialize(new[]
        {
            "Capacity and workload snapshots remain representative of the planning window.",
            "Decision Engine ranking inputs are complete and current.",
            $"Planning template: {(recommendation.PlanningTemplateId?.ToString() ?? "none")}.",
            aiRecommendation is null
                ? "Briefing is based on Recommendation data only."
                : "Briefing incorporates AI advisory context without replacing the Recommendation.",
            explainability is null
                ? "No Explainability overlay was selected."
                : "Explainability context was consolidated into this briefing.",
            "Human approval remains mandatory before execution (informational briefing only)."
        }, JsonOptions);

    private static string BuildRecommendedActions(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Decision? decision)
    {
        var actions = new List<object>
        {
            new
            {
                priority = 1,
                action = "Review Recommendation with planners",
                rationale = $"Validate '{recommendation.Title}' against current capacity and commitments."
            },
            new
            {
                priority = 2,
                action = decision is null
                    ? "Start Decision Tracking if Recommendation is approved"
                    : $"Advance Decision ({decision.DecisionStatus})",
                rationale = decision is null
                    ? "Approved Recommendations should enter Decision Tracking before execution."
                    : "Continue tracked implementation without changing Recommendation data."
            }
        };

        if (aiRecommendation is not null)
        {
            actions.Add(new
            {
                priority = 3,
                action = "Compare AI advisory with Recommendation",
                rationale =
                    $"Evaluate advisory '{aiRecommendation.SuggestedDeliveryStrategy}' before accepting or ignoring."
            });
        }

        actions.Add(new
        {
            priority = 4,
            action = "Confirm human approval before execution",
            rationale = "Executive briefing is informational and never authorizes autonomous execution."
        });

        return JsonSerializer.Serialize(actions, JsonOptions);
    }

    private static string BuildBusinessImpact(Recommendation recommendation, Decision? decision) =>
        "Business impact briefing for Recommendation " +
        $"'{recommendation.RecommendationNumber}': {recommendation.Summary ?? recommendation.Reason ?? "n/a"}. " +
        $"Engine score={recommendation.Score?.ToString("0.##") ?? "n/a"}, rank=#{recommendation.Rank?.ToString() ?? "n/a"}. " +
        (decision is null
            ? "No Decision outcome recorded yet."
            : $"Tracked Decision status={decision.DecisionStatus}, implementation={decision.ImplementationStatus}.") +
        " This briefing does not change Recommendation or Decision records (BR-1801).";

    private static string BuildCapacityImpact(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability)
    {
        var ai = aiRecommendation is null
            ? "No AI capacity delta."
            : $"AI capacity advisory present (length={aiRecommendation.SuggestedCapacityImpact.Length}).";
        var expl = explainability is null
            ? "No explainability capacity text."
            : $"Explainability capacity note length={explainability.CapacityExplanation.Length}.";

        return
            $"Capacity impact derived from Recommendation snapshot (length={recommendation.CapacitySnapshot?.Length ?? 0}). " +
            $"{ai} {expl} Executive briefing never recalculates Capacity Engine results.";
    }

    private static string BuildWorkloadImpact(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability)
    {
        var ai = aiRecommendation is null
            ? "No AI workload delta."
            : $"AI workload advisory present (length={aiRecommendation.SuggestedWorkloadImpact.Length}).";
        var expl = explainability is null
            ? "No explainability workload text."
            : $"Explainability workload note length={explainability.WorkloadExplanation.Length}.";

        return
            $"Workload impact derived from Recommendation snapshot (length={recommendation.WorkloadSnapshot?.Length ?? 0}). " +
            $"{ai} {expl} Executive briefing never recalculates Workload Engine results.";
    }
}

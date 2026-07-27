namespace AgencyOS.Domain.Entities;

/// <summary>
/// LLM Explainability aggregate (US-302 / BR-1701..BR-1710).
/// Informational only. Never changes Recommendations or Decision Engine calculations.
/// Generate + Archive only.
/// </summary>
public class Explainability
{
    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public Guid? AIRecommendationId { get; private set; }

    public string ExplanationType { get; private set; } = string.Empty;

    public int GenerationVersion { get; private set; }

    public string ExecutiveSummary { get; private set; } = string.Empty;

    public string DetailedExplanation { get; private set; } = string.Empty;

    public string DecisionFactors { get; private set; } = string.Empty;

    public string Assumptions { get; private set; } = string.Empty;

    public string Risks { get; private set; } = string.Empty;

    public string ConfidenceExplanation { get; private set; } = string.Empty;

    public string CapacityExplanation { get; private set; } = string.Empty;

    public string WorkloadExplanation { get; private set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; private set; }

    public string GeneratedBy { get; private set; } = string.Empty;

    public string ModelVersion { get; private set; } = string.Empty;

    public string PromptVersion { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTimeOffset? ArchivedAt { get; private set; }

    public Guid? DecisionProfileId { get; private set; }

    public int DecisionProfileVersion { get; private set; }

    /// <summary>Company that owns the originating Recommendation (US-402 / BR-2004).</summary>
    public Guid? CompanyId { get; private set; }

    public bool Archived => ExplainabilityStatus.IsArchived(Status);

    private Explainability()
    {
    }

    public static Explainability Generate(
        Guid recommendationId,
        Guid? aiRecommendationId,
        string explanationType,
        int generationVersion,
        DateTimeOffset generatedAt,
        string generatedBy,
        string executiveSummary,
        string detailedExplanation,
        string decisionFactors,
        string assumptions,
        string risks,
        string confidenceExplanation,
        string capacityExplanation,
        string workloadExplanation,
        string modelVersion,
        string promptVersion,
        Guid? decisionProfileId = null,
        int decisionProfileVersion = 0,
        Guid? companyId = null)
    {
        ValidateInputs(
            recommendationId,
            aiRecommendationId,
            explanationType,
            generationVersion,
            generatedBy,
            executiveSummary,
            detailedExplanation,
            decisionFactors,
            assumptions,
            risks,
            confidenceExplanation,
            capacityExplanation,
            workloadExplanation,
            modelVersion,
            promptVersion);

        return new Explainability
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            AIRecommendationId = aiRecommendationId,
            ExplanationType = explanationType.Trim(),
            GenerationVersion = generationVersion,
            GeneratedAt = generatedAt,
            GeneratedBy = generatedBy.Trim(),
            ExecutiveSummary = executiveSummary.Trim(),
            DetailedExplanation = detailedExplanation.Trim(),
            DecisionFactors = decisionFactors.Trim(),
            Assumptions = assumptions.Trim(),
            Risks = risks.Trim(),
            ConfidenceExplanation = confidenceExplanation.Trim(),
            CapacityExplanation = capacityExplanation.Trim(),
            WorkloadExplanation = workloadExplanation.Trim(),
            ModelVersion = modelVersion.Trim(),
            PromptVersion = promptVersion.Trim(),
            Status = ExplainabilityStatus.Active,
            DecisionProfileId = decisionProfileId,
            DecisionProfileVersion = decisionProfileVersion < 0 ? 0 : decisionProfileVersion,
            CompanyId = companyId
        };
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Explainability record is already archived.");
        }

        Status = ExplainabilityStatus.Archived;
        ArchivedAt = archivedAt;
    }

    public bool Validate()
    {
        try
        {
            ValidateInputs(
                RecommendationId,
                AIRecommendationId,
                ExplanationType,
                GenerationVersion,
                GeneratedBy,
                ExecutiveSummary,
                DetailedExplanation,
                DecisionFactors,
                Assumptions,
                Risks,
                ConfidenceExplanation,
                CapacityExplanation,
                WorkloadExplanation,
                ModelVersion,
                PromptVersion);
            return ExplainabilityStatus.IsKnown(Status);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static void ValidateInputs(
        Guid recommendationId,
        Guid? aiRecommendationId,
        string explanationType,
        int generationVersion,
        string generatedBy,
        string executiveSummary,
        string detailedExplanation,
        string decisionFactors,
        string assumptions,
        string risks,
        string confidenceExplanation,
        string capacityExplanation,
        string workloadExplanation,
        string modelVersion,
        string promptVersion)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory (BR-1702).");
        }

        if (!ExplainabilityTypes.IsKnown(explanationType))
        {
            throw new InvalidOperationException(
                $"ExplanationType must be one of: {string.Join(", ", ExplainabilityTypes.All)}.");
        }

        if (ExplainabilityTypes.IsAIRecommendation(explanationType))
        {
            if (!aiRecommendationId.HasValue || aiRecommendationId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "AIRecommendationId is mandatory when ExplanationType is AIRecommendation.");
            }
        }
        else if (aiRecommendationId.HasValue)
        {
            throw new InvalidOperationException(
                "AIRecommendationId must be null when ExplanationType is Recommendation.");
        }

        if (generationVersion < 1)
        {
            throw new InvalidOperationException("GenerationVersion must be >= 1 (BR-1704).");
        }

        if (string.IsNullOrWhiteSpace(generatedBy))
        {
            throw new InvalidOperationException("GeneratedBy is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(executiveSummary))
        {
            throw new InvalidOperationException("ExecutiveSummary is mandatory (BR-1710).");
        }

        if (string.IsNullOrWhiteSpace(detailedExplanation))
        {
            throw new InvalidOperationException("DetailedExplanation is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(decisionFactors))
        {
            throw new InvalidOperationException("DecisionFactors are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(assumptions))
        {
            throw new InvalidOperationException("Assumptions are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(risks))
        {
            throw new InvalidOperationException("Risks are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(confidenceExplanation))
        {
            throw new InvalidOperationException("ConfidenceExplanation is mandatory (BR-1709).");
        }

        if (string.IsNullOrWhiteSpace(capacityExplanation))
        {
            throw new InvalidOperationException("CapacityExplanation is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(workloadExplanation))
        {
            throw new InvalidOperationException("WorkloadExplanation is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(modelVersion))
        {
            throw new InvalidOperationException("ModelVersion is mandatory (BR-1707).");
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            throw new InvalidOperationException("PromptVersion is mandatory (BR-1708).");
        }
    }
}

public static class ExplainabilityStatus
{
    public const string Active = "Active";
    public const string Archived = "Archived";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Active, Archived };

    public static bool IsKnown(string status) => All.Contains(status);

    public static bool IsArchived(string status) =>
        string.Equals(status, Archived, StringComparison.OrdinalIgnoreCase);

    public static bool IsActive(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);
}

public static class ExplainabilityTypes
{
    public const string Recommendation = "Recommendation";
    public const string AIRecommendation = "AIRecommendation";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Recommendation, AIRecommendation };

    public static bool IsKnown(string? type) =>
        !string.IsNullOrWhiteSpace(type) && All.Contains(type);

    public static bool IsAIRecommendation(string type) =>
        string.Equals(type, AIRecommendation, StringComparison.OrdinalIgnoreCase);

    public static bool IsRecommendation(string type) =>
        string.Equals(type, Recommendation, StringComparison.OrdinalIgnoreCase);
}

public static class ExplainabilityVersions
{
    public const string CurrentModelVersion = "1.1.0-explainability";
    public const string CurrentPromptVersion = "1.0.0-deterministic-explainer";
}

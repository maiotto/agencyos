namespace AgencyOS.Domain.Entities;

/// <summary>
/// AI-assisted Recommendation aggregate (US-301 / BR-1601..BR-1610).
/// Advisory only. Never replaces Recommendations. Create + Archive only.
/// </summary>
public class AIRecommendation
{
    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public int RecommendationVersion { get; private set; }

    public int GenerationVersion { get; private set; }

    public DateTimeOffset GeneratedAt { get; private set; }

    public string GeneratedBy { get; private set; } = string.Empty;

    public decimal ConfidenceScore { get; private set; }

    public string ExecutiveSummary { get; private set; } = string.Empty;

    public string Reasoning { get; private set; } = string.Empty;

    public string Assumptions { get; private set; } = string.Empty;

    public string Risks { get; private set; } = string.Empty;

    public string Alternatives { get; private set; } = string.Empty;

    public string SuggestedDeliveryStrategy { get; private set; } = string.Empty;

    public string SuggestedCapacityImpact { get; private set; } = string.Empty;

    public string SuggestedWorkloadImpact { get; private set; } = string.Empty;

    public string ModelVersion { get; private set; } = string.Empty;

    public string PromptVersion { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTimeOffset? ArchivedAt { get; private set; }

    public Guid? DecisionProfileId { get; private set; }

    public int DecisionProfileVersion { get; private set; }

    /// <summary>Company that owns the originating Recommendation (US-402 / BR-2004).</summary>
    public Guid? CompanyId { get; private set; }

    public bool Archived => AIRecommendationStatus.IsArchived(Status);

    private AIRecommendation()
    {
    }

    public static AIRecommendation Generate(
        Guid recommendationId,
        int recommendationVersion,
        int generationVersion,
        DateTimeOffset generatedAt,
        string generatedBy,
        decimal confidenceScore,
        string executiveSummary,
        string reasoning,
        string assumptions,
        string risks,
        string alternatives,
        string suggestedDeliveryStrategy,
        string suggestedCapacityImpact,
        string suggestedWorkloadImpact,
        string modelVersion,
        string promptVersion,
        Guid? decisionProfileId = null,
        int decisionProfileVersion = 0,
        Guid? companyId = null)
    {
        ValidateInputs(
            recommendationId,
            recommendationVersion,
            generationVersion,
            generatedBy,
            confidenceScore,
            executiveSummary,
            reasoning,
            assumptions,
            risks,
            alternatives,
            suggestedDeliveryStrategy,
            suggestedCapacityImpact,
            suggestedWorkloadImpact,
            modelVersion,
            promptVersion);

        return new AIRecommendation
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            RecommendationVersion = recommendationVersion,
            GenerationVersion = generationVersion,
            GeneratedAt = generatedAt,
            GeneratedBy = generatedBy.Trim(),
            ConfidenceScore = decimal.Round(confidenceScore, 2, MidpointRounding.AwayFromZero),
            ExecutiveSummary = executiveSummary.Trim(),
            Reasoning = reasoning.Trim(),
            Assumptions = assumptions.Trim(),
            Risks = risks.Trim(),
            Alternatives = alternatives.Trim(),
            SuggestedDeliveryStrategy = suggestedDeliveryStrategy.Trim(),
            SuggestedCapacityImpact = suggestedCapacityImpact.Trim(),
            SuggestedWorkloadImpact = suggestedWorkloadImpact.Trim(),
            ModelVersion = modelVersion.Trim(),
            PromptVersion = promptVersion.Trim(),
            Status = AIRecommendationStatus.Active,
            DecisionProfileId = decisionProfileId,
            DecisionProfileVersion = decisionProfileVersion < 0 ? 0 : decisionProfileVersion,
            CompanyId = companyId
        };
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("AI Recommendation is already archived.");
        }

        Status = AIRecommendationStatus.Archived;
        ArchivedAt = archivedAt;
    }

    public bool Validate()
    {
        try
        {
            ValidateInputs(
                RecommendationId,
                RecommendationVersion,
                GenerationVersion,
                GeneratedBy,
                ConfidenceScore,
                ExecutiveSummary,
                Reasoning,
                Assumptions,
                Risks,
                Alternatives,
                SuggestedDeliveryStrategy,
                SuggestedCapacityImpact,
                SuggestedWorkloadImpact,
                ModelVersion,
                PromptVersion);
            return AIRecommendationStatus.IsKnown(Status);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>
    /// Compares this advisory AI Recommendation against the originating Recommendation snapshot (BR-1601).
    /// </summary>
    public AIRecommendationComparison Compare(Recommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        if (recommendation.Id != RecommendationId)
        {
            throw new InvalidOperationException(
                "AI Recommendation can only be compared with its originating Recommendation (BR-1604).");
        }

        return AIRecommendationComparison.Create(this, recommendation);
    }

    private static void ValidateInputs(
        Guid recommendationId,
        int recommendationVersion,
        int generationVersion,
        string generatedBy,
        decimal confidenceScore,
        string executiveSummary,
        string reasoning,
        string assumptions,
        string risks,
        string alternatives,
        string suggestedDeliveryStrategy,
        string suggestedCapacityImpact,
        string suggestedWorkloadImpact,
        string modelVersion,
        string promptVersion)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory (BR-1604).");
        }

        if (recommendationVersion < 1)
        {
            throw new InvalidOperationException("RecommendationVersion must be >= 1.");
        }

        if (generationVersion < 1)
        {
            throw new InvalidOperationException("GenerationVersion must be >= 1 (BR-1606).");
        }

        if (string.IsNullOrWhiteSpace(generatedBy))
        {
            throw new InvalidOperationException("GeneratedBy is mandatory.");
        }

        if (confidenceScore is < 0 or > 100)
        {
            throw new InvalidOperationException("ConfidenceScore must be between 0 and 100 (BR-1609).");
        }

        if (string.IsNullOrWhiteSpace(executiveSummary))
        {
            throw new InvalidOperationException("ExecutiveSummary is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(reasoning))
        {
            throw new InvalidOperationException("Reasoning is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(assumptions))
        {
            throw new InvalidOperationException("Assumptions are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(risks))
        {
            throw new InvalidOperationException("Risks are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(alternatives))
        {
            throw new InvalidOperationException("Alternatives are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(suggestedDeliveryStrategy))
        {
            throw new InvalidOperationException("SuggestedDeliveryStrategy is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(suggestedCapacityImpact))
        {
            throw new InvalidOperationException("SuggestedCapacityImpact is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(suggestedWorkloadImpact))
        {
            throw new InvalidOperationException("SuggestedWorkloadImpact is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(modelVersion))
        {
            throw new InvalidOperationException("ModelVersion is mandatory (BR-1610).");
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            throw new InvalidOperationException("PromptVersion is mandatory (BR-1610).");
        }
    }
}

public static class AIRecommendationStatus
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

public static class AIRecommendationVersions
{
    public const string CurrentModelVersion = "1.1.0-ai-advisor";
    public const string CurrentPromptVersion = "1.0.0-deterministic-advisor";
}

/// <summary>
/// Read-only comparison between an AI Recommendation and its Recommendation (US-301).
/// </summary>
public sealed class AIRecommendationComparison
{
    private AIRecommendationComparison(
        AIRecommendation aiRecommendation,
        Recommendation recommendation,
        IReadOnlyList<AIRecommendationComparisonDifference> differences)
    {
        AIRecommendation = aiRecommendation;
        Recommendation = recommendation;
        Differences = differences;
    }

    public AIRecommendation AIRecommendation { get; }

    public Recommendation Recommendation { get; }

    public IReadOnlyList<AIRecommendationComparisonDifference> Differences { get; }

    public bool HasDifferences => Differences.Any(difference => difference.Changed);

    public static AIRecommendationComparison Create(
        AIRecommendation aiRecommendation,
        Recommendation recommendation)
    {
        var differences = new List<AIRecommendationComparisonDifference>
        {
            Diff("title", recommendation.Title, aiRecommendation.SuggestedDeliveryStrategy),
            Diff("score", Format(recommendation.Score), Format(aiRecommendation.ConfidenceScore)),
            Diff("rank", Format(recommendation.Rank), null),
            Diff("version", recommendation.Version.ToString(), aiRecommendation.RecommendationVersion.ToString()),
            Diff("capacitySnapshot", recommendation.CapacitySnapshot, aiRecommendation.SuggestedCapacityImpact),
            Diff("workloadSnapshot", recommendation.WorkloadSnapshot, aiRecommendation.SuggestedWorkloadImpact),
            Diff("payload", recommendation.RecommendationPayload, aiRecommendation.Alternatives)
        };

        return new AIRecommendationComparison(aiRecommendation, recommendation, differences);
    }

    private static AIRecommendationComparisonDifference Diff(string path, string? left, string? right) =>
        new(path, left, right, !string.Equals(left, right, StringComparison.Ordinal));

    private static string? Format(decimal? value) => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Format(int? value) => value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

public sealed class AIRecommendationComparisonDifference
{
    public AIRecommendationComparisonDifference(string path, string? leftValue, string? rightValue, bool changed)
    {
        Path = path;
        LeftValue = leftValue;
        RightValue = rightValue;
        Changed = changed;
    }

    public string Path { get; }

    public string? LeftValue { get; }

    public string? RightValue { get; }

    public bool Changed { get; }
}

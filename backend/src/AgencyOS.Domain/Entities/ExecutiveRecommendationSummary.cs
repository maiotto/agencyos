namespace AgencyOS.Domain.Entities;

/// <summary>
/// Executive Recommendation Summary aggregate (US-303 / BR-1801..BR-1810).
/// Informational executive briefing. Never changes Recommendations, AI Recommendations, or Decisions.
/// Generate + CreateNewVersion + Archive only.
/// </summary>
public class ExecutiveRecommendationSummary
{
    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public Guid? AIRecommendationId { get; private set; }

    public Guid? ExplainabilityId { get; private set; }

    public int SummaryVersion { get; private set; }

    public string ExecutiveSummary { get; private set; } = string.Empty;

    public string KeyDecisionFactors { get; private set; } = string.Empty;

    public string BusinessImpact { get; private set; } = string.Empty;

    public string CapacityImpact { get; private set; } = string.Empty;

    public string WorkloadImpact { get; private set; } = string.Empty;

    public string Risks { get; private set; } = string.Empty;

    public string Assumptions { get; private set; } = string.Empty;

    public decimal ConfidenceLevel { get; private set; }

    public string RecommendedActions { get; private set; } = string.Empty;

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

    public bool Archived => ExecutiveRecommendationSummaryStatus.IsArchived(Status);

    private ExecutiveRecommendationSummary()
    {
    }

    public static ExecutiveRecommendationSummary Generate(
        Guid recommendationId,
        Guid? aiRecommendationId,
        Guid? explainabilityId,
        int summaryVersion,
        DateTimeOffset generatedAt,
        string generatedBy,
        string executiveSummary,
        string keyDecisionFactors,
        string businessImpact,
        string capacityImpact,
        string workloadImpact,
        string risks,
        string assumptions,
        decimal confidenceLevel,
        string recommendedActions,
        string modelVersion,
        string promptVersion,
        Guid? decisionProfileId = null,
        int decisionProfileVersion = 0,
        Guid? companyId = null)
    {
        ValidateInputs(
            recommendationId,
            summaryVersion,
            generatedBy,
            executiveSummary,
            keyDecisionFactors,
            businessImpact,
            capacityImpact,
            workloadImpact,
            risks,
            assumptions,
            confidenceLevel,
            recommendedActions,
            modelVersion,
            promptVersion);

        return new ExecutiveRecommendationSummary
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            AIRecommendationId = aiRecommendationId,
            ExplainabilityId = explainabilityId,
            SummaryVersion = summaryVersion,
            GeneratedAt = generatedAt,
            GeneratedBy = generatedBy.Trim(),
            ExecutiveSummary = executiveSummary.Trim(),
            KeyDecisionFactors = keyDecisionFactors.Trim(),
            BusinessImpact = businessImpact.Trim(),
            CapacityImpact = capacityImpact.Trim(),
            WorkloadImpact = workloadImpact.Trim(),
            Risks = risks.Trim(),
            Assumptions = assumptions.Trim(),
            ConfidenceLevel = decimal.Round(confidenceLevel, 2, MidpointRounding.AwayFromZero),
            RecommendedActions = recommendedActions.Trim(),
            ModelVersion = modelVersion.Trim(),
            PromptVersion = promptVersion.Trim(),
            Status = ExecutiveRecommendationSummaryStatus.Active,
            DecisionProfileId = decisionProfileId,
            DecisionProfileVersion = decisionProfileVersion < 0 ? 0 : decisionProfileVersion,
            CompanyId = companyId
        };
    }

    /// <summary>
    /// Creates a new immutable version based on this summary's Recommendation lineage (BR-1804).
    /// Does not mutate the current instance.
    /// </summary>
    public ExecutiveRecommendationSummary CreateNewVersion(
        int nextSummaryVersion,
        DateTimeOffset generatedAt,
        string generatedBy,
        string executiveSummary,
        string keyDecisionFactors,
        string businessImpact,
        string capacityImpact,
        string workloadImpact,
        string risks,
        string assumptions,
        decimal confidenceLevel,
        string recommendedActions,
        Guid? aiRecommendationId,
        Guid? explainabilityId,
        string modelVersion,
        string promptVersion)
    {
        if (nextSummaryVersion <= SummaryVersion)
        {
            throw new InvalidOperationException(
                "CreateNewVersion requires a SummaryVersion greater than the current version (BR-1804).");
        }

        return Generate(
            RecommendationId,
            aiRecommendationId ?? AIRecommendationId,
            explainabilityId ?? ExplainabilityId,
            nextSummaryVersion,
            generatedAt,
            generatedBy,
            executiveSummary,
            keyDecisionFactors,
            businessImpact,
            capacityImpact,
            workloadImpact,
            risks,
            assumptions,
            confidenceLevel,
            recommendedActions,
            modelVersion,
            promptVersion,
            DecisionProfileId,
            DecisionProfileVersion,
            CompanyId);
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Executive Recommendation Summary is already archived.");
        }

        Status = ExecutiveRecommendationSummaryStatus.Archived;
        ArchivedAt = archivedAt;
    }

    public bool Validate()
    {
        try
        {
            ValidateInputs(
                RecommendationId,
                SummaryVersion,
                GeneratedBy,
                ExecutiveSummary,
                KeyDecisionFactors,
                BusinessImpact,
                CapacityImpact,
                WorkloadImpact,
                Risks,
                Assumptions,
                ConfidenceLevel,
                RecommendedActions,
                ModelVersion,
                PromptVersion);
            return ExecutiveRecommendationSummaryStatus.IsKnown(Status);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public ExecutiveRecommendationSummaryComparison Compare(Recommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);

        if (recommendation.Id != RecommendationId)
        {
            throw new InvalidOperationException(
                "Executive Summary can only be compared with its originating Recommendation (BR-1802).");
        }

        return ExecutiveRecommendationSummaryComparison.Create(this, recommendation);
    }

    private static void ValidateInputs(
        Guid recommendationId,
        int summaryVersion,
        string generatedBy,
        string executiveSummary,
        string keyDecisionFactors,
        string businessImpact,
        string capacityImpact,
        string workloadImpact,
        string risks,
        string assumptions,
        decimal confidenceLevel,
        string recommendedActions,
        string modelVersion,
        string promptVersion)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory (BR-1802).");
        }

        if (summaryVersion < 1)
        {
            throw new InvalidOperationException("SummaryVersion must be >= 1 (BR-1804).");
        }

        if (string.IsNullOrWhiteSpace(generatedBy))
        {
            throw new InvalidOperationException("GeneratedBy is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(executiveSummary))
        {
            throw new InvalidOperationException("ExecutiveSummary is mandatory (BR-1808).");
        }

        if (string.IsNullOrWhiteSpace(keyDecisionFactors))
        {
            throw new InvalidOperationException("KeyDecisionFactors are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(businessImpact))
        {
            throw new InvalidOperationException("BusinessImpact is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(capacityImpact))
        {
            throw new InvalidOperationException("CapacityImpact is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(workloadImpact))
        {
            throw new InvalidOperationException("WorkloadImpact is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(risks))
        {
            throw new InvalidOperationException("Risks are mandatory.");
        }

        if (string.IsNullOrWhiteSpace(assumptions))
        {
            throw new InvalidOperationException("Assumptions are mandatory.");
        }

        if (confidenceLevel is < 0 or > 100)
        {
            throw new InvalidOperationException("ConfidenceLevel must be between 0 and 100 (BR-1807).");
        }

        if (string.IsNullOrWhiteSpace(recommendedActions))
        {
            throw new InvalidOperationException("RecommendedActions are mandatory (BR-1809).");
        }

        if (string.IsNullOrWhiteSpace(modelVersion))
        {
            throw new InvalidOperationException("ModelVersion is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(promptVersion))
        {
            throw new InvalidOperationException("PromptVersion is mandatory.");
        }
    }
}

public static class ExecutiveRecommendationSummaryStatus
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

public static class ExecutiveRecommendationSummaryVersions
{
    public const string CurrentModelVersion = "1.1.0-executive-briefing";
    public const string CurrentPromptVersion = "1.0.0-deterministic-executive-summary";
}

/// <summary>
/// Read-only comparison between an Executive Summary and its Recommendation (US-303).
/// </summary>
public sealed class ExecutiveRecommendationSummaryComparison
{
    private ExecutiveRecommendationSummaryComparison(
        ExecutiveRecommendationSummary summary,
        Recommendation recommendation,
        IReadOnlyList<ExecutiveRecommendationSummaryComparisonDifference> differences)
    {
        Summary = summary;
        Recommendation = recommendation;
        Differences = differences;
    }

    public ExecutiveRecommendationSummary Summary { get; }

    public Recommendation Recommendation { get; }

    public IReadOnlyList<ExecutiveRecommendationSummaryComparisonDifference> Differences { get; }

    public bool HasDifferences => Differences.Any(difference => difference.Changed);

    public static ExecutiveRecommendationSummaryComparison Create(
        ExecutiveRecommendationSummary summary,
        Recommendation recommendation)
    {
        var differences = new List<ExecutiveRecommendationSummaryComparisonDifference>
        {
            Diff("title", recommendation.Title, Truncate(summary.ExecutiveSummary, 120)),
            Diff("score", Format(recommendation.Score), Format(summary.ConfidenceLevel)),
            Diff("rank", Format(recommendation.Rank), null),
            Diff("reason", recommendation.Reason, Truncate(summary.BusinessImpact, 200)),
            Diff("capacitySnapshot", recommendation.CapacitySnapshot, summary.CapacityImpact),
            Diff("workloadSnapshot", recommendation.WorkloadSnapshot, summary.WorkloadImpact),
            Diff("summary", recommendation.Summary, summary.ExecutiveSummary)
        };

        return new ExecutiveRecommendationSummaryComparison(summary, recommendation, differences);
    }

    private static ExecutiveRecommendationSummaryComparisonDifference Diff(
        string path,
        string? left,
        string? right) =>
        new(path, left, right, !string.Equals(left, right, StringComparison.Ordinal));

    private static string? Format(decimal? value) =>
        value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Format(int? value) =>
        value?.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}

public sealed class ExecutiveRecommendationSummaryComparisonDifference
{
    public ExecutiveRecommendationSummaryComparisonDifference(
        string path,
        string? leftValue,
        string? rightValue,
        bool changed)
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

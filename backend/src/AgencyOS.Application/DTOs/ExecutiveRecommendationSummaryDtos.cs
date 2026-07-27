namespace AgencyOS.Application.DTOs;

public class ExecutiveRecommendationSummaryQueryParameters
{
    public Guid? RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public Guid? ExplainabilityId { get; set; }

    public Guid? CompanyId { get; set; }

    public string? Status { get; set; }

    public string? Search { get; set; }

    public decimal? MinConfidenceLevel { get; set; }

    public bool IncludeArchived { get; set; } = true;

    public DateTimeOffset? GeneratedFrom { get; set; }

    public DateTimeOffset? GeneratedTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class GenerateExecutiveRecommendationSummaryRequest
{
    public Guid RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public Guid? ExplainabilityId { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;
}

public class CreateExecutiveRecommendationSummaryVersionRequest
{
    public Guid? AIRecommendationId { get; set; }

    public Guid? ExplainabilityId { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;
}

public class ExecutiveRecommendationSummaryResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public Guid? ExplainabilityId { get; set; }

    public int SummaryVersion { get; set; }

    public string ExecutiveSummary { get; set; } = string.Empty;

    public string KeyDecisionFactors { get; set; } = string.Empty;

    public string BusinessImpact { get; set; } = string.Empty;

    public string CapacityImpact { get; set; } = string.Empty;

    public string WorkloadImpact { get; set; } = string.Empty;

    public string Risks { get; set; } = string.Empty;

    public string Assumptions { get; set; } = string.Empty;

    public decimal ConfidenceLevel { get; set; }

    public string RecommendedActions { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;

    public string PromptVersion { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? ArchivedAt { get; set; }

    public bool Archived { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public int DecisionProfileVersion { get; set; }

    public Guid? CompanyId { get; set; }
}

public class ExecutiveRecommendationSummaryComparisonFieldResponse
{
    public string Path { get; set; } = string.Empty;

    public string? LeftValue { get; set; }

    public string? RightValue { get; set; }

    public bool Changed { get; set; }
}

public class ExecutiveRecommendationSummaryComparisonResponse
{
    public ExecutiveRecommendationSummaryResponse Summary { get; set; } = null!;

    public Guid RecommendationId { get; set; }

    public string RecommendationTitle { get; set; } = string.Empty;

    public decimal? RecommendationScore { get; set; }

    public int? RecommendationRank { get; set; }

    public int RecommendationVersion { get; set; }

    public bool HasDifferences { get; set; }

    public IReadOnlyList<ExecutiveRecommendationSummaryComparisonFieldResponse> Differences { get; set; } = [];
}

namespace AgencyOS.Application.DTOs;

public class AIRecommendationQueryParameters
{
    public Guid? RecommendationId { get; set; }

    public Guid? CompanyId { get; set; }

    public string? Status { get; set; }

    public string? Search { get; set; }

    public decimal? MinConfidenceScore { get; set; }

    public DateTimeOffset? GeneratedFrom { get; set; }

    public DateTimeOffset? GeneratedTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class GenerateAIRecommendationRequest
{
    public Guid RecommendationId { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;
}

public class AIRecommendationResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public int RecommendationVersion { get; set; }

    public int GenerationVersion { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public string ExecutiveSummary { get; set; } = string.Empty;

    public string Reasoning { get; set; } = string.Empty;

    public string Assumptions { get; set; } = string.Empty;

    public string Risks { get; set; } = string.Empty;

    public string Alternatives { get; set; } = string.Empty;

    public string SuggestedDeliveryStrategy { get; set; } = string.Empty;

    public string SuggestedCapacityImpact { get; set; } = string.Empty;

    public string SuggestedWorkloadImpact { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;

    public string PromptVersion { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? ArchivedAt { get; set; }

    public bool Archived { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public int DecisionProfileVersion { get; set; }

    public Guid? CompanyId { get; set; }
}

public class AIRecommendationComparisonFieldResponse
{
    public string Path { get; set; } = string.Empty;

    public string? LeftValue { get; set; }

    public string? RightValue { get; set; }

    public bool Changed { get; set; }
}

public class AIRecommendationComparisonResponse
{
    public AIRecommendationResponse AIRecommendation { get; set; } = null!;

    public Guid RecommendationId { get; set; }

    public string RecommendationTitle { get; set; } = string.Empty;

    public decimal? RecommendationScore { get; set; }

    public int? RecommendationRank { get; set; }

    public int RecommendationVersion { get; set; }

    public bool HasDifferences { get; set; }

    public IReadOnlyList<AIRecommendationComparisonFieldResponse> Differences { get; set; } = [];
}

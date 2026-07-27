namespace AgencyOS.Application.DTOs;

public class ExplainabilityQueryParameters
{
    public Guid? RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public Guid? CompanyId { get; set; }

    public string? ExplanationType { get; set; }

    public string? Status { get; set; }

    public string? Search { get; set; }

    public DateTimeOffset? GeneratedFrom { get; set; }

    public DateTimeOffset? GeneratedTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class GenerateExplainabilityRequest
{
    public Guid RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public string? ExplanationType { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;
}

public class ExplainabilityResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid? AIRecommendationId { get; set; }

    public string ExplanationType { get; set; } = string.Empty;

    public int GenerationVersion { get; set; }

    public string ExecutiveSummary { get; set; } = string.Empty;

    public string DetailedExplanation { get; set; } = string.Empty;

    public string DecisionFactors { get; set; } = string.Empty;

    public string Assumptions { get; set; } = string.Empty;

    public string Risks { get; set; } = string.Empty;

    public string ConfidenceExplanation { get; set; } = string.Empty;

    public string CapacityExplanation { get; set; } = string.Empty;

    public string WorkloadExplanation { get; set; } = string.Empty;

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

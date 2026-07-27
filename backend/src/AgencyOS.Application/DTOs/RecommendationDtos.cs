namespace AgencyOS.Application.DTOs;

public class RecommendationQueryParameters
{
    public Guid? CompanyId { get; set; }

    public Guid? MissionId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? DeliveryStrategyId { get; set; }

    public string? Status { get; set; }

    public string? RecommendationNumber { get; set; }

    public int? Version { get; set; }

    public bool? Archived { get; set; }

    public string? Search { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTimeOffset? GeneratedFrom { get; set; }

    public DateTimeOffset? GeneratedTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }

    public bool IncludeArchived { get; set; }
}

public class CreateRecommendationRequest
{
    public Guid CompanyId { get; set; }

    public Guid MissionId { get; set; }

    public Guid ContractId { get; set; }

    public Guid DeliveryStrategyId { get; set; }

    public string? RecommendationNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Reason { get; set; }

    public decimal? Score { get; set; }

    public int? Rank { get; set; }

    public int? Version { get; set; }

    public string? DecisionEngineVersion { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public string CapacitySnapshot { get; set; } = "{}";

    public string WorkloadSnapshot { get; set; } = "{}";

    public string RecommendationPayload { get; set; } = string.Empty;

    public string GeneratedBy { get; set; } = string.Empty;

    public DateTimeOffset? GeneratedAt { get; set; }
}

public class CreateRecommendationVersionRequest
{
    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? Reason { get; set; }

    public decimal? Score { get; set; }

    public int? Rank { get; set; }

    public string? DecisionEngineVersion { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public string CapacitySnapshot { get; set; } = "{}";

    public string WorkloadSnapshot { get; set; } = "{}";

    public string RecommendationPayload { get; set; } = string.Empty;

    public string GeneratedBy { get; set; } = string.Empty;
}

public class RecommendationResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid MissionId { get; set; }

    public Guid ContractId { get; set; }

    public Guid DeliveryStrategyId { get; set; }

    public string RecommendationNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Reason { get; set; }

    public decimal? Score { get; set; }

    public int? Rank { get; set; }

    public string Status { get; set; } = string.Empty;

    public int Version { get; set; }

    public string DecisionEngineVersion { get; set; } = string.Empty;

    public Guid? PlanningTemplateId { get; set; }

    public string CapacitySnapshot { get; set; } = string.Empty;

    public string WorkloadSnapshot { get; set; } = string.Empty;

    public string RecommendationPayload { get; set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public bool Archived { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public int DecisionProfileVersion { get; set; }
}

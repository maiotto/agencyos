namespace AgencyOS.Application.DTOs;

public class RecommendationHistoryQueryParameters
{
    public Guid? CompanyId { get; set; }

    public Guid? MissionId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? RecommendationId { get; set; }

    public string? RecommendationNumber { get; set; }

    public int? Version { get; set; }

    public string? WorkflowStatus { get; set; }

    public string? EventType { get; set; }

    public string? Search { get; set; }

    public DateTimeOffset? CreatedFrom { get; set; }

    public DateTimeOffset? CreatedTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class RecommendationHistoryResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public string RecommendationNumber { get; set; } = string.Empty;

    public int RecommendationVersion { get; set; }

    public Guid CompanyId { get; set; }

    public Guid MissionId { get; set; }

    public Guid ContractId { get; set; }

    public Guid DeliveryStrategyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string RecommendationStatus { get; set; } = string.Empty;

    public string? WorkflowStatus { get; set; }

    public Guid? WorkflowId { get; set; }

    public string? Approver { get; set; }

    public DateTimeOffset? ApprovalDate { get; set; }

    public string? ApprovalComment { get; set; }

    public decimal? Score { get; set; }

    public int? Rank { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public string DecisionEngineVersion { get; set; } = string.Empty;

    public string CapacitySnapshot { get; set; } = string.Empty;

    public string WorkloadSnapshot { get; set; } = string.Empty;

    public string RecommendationPayload { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public int DecisionProfileVersion { get; set; }
}

public class RecommendationHistoryTimelineEntryResponse
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public int RecommendationVersion { get; set; }

    public string RecommendationStatus { get; set; } = string.Empty;

    public string? WorkflowStatus { get; set; }

    public Guid? WorkflowId { get; set; }

    public string? Approver { get; set; }

    public string? ApprovalComment { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string Title { get; set; } = string.Empty;
}

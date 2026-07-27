namespace AgencyOS.Application.DTOs;

public class RecommendationWorkflowQueryParameters
{
    public string? Status { get; set; }

    public Guid? RecommendationId { get; set; }

    public Guid? DeliveryStrategyId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? MissionId { get; set; }

    public string? Search { get; set; }

    public string? CreatedBy { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class CreateRecommendationWorkflowRequest
{
    public Guid RecommendationId { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public Guid? CompanyDecisionProfileId { get; set; }

    /// <summary>Optional override; defaults from persisted Recommendation.</summary>
    public string? Title { get; set; }

    /// <summary>Optional override; defaults from persisted Recommendation.</summary>
    public string? Summary { get; set; }
}

public class RecommendationWorkflowActionRequest
{
    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }
}

public class ApproveRecommendationWorkflowRequest
{
    public string Approver { get; set; } = string.Empty;

    public DateTimeOffset? ApprovalDate { get; set; }

    public string? Comment { get; set; }
}

public class RecommendationWorkflowTransitionResponse
{
    public Guid Id { get; set; }

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}

public class RecommendationWorkflowResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid DeliveryStrategyId { get; set; }

    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public Guid? CompanyDecisionProfileId { get; set; }

    public Guid? CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public string? Approver { get; set; }

    public DateTimeOffset? ApprovalDate { get; set; }

    public string? ApprovalComment { get; set; }

    public int? RankPosition { get; set; }

    public decimal? FinalScore { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IReadOnlyList<RecommendationWorkflowTransitionResponse> Transitions { get; set; } = [];
}

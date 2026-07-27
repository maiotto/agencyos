namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable Recommendation History aggregate (US-203 / BR-1201..BR-1206).
/// Create-only. No Update. No Delete.
/// </summary>
public class RecommendationHistory
{
    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public string RecommendationNumber { get; private set; } = string.Empty;

    public int RecommendationVersion { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid MissionId { get; private set; }

    public Guid ContractId { get; private set; }

    public Guid DeliveryStrategyId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public string RecommendationStatus { get; private set; } = string.Empty;

    public string? WorkflowStatus { get; private set; }

    public Guid? WorkflowId { get; private set; }

    public string? Approver { get; private set; }

    public DateTimeOffset? ApprovalDate { get; private set; }

    public string? ApprovalComment { get; private set; }

    public decimal? Score { get; private set; }

    public int? Rank { get; private set; }

    public Guid? PlanningTemplateId { get; private set; }

    public string DecisionEngineVersion { get; private set; } = string.Empty;

    public string CapacitySnapshot { get; private set; } = string.Empty;

    public string WorkloadSnapshot { get; private set; } = string.Empty;

    public string RecommendationPayload { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? DecisionProfileId { get; private set; }

    public int DecisionProfileVersion { get; private set; }

    private RecommendationHistory()
    {
    }

    public static RecommendationHistory Create(
        Guid recommendationId,
        string recommendationNumber,
        int recommendationVersion,
        Guid companyId,
        Guid missionId,
        Guid contractId,
        Guid deliveryStrategyId,
        string title,
        string? summary,
        string eventType,
        string recommendationStatus,
        string? workflowStatus,
        Guid? workflowId,
        string? approver,
        DateTimeOffset? approvalDate,
        string? approvalComment,
        decimal? score,
        int? rank,
        Guid? planningTemplateId,
        string decisionEngineVersion,
        string capacitySnapshot,
        string workloadSnapshot,
        string recommendationPayload,
        string createdBy,
        DateTimeOffset createdAt,
        Guid? decisionProfileId = null,
        int decisionProfileVersion = 0)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(recommendationNumber))
        {
            throw new InvalidOperationException("RecommendationNumber is mandatory.");
        }

        if (recommendationVersion < 1)
        {
            throw new InvalidOperationException("RecommendationVersion must be at least 1.");
        }

        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory.");
        }

        if (missionId == Guid.Empty)
        {
            throw new InvalidOperationException("MissionId is mandatory.");
        }

        if (contractId == Guid.Empty)
        {
            throw new InvalidOperationException("ContractId is mandatory.");
        }

        if (deliveryStrategyId == Guid.Empty)
        {
            throw new InvalidOperationException("DeliveryStrategyId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Title is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(decisionEngineVersion))
        {
            throw new InvalidOperationException("DecisionEngineVersion is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(capacitySnapshot))
        {
            throw new InvalidOperationException("CapacitySnapshot is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(workloadSnapshot))
        {
            throw new InvalidOperationException("WorkloadSnapshot is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(recommendationPayload))
        {
            throw new InvalidOperationException("RecommendationPayload is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new InvalidOperationException("CreatedBy is mandatory.");
        }

        return new RecommendationHistory
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            RecommendationNumber = recommendationNumber.Trim(),
            RecommendationVersion = recommendationVersion,
            CompanyId = companyId,
            MissionId = missionId,
            ContractId = contractId,
            DeliveryStrategyId = deliveryStrategyId,
            Title = title.Trim(),
            Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim(),
            EventType = RecommendationHistoryEventType.Canonicalize(eventType),
            RecommendationStatus = AgencyOS.Domain.Entities.RecommendationStatus.Canonicalize(recommendationStatus),
            WorkflowStatus = string.IsNullOrWhiteSpace(workflowStatus) ? null : workflowStatus.Trim(),
            WorkflowId = workflowId,
            Approver = string.IsNullOrWhiteSpace(approver) ? null : approver.Trim(),
            ApprovalDate = approvalDate,
            ApprovalComment = string.IsNullOrWhiteSpace(approvalComment) ? null : approvalComment.Trim(),
            Score = score,
            Rank = rank,
            PlanningTemplateId = planningTemplateId,
            DecisionEngineVersion = decisionEngineVersion.Trim(),
            CapacitySnapshot = capacitySnapshot,
            WorkloadSnapshot = workloadSnapshot,
            RecommendationPayload = recommendationPayload,
            CreatedBy = createdBy.Trim(),
            CreatedAt = createdAt,
            DecisionProfileId = decisionProfileId,
            DecisionProfileVersion = decisionProfileVersion < 0 ? 0 : decisionProfileVersion
        };
    }

    public static RecommendationHistory FromRecommendation(
        Recommendation recommendation,
        string eventType,
        string createdBy,
        DateTimeOffset createdAt,
        string? workflowStatus = null,
        Guid? workflowId = null,
        string? approver = null,
        DateTimeOffset? approvalDate = null,
        string? approvalComment = null) =>
        Create(
            recommendation.Id,
            recommendation.RecommendationNumber,
            recommendation.Version,
            recommendation.CompanyId,
            recommendation.MissionId,
            recommendation.ContractId,
            recommendation.DeliveryStrategyId,
            recommendation.Title,
            recommendation.Summary,
            eventType,
            recommendation.Status,
            workflowStatus,
            workflowId,
            approver,
            approvalDate,
            approvalComment,
            recommendation.Score,
            recommendation.Rank,
            recommendation.PlanningTemplateId,
            recommendation.DecisionEngineVersion,
            recommendation.CapacitySnapshot,
            recommendation.WorkloadSnapshot,
            recommendation.RecommendationPayload,
            createdBy,
            createdAt,
            recommendation.DecisionProfileId,
            recommendation.DecisionProfileVersion);
}

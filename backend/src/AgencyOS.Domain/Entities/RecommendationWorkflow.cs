namespace AgencyOS.Domain.Entities;

/// <summary>
/// Recommendation Approval Workflow aggregate (US-201 / BR-1001..BR-1010).
/// Governance layer over Decision Engine recommendations — does not generate strategies.
/// </summary>
public class RecommendationWorkflow
{
    private readonly List<RecommendationWorkflowTransition> _transitions = [];

    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public Guid DeliveryStrategyId { get; private set; }

    public Guid ContractId { get; private set; }

    public Guid MissionId { get; private set; }

    public Guid? CompanyDecisionProfileId { get; private set; }

    /// <summary>Company that owns the originating Recommendation (US-402 / BR-2004).</summary>
    public Guid? CompanyId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public string CreatedBy { get; private set; } = string.Empty;

    public string? Approver { get; private set; }

    public DateTimeOffset? ApprovalDate { get; private set; }

    public string? ApprovalComment { get; private set; }

    public int? RankPosition { get; private set; }

    public decimal? FinalScore { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<RecommendationWorkflowTransition> Transitions => _transitions.AsReadOnly();

    public bool IsApproved => RecommendationWorkflowStatus.IsApproved(Status);

    public bool IsImmutable => RecommendationWorkflowStatus.IsImmutable(Status);

    private RecommendationWorkflow()
    {
    }

    public static RecommendationWorkflow Create(
        Guid recommendationId,
        Guid deliveryStrategyId,
        Guid contractId,
        Guid missionId,
        Guid? companyDecisionProfileId,
        string title,
        string? summary,
        string createdBy,
        int? rankPosition,
        decimal? finalScore,
        DateTimeOffset createdAt,
        Guid? companyId = null)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory.");
        }

        if (deliveryStrategyId == Guid.Empty)
        {
            throw new InvalidOperationException("DeliveryStrategyId is mandatory.");
        }

        if (contractId == Guid.Empty)
        {
            throw new InvalidOperationException("ContractId is mandatory.");
        }

        if (missionId == Guid.Empty)
        {
            throw new InvalidOperationException("MissionId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Title is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new InvalidOperationException("CreatedBy is mandatory.");
        }

        var workflow = new RecommendationWorkflow
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            DeliveryStrategyId = deliveryStrategyId,
            ContractId = contractId,
            MissionId = missionId,
            CompanyDecisionProfileId = companyDecisionProfileId,
            CompanyId = companyId,
            Title = title.Trim(),
            Summary = NormalizeOptional(summary),
            Status = RecommendationWorkflowStatus.Draft,
            CreatedBy = createdBy.Trim(),
            RankPosition = rankPosition,
            FinalScore = finalScore,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        workflow._transitions.Add(RecommendationWorkflowTransition.Create(
            workflow.Id,
            RecommendationWorkflowStatus.Draft,
            RecommendationWorkflowStatus.Draft,
            createdBy,
            "Recommendation workflow created in Draft.",
            createdAt));

        return workflow;
    }

    public void Submit(string actor, string? comment, DateTimeOffset occurredAt)
    {
        EnsureNotImmutable();
        TransitionTo(RecommendationWorkflowStatus.PendingApproval, actor, comment, occurredAt);
    }

    public void Approve(string approver, DateTimeOffset approvalDate, string? comment, DateTimeOffset occurredAt)
    {
        EnsureNotImmutable();

        if (string.IsNullOrWhiteSpace(approver))
        {
            throw new InvalidOperationException("Approver is mandatory.");
        }

        TransitionTo(RecommendationWorkflowStatus.Approved, approver, comment, occurredAt);
        Approver = approver.Trim();
        ApprovalDate = approvalDate;
        ApprovalComment = NormalizeOptional(comment);
    }

    public void Reject(string actor, string? comment, DateTimeOffset occurredAt)
    {
        EnsureNotImmutable();
        TransitionTo(RecommendationWorkflowStatus.Rejected, actor, comment, occurredAt);
    }

    public void Cancel(string actor, string? comment, DateTimeOffset occurredAt)
    {
        EnsureNotImmutable();
        TransitionTo(RecommendationWorkflowStatus.Cancelled, actor, comment, occurredAt);
    }

    public void Reopen(string actor, string? comment, DateTimeOffset occurredAt)
    {
        if (RecommendationWorkflowStatus.IsCancelled(Status))
        {
            throw new InvalidOperationException("Cancelled Recommendations cannot be reopened.");
        }

        if (RecommendationWorkflowStatus.IsApproved(Status))
        {
            throw new InvalidOperationException("Approved Recommendations become immutable.");
        }

        TransitionTo(RecommendationWorkflowStatus.Reopened, actor, comment, occurredAt);
    }

    public bool ValidateTransition(string toStatus) =>
        RecommendationWorkflowStatus.ValidateTransition(Status, toStatus);

    private void TransitionTo(
        string toStatus,
        string actor,
        string? comment,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException("Actor is mandatory for workflow transitions.");
        }

        var canonicalTo = RecommendationWorkflowStatus.Canonicalize(toStatus);

        if (!RecommendationWorkflowStatus.ValidateTransition(Status, canonicalTo))
        {
            throw new InvalidOperationException(
                $"Transition from '{Status}' to '{canonicalTo}' is not allowed.");
        }

        var fromStatus = Status;
        Status = canonicalTo;
        UpdatedAt = occurredAt;
        _transitions.Add(RecommendationWorkflowTransition.Create(
            Id,
            fromStatus,
            canonicalTo,
            actor,
            comment,
            occurredAt));
    }

    private void EnsureNotImmutable()
    {
        if (IsImmutable)
        {
            throw new InvalidOperationException("Approved Recommendations become immutable.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

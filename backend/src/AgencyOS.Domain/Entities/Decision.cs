namespace AgencyOS.Domain.Entities;

/// <summary>
/// Decision Tracking aggregate (US-205 / BR-1401..BR-1407).
/// Lifecycle monitoring of business decisions derived from Approved Recommendations.
/// Delete is prohibited. Originating RecommendationId is immutable.
/// </summary>
public class Decision
{
    private readonly List<DecisionTimelineEntry> _timeline = [];

    public Guid Id { get; private set; }

    public Guid RecommendationId { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid MissionId { get; private set; }

    public Guid ContractId { get; private set; }

    public string DecisionStatus { get; private set; } = string.Empty;

    public string ImplementationStatus { get; private set; } = string.Empty;

    public DateTimeOffset DecisionDate { get; private set; }

    public DateTimeOffset? ImplementationDate { get; private set; }

    public DateTimeOffset? CompletedDate { get; private set; }

    public string? Outcome { get; private set; }

    public string? BusinessValue { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<DecisionTimelineEntry> Timeline => _timeline.AsReadOnly();

    public bool IsCompleted => AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus);

    public bool IsCancelled => AgencyOS.Domain.Entities.DecisionStatus.IsCancelled(DecisionStatus);

    private Decision()
    {
    }

    public static Decision Create(
        Guid recommendationId,
        Guid companyId,
        Guid missionId,
        Guid contractId,
        string createdBy,
        DateTimeOffset decisionDate)
    {
        if (recommendationId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationId is mandatory.");
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

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new InvalidOperationException("CreatedBy is mandatory.");
        }

        var decision = new Decision
        {
            Id = Guid.NewGuid(),
            RecommendationId = recommendationId,
            CompanyId = companyId,
            MissionId = missionId,
            ContractId = contractId,
            DecisionStatus = AgencyOS.Domain.Entities.DecisionStatus.Created,
            ImplementationStatus = DecisionImplementationStatus.NotStarted,
            DecisionDate = decisionDate,
            CreatedBy = createdBy.Trim(),
            CreatedAt = decisionDate,
            UpdatedAt = decisionDate
        };

        decision.AppendTimeline(
            DecisionTimelineEventType.Created,
            AgencyOS.Domain.Entities.DecisionStatus.Created,
            AgencyOS.Domain.Entities.DecisionStatus.Created,
            DecisionImplementationStatus.NotStarted,
            DecisionImplementationStatus.NotStarted,
            createdBy,
            "Decision created from approved recommendation.",
            decisionDate);

        return decision;
    }

    public void StartImplementation(string actor, string? comment, DateTimeOffset occurredAt)
    {
        EnsureCanMutateLifecycle();

        if (!AgencyOS.Domain.Entities.DecisionStatus.CanStartImplementation(DecisionStatus)
            || !DecisionImplementationStatus.CanStart(ImplementationStatus))
        {
            throw new InvalidOperationException(
                $"Cannot start implementation from DecisionStatus '{DecisionStatus}' and ImplementationStatus '{ImplementationStatus}'.");
        }

        var fromDecision = DecisionStatus;
        var fromImplementation = ImplementationStatus;
        DecisionStatus = AgencyOS.Domain.Entities.DecisionStatus.InProgress;
        ImplementationStatus = DecisionImplementationStatus.InProgress;
        ImplementationDate = occurredAt;
        UpdatedAt = occurredAt;

        AppendTimeline(
            DecisionTimelineEventType.StartImplementation,
            fromDecision,
            DecisionStatus,
            fromImplementation,
            ImplementationStatus,
            actor,
            comment,
            occurredAt);
    }

    public void Complete(string actor, string? comment, DateTimeOffset occurredAt)
    {
        EnsureCanMutateLifecycle();

        if (!AgencyOS.Domain.Entities.DecisionStatus.CanComplete(DecisionStatus)
            || !DecisionImplementationStatus.CanComplete(ImplementationStatus))
        {
            throw new InvalidOperationException(
                $"Cannot complete decision from DecisionStatus '{DecisionStatus}' and ImplementationStatus '{ImplementationStatus}'.");
        }

        var fromDecision = DecisionStatus;
        var fromImplementation = ImplementationStatus;
        DecisionStatus = AgencyOS.Domain.Entities.DecisionStatus.Completed;
        ImplementationStatus = DecisionImplementationStatus.Completed;
        CompletedDate = occurredAt;
        UpdatedAt = occurredAt;

        AppendTimeline(
            DecisionTimelineEventType.Complete,
            fromDecision,
            DecisionStatus,
            fromImplementation,
            ImplementationStatus,
            actor,
            comment,
            occurredAt);
    }

    public void Cancel(string actor, string? comment, DateTimeOffset occurredAt)
    {
        if (AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus))
        {
            throw new InvalidOperationException("Completed Decisions cannot be cancelled.");
        }

        if (AgencyOS.Domain.Entities.DecisionStatus.IsCancelled(DecisionStatus))
        {
            throw new InvalidOperationException("Decision is already cancelled.");
        }

        if (!AgencyOS.Domain.Entities.DecisionStatus.CanCancel(DecisionStatus)
            || !DecisionImplementationStatus.CanCancel(ImplementationStatus))
        {
            throw new InvalidOperationException(
                $"Cannot cancel decision from DecisionStatus '{DecisionStatus}' and ImplementationStatus '{ImplementationStatus}'.");
        }

        var fromDecision = DecisionStatus;
        var fromImplementation = ImplementationStatus;
        DecisionStatus = AgencyOS.Domain.Entities.DecisionStatus.Cancelled;
        ImplementationStatus = DecisionImplementationStatus.Cancelled;
        UpdatedAt = occurredAt;

        AppendTimeline(
            DecisionTimelineEventType.Cancel,
            fromDecision,
            DecisionStatus,
            fromImplementation,
            ImplementationStatus,
            actor,
            comment,
            occurredAt);
    }

    public void RecordOutcome(
        string outcome,
        string? businessValue,
        string actor,
        string? comment,
        DateTimeOffset occurredAt)
    {
        if (!AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus))
        {
            throw new InvalidOperationException("Decision outcome is recorded only after completion.");
        }

        if (string.IsNullOrWhiteSpace(outcome))
        {
            throw new InvalidOperationException("Outcome is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException("Actor is mandatory.");
        }

        var fromDecision = DecisionStatus;
        var fromImplementation = ImplementationStatus;
        Outcome = outcome.Trim();
        BusinessValue = string.IsNullOrWhiteSpace(businessValue) ? null : businessValue.Trim();
        UpdatedAt = occurredAt;

        AppendTimeline(
            DecisionTimelineEventType.RecordOutcome,
            fromDecision,
            DecisionStatus,
            fromImplementation,
            ImplementationStatus,
            actor,
            comment ?? $"Outcome recorded: {Outcome}",
            occurredAt);
    }

    public bool Validate()
    {
        if (RecommendationId == Guid.Empty
            || CompanyId == Guid.Empty
            || MissionId == Guid.Empty
            || ContractId == Guid.Empty
            || string.IsNullOrWhiteSpace(CreatedBy))
        {
            return false;
        }

        if (!AgencyOS.Domain.Entities.DecisionStatus.IsKnown(DecisionStatus)
            || !DecisionImplementationStatus.IsKnown(ImplementationStatus))
        {
            return false;
        }

        if (AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus)
            && !CompletedDate.HasValue)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Outcome)
            && !AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus))
        {
            return false;
        }

        return true;
    }

    private void EnsureCanMutateLifecycle()
    {
        if (AgencyOS.Domain.Entities.DecisionStatus.IsCompleted(DecisionStatus))
        {
            throw new InvalidOperationException("Completed Decisions cannot return to In Progress.");
        }

        if (AgencyOS.Domain.Entities.DecisionStatus.IsCancelled(DecisionStatus))
        {
            throw new InvalidOperationException("Cancelled Decisions cannot be modified.");
        }
    }

    private void AppendTimeline(
        string eventType,
        string fromDecisionStatus,
        string toDecisionStatus,
        string fromImplementationStatus,
        string toImplementationStatus,
        string actor,
        string? comment,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException("Actor is mandatory for decision timeline entries.");
        }

        _timeline.Add(DecisionTimelineEntry.Create(
            Id,
            eventType,
            fromDecisionStatus,
            toDecisionStatus,
            fromImplementationStatus,
            toImplementationStatus,
            actor,
            comment,
            occurredAt));
    }
}

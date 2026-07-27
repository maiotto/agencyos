namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable Recommendation aggregate (US-202 / BR-1101..BR-1110).
/// No update or delete. Archive/Restore and CreateNewVersion only.
/// </summary>
public class Recommendation
{
    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid MissionId { get; private set; }

    public Guid ContractId { get; private set; }

    public Guid DeliveryStrategyId { get; private set; }

    public string RecommendationNumber { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Summary { get; private set; }

    public string? Reason { get; private set; }

    public decimal? Score { get; private set; }

    public int? Rank { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public string DecisionEngineVersion { get; private set; } = string.Empty;

    public Guid? PlanningTemplateId { get; private set; }

    /// <summary>JSON snapshot of capacity inputs/context used during generation (BR-1105).</summary>
    public string CapacitySnapshot { get; private set; } = string.Empty;

    /// <summary>JSON snapshot of workload inputs/context used during generation (BR-1105).</summary>
    public string WorkloadSnapshot { get; private set; } = string.Empty;

    /// <summary>Full operational payload JSON for the generated recommendation (BR-1105).</summary>
    public string RecommendationPayload { get; private set; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; private set; }

    public string GeneratedBy { get; private set; } = string.Empty;

    public bool Archived { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? DecisionProfileId { get; private set; }

    public int DecisionProfileVersion { get; private set; }

    private Recommendation()
    {
    }

    public static Recommendation Create(
        Guid companyId,
        Guid missionId,
        Guid contractId,
        Guid deliveryStrategyId,
        string recommendationNumber,
        string title,
        string? summary,
        string? reason,
        decimal? score,
        int? rank,
        int version,
        string decisionEngineVersion,
        Guid? planningTemplateId,
        string capacitySnapshot,
        string workloadSnapshot,
        string recommendationPayload,
        string generatedBy,
        DateTimeOffset generatedAt,
        Guid? decisionProfileId = null,
        int decisionProfileVersion = 0)
    {
        Validate(
            companyId,
            missionId,
            contractId,
            deliveryStrategyId,
            recommendationNumber,
            title,
            version,
            decisionEngineVersion,
            capacitySnapshot,
            workloadSnapshot,
            recommendationPayload,
            generatedBy);

        if (decisionProfileVersion < 0)
        {
            throw new InvalidOperationException("DecisionProfileVersion cannot be negative.");
        }

        return new Recommendation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            MissionId = missionId,
            ContractId = contractId,
            DeliveryStrategyId = deliveryStrategyId,
            RecommendationNumber = recommendationNumber.Trim(),
            Title = title.Trim(),
            Summary = NormalizeOptional(summary),
            Reason = NormalizeOptional(reason),
            Score = score,
            Rank = rank,
            Status = RecommendationStatus.Active,
            Version = version,
            DecisionEngineVersion = decisionEngineVersion.Trim(),
            PlanningTemplateId = planningTemplateId,
            CapacitySnapshot = capacitySnapshot,
            WorkloadSnapshot = workloadSnapshot,
            RecommendationPayload = recommendationPayload,
            GeneratedAt = generatedAt,
            GeneratedBy = generatedBy.Trim(),
            Archived = false,
            ArchivedAt = null,
            CreatedAt = generatedAt,
            DecisionProfileId = decisionProfileId,
            DecisionProfileVersion = decisionProfileVersion
        };
    }

    /// <summary>
    /// Creates a new immutable version (BR-1106). Never overwrites the source.
    /// </summary>
    public Recommendation CreateNewVersion(
        string? title,
        string? summary,
        string? reason,
        decimal? score,
        int? rank,
        string decisionEngineVersion,
        Guid? planningTemplateId,
        string capacitySnapshot,
        string workloadSnapshot,
        string recommendationPayload,
        string generatedBy,
        DateTimeOffset generatedAt,
        Guid? decisionProfileId = null,
        int? decisionProfileVersion = null)
    {
        return Create(
            CompanyId,
            MissionId,
            ContractId,
            DeliveryStrategyId,
            RecommendationNumber,
            string.IsNullOrWhiteSpace(title) ? Title : title,
            summary ?? Summary,
            reason ?? Reason,
            score,
            rank,
            Version + 1,
            decisionEngineVersion,
            planningTemplateId ?? PlanningTemplateId,
            capacitySnapshot,
            workloadSnapshot,
            recommendationPayload,
            generatedBy,
            generatedAt,
            decisionProfileId ?? DecisionProfileId,
            decisionProfileVersion ?? DecisionProfileVersion);
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Recommendation is already archived.");
        }

        Archived = true;
        ArchivedAt = archivedAt;
        Status = RecommendationStatus.Archived;
    }

    public void Restore()
    {
        if (!Archived)
        {
            throw new InvalidOperationException("Recommendation is not archived.");
        }

        Archived = false;
        ArchivedAt = null;
        Status = RecommendationStatus.Active;
    }

    public static void Validate(
        Guid companyId,
        Guid missionId,
        Guid contractId,
        Guid deliveryStrategyId,
        string recommendationNumber,
        string title,
        int version,
        string decisionEngineVersion,
        string capacitySnapshot,
        string workloadSnapshot,
        string recommendationPayload,
        string generatedBy)
    {
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

        if (string.IsNullOrWhiteSpace(recommendationNumber))
        {
            throw new InvalidOperationException("RecommendationNumber is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Title is mandatory.");
        }

        if (version < 1)
        {
            throw new InvalidOperationException("RecommendationVersion must be at least 1.");
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

        if (string.IsNullOrWhiteSpace(generatedBy))
        {
            throw new InvalidOperationException("GeneratedBy is mandatory.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

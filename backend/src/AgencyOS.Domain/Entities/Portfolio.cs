namespace AgencyOS.Domain.Entities;

/// <summary>
/// Portfolio Planning aggregate (US-109 / BR-901..BR-912).
/// Aggregation layer over Missions — never mutates Mission planning.
/// </summary>
public class Portfolio
{
    private readonly List<PortfolioMission> _missions = [];

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public Guid? PlanningTemplateId { get; private set; }

    public DateOnly PlanningPeriodStart { get; private set; }

    public DateOnly PlanningPeriodEnd { get; private set; }

    /// <summary>JSON capacity summary snapshot from Capacity Engine (BR-908).</summary>
    public string? CapacitySummary { get; private set; }

    /// <summary>JSON workload summary snapshot from Workload Engine (BR-909).</summary>
    public string? WorkloadSummary { get; private set; }

    public string PortfolioHealth { get; private set; } = string.Empty;

    /// <summary>JSON health detail including historical aggregates (BR-910).</summary>
    public string? HealthDetails { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<PortfolioMission> Missions => _missions.AsReadOnly();

    public bool IsActive => PortfolioStatus.IsActive(Status);

    public bool IsInactive => PortfolioStatus.IsInactive(Status);

    private Portfolio()
    {
    }

    public static Portfolio Create(
        Guid companyId,
        string name,
        string? description,
        DateOnly planningPeriodStart,
        DateOnly planningPeriodEnd,
        Guid? planningTemplateId,
        IReadOnlyList<(Guid MissionId, int Priority)> missions,
        DateTimeOffset createdAt)
    {
        ValidateIdentity(companyId, name);
        ValidatePeriod(planningPeriodStart, planningPeriodEnd);

        if (missions is null || missions.Count == 0)
        {
            throw new InvalidOperationException("Portfolio must contain at least one Mission.");
        }

        var portfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = name.Trim(),
            Description = NormalizeOptional(description),
            Status = PortfolioStatus.Active,
            PlanningTemplateId = planningTemplateId,
            PlanningPeriodStart = planningPeriodStart,
            PlanningPeriodEnd = planningPeriodEnd,
            CapacitySummary = null,
            WorkloadSummary = null,
            PortfolioHealth = Domain.Entities.PortfolioHealth.Unknown,
            HealthDetails = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        foreach (var mission in missions.OrderBy(item => item.Priority).ThenBy(item => item.MissionId))
        {
            portfolio.AddMissionInternal(mission.MissionId, mission.Priority, createdAt);
        }

        return portfolio;
    }

    public void Update(
        string name,
        string? description,
        DateOnly planningPeriodStart,
        DateOnly planningPeriodEnd,
        DateTimeOffset updatedAt)
    {
        EnsureCanModify();
        ValidateIdentity(CompanyId, name);
        ValidatePeriod(planningPeriodStart, planningPeriodEnd);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        PlanningPeriodStart = planningPeriodStart;
        PlanningPeriodEnd = planningPeriodEnd;
        UpdatedAt = updatedAt;
        InvalidateCalculations();
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (_missions.Count == 0)
        {
            throw new InvalidOperationException("Portfolio must contain at least one Mission.");
        }

        if (IsActive)
        {
            return;
        }

        Status = PortfolioStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = PortfolioStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete()
    {
        if (!PortfolioStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Deleting Active Portfolios is prohibited.");
        }
    }

    public void AssociateMission(Guid missionId, int priority, DateTimeOffset includedAt)
    {
        EnsureCanModify();
        AddMissionInternal(missionId, priority, includedAt);
        UpdatedAt = includedAt;
        InvalidateCalculations();
    }

    public void RemoveMission(Guid missionId, DateTimeOffset updatedAt)
    {
        EnsureCanModify();

        var existing = _missions.FirstOrDefault(mission => mission.MissionId == missionId);
        if (existing is null)
        {
            throw new InvalidOperationException("Mission is not associated with this Portfolio.");
        }

        if (_missions.Count == 1)
        {
            throw new InvalidOperationException("Portfolio must contain at least one Mission.");
        }

        _missions.Remove(existing);
        UpdatedAt = updatedAt;
        InvalidateCalculations();
    }

    public void AssignPlanningTemplate(Guid? planningTemplateId, DateTimeOffset updatedAt)
    {
        EnsureCanModify();
        PlanningTemplateId = planningTemplateId;
        UpdatedAt = updatedAt;
        InvalidateCalculations();
    }

    public void CalculateCapacity(string capacitySummaryJson, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(capacitySummaryJson))
        {
            throw new InvalidOperationException("CapacitySummary is mandatory.");
        }

        CapacitySummary = capacitySummaryJson;
        UpdatedAt = updatedAt;
    }

    public void CalculateWorkload(string workloadSummaryJson, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(workloadSummaryJson))
        {
            throw new InvalidOperationException("WorkloadSummary is mandatory.");
        }

        WorkloadSummary = workloadSummaryJson;
        UpdatedAt = updatedAt;
    }

    public void CalculateHealth(
        decimal utilizationPercentage,
        decimal workloadPercentage,
        decimal? warningPercentage,
        string healthDetailsJson,
        DateTimeOffset updatedAt)
    {
        PortfolioHealth = Domain.Entities.PortfolioHealth.Calculate(
            utilizationPercentage,
            workloadPercentage,
            warningPercentage);
        HealthDetails = string.IsNullOrWhiteSpace(healthDetailsJson) ? "{}" : healthDetailsJson;
        UpdatedAt = updatedAt;
    }

    public void Validate()
    {
        ValidateIdentity(CompanyId, Name);
        ValidatePeriod(PlanningPeriodStart, PlanningPeriodEnd);

        if (_missions.Count == 0)
        {
            throw new InvalidOperationException("Portfolio must contain at least one Mission.");
        }

        var duplicates = _missions
            .GroupBy(mission => mission.MissionId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException("The same Mission cannot belong twice to the same Portfolio.");
        }
    }

    private void AddMissionInternal(Guid missionId, int priority, DateTimeOffset includedAt)
    {
        if (_missions.Any(mission => mission.MissionId == missionId))
        {
            throw new InvalidOperationException("The same Mission cannot belong twice to the same Portfolio.");
        }

        _missions.Add(PortfolioMission.Create(Id, missionId, priority, includedAt));
    }

    private void EnsureCanModify()
    {
        if (!PortfolioStatus.CanModify(Status))
        {
            throw new InvalidOperationException("Inactive Portfolios cannot be modified.");
        }
    }

    private void InvalidateCalculations()
    {
        CapacitySummary = null;
        WorkloadSummary = null;
        PortfolioHealth = Domain.Entities.PortfolioHealth.Unknown;
        HealthDetails = null;
    }

    private static void ValidateIdentity(Guid companyId, string name)
    {
        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Portfolio Name is mandatory.");
        }
    }

    private static void ValidatePeriod(DateOnly planningPeriodStart, DateOnly planningPeriodEnd)
    {
        if (planningPeriodEnd < planningPeriodStart)
        {
            throw new InvalidOperationException("PlanningPeriodEnd cannot be earlier than PlanningPeriodStart.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

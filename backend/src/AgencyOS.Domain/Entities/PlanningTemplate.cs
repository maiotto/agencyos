namespace AgencyOS.Domain.Entities;

/// <summary>
/// Planning Template aggregate (US-108 / BR-801..BR-810).
/// Stores references to planning configuration only — never operational data.
/// </summary>
public class PlanningTemplate
{
    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public Guid WorkingCalendarId { get; private set; }

    public Guid WorkingHoursId { get; private set; }

    public string ResourceAvailabilityStrategy { get; private set; } = string.Empty;

    public int DefaultPlanningWindowDays { get; private set; }

    public int DefaultPeriodStartOffsetDays { get; private set; }

    public decimal? UtilizationWarningPercentage { get; private set; }

    public bool IncludeAssignmentDistribution { get; private set; }

    public string? PlanningParametersJson { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => PlanningTemplateStatus.IsActive(Status);

    public bool IsInactive => PlanningTemplateStatus.IsInactive(Status);

    private PlanningTemplate()
    {
    }

    public static PlanningTemplate Create(
        Guid companyId,
        string name,
        string? description,
        Guid workingCalendarId,
        Guid workingHoursId,
        string resourceAvailabilityStrategy,
        int defaultPlanningWindowDays,
        int defaultPeriodStartOffsetDays,
        PlanningTemplateCapacityRules capacityRules,
        string? planningParametersJson,
        DateTimeOffset createdAt)
    {
        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory for Planning Template.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Template Name is mandatory.");
        }

        if (workingCalendarId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Calendar reference is mandatory.");
        }

        if (workingHoursId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Hours reference is mandatory.");
        }

        if (defaultPlanningWindowDays <= 0)
        {
            throw new InvalidOperationException("DefaultPlanningWindowDays must be greater than zero.");
        }

        if (defaultPeriodStartOffsetDays < 0)
        {
            throw new InvalidOperationException("DefaultPeriodStartOffsetDays cannot be negative.");
        }

        return new PlanningTemplate
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = name.Trim(),
            Description = NormalizeOptionalText(description),
            Status = PlanningTemplateStatus.Inactive,
            WorkingCalendarId = workingCalendarId,
            WorkingHoursId = workingHoursId,
            ResourceAvailabilityStrategy = ResourceAvailabilityStrategies.Canonicalize(resourceAvailabilityStrategy),
            DefaultPlanningWindowDays = defaultPlanningWindowDays,
            DefaultPeriodStartOffsetDays = defaultPeriodStartOffsetDays,
            UtilizationWarningPercentage = capacityRules.UtilizationWarningPercentage,
            IncludeAssignmentDistribution = capacityRules.IncludeAssignmentDistribution,
            PlanningParametersJson = NormalizeOptionalText(planningParametersJson),
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void Reconfigure(
        string name,
        string? description,
        Guid workingCalendarId,
        Guid workingHoursId,
        string resourceAvailabilityStrategy,
        int defaultPlanningWindowDays,
        int defaultPeriodStartOffsetDays,
        PlanningTemplateCapacityRules capacityRules,
        string? planningParametersJson,
        DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Template Name is mandatory.");
        }

        if (workingCalendarId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Calendar reference is mandatory.");
        }

        if (workingHoursId == Guid.Empty)
        {
            throw new InvalidOperationException("Working Hours reference is mandatory.");
        }

        if (defaultPlanningWindowDays <= 0)
        {
            throw new InvalidOperationException("DefaultPlanningWindowDays must be greater than zero.");
        }

        if (defaultPeriodStartOffsetDays < 0)
        {
            throw new InvalidOperationException("DefaultPeriodStartOffsetDays cannot be negative.");
        }

        Name = name.Trim();
        Description = NormalizeOptionalText(description);
        WorkingCalendarId = workingCalendarId;
        WorkingHoursId = workingHoursId;
        ResourceAvailabilityStrategy = ResourceAvailabilityStrategies.Canonicalize(resourceAvailabilityStrategy);
        DefaultPlanningWindowDays = defaultPlanningWindowDays;
        DefaultPeriodStartOffsetDays = defaultPeriodStartOffsetDays;
        UtilizationWarningPercentage = capacityRules.UtilizationWarningPercentage;
        IncludeAssignmentDistribution = capacityRules.IncludeAssignmentDistribution;
        PlanningParametersJson = NormalizeOptionalText(planningParametersJson);
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (IsActive)
        {
            return;
        }

        Status = PlanningTemplateStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = PlanningTemplateStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete()
    {
        if (!PlanningTemplateStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Deleting Active Templates is prohibited.");
        }
    }

    public void EnsureCanApply()
    {
        if (!PlanningTemplateStatus.CanApply(Status))
        {
            throw new InvalidOperationException("Inactive templates cannot be applied.");
        }
    }

    /// <summary>
    /// Creates a configuration-only clone (BR-809). Never copies operational history.
    /// </summary>
    public PlanningTemplate Clone(string clonedName, DateTimeOffset createdAt)
    {
        return Create(
            CompanyId,
            clonedName,
            Description,
            WorkingCalendarId,
            WorkingHoursId,
            ResourceAvailabilityStrategy,
            DefaultPlanningWindowDays,
            DefaultPeriodStartOffsetDays,
            new PlanningTemplateCapacityRules(UtilizationWarningPercentage, IncludeAssignmentDistribution),
            PlanningParametersJson,
            createdAt);
    }

    public (DateOnly PeriodStart, DateOnly PeriodEnd) ResolveDefaultPlanningWindow(DateOnly asOfDate)
    {
        var periodStart = asOfDate.AddDays(DefaultPeriodStartOffsetDays);
        var periodEnd = periodStart.AddDays(DefaultPlanningWindowDays - 1);
        return (periodStart, periodEnd);
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

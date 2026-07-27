namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable value object for default capacity planning rules stored on a template.
/// </summary>
public sealed class PlanningTemplateCapacityRules
{
    public decimal? UtilizationWarningPercentage { get; }

    public bool IncludeAssignmentDistribution { get; }

    public PlanningTemplateCapacityRules(
        decimal? utilizationWarningPercentage,
        bool includeAssignmentDistribution)
    {
        if (utilizationWarningPercentage is < 0 or > 100)
        {
            throw new InvalidOperationException(
                "UtilizationWarningPercentage must be between 0 and 100 when provided.");
        }

        UtilizationWarningPercentage = utilizationWarningPercentage;
        IncludeAssignmentDistribution = includeAssignmentDistribution;
    }

    public static PlanningTemplateCapacityRules Default => new(null, true);
}

namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyPlanningMetadataResponse
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int TaskCount { get; set; }

    public int ActiveResourceCount { get; set; }

    public int DetectedConflictCount { get; set; }

    public IReadOnlyList<string> AppliedCompanyPolicyRules { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> AppliedClientPolicyRules { get; set; } = Array.Empty<string>();
}

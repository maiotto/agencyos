namespace AgencyOS.Application.DTOs;

/// <summary>
/// Query filters for Workload History (BR-707).
/// </summary>
public class WorkloadHistoryQueryParameters
{
    public Guid? ExecutionResourceId { get; set; }

    public Guid? CompanyId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public string? CalculationVersion { get; set; }

    public DateTimeOffset? CalculatedFrom { get; set; }

    public DateTimeOffset? CalculatedTo { get; set; }
}

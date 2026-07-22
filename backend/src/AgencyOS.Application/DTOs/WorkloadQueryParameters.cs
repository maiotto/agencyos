namespace AgencyOS.Application.DTOs;

public class WorkloadQueryParameters
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public Guid? ExcludeMissionId { get; set; }
}

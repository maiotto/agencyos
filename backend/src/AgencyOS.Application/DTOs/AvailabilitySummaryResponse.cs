namespace AgencyOS.Application.DTOs;

public class AvailabilitySummaryResponse
{
    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int ActiveResourceCount { get; set; }

    public decimal TotalAvailableHours { get; set; }

    public decimal TotalOccupiedHours { get; set; }

    public decimal OverallAvailabilityPercentage { get; set; }

    public int ResourcesWithAvailability { get; set; }
}

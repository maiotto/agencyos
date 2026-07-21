namespace AgencyOS.Application.DTOs;

public class AvailabilityResponse
{
    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public DateOnly? NextAvailableDate { get; set; }

    public decimal AvailableHours { get; set; }

    public decimal OccupiedHours { get; set; }

    public decimal AvailabilityPercentage { get; set; }

    public IReadOnlyList<AvailabilityTimeSlotResponse> AvailableTimeSlots { get; set; } =
        Array.Empty<AvailabilityTimeSlotResponse>();
}

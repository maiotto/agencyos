namespace AgencyOS.Application.DTOs;

public class AvailabilityTimeSlotResponse
{
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AvailableHours { get; set; }
}

namespace AgencyOS.Application.DTOs;

public class CreateWorkingHoursRequest
{
    public Guid WorkingCalendarId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<WorkingHoursDayRequest> Days { get; set; } = [];
}

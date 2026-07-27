namespace AgencyOS.Application.DTOs;

public class UpdateWorkingHoursRequest
{
    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<WorkingHoursDayRequest> Days { get; set; } = [];
}

namespace AgencyOS.Application.DTOs;

public class WorkingHoursResponse
{
    public Guid Id { get; set; }

    public Guid WorkingCalendarId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<WorkingHoursDayResponse> Days { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

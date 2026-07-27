namespace AgencyOS.Application.DTOs;

public class CreateWorkingCalendarRequest
{
    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public IReadOnlyList<string> WorkingDays { get; set; } = [];
}

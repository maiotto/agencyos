namespace AgencyOS.Application.DTOs;

public class WorkingHoursQueryParameters
{
    public Guid? WorkingCalendarId { get; set; }

    public string? Status { get; set; }

    public string? Name { get; set; }

    public string OrderBy { get; set; } = "name";

    public string OrderDirection { get; set; } = "asc";
}

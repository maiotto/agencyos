namespace AgencyOS.Application.DTOs;

public class CreateHolidayRequest
{
    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string HolidayType { get; set; } = string.Empty;

    public DateOnly HolidayDate { get; set; }

    public string? StateCode { get; set; }

    public string? City { get; set; }

    public bool Recurring { get; set; }
}

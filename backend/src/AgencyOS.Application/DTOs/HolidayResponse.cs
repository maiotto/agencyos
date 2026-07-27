namespace AgencyOS.Application.DTOs;

public class HolidayResponse
{
    public Guid Id { get; set; }

    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string HolidayType { get; set; } = string.Empty;

    public DateOnly HolidayDate { get; set; }

    public string? StateCode { get; set; }

    public string? City { get; set; }

    public bool Recurring { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

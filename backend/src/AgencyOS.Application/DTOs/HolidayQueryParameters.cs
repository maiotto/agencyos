namespace AgencyOS.Application.DTOs;

public class HolidayQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? Status { get; set; }

    public string? HolidayType { get; set; }

    public string? Name { get; set; }

    public string? StateCode { get; set; }

    public string? City { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public bool? Recurring { get; set; }

    public string OrderBy { get; set; } = "holidayDate";

    public string OrderDirection { get; set; } = "asc";
}

namespace AgencyOS.Application.DTOs;

public class OperationalWorkingDayResponse
{
    public Guid CompanyId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsWorkingDay { get; set; }

    public bool IsCalendarWorkingWeekday { get; set; }

    public bool IsHoliday { get; set; }

    public Guid? WorkingCalendarId { get; set; }

    public Guid? WorkingHoursId { get; set; }

    public WorkingHoursDayResponse? Schedule { get; set; }

    public decimal? PlannedNetHours { get; set; }

    public IReadOnlyList<HolidayResponse> Holidays { get; set; } = [];
}

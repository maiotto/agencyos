namespace AgencyOS.Application.DTOs;

public class WorkingHoursDayRequest
{
    public string DayOfWeek { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public TimeOnly? BreakStart { get; set; }

    public TimeOnly? BreakEnd { get; set; }
}

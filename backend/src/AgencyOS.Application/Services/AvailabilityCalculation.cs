using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Services;

public static class AvailabilityCalculation
{
    private static readonly DayOfWeek[] WorkingDays =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    public static bool IsWorkingDay(DateOnly date) =>
        Array.Exists(WorkingDays, day => day == date.DayOfWeek);

    public static IReadOnlyList<DateOnly> GetWorkingDaysInPeriod(DateOnly periodStartDate, DateOnly periodEndDate)
    {
        var workingDays = new List<DateOnly>();

        for (var date = periodStartDate; date <= periodEndDate; date = date.AddDays(1))
        {
            if (IsWorkingDay(date))
            {
                workingDays.Add(date);
            }
        }

        return workingDays;
    }

    public static decimal CalculateDailyCapacityHours(decimal capacityHoursPerWeek)
    {
        return capacityHoursPerWeek / 5m;
    }

    public static decimal DeriveCapacityHoursPerWeek(decimal totalCapacityHours, int periodDays)
    {
        if (periodDays <= 0)
        {
            return 0;
        }

        return totalCapacityHours * 7m / periodDays;
    }

    public static decimal CalculateAvailabilityPercentage(decimal totalCapacityHours, decimal availableHours)
    {
        if (totalCapacityHours <= 0)
        {
            return 0;
        }

        var cappedAvailableHours = CapAvailableHours(availableHours, totalCapacityHours);
        var percentage = (cappedAvailableHours / totalCapacityHours) * 100;

        return decimal.Round(
            Math.Min(100, Math.Max(0, percentage)),
            2,
            MidpointRounding.AwayFromZero);
    }

    public static decimal CapAvailableHours(decimal availableHours, decimal totalCapacityHours)
    {
        return Math.Min(Math.Max(0, availableHours), Math.Max(0, totalCapacityHours));
    }

    public static Dictionary<DateOnly, decimal> BuildDailyOccupiedHours(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var occupiedHoursByDay = new Dictionary<DateOnly, decimal>();

        foreach (var assignment in assignments)
        {
            var rangeStart = assignment.PlannedStartDate > periodStartDate
                ? assignment.PlannedStartDate
                : periodStartDate;
            var rangeEnd = assignment.PlannedEndDate < periodEndDate
                ? assignment.PlannedEndDate
                : periodEndDate;

            if (rangeStart > rangeEnd)
            {
                continue;
            }

            var workingDays = GetWorkingDaysInPeriod(rangeStart, rangeEnd);

            if (workingDays.Count == 0)
            {
                continue;
            }

            var hoursPerDay = assignment.PlannedHours / workingDays.Count;

            foreach (var day in workingDays)
            {
                occupiedHoursByDay[day] = occupiedHoursByDay.GetValueOrDefault(day) + hoursPerDay;
            }
        }

        return occupiedHoursByDay;
    }

    public static IReadOnlyList<AvailabilityTimeSlotResponse> BuildAvailableTimeSlots(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        decimal capacityHoursPerWeek,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var dailyCapacityHours = CalculateDailyCapacityHours(capacityHoursPerWeek);
        var dailyOccupiedHours = BuildDailyOccupiedHours(periodStartDate, periodEndDate, assignments);
        var workingDays = GetWorkingDaysInPeriod(periodStartDate, periodEndDate);
        var timeSlots = new List<AvailabilityTimeSlotResponse>();

        DateOnly? slotStart = null;
        DateOnly? slotEnd = null;
        decimal slotHours = 0;

        for (var index = 0; index < workingDays.Count; index++)
        {
            var day = workingDays[index];
            var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
            var availableHours = Math.Max(0, dailyCapacityHours - occupiedHours);

            if (availableHours <= 0)
            {
                if (slotStart.HasValue && slotEnd.HasValue)
                {
                    timeSlots.Add(CreateTimeSlot(slotStart.Value, slotEnd.Value, slotHours));
                }

                slotStart = null;
                slotEnd = null;
                slotHours = 0;
                continue;
            }

            var continuesPreviousSlot = slotStart.HasValue
                && index > 0
                && HasAvailabilityOnDay(workingDays[index - 1], dailyCapacityHours, dailyOccupiedHours);

            if (continuesPreviousSlot)
            {
                slotEnd = day;
                slotHours += availableHours;
                continue;
            }

            if (slotStart.HasValue && slotEnd.HasValue)
            {
                timeSlots.Add(CreateTimeSlot(slotStart.Value, slotEnd.Value, slotHours));
            }

            slotStart = day;
            slotEnd = day;
            slotHours = availableHours;
        }

        if (slotStart.HasValue && slotEnd.HasValue)
        {
            timeSlots.Add(CreateTimeSlot(slotStart.Value, slotEnd.Value, slotHours));
        }

        return timeSlots;
    }

    public static DateOnly? FindNextAvailableDate(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        decimal capacityHoursPerWeek,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var dailyCapacityHours = CalculateDailyCapacityHours(capacityHoursPerWeek);
        var dailyOccupiedHours = BuildDailyOccupiedHours(periodStartDate, periodEndDate, assignments);

        foreach (var day in GetWorkingDaysInPeriod(periodStartDate, periodEndDate))
        {
            var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
            var availableHours = Math.Max(0, dailyCapacityHours - occupiedHours);

            if (availableHours > 0)
            {
                return day;
            }
        }

        return null;
    }

    private static bool HasAvailabilityOnDay(
        DateOnly day,
        decimal dailyCapacityHours,
        Dictionary<DateOnly, decimal> dailyOccupiedHours)
    {
        var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
        return Math.Max(0, dailyCapacityHours - occupiedHours) > 0;
    }

    private static AvailabilityTimeSlotResponse CreateTimeSlot(
        DateOnly startDate,
        DateOnly endDate,
        decimal availableHours)
    {
        return new AvailabilityTimeSlotResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            AvailableHours = decimal.Round(availableHours, 2, MidpointRounding.AwayFromZero)
        };
    }
}

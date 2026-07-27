using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public static class AvailabilityCalculation
{
    public static bool IsWorkingDay(DateOnly date, IReadOnlyCollection<string> configuredWorkingDays)
    {
        if (configuredWorkingDays is null || configuredWorkingDays.Count == 0)
        {
            throw new InvalidOperationException(
                "Configured working days are required. Availability cannot fall back to Monday–Friday defaults.");
        }

        return WorkingDayNames.IsWorkingDay(date, configuredWorkingDays);
    }

    public static IReadOnlyList<DateOnly> GetWorkingDaysInPeriod(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        IReadOnlyCollection<string> configuredWorkingDays)
    {
        var workingDays = new List<DateOnly>();

        for (var date = periodStartDate; date <= periodEndDate; date = date.AddDays(1))
        {
            if (IsWorkingDay(date, configuredWorkingDays))
            {
                workingDays.Add(date);
            }
        }

        return workingDays;
    }

    public static IReadOnlyList<DateOnly> GetOperationalDaysFromCapacity(
        IReadOnlyList<CapacityDayBreakdownResponse> operationalDays) =>
        operationalDays
            .Where(day => day.IsOperationalDay && day.PlannedCapacityHours > 0)
            .Select(day => day.Date)
            .OrderBy(date => date)
            .ToList();

    public static Dictionary<DateOnly, decimal> GetDailyCapacityHoursFromCapacity(
        IReadOnlyList<CapacityDayBreakdownResponse> operationalDays) =>
        operationalDays
            .Where(day => day.IsOperationalDay && day.PlannedCapacityHours > 0)
            .ToDictionary(day => day.Date, day => day.PlannedCapacityHours);

    public static decimal CalculateDailyCapacityHours(
        decimal capacityHoursPerWeek,
        int workingDaysPerWeek)
    {
        if (workingDaysPerWeek <= 0)
        {
            return 0;
        }

        return capacityHoursPerWeek / workingDaysPerWeek;
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
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments,
        IReadOnlyList<DateOnly> operationalDays)
    {
        var operationalDaySet = operationalDays.ToHashSet();
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

            var workingDays = operationalDays
                .Where(day => day >= rangeStart && day <= rangeEnd)
                .ToList();

            if (workingDays.Count == 0)
            {
                continue;
            }

            var hoursPerDay = assignment.PlannedHours / workingDays.Count;

            foreach (var day in workingDays)
            {
                if (!operationalDaySet.Contains(day))
                {
                    continue;
                }

                occupiedHoursByDay[day] = occupiedHoursByDay.GetValueOrDefault(day) + hoursPerDay;
            }
        }

        return occupiedHoursByDay;
    }

    public static IReadOnlyList<AvailabilityTimeSlotResponse> BuildAvailableTimeSlots(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        IReadOnlyDictionary<DateOnly, decimal> dailyCapacityHours,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var operationalDays = dailyCapacityHours.Keys.OrderBy(date => date).ToList();
        var dailyOccupiedHours = BuildDailyOccupiedHours(
            periodStartDate,
            periodEndDate,
            assignments,
            operationalDays);
        var timeSlots = new List<AvailabilityTimeSlotResponse>();

        DateOnly? slotStart = null;
        DateOnly? slotEnd = null;
        decimal slotHours = 0;

        for (var index = 0; index < operationalDays.Count; index++)
        {
            var day = operationalDays[index];
            var dayCapacity = dailyCapacityHours.GetValueOrDefault(day);
            var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
            var availableHours = Math.Max(0, dayCapacity - occupiedHours);

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
                && HasAvailabilityOnDay(operationalDays[index - 1], dailyCapacityHours, dailyOccupiedHours);

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
        IReadOnlyDictionary<DateOnly, decimal> dailyCapacityHours,
        IReadOnlyList<WorkloadAssignmentDistributionItem> assignments)
    {
        var operationalDays = dailyCapacityHours.Keys.OrderBy(date => date).ToList();
        var dailyOccupiedHours = BuildDailyOccupiedHours(
            periodStartDate,
            periodEndDate,
            assignments,
            operationalDays);

        foreach (var day in operationalDays)
        {
            var dayCapacity = dailyCapacityHours.GetValueOrDefault(day);
            var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
            var availableHours = Math.Max(0, dayCapacity - occupiedHours);

            if (availableHours > 0)
            {
                return day;
            }
        }

        return null;
    }

    private static bool HasAvailabilityOnDay(
        DateOnly day,
        IReadOnlyDictionary<DateOnly, decimal> dailyCapacityHours,
        Dictionary<DateOnly, decimal> dailyOccupiedHours)
    {
        var dayCapacity = dailyCapacityHours.GetValueOrDefault(day);
        var occupiedHours = dailyOccupiedHours.GetValueOrDefault(day);
        return Math.Max(0, dayCapacity - occupiedHours) > 0;
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

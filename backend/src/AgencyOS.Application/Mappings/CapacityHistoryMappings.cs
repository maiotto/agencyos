using System.Text.Json;
using System.Text.Json.Serialization;
using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Mappings;

public static class CapacityHistoryMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string BuildOperationalInputsJson(CapacityResponse capacity)
    {
        var snapshot = new
        {
            executionResourceCode = capacity.ExecutionResourceCode,
            executionResourceName = capacity.ExecutionResourceName,
            periodStartDate = capacity.PeriodStartDate,
            periodEndDate = capacity.PeriodEndDate,
            operationalDayCount = capacity.OperationalDayCount,
            holidayImpactDayCount = capacity.HolidayImpactDayCount,
            resourceAvailabilityExcludedDayCount = capacity.ResourceAvailabilityExcludedDayCount,
            configuredWorkingHoursTotal = capacity.ConfiguredWorkingHoursTotal,
            totalCapacityHours = capacity.TotalCapacityHours,
            allocatedHours = capacity.AllocatedHours,
            availableHours = capacity.AvailableHours,
            utilizationPercentage = capacity.UtilizationPercentage,
            remainingCapacityHours = capacity.RemainingCapacityHours,
            operationalDays = capacity.OperationalDays
        };

        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    public static CapacityHistory CreateHistory(
        CapacityResponse capacity,
        Guid companyId,
        DateTimeOffset calculationDate)
    {
        var workingDays = capacity.OperationalDays.Count(day => day.IsCalendarWorkingWeekday);
        var availableDays = capacity.OperationalDayCount;
        var holidayDays = capacity.HolidayImpactDayCount;

        return CapacityHistory.Create(
            executionResourceId: capacity.ExecutionResourceId,
            companyId: companyId,
            periodStart: capacity.PeriodStartDate,
            periodEnd: capacity.PeriodEndDate,
            workingDays: workingDays,
            holidayDays: holidayDays,
            availableDays: availableDays,
            configuredHours: capacity.ConfiguredWorkingHoursTotal,
            availableHours: capacity.AvailableHours,
            capacityHours: capacity.TotalCapacityHours,
            allocatedHours: capacity.AllocatedHours,
            utilizationPercentage: capacity.UtilizationPercentage,
            calculationVersion: CapacityHistoryVersions.Current,
            operationalInputsJson: BuildOperationalInputsJson(capacity),
            calculationDate: calculationDate);
    }

    public static CapacityHistoryResponse ToResponse(CapacityHistory history)
    {
        return new CapacityHistoryResponse
        {
            HistoryId = history.Id,
            ExecutionResourceId = history.ExecutionResourceId,
            CompanyId = history.CompanyId,
            CalculationDate = history.CalculationDate,
            PeriodStart = history.PeriodStart,
            PeriodEnd = history.PeriodEnd,
            WorkingDays = history.WorkingDays,
            HolidayDays = history.HolidayDays,
            AvailableDays = history.AvailableDays,
            ConfiguredHours = history.ConfiguredHours,
            AvailableHours = history.AvailableHours,
            CapacityHours = history.CapacityHours,
            AllocatedHours = history.AllocatedHours,
            UtilizationPercentage = history.UtilizationPercentage,
            CalculationVersion = history.CalculationVersion,
            CreatedAt = history.CreatedAt,
            OperationalDays = DeserializeOperationalDays(history.OperationalInputsJson)
        };
    }

    public static CapacityHistoryAggregateResponse ToAggregate(
        IReadOnlyList<CapacityHistory> histories,
        CapacityHistoryQueryParameters parameters)
    {
        var recordCount = histories.Count;
        var totalCapacity = histories.Sum(item => item.CapacityHours);
        var totalAllocated = histories.Sum(item => item.AllocatedHours);

        return new CapacityHistoryAggregateResponse
        {
            CompanyId = parameters.CompanyId,
            ExecutionResourceId = parameters.ExecutionResourceId,
            PeriodStart = parameters.PeriodStart,
            PeriodEnd = parameters.PeriodEnd,
            CalculationVersion = parameters.CalculationVersion,
            RecordCount = recordCount,
            TotalConfiguredHours = histories.Sum(item => item.ConfiguredHours),
            TotalAvailableHours = histories.Sum(item => item.AvailableHours),
            TotalCapacityHours = totalCapacity,
            TotalAllocatedHours = totalAllocated,
            AverageUtilizationPercentage = recordCount == 0
                ? 0
                : Math.Round(histories.Average(item => item.UtilizationPercentage), 2, MidpointRounding.AwayFromZero)
        };
    }

    public static CapacityHistoryCompareResponse ToCompare(
        CapacityHistory left,
        CapacityHistory right)
    {
        var leftResponse = ToResponse(left);
        var rightResponse = ToResponse(right);

        return new CapacityHistoryCompareResponse
        {
            Left = leftResponse,
            Right = rightResponse,
            CapacityHoursDelta = rightResponse.CapacityHours - leftResponse.CapacityHours,
            AvailableHoursDelta = rightResponse.AvailableHours - leftResponse.AvailableHours,
            ConfiguredHoursDelta = rightResponse.ConfiguredHours - leftResponse.ConfiguredHours,
            UtilizationPercentageDelta =
                rightResponse.UtilizationPercentage - leftResponse.UtilizationPercentage,
            WorkingDaysDelta = rightResponse.WorkingDays - leftResponse.WorkingDays,
            HolidayDaysDelta = rightResponse.HolidayDays - leftResponse.HolidayDays,
            AvailableDaysDelta = rightResponse.AvailableDays - leftResponse.AvailableDays
        };
    }

    private static IReadOnlyList<CapacityHistoryDaySnapshotResponse> DeserializeOperationalDays(
        string operationalInputsJson)
    {
        try
        {
            using var document = JsonDocument.Parse(operationalInputsJson);
            if (!document.RootElement.TryGetProperty("operationalDays", out var daysElement)
                || daysElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var days = JsonSerializer.Deserialize<List<CapacityHistoryDaySnapshotResponse>>(
                daysElement.GetRawText(),
                JsonOptions);

            return days ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

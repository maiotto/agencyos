using System.Text.Json;
using System.Text.Json.Serialization;
using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Mappings;

public sealed class WorkloadHistoryPersistModel
{
    public required WorkloadResponse Workload { get; init; }

    public required Guid CompanyId { get; init; }

    public required decimal CapacityHours { get; init; }

    public required int WorkingDays { get; init; }

    public required int HolidayDays { get; init; }

    public required int AvailableDays { get; init; }
}

public static class WorkloadHistoryMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string BuildOperationalInputsJson(WorkloadHistoryPersistModel model)
    {
        var capacity = model.Workload;
        var snapshot = new
        {
            executionResourceCode = capacity.ExecutionResourceCode,
            executionResourceName = capacity.ExecutionResourceName,
            periodStartDate = capacity.PeriodStartDate,
            periodEndDate = capacity.PeriodEndDate,
            totalPlannedHours = capacity.TotalPlannedHours,
            assignmentCount = capacity.AssignmentCount,
            averageHoursPerAssignment = capacity.AverageHoursPerAssignment,
            workloadPercentage = capacity.WorkloadPercentage,
            capacityHours = model.CapacityHours,
            workingDays = model.WorkingDays,
            holidayDays = model.HolidayDays,
            availableDays = model.AvailableDays,
            companyId = model.CompanyId,
            assignmentDistribution = capacity.AssignmentDistribution
        };

        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    public static WorkloadHistory CreateHistory(
        WorkloadHistoryPersistModel model,
        DateTimeOffset calculationDate)
    {
        return WorkloadHistory.Create(
            executionResourceId: model.Workload.ExecutionResourceId,
            companyId: model.CompanyId,
            periodStart: model.Workload.PeriodStartDate,
            periodEnd: model.Workload.PeriodEndDate,
            allocatedHours: model.Workload.TotalPlannedHours,
            capacityHours: model.CapacityHours,
            workloadPercentage: model.Workload.WorkloadPercentage,
            workingDays: model.WorkingDays,
            holidayDays: model.HolidayDays,
            availableDays: model.AvailableDays,
            calculationVersion: WorkloadHistoryVersions.Current,
            operationalInputsJson: BuildOperationalInputsJson(model),
            calculationDate: calculationDate);
    }

    public static WorkloadHistoryResponse ToResponse(WorkloadHistory history)
    {
        var snapshot = DeserializeSnapshot(history.OperationalInputsJson);

        return new WorkloadHistoryResponse
        {
            HistoryId = history.Id,
            ExecutionResourceId = history.ExecutionResourceId,
            CompanyId = history.CompanyId,
            CalculationDate = history.CalculationDate,
            PeriodStart = history.PeriodStart,
            PeriodEnd = history.PeriodEnd,
            AllocatedHours = history.AllocatedHours,
            CapacityHours = history.CapacityHours,
            WorkloadPercentage = history.WorkloadPercentage,
            WorkingDays = history.WorkingDays,
            HolidayDays = history.HolidayDays,
            AvailableDays = history.AvailableDays,
            CalculationVersion = history.CalculationVersion,
            CreatedAt = history.CreatedAt,
            AssignmentCount = snapshot.AssignmentCount,
            AverageHoursPerAssignment = snapshot.AverageHoursPerAssignment,
            ExecutionResourceCode = snapshot.ExecutionResourceCode,
            ExecutionResourceName = snapshot.ExecutionResourceName,
            AssignmentDistribution = snapshot.AssignmentDistribution
        };
    }

    public static WorkloadHistoryAggregateResponse ToAggregate(
        IReadOnlyList<WorkloadHistory> histories,
        WorkloadHistoryQueryParameters parameters)
    {
        var recordCount = histories.Count;

        return new WorkloadHistoryAggregateResponse
        {
            CompanyId = parameters.CompanyId,
            ExecutionResourceId = parameters.ExecutionResourceId,
            PeriodStart = parameters.PeriodStart,
            PeriodEnd = parameters.PeriodEnd,
            CalculationVersion = parameters.CalculationVersion,
            RecordCount = recordCount,
            TotalAllocatedHours = histories.Sum(item => item.AllocatedHours),
            TotalCapacityHours = histories.Sum(item => item.CapacityHours),
            AverageWorkloadPercentage = recordCount == 0
                ? 0
                : Math.Round(histories.Average(item => item.WorkloadPercentage), 2, MidpointRounding.AwayFromZero)
        };
    }

    public static WorkloadHistoryCompareResponse ToCompare(WorkloadHistory left, WorkloadHistory right)
    {
        var leftResponse = ToResponse(left);
        var rightResponse = ToResponse(right);

        return new WorkloadHistoryCompareResponse
        {
            Left = leftResponse,
            Right = rightResponse,
            AllocatedHoursDelta = rightResponse.AllocatedHours - leftResponse.AllocatedHours,
            CapacityHoursDelta = rightResponse.CapacityHours - leftResponse.CapacityHours,
            WorkloadPercentageDelta = rightResponse.WorkloadPercentage - leftResponse.WorkloadPercentage,
            WorkingDaysDelta = rightResponse.WorkingDays - leftResponse.WorkingDays,
            HolidayDaysDelta = rightResponse.HolidayDays - leftResponse.HolidayDays,
            AvailableDaysDelta = rightResponse.AvailableDays - leftResponse.AvailableDays
        };
    }

    public static WorkloadHistoryTrendResponse ToTrend(
        IReadOnlyList<WorkloadHistory> histories,
        WorkloadHistoryQueryParameters parameters)
    {
        var ordered = histories
            .OrderBy(item => item.CalculationDate)
            .ThenBy(item => item.PeriodStart)
            .ToList();

        return new WorkloadHistoryTrendResponse
        {
            CompanyId = parameters.CompanyId,
            ExecutionResourceId = parameters.ExecutionResourceId,
            PointCount = ordered.Count,
            Points = ordered.Select(item => new WorkloadHistoryTrendPointResponse
            {
                CalculationDate = item.CalculationDate,
                PeriodStart = item.PeriodStart,
                PeriodEnd = item.PeriodEnd,
                AllocatedHours = item.AllocatedHours,
                CapacityHours = item.CapacityHours,
                WorkloadPercentage = item.WorkloadPercentage,
                CalculationVersion = item.CalculationVersion
            }).ToList()
        };
    }

    private static Snapshot DeserializeSnapshot(string operationalInputsJson)
    {
        try
        {
            var snapshot = JsonSerializer.Deserialize<Snapshot>(operationalInputsJson, JsonOptions);
            return snapshot ?? new Snapshot();
        }
        catch (JsonException)
        {
            return new Snapshot();
        }
    }

    private sealed class Snapshot
    {
        public string ExecutionResourceCode { get; set; } = string.Empty;

        public string ExecutionResourceName { get; set; } = string.Empty;

        public int AssignmentCount { get; set; }

        public decimal AverageHoursPerAssignment { get; set; }

        public List<WorkloadHistoryAssignmentSnapshotResponse> AssignmentDistribution { get; set; } = [];
    }
}

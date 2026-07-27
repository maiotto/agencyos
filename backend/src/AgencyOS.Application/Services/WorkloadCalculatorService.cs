using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class WorkloadCalculatorService : IWorkloadCalculatorService
{
    /// <summary>
    /// Fallback CompanyId when Resource Availability / Working Calendar is not configured
    /// for a resource (matches Release 1.1 administrative default).
    /// </summary>
    public static readonly Guid DefaultCompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IResourceAvailabilityRepository _resourceAvailabilityRepository;
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IWorkloadHistoryService _workloadHistoryService;
    private readonly ILogger<WorkloadCalculatorService> _logger;

    public WorkloadCalculatorService(
        IExecutionResourceRepository executionResourceRepository,
        IAssignmentRepository assignmentRepository,
        IResourceAvailabilityRepository resourceAvailabilityRepository,
        IWorkingCalendarRepository workingCalendarRepository,
        IHolidayRepository holidayRepository,
        IWorkloadHistoryService workloadHistoryService,
        ILogger<WorkloadCalculatorService> logger)
    {
        _executionResourceRepository = executionResourceRepository;
        _assignmentRepository = assignmentRepository;
        _resourceAvailabilityRepository = resourceAvailabilityRepository;
        _workingCalendarRepository = workingCalendarRepository;
        _holidayRepository = holidayRepository;
        _workloadHistoryService = workloadHistoryService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkloadResponse>> GetAllAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var (results, historyPayload) = await CalculateAllWithHistoryPayloadAsync(
                parameters,
                cancellationToken);

            await _workloadHistoryService.PersistCompletedCalculationsAsync(
                historyPayload,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                results.Count,
                stopwatch.ElapsedMilliseconds);

            return results;
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload calculation failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<WorkloadResponse> GetByResourceIdAsync(
        Guid resourceId,
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
            resourceId,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var resource = await GetActiveResourceOrThrowAsync(resourceId, cancellationToken);
            var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                executionResourceId: resourceId,
                excludeMissionId: parameters.ExcludeMissionId,
                cancellationToken: cancellationToken);

            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var workload = BuildWorkloadResponse(resource, parameters, periodDays, assignments);
            var historyModel = await BuildHistoryPersistModelAsync(
                resource,
                workload,
                parameters,
                periodDays,
                cancellationToken);

            await _workloadHistoryService.PersistCompletedCalculationAsync(
                historyModel,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for execution resource {ExecutionResourceId} in {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            return workload;
        }
        catch (Exception ex) when (ex is not NotFoundException and not BusinessRuleException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload calculation failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<WorkloadSummaryResponse> GetSummaryAsync(
        WorkloadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Workload calculation started for summary from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            // Summary recalculates without persisting history (US-107).
            var activeResources = await GetActiveResourcesAsync(cancellationToken);
            var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                excludeMissionId: parameters.ExcludeMissionId,
                cancellationToken: cancellationToken);

            var assignmentsByResource = GroupAssignments(assignments);
            var periodDays = CapacityCalculation.GetInclusivePeriodDays(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate);

            var totalPlannedHours = 0m;
            var totalAssignmentCount = 0;
            var totalCapacityHours = 0m;

            foreach (var resource in activeResources)
            {
                var resourceAssignments = assignmentsByResource.GetValueOrDefault(resource.Id, Array.Empty<Assignment>());
                totalPlannedHours += resourceAssignments.Sum(assignment => assignment.PlannedHours);
                totalAssignmentCount += resourceAssignments.Count;
                totalCapacityHours += CapacityCalculation.ProrateWeeklyHours(
                    resource.CapacityHoursPerWeek,
                    periodDays);
            }

            var summary = new WorkloadSummaryResponse
            {
                PeriodStartDate = parameters.PeriodStartDate,
                PeriodEndDate = parameters.PeriodEndDate,
                ActiveResourceCount = activeResources.Count,
                TotalPlannedHours = totalPlannedHours,
                TotalAssignmentCount = totalAssignmentCount,
                AverageHoursPerAssignment = WorkloadCalculation.CalculateAverageHoursPerAssignment(
                    totalPlannedHours,
                    totalAssignmentCount),
                OverallWorkloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(
                    totalCapacityHours,
                    totalPlannedHours)
            };

            stopwatch.Stop();

            _logger.LogInformation(
                "Workload calculation completed for summary across {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                summary.ActiveResourceCount,
                stopwatch.ElapsedMilliseconds);

            return summary;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Workload summary calculation failed after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private async Task<(
            IReadOnlyList<WorkloadResponse> Results,
            IReadOnlyList<WorkloadHistoryPersistModel> HistoryPayload)>
        CalculateAllWithHistoryPayloadAsync(
            WorkloadQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var activeResources = await GetActiveResourcesAsync(cancellationToken);
        var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
            parameters.PeriodStartDate,
            parameters.PeriodEndDate,
            excludeMissionId: parameters.ExcludeMissionId,
            cancellationToken: cancellationToken);

        var assignmentsByResource = GroupAssignments(assignments);
        var periodDays = CapacityCalculation.GetInclusivePeriodDays(
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        var results = new List<WorkloadResponse>();
        var historyPayload = new List<WorkloadHistoryPersistModel>();

        foreach (var resource in activeResources
                     .OrderBy(item => item.Name)
                     .ThenBy(item => item.Code))
        {
            var resourceAssignments = assignmentsByResource.GetValueOrDefault(
                resource.Id,
                Array.Empty<Assignment>());
            var workload = BuildWorkloadResponse(resource, parameters, periodDays, resourceAssignments);
            var historyModel = await BuildHistoryPersistModelAsync(
                resource,
                workload,
                parameters,
                periodDays,
                cancellationToken);

            results.Add(workload);
            historyPayload.Add(historyModel);
        }

        return (results, historyPayload);
    }

    private async Task<WorkloadHistoryPersistModel> BuildHistoryPersistModelAsync(
        ExecutionResource resource,
        WorkloadResponse workload,
        WorkloadQueryParameters parameters,
        int periodDays,
        CancellationToken cancellationToken)
    {
        var capacityHours = CapacityCalculation.ProrateWeeklyHours(
            resource.CapacityHoursPerWeek,
            periodDays);

        var enrichment = await ResolveOperationalEnrichmentAsync(
            resource.Id,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate,
            cancellationToken);

        return new WorkloadHistoryPersistModel
        {
            Workload = workload,
            CompanyId = enrichment.CompanyId,
            CapacityHours = capacityHours,
            WorkingDays = enrichment.WorkingDays,
            HolidayDays = enrichment.HolidayDays,
            AvailableDays = enrichment.AvailableDays
        };
    }

    private async Task<OperationalEnrichment> ResolveOperationalEnrichmentAsync(
        Guid executionResourceId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        var availabilities = await _resourceAvailabilityRepository.GetActiveCoveringPeriodAsync(
            executionResourceId,
            periodStart,
            periodEnd,
            cancellationToken);

        if (availabilities.Count == 0)
        {
            var weekdayCount = CountWeekdays(periodStart, periodEnd);
            return new OperationalEnrichment(DefaultCompanyId, weekdayCount, 0, weekdayCount);
        }

        var calendarIds = availabilities.Select(item => item.WorkingCalendarId).Distinct().ToList();
        var calendars = new Dictionary<Guid, WorkingCalendar>();
        foreach (var calendarId in calendarIds)
        {
            var calendar = await _workingCalendarRepository.GetByIdAsync(calendarId, cancellationToken);
            if (calendar is not null)
            {
                calendars[calendarId] = calendar;
            }
        }

        if (calendars.Count == 0)
        {
            var weekdayCount = CountWeekdays(periodStart, periodEnd);
            return new OperationalEnrichment(DefaultCompanyId, weekdayCount, 0, weekdayCount);
        }

        var companyId = calendars.Values.Select(calendar => calendar.CompanyId).First();
        var holidaysByCompany = new Dictionary<Guid, IReadOnlyList<Holiday>>();
        foreach (var company in calendars.Values.Select(calendar => calendar.CompanyId).Distinct())
        {
            holidaysByCompany[company] = await _holidayRepository.GetActiveForCompanyInRangeAsync(
                company,
                periodStart,
                periodEnd,
                cancellationToken);
        }

        var workingDays = 0;
        var holidayDays = 0;
        var availableDays = 0;

        foreach (var date in CapacityCalculation.EnumerateDates(periodStart, periodEnd))
        {
            var availability = availabilities.FirstOrDefault(item => item.CoversDate(date));
            if (availability is null
                || !calendars.TryGetValue(availability.WorkingCalendarId, out var calendar))
            {
                continue;
            }

            var holidays = holidaysByCompany.GetValueOrDefault(calendar.CompanyId, []);
            var isHoliday = holidays.Any(holiday => holiday.OccursOn(date));
            var isCalendarWorkingWeekday = calendar.IsWorkingDay(date);

            if (isCalendarWorkingWeekday)
            {
                workingDays++;
            }

            if (isHoliday && isCalendarWorkingWeekday)
            {
                holidayDays++;
            }

            if (isCalendarWorkingWeekday && !isHoliday && availability.IsAvailableOn(date))
            {
                availableDays++;
            }
        }

        return new OperationalEnrichment(companyId, workingDays, holidayDays, availableDays);
    }

    private static int CountWeekdays(DateOnly periodStart, DateOnly periodEnd)
    {
        var count = 0;
        foreach (var date in CapacityCalculation.EnumerateDates(periodStart, periodEnd))
        {
            if (date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                count++;
            }
        }

        return count;
    }

    private async Task<IReadOnlyList<ExecutionResource>> GetActiveResourcesAsync(
        CancellationToken cancellationToken)
    {
        var parameters = new ExecutionResourceQueryParameters
        {
            Status = ExecutionResourceStatus.Active
        };

        return await _executionResourceRepository.GetAllAsync(parameters, cancellationToken);
    }

    private async Task<ExecutionResource> GetActiveResourceOrThrowAsync(
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(resourceId, cancellationToken);

        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{resourceId}' was not found.");
        }

        if (!ExecutionResourceStatus.CanReceiveAssignments(resource.Status))
        {
            throw new NotFoundException(
                $"Workload is calculated only for Active Execution Resources. Resource '{resourceId}' is not active.");
        }

        return resource;
    }

    private static Dictionary<Guid, IReadOnlyList<Assignment>> GroupAssignments(
        IReadOnlyList<Assignment> assignments)
    {
        return assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Assignment>)group.ToList());
    }

    private static WorkloadResponse BuildWorkloadResponse(
        ExecutionResource resource,
        WorkloadQueryParameters parameters,
        int periodDays,
        IReadOnlyList<Assignment> assignments)
    {
        var orderedAssignments = assignments
            .OrderBy(assignment => assignment.PlannedStartDate)
            .ThenBy(assignment => assignment.Id)
            .ToList();

        var totalPlannedHours = orderedAssignments.Sum(assignment => assignment.PlannedHours);
        var assignmentCount = orderedAssignments.Count;
        var totalCapacityHours = CapacityCalculation.ProrateWeeklyHours(
            resource.CapacityHoursPerWeek,
            periodDays);

        return new WorkloadResponse
        {
            ExecutionResourceId = resource.Id,
            ExecutionResourceCode = resource.Code,
            ExecutionResourceName = resource.Name,
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            TotalPlannedHours = totalPlannedHours,
            AssignmentCount = assignmentCount,
            AverageHoursPerAssignment = WorkloadCalculation.CalculateAverageHoursPerAssignment(
                totalPlannedHours,
                assignmentCount),
            WorkloadPercentage = WorkloadCalculation.CalculateWorkloadPercentage(
                totalCapacityHours,
                totalPlannedHours),
            AssignmentDistribution = orderedAssignments
                .Select(MapAssignmentDistributionItem)
                .ToList()
        };
    }

    private static WorkloadAssignmentDistributionItem MapAssignmentDistributionItem(Assignment assignment)
    {
        return new WorkloadAssignmentDistributionItem
        {
            AssignmentId = assignment.Id,
            TaskId = assignment.TaskId,
            AssignmentRole = assignment.AssignmentRole,
            PlannedHours = assignment.PlannedHours,
            PlannedStartDate = assignment.PlannedStartDate,
            PlannedEndDate = assignment.PlannedEndDate,
            Status = assignment.Status
        };
    }

    private sealed record OperationalEnrichment(
        Guid CompanyId,
        int WorkingDays,
        int HolidayDays,
        int AvailableDays);
}

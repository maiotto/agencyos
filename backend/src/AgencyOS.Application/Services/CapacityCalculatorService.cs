using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AgencyOS.Application.Services;

public class CapacityCalculatorService : ICapacityCalculatorService
{
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IResourceAvailabilityRepository _resourceAvailabilityRepository;
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly IWorkingHoursRepository _workingHoursRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly ICapacityHistoryService _capacityHistoryService;
    private readonly ILogger<CapacityCalculatorService> _logger;

    public CapacityCalculatorService(
        IExecutionResourceRepository executionResourceRepository,
        IAssignmentRepository assignmentRepository,
        IResourceAvailabilityRepository resourceAvailabilityRepository,
        IWorkingCalendarRepository workingCalendarRepository,
        IWorkingHoursRepository workingHoursRepository,
        IHolidayRepository holidayRepository,
        ICapacityHistoryService capacityHistoryService,
        ILogger<CapacityCalculatorService> logger)
    {
        _executionResourceRepository = executionResourceRepository;
        _assignmentRepository = assignmentRepository;
        _resourceAvailabilityRepository = resourceAvailabilityRepository;
        _workingCalendarRepository = workingCalendarRepository;
        _workingHoursRepository = workingHoursRepository;
        _holidayRepository = holidayRepository;
        _capacityHistoryService = capacityHistoryService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CapacityResponse>> GetAllAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Capacity calculation started for all active execution resources from {PeriodStartDate} to {PeriodEndDate}",
            parameters.PeriodStartDate,
            parameters.PeriodEndDate);

        try
        {
            var (results, historyPayload) = await CalculateAllWithHistoryPayloadAsync(
                parameters,
                cancellationToken);

            await _capacityHistoryService.PersistCompletedCalculationsAsync(
                historyPayload,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Capacity calculation completed for {ResourceCount} active execution resources in {ElapsedMilliseconds} ms",
                results.Count,
                stopwatch.ElapsedMilliseconds);

            return results;
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Capacity calculation failed for all active execution resources after {ElapsedMilliseconds} ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<CapacityResponse> GetByResourceIdAsync(
        Guid resourceId,
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Capacity calculation started for execution resource {ExecutionResourceId} from {PeriodStartDate} to {PeriodEndDate}",
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

            var availabilities = await _resourceAvailabilityRepository.GetActiveCoveringPeriodAsync(
                resourceId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                cancellationToken);

            var context = await BuildOperationalContextAsync(
                availabilities,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                cancellationToken);

            var allocatedHours = assignments.Sum(a => a.PlannedHours);
            var result = BuildCapacityResponse(resource, parameters, allocatedHours, availabilities, context);
            var companyId = ResolveCompanyId(availabilities, context);

            await _capacityHistoryService.PersistCompletedCalculationAsync(
                result,
                companyId,
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Capacity calculation completed for execution resource {ExecutionResourceId} in {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException and not BusinessRuleException)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Capacity calculation failed for execution resource {ExecutionResourceId} after {ElapsedMilliseconds} ms",
                resourceId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<CapacitySummaryResponse> GetSummaryAsync(
        CapacityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // Summary uses the same engine without persisting history (GetAll/GetByResource persist).
        var (capacities, _) = await CalculateAllWithHistoryPayloadAsync(parameters, cancellationToken);

        var totalCapacityHours = capacities.Sum(c => c.TotalCapacityHours);
        var totalAllocatedHours = capacities.Sum(c => c.AllocatedHours);
        var totalAvailableHours = capacities.Sum(c => c.AvailableHours);
        var totalRemainingCapacityHours = capacities.Sum(c => c.RemainingCapacityHours);

        return new CapacitySummaryResponse
        {
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            ActiveResourceCount = capacities.Count,
            TotalCapacityHours = totalCapacityHours,
            TotalAllocatedHours = totalAllocatedHours,
            TotalAvailableHours = totalAvailableHours,
            OverallUtilizationPercentage = CapacityCalculation.CalculateUtilizationPercentage(
                totalCapacityHours,
                totalAllocatedHours),
            TotalRemainingCapacityHours = totalRemainingCapacityHours
        };
    }

    private async Task<(
            IReadOnlyList<CapacityResponse> Results,
            IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)> HistoryPayload)>
        CalculateAllWithHistoryPayloadAsync(
            CapacityQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var activeResources = await GetActiveResourcesAsync(cancellationToken);
        var assignments = await _assignmentRepository.GetForCapacityCalculationAsync(
            parameters.PeriodStartDate,
            parameters.PeriodEndDate,
            excludeMissionId: parameters.ExcludeMissionId,
            cancellationToken: cancellationToken);

        var allocatedHoursByResource = GroupAllocatedHours(assignments);
        var resourceIds = activeResources.Select(resource => resource.Id).ToList();
        var availabilities = await _resourceAvailabilityRepository.GetActiveCoveringPeriodForResourcesAsync(
            resourceIds,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate,
            cancellationToken);

        var context = await BuildOperationalContextAsync(
            availabilities,
            parameters.PeriodStartDate,
            parameters.PeriodEndDate,
            cancellationToken);

        var historyPayload = new List<(CapacityResponse Capacity, Guid CompanyId)>();
        var results = new List<CapacityResponse>();

        foreach (var resource in activeResources
                     .OrderBy(item => item.Name)
                     .ThenBy(item => item.Code))
        {
            var resourceAvailabilities = availabilities
                .Where(item => item.ExecutionResourceId == resource.Id)
                .ToList();
            var capacity = BuildCapacityResponse(
                resource,
                parameters,
                allocatedHoursByResource.GetValueOrDefault(resource.Id, 0),
                resourceAvailabilities,
                context);
            var companyId = ResolveCompanyId(resourceAvailabilities, context);

            results.Add(capacity);
            historyPayload.Add((capacity, companyId));
        }

        return (results, historyPayload);
    }

    private static Guid ResolveCompanyId(
        IReadOnlyList<ResourceAvailability> availabilities,
        OperationalContext context)
    {
        foreach (var availability in availabilities)
        {
            if (context.Calendars.TryGetValue(availability.WorkingCalendarId, out var calendar))
            {
                return calendar.CompanyId;
            }
        }

        throw new BusinessRuleException(
            "Unable to resolve CompanyId for capacity history because no Working Calendar was loaded for the resource availability configuration.");
    }

    private async Task<OperationalContext> BuildOperationalContextAsync(
        IReadOnlyList<ResourceAvailability> availabilities,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken)
    {
        var calendarIds = (availabilities ?? Array.Empty<ResourceAvailability>())
            .Select(item => item.WorkingCalendarId)
            .Distinct()
            .ToList();
        var workingHoursIds = (availabilities ?? Array.Empty<ResourceAvailability>())
            .Select(item => item.WorkingHoursId)
            .Distinct()
            .ToList();

        var calendars = new Dictionary<Guid, WorkingCalendar>();
        foreach (var calendarId in calendarIds)
        {
            var calendar = await _workingCalendarRepository.GetByIdAsync(calendarId, cancellationToken);
            if (calendar is not null)
            {
                calendars[calendarId] = calendar;
            }
        }

        var workingHoursById = new Dictionary<Guid, WorkingHours>();
        foreach (var workingHoursId in workingHoursIds)
        {
            var workingHours = await _workingHoursRepository.GetByIdAsync(workingHoursId, cancellationToken);
            if (workingHours is not null)
            {
                workingHoursById[workingHoursId] = workingHours;
            }
        }

        var companyIds = calendars.Values.Select(calendar => calendar.CompanyId).Distinct().ToList();
        var holidaysByCompany = new Dictionary<Guid, IReadOnlyList<Holiday>>();

        foreach (var companyId in companyIds)
        {
            holidaysByCompany[companyId] = await _holidayRepository.GetActiveForCompanyInRangeAsync(
                companyId,
                periodStartDate,
                periodEndDate,
                cancellationToken);
        }

        return new OperationalContext(calendars, workingHoursById, holidaysByCompany);
    }

    private static CapacityResponse BuildCapacityResponse(
        ExecutionResource resource,
        CapacityQueryParameters parameters,
        decimal allocatedHours,
        IReadOnlyList<ResourceAvailability> availabilities,
        OperationalContext context)
    {
        if (availabilities.Count == 0)
        {
            throw new BusinessRuleException(
                $"No active Resource Availability configuration exists for execution resource '{resource.Code}' in the planning period. Capacity cannot fall back to Monday–Friday defaults.");
        }

        var dayBreakdowns = new List<CapacityDayBreakdownResponse>();
        var operationalDayCount = 0;
        var holidayImpactDayCount = 0;
        var resourceAvailabilityExcludedDayCount = 0;
        var configuredWorkingHoursTotal = 0m;
        var totalCapacityHours = 0m;

        foreach (var date in CapacityCalculation.EnumerateDates(parameters.PeriodStartDate, parameters.PeriodEndDate))
        {
            var availability = availabilities.FirstOrDefault(item => item.CoversDate(date));
            if (availability is null)
            {
                throw new BusinessRuleException(
                    $"No active Resource Availability covers {date:yyyy-MM-dd} for execution resource '{resource.Code}'. Capacity cannot fall back to Monday–Friday defaults.");
            }

            if (!context.Calendars.TryGetValue(availability.WorkingCalendarId, out var calendar)
                || !calendar.IsActive
                || !calendar.CoversDate(date))
            {
                throw new BusinessRuleException(
                    $"No active Working Calendar covers {date:yyyy-MM-dd} for execution resource '{resource.Code}'. Capacity cannot fall back to Monday–Friday defaults.");
            }

            if (!context.WorkingHours.TryGetValue(availability.WorkingHoursId, out var workingHours)
                || !workingHours.IsActive
                || !workingHours.CoversDate(date))
            {
                throw new BusinessRuleException(
                    $"No active Working Hours configuration covers {date:yyyy-MM-dd} for execution resource '{resource.Code}'. Capacity cannot fall back to Monday–Friday defaults.");
            }

            var holidays = context.HolidaysByCompany.GetValueOrDefault(calendar.CompanyId, []);
            var holidaysOnDate = holidays.Where(holiday => holiday.OccursOn(date)).ToList();
            var isHoliday = holidaysOnDate.Count > 0;
            var isCalendarWorkingWeekday = calendar.IsWorkingDay(date);
            var isOperationalCalendarDay = calendar.IsWorkingDay(date, holidays);
            var isResourceAvailable = availability.IsAvailableOn(date);
            var scheduleDay = workingHours.GetDay(date);
            var dayOverride = availability.GetOverride(date);

            string? exclusionReason = null;
            decimal plannedHours = 0;

            if (!isCalendarWorkingWeekday)
            {
                exclusionReason = "NonWorkingWeekday";
            }
            else if (isHoliday)
            {
                exclusionReason = "Holiday";
                holidayImpactDayCount++;
            }
            else if (!isResourceAvailable)
            {
                exclusionReason = "ResourceUnavailable";
                resourceAvailabilityExcludedDayCount++;
            }
            else if (scheduleDay is null || !scheduleDay.Enabled)
            {
                exclusionReason = "WorkingHoursDisabled";
            }
            else
            {
                var resolved = CapacityCalculation.ResolvePlannedNetHours(scheduleDay, dayOverride);
                if (resolved is null || resolved <= 0)
                {
                    exclusionReason = "NoPlannedHours";
                }
                else
                {
                    plannedHours = resolved.Value;
                    configuredWorkingHoursTotal += scheduleDay.NetHours ?? plannedHours;
                    operationalDayCount++;
                    totalCapacityHours += plannedHours;
                }
            }

            dayBreakdowns.Add(new CapacityDayBreakdownResponse
            {
                Date = date,
                IsOperationalDay = plannedHours > 0,
                IsCalendarWorkingWeekday = isCalendarWorkingWeekday,
                IsHoliday = isHoliday,
                IsResourceAvailable = isResourceAvailable,
                PlannedCapacityHours = plannedHours,
                ResourceAvailabilityId = availability.Id,
                WorkingCalendarId = calendar.Id,
                WorkingHoursId = workingHours.Id,
                ExclusionReason = exclusionReason
            });

            _ = isOperationalCalendarDay;
        }

        var availableHours = CapacityCalculation.CalculateAvailableHours(totalCapacityHours, allocatedHours);

        return new CapacityResponse
        {
            ExecutionResourceId = resource.Id,
            ExecutionResourceCode = resource.Code,
            ExecutionResourceName = resource.Name,
            PeriodStartDate = parameters.PeriodStartDate,
            PeriodEndDate = parameters.PeriodEndDate,
            OperationalDayCount = operationalDayCount,
            HolidayImpactDayCount = holidayImpactDayCount,
            ResourceAvailabilityExcludedDayCount = resourceAvailabilityExcludedDayCount,
            ConfiguredWorkingHoursTotal = configuredWorkingHoursTotal,
            TotalCapacityHours = totalCapacityHours,
            AllocatedHours = allocatedHours,
            AvailableHours = availableHours,
            UtilizationPercentage = CapacityCalculation.CalculateUtilizationPercentage(
                totalCapacityHours,
                allocatedHours),
            RemainingCapacityHours = availableHours,
            OperationalDays = dayBreakdowns
        };
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
                $"Capacity is calculated only for Active Execution Resources. Resource '{resourceId}' is not active.");
        }

        return resource;
    }

    private static Dictionary<Guid, decimal> GroupAllocatedHours(IReadOnlyList<Assignment> assignments)
    {
        return assignments
            .GroupBy(assignment => assignment.ExecutionResourceId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(assignment => assignment.PlannedHours));
    }

    private sealed class OperationalContext(
        IReadOnlyDictionary<Guid, WorkingCalendar> calendars,
        IReadOnlyDictionary<Guid, WorkingHours> workingHours,
        IReadOnlyDictionary<Guid, IReadOnlyList<Holiday>> holidaysByCompany)
    {
        public IReadOnlyDictionary<Guid, WorkingCalendar> Calendars { get; } = calendars;

        public IReadOnlyDictionary<Guid, WorkingHours> WorkingHours { get; } = workingHours;

        public IReadOnlyDictionary<Guid, IReadOnlyList<Holiday>> HolidaysByCompany { get; } = holidaysByCompany;
    }
}

using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ResourceAvailabilityService : IResourceAvailabilityService
{
    private readonly IResourceAvailabilityRepository _resourceAvailabilityRepository;
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly IWorkingHoursRepository _workingHoursRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly ILogger<ResourceAvailabilityService> _logger;

    public ResourceAvailabilityService(
        IResourceAvailabilityRepository resourceAvailabilityRepository,
        IExecutionResourceRepository executionResourceRepository,
        IWorkingCalendarRepository workingCalendarRepository,
        IWorkingHoursRepository workingHoursRepository,
        IHolidayRepository holidayRepository,
        ILogger<ResourceAvailabilityService> logger)
    {
        _resourceAvailabilityRepository = resourceAvailabilityRepository;
        _executionResourceRepository = executionResourceRepository;
        _workingCalendarRepository = workingCalendarRepository;
        _workingHoursRepository = workingHoursRepository;
        _holidayRepository = holidayRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ResourceAvailabilityResponse>> GetAllAsync(
        ResourceAvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _resourceAvailabilityRepository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<ResourceAvailabilityResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var availability = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(availability);
    }

    public async Task<ResourceAvailabilityResponse> CreateAsync(
        CreateResourceAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssociationsExistAsync(
            request.ExecutionResourceId,
            request.WorkingCalendarId,
            request.WorkingHoursId,
            cancellationToken);

        ResourceAvailability availability;

        try
        {
            availability = ResourceAvailability.Create(
                request.ExecutionResourceId,
                request.WorkingCalendarId,
                request.WorkingHoursId,
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                MapWeekDayDefinitions(request.WeeklyAvailability),
                MapOverrideDefinitions(request.DailyOverrides),
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _resourceAvailabilityRepository.AddAsync(availability, cancellationToken);

        _logger.LogInformation(
            "Resource Availability Created: {ResourceAvailabilityId} ({Name}) Resource={ExecutionResourceId}",
            created.Id,
            created.Name,
            created.ExecutionResourceId);

        return MapToResponse(created);
    }

    public async Task<ResourceAvailabilityResponse> UpdateAsync(
        Guid id,
        UpdateResourceAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var availability = await GetOrThrowAsync(id, cancellationToken);
        await EnsureAssociationsExistAsync(
            availability.ExecutionResourceId,
            request.WorkingCalendarId,
            request.WorkingHoursId,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        try
        {
            availability.Reconfigure(
                request.WorkingCalendarId,
                request.WorkingHoursId,
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                MapWeekDayDefinitions(request.WeeklyAvailability),
                MapOverrideDefinitions(request.DailyOverrides),
                today,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (availability.IsActive)
        {
            await EnsureNoActiveOverlapAsync(
                availability.ExecutionResourceId,
                availability.EffectiveFrom,
                availability.EffectiveTo,
                availability.Id,
                cancellationToken);
        }

        var updated = await _resourceAvailabilityRepository.UpdateAsync(availability, cancellationToken);

        _logger.LogInformation(
            "Resource Availability Updated: {ResourceAvailabilityId} ({Name})",
            updated.Id,
            updated.Name);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var availability = await GetOrThrowAsync(id, cancellationToken);

        if (availability.IsActive)
        {
            return;
        }

        await EnsureNoActiveOverlapAsync(
            availability.ExecutionResourceId,
            availability.EffectiveFrom,
            availability.EffectiveTo,
            availability.Id,
            cancellationToken);

        availability.Activate(DateTimeOffset.UtcNow);
        await _resourceAvailabilityRepository.UpdateAsync(availability, cancellationToken);

        _logger.LogInformation(
            "Resource Availability Activated: {ResourceAvailabilityId} ({Name})",
            availability.Id,
            availability.Name);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var availability = await GetOrThrowAsync(id, cancellationToken);

        if (availability.IsInactive)
        {
            return;
        }

        availability.Deactivate(DateTimeOffset.UtcNow);
        await _resourceAvailabilityRepository.UpdateAsync(availability, cancellationToken);

        _logger.LogInformation(
            "Resource Availability Deactivated: {ResourceAvailabilityId} ({Name})",
            availability.Id,
            availability.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var availability = await GetOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        try
        {
            availability.EnsureCanDelete(today);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _resourceAvailabilityRepository.DeleteAsync(availability, cancellationToken);

        _logger.LogInformation(
            "Resource Availability Deleted: {ResourceAvailabilityId} ({Name})",
            availability.Id,
            availability.Name);
    }

    public async Task<OperationalResourceAvailabilityResponse> GetOperationalAvailabilityAsync(
        Guid executionResourceId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(executionResourceId, cancellationToken);
        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{executionResourceId}' was not found.");
        }

        var availability = await _resourceAvailabilityRepository.GetActiveCoveringDateAsync(
            executionResourceId,
            date,
            cancellationToken);

        if (availability is null)
        {
            throw new BusinessRuleException(
                $"No active Resource Availability configuration exists for execution resource '{executionResourceId}' on {date:yyyy-MM-dd}. Capacity and availability cannot fall back to Monday–Friday defaults.");
        }

        var calendar = await _workingCalendarRepository.GetByIdAsync(
            availability.WorkingCalendarId,
            cancellationToken);

        if (calendar is null || !calendar.IsActive || !calendar.CoversDate(date))
        {
            throw new BusinessRuleException(
                $"No active Working Calendar covers {date:yyyy-MM-dd} for Resource Availability '{availability.Id}'.");
        }

        var workingHours = await _workingHoursRepository.GetByIdAsync(
            availability.WorkingHoursId,
            cancellationToken);

        if (workingHours is null || !workingHours.IsActive || !workingHours.CoversDate(date))
        {
            throw new BusinessRuleException(
                $"No active Working Hours configuration covers {date:yyyy-MM-dd} for Resource Availability '{availability.Id}'.");
        }

        var holidays = await _holidayRepository.GetActiveForCompanyOnDateAsync(
            calendar.CompanyId,
            date,
            cancellationToken);

        var isHoliday = holidays.Count > 0;
        var isOperationalWorkingDay = calendar.IsWorkingDay(date, holidays);
        var isResourceAvailable = availability.IsAvailableOn(date) && isOperationalWorkingDay;

        var day = workingHours.GetDay(date);
        WorkingHoursDayResponse? schedule = null;
        decimal? plannedNetHours = null;

        if (day is not null)
        {
            schedule = new WorkingHoursDayResponse
            {
                DayOfWeek = day.DayOfWeek,
                Enabled = day.Enabled,
                StartTime = day.StartTime,
                EndTime = day.EndTime,
                BreakStart = day.BreakStart,
                BreakEnd = day.BreakEnd,
                NetHours = day.NetHours
            };
        }

        var dayOverride = availability.GetOverride(date);
        ResourceAvailabilityDayOverrideResponse? overrideResponse = null;

        if (dayOverride is not null)
        {
            overrideResponse = new ResourceAvailabilityDayOverrideResponse
            {
                OverrideDate = dayOverride.OverrideDate,
                Available = dayOverride.Available,
                StartTime = dayOverride.StartTime,
                EndTime = dayOverride.EndTime,
                Notes = dayOverride.Notes
            };
        }

        if (isResourceAvailable)
        {
            plannedNetHours = CapacityCalculation.ResolvePlannedNetHours(day, dayOverride);
        }

        return new OperationalResourceAvailabilityResponse
        {
            ExecutionResourceId = executionResourceId,
            Date = date,
            IsResourceAvailable = isResourceAvailable,
            IsOperationalWorkingDay = isOperationalWorkingDay,
            IsHoliday = isHoliday,
            ResourceAvailabilityId = availability.Id,
            WorkingCalendarId = calendar.Id,
            WorkingHoursId = workingHours.Id,
            Schedule = schedule,
            PlannedNetHours = plannedNetHours,
            DayOverride = overrideResponse,
            Holidays = holidays.Select(HolidayService.MapToResponse).ToList()
        };
    }

    private async Task EnsureAssociationsExistAsync(
        Guid executionResourceId,
        Guid workingCalendarId,
        Guid workingHoursId,
        CancellationToken cancellationToken)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(executionResourceId, cancellationToken);
        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{executionResourceId}' was not found.");
        }

        var calendar = await _workingCalendarRepository.GetByIdAsync(workingCalendarId, cancellationToken);
        if (calendar is null)
        {
            throw new NotFoundException($"Working Calendar with id '{workingCalendarId}' was not found.");
        }

        var workingHours = await _workingHoursRepository.GetByIdAsync(workingHoursId, cancellationToken);
        if (workingHours is null)
        {
            throw new NotFoundException($"Working Hours with id '{workingHoursId}' was not found.");
        }

        if (workingHours.WorkingCalendarId != workingCalendarId)
        {
            throw new BusinessRuleException(
                "Working Hours configuration must belong to the selected Working Calendar.");
        }
    }

    private async Task EnsureNoActiveOverlapAsync(
        Guid executionResourceId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var overlapping = await _resourceAvailabilityRepository.GetActiveOverlappingAsync(
            executionResourceId,
            effectiveFrom,
            effectiveTo,
            excludeId,
            cancellationToken);

        if (overlapping.Count > 0)
        {
            throw new ConflictException(
                "Only one Active Availability configuration may exist for the same Resource during the same effective period.");
        }
    }

    private async Task<ResourceAvailability> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var availability = await _resourceAvailabilityRepository.GetByIdAsync(id, cancellationToken);

        if (availability is null)
        {
            throw new NotFoundException($"Resource Availability with id '{id}' was not found.");
        }

        return availability;
    }

    private static IReadOnlyList<ResourceAvailabilityWeekDayDefinition> MapWeekDayDefinitions(
        IReadOnlyList<ResourceAvailabilityWeekDayRequest> days) =>
        days.Select(day => new ResourceAvailabilityWeekDayDefinition
        {
            DayOfWeek = day.DayOfWeek,
            Enabled = day.Enabled
        }).ToList();

    private static IReadOnlyList<ResourceAvailabilityDayOverrideDefinition> MapOverrideDefinitions(
        IReadOnlyList<ResourceAvailabilityDayOverrideRequest> overrides) =>
        overrides.Select(day => new ResourceAvailabilityDayOverrideDefinition
        {
            OverrideDate = day.OverrideDate,
            Available = day.Available,
            StartTime = day.StartTime,
            EndTime = day.EndTime,
            Notes = day.Notes
        }).ToList();

    internal static ResourceAvailabilityResponse MapToResponse(ResourceAvailability availability)
    {
        return new ResourceAvailabilityResponse
        {
            Id = availability.Id,
            ExecutionResourceId = availability.ExecutionResourceId,
            WorkingCalendarId = availability.WorkingCalendarId,
            WorkingHoursId = availability.WorkingHoursId,
            Name = availability.Name,
            Status = availability.Status,
            EffectiveFrom = availability.EffectiveFrom,
            EffectiveTo = availability.EffectiveTo,
            WeeklyAvailability = availability.WeeklyAvailability
                .OrderBy(day => Array.FindIndex(
                    WorkingDayNames.Ordered.ToArray(),
                    ordered => string.Equals(ordered, day.DayOfWeek, StringComparison.OrdinalIgnoreCase)))
                .Select(day => new ResourceAvailabilityWeekDayResponse
                {
                    DayOfWeek = day.DayOfWeek,
                    Enabled = day.Enabled
                })
                .ToList(),
            DailyOverrides = availability.DailyOverrides
                .OrderBy(day => day.OverrideDate)
                .Select(day => new ResourceAvailabilityDayOverrideResponse
                {
                    OverrideDate = day.OverrideDate,
                    Available = day.Available,
                    StartTime = day.StartTime,
                    EndTime = day.EndTime,
                    Notes = day.Notes
                })
                .ToList(),
            CreatedAt = availability.CreatedAt,
            UpdatedAt = availability.UpdatedAt
        };
    }
}
